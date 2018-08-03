using Depo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace miraclelock
{
    public partial class Выселение : Form
    {
        List<HotelRooms> HR;
        HotelRooms current;
        int Building;
        int Floor;
        int Room;
        string Card = string.Empty;

        public Выселение()
        {
            InitializeComponent();
            label2.Text = string.Empty;
            label5.Text = string.Empty;
            label6.Visible = true;
        }

        // выселение по карте в авторизаторе
        public Выселение(List<HotelRooms> h) : this()
        {
            HR = h;
        }

        public Выселение(List<HotelRooms> h, HotelRooms c) : this()
        {
            label6.Visible = false;
            HR = h;
            if (c != null)
            {
                current = c;
                label2.Text = c.HumanReadableRoom;
                label5.Text = $"{DateTime.Now:dd MMM yyyy HH:mm}";
            }
            else
            {
                MessageBox.Show("Карта не от номера");
                button1.Enabled = false;
            }
            if (M1Enc.InitializeUSB(1) != 0)
            {
                timer1.Stop();
                //MessageBox.Show("Проблемы с авторизатором");
                button1.Enabled = false;
            }
        }

        private void Выселение_Load(object sender, EventArgs e)
        {
            if (current == null)
                Close();
            timer1.Start();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            byte[] cardbuf = new byte[250];
            byte[] lockno = new byte[8];
            string R;

            if (M1Enc.InitializeUSB(1) != 0)
            {
                timer1.Stop();
                //MessageBox.Show("Проблемы с авторизатором");
                button1.Enabled = false;
                return;
            }
            if (M1Enc.ReadCard(1, cardbuf) != 0)
            {
                MessageBox.Show("Ошибка чтения");
            }
            else
            {
                string Signature = Encoding.ASCII.GetString(cardbuf);
                if (Signature.Length > 6)
                {
                    if (Signature.Substring(0, 6) == "551501")
                    {
                        Card = Encoding.ASCII.GetString(cardbuf).Substring(24, 8);
                        if (Card != "FFFFFFFF")
                        {
                            timer1.Stop();
                            if (M1Enc.GetGuestLockNoByCardDataStr(Program.HotelId, cardbuf, lockno) == 0)
                            {
                                R = Encoding.ASCII.GetString(lockno).Substring(0, 6);
                                Building = int.Parse(R.Substring(0, 2));
                                Floor = int.Parse(R.Substring(2, 2));
                                Room = int.Parse(R.Substring(4, 2));
                                label2.Text = Find(R);
                            }
                            if (current == null)
                            {
                                current = HR.Find(x => x.Building == Building && x.Floor == Floor && x.Room == Room);
                            }
                        }
                    }
                }
            }
        }

        private string Find(string room)
        {
            if (HR != null)
            {
                foreach (HotelRooms r in HR)
                {
                    if ($"{r.Building:00}{r.Floor:00}{r.Room:00}" == room)
                    {
                        return r.HumanReadableRoom;
                    }
                }
            }
            return string.Empty;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            // проверяем депозит
            if (Convert.ToBoolean(Program.Config["Depo"] ?? false))
            {
                IDepo c = (new Depo.Depo()).Connect(Convert.ToString(Program.Config["DepoServerAddress"]));
                if (c.CheckAmount(Card, out decimal Amount) == Results.Succsess)
                {
                    if (Amount > 0)
                    {
                        MessageBox.Show("На карте есть активный депозит. Выселение не возможно");
                        return;
                    }
                }
            }
            byte[] cardbuf = new byte[250];
            using (LockDbDataContext Db = new LockDbDataContext(Convert.ToString(Program.Config["ConnectionString"])))
            {
                DateTime Now = DateTime.Now;

                var _C = Db.Cards.Where(x => x.Rooms.Building.Value == current.Building &&
                                         x.Rooms.Floor.Value == current.Floor &&
                                         x.Rooms.Room.Value == current.Room &&
                                         x.Rooms.Room.Value == current.Room && x.Co.Value > Now);
                int count = _C.Count();
                if (count > 0)
                {
                    foreach (var c in _C)
                    {
                        Form f = new Form()
                        {
                            StartPosition = FormStartPosition.CenterScreen,
                            AutoSize = true,
                            AutoSizeMode = AutoSizeMode.GrowAndShrink,
                            ShowIcon = false,
                            ShowInTaskbar = false,
                            ControlBox = false,
                            Text = $"Положите карту {count--} от номера в авторизатор"
                        };
                        //f.Controls.Add(new Label
                        //{
                        //    Font = new Font(DefaultFont.FontFamily, 12),
                        //    TextAlign = ContentAlignment.MiddleCenter,
                        //    Text = $"Положите карту {count--} от номера в авторизатор"
                        //});
                        f.Controls.Add(new Button
                        {
                            DialogResult = DialogResult.OK,
                            Font = new Font(DefaultFont.FontFamily, 12),
                            AutoSize = true,
                            Text = $"Записать карту от номера"
                        });
                        Hide();
                        if (f.ShowDialog() == DialogResult.OK)
                        {
                            if (Program.IshueCard(Now) == 0)
                            //{ }
                            //Стереть_карту f = new Стереть_карту() { StartPosition = FormStartPosition.CenterScreen, Tag = c.Card.Trim() };
                            //if (f.ShowDialog() == DialogResult.OK)
                            {
                                c.Co = DateTime.Now;
                                f.Close();
                                //Db.Cards.DeleteOnSubmit(c);
                            }
                        }
                    }
                }

                var r = Db.Rooms.Where(x => x.Building == Building && x.Floor == Floor && x.Room == Room && x.Cards.Where(y => y.Co.Value > Now).Count() > 0);
                if (r.Count() > 0)
                {
                    Rooms R = r.First();
                    if (R.Cards.Count(x => x.Co.Value > DateTime.Now) <= 0)
                        r.First().State = (int)RoomStates.Vacant;
                }
                Db.SubmitChanges();
            }
            Close();
        }

        private void CheckOutWithOutCard_Click(object sender, EventArgs e)
        {
            using (LockDbDataContext Db = new LockDbDataContext(Convert.ToString(Program.Config["ConnectionString"])))
            {
                //int CardNo = 0;
                //Выбор_карты f = new Выбор_карты() { Tag = Db.Rooms.Where(x => x.Building.Value == current.Building && x.Floor.Value == current.Floor && x.Room.Value == current.Room).First().Id };
                //if (f.ShowDialog() == DialogResult.OK)
                //{
                //CardNo = Convert.ToInt32(f.Tag);

                DateTime Now = DateTime.Now;
                // пишем lost по количеству карт в номере
                var _C = Db.Cards.Where(x => x.Rooms.Building.Value == current.Building &&
                  x.Rooms.Floor.Value == current.Floor &&
                  x.Rooms.Room.Value == current.Room && x.Co.Value > Now);
                if (_C.Count() > 0)
                {
                    int count = _C.Count();
                    foreach (var c in _C)
                    {
                        Hide();
                        LostForm lf = new LostForm()
                        {
                            Tag = c.Card,
                            StartPosition = FormStartPosition.CenterScreen,
                            ShowIcon = false,
                            ShowInTaskbar = false,
                            Text = $"Положите чистую карту в авторизатор"
                        };
                        if (lf.ShowDialog() == DialogResult.OK)
                        {
                            count--;
                            c.Co = DateTime.Now;
                            //Db.Cards.DeleteOnSubmit(c);
                        }
                    }
                }

                //var r1 = HR.Where(x => x.Building == current.Building && x.Floor == current.Floor && x.Room == current.Room);
                //if (r1.Count() > 0)
                //{
                //    r1.First().State = RoomStates.Vacant;
                //}
                var _r = Db.Rooms.Where(x => x.Building.Value == current.Building && x.Floor.Value == current.Floor && x.Room.Value == current.Room && x.Cards.Where(y => y.Co.Value > Now).Count() > 0);
                if (_r.Count() > 0)
                {
                    Rooms R = _r.First();
                    if (R.Cards.Count(x => x.Co.Value > DateTime.Now) <= 0)
                        R.State = (int)RoomStates.Vacant;
                }
                Db.SubmitChanges();
            }
            Close();
        }

        private byte[] Card_Pack(string p)
        {
            byte[] res = new byte[4];
            char[] ar = p.ToCharArray();
            for (int i = 0; i < 4; i++)
            {
                res[i] = Byte.Parse(p.Substring(2 * i, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            return res;
        }

        private void Room2HotelRooms(int roomId)
        {
            using (LockDbDataContext Db = new LockDbDataContext(Convert.ToString(Program.Config["ConnectionString"])))
            {
                var _r = Db.Rooms.Where(x => x.Id == roomId);
                if (_r.Count() > 0)
                {
                    Rooms R = _r.First();
                    Building = R.Building.Value;
                    Floor = R.Floor.Value;
                    Room = R.Room.Value;
                }
            }
        }
    }
}
