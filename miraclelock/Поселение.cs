using Depo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace miraclelock
{
    public partial class Поселение : Form
    {
        int Building;
        int Floor;
        int Room;
        List<HotelRooms> HR;

        public Поселение()
        { }

        public Поселение(HotelRooms R, List<HotelRooms> h) : this()
        {
            HR = h;
            Building = R.Building;
            Floor = R.Floor;
            Room = R.Room;
            InitializeComponent();
            label2.Text = $"{R.HumanReadableRoom}";
            label5.Text = DateTime.Now.ToString("dd MMM yyyy HH:mm");
            dateTimePicker2.MinDate = DateTime.Today;
            if (DateTime.Today == dateTimePicker2.Value.Date)
            {
                dateTimePicker2.Value = DateTime.Today.AddHours(DateTime.Now.Hour + 1).AddMinutes(00);
            }
            else
            {
                dateTimePicker2.Value = DateTime.Today.AddHours(12).AddMinutes(00);
            }
            if (M1Enc.InitializeUSB(1) != 0)
            {
                //MessageBox.Show("Проблемы с авторизатором");
                button1.Enabled = false;
                return;
            }
            if (Convert.ToBoolean(Program.Config["Depo"] ?? false))
            {
                textBox2.Visible = Convert.ToBoolean(Program.Config["Depo"] ?? false);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            int ret;
            byte[] cardbuf = new byte[250];
            string Bdate = DateTime.Now.ToString("yyMMddHHmm");
            string Edate = dateTimePicker2.Value.ToString("yyMMddHHmm");


            if (M1Enc.InitializeUSB(1) != 0)
            {
                //MessageBox.Show("Проблемы с авторизатором");
                button1.Enabled = false;
                return;
            }
            string Crd = string.Empty;
            if ((M1Enc.ReadCard(1, cardbuf) == 0) && (cardbuf[5] != 48))
            {
                byte b = cardbuf[5];
                if ((ret = M1Enc.GuestCard(1, Program.HotelId,
                1, 0, 0, 0, Bdate.ToCharArray(0, 10), Edate.ToCharArray(0, 10),
                ($"{Building:00}{Floor:00}{Room:00}" + "99").ToCharArray(0, 8), cardbuf)) == 0)
                {
                    if ((ret = M1Enc.ReadCard(1, cardbuf)) != 0)
                    {
                        MessageBox.Show("Ошибка чтения карты");
                    }
                    else
                    {
                        DateTime Ci = DateTime.Now;
                        DateTime Co;
                        byte[] E = new byte[10];
                        if ((M1Enc.GetGuestETimeByCardDataStr(Program.HotelId, cardbuf, E)) == 0)
                        {
                            string p = Encoding.ASCII.GetString(E);
                            Co = new DateTime(2000 + Int16.Parse(p.Substring(0, 2)),
                                Int16.Parse(p.Substring(2, 2)), Int16.Parse(p.Substring(4, 2)), Int16.Parse(p.Substring(6, 2)), Int16.Parse(p.Substring(8, 2)), 0);
                        }
                        else
                        {
                            Co = DateTime.Today;
                        }
                        string Card = Encoding.ASCII.GetString(cardbuf).Substring(24, 8);
                        string Holder = (textBox1.Text.Length > 0) ? textBox1.Text.Trim() : string.Empty;

                        using (LockDbDataContext Db = new LockDbDataContext(Convert.ToString(Program.Config["ConnectionString"])))
                        {
                            //var r1 = HR.Where(x => x.Building == Building && x.Floor == Floor && x.Room == Room);
                            //if (r1.Count() > 0)
                            //{
                            //    r1.First().State = RoomStates.Busy;
                            //}

                            var r = Db.Rooms.Where(x => x.Building == Building && x.Floor == Floor && x.Room == Room);
                            if (r.Count() > 0)
                            {
                                r.First().State = (int)RoomStates.Busy;
                            }

                            //if (r.First().Cards.Count() > 0)
                            //{
                            //    Cards C = r.First().Cards.First();
                            //    C.Ci = Ci;
                            //    C.Co = Co;
                            //    C.Card = Card;
                            //    C.RoomId = r.First().Id;
                            //    C.Holder = Holder;
                            //}
                            //else
                            {
                                Cards C = new Cards
                                {
                                    Ci = Ci,
                                    Co = Co,
                                    Card = Card,
                                    RoomId = r.First().Id,
                                    Holder = Holder
                                };
                                //Db.Cards.InsertOnSubmit(C);
                                r.First().Cards.Add(C);
                            }
                            Db.SubmitChanges();
                            Db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues);
                        }
                        // работаем с депозитной системой
                        if (Convert.ToBoolean(Program.Config["Depo"] ?? false))
                        {
                            if (decimal.TryParse(textBox2.Text, out decimal Deposite))
                            {
                                // прверяем существование счета пользователя
                                string address = Convert.ToString(Program.Config["DepoServerAddress"]);
                                IDepo c = (new Depo.Depo()).Connect(address);
                                try
                                {
                                    if (c.CheckAmount(Card, out decimal Amount) == Results.Succsess)
                                    {
                                        // Счет существует, пополняем его
                                        if (c.Plus(Card, Deposite) == Results.Succsess)
                                        {
                                            // Пополнение успешно
                                            MessageBox.Show("Пополнение успешно");
                                        }
                                        else
                                        {
                                            MessageBox.Show("Ошибка ополнения счета");
                                        }

                                    }
                                    else
                                    {
                                        // Счета нет, создаем его
                                        if (c.CreateAccount(Card, Holder, Deposite) == Results.Succsess)
                                        {
                                            // Счет создан
                                            MessageBox.Show("Счет создан");
                                        }
                                        else
                                        {
                                            MessageBox.Show("Ошибка создания счета");
                                        }
                                    }
                                }
                                catch (Exception err)
                                {
                                    MessageBox.Show($"{err.Message}");
                                }
                            }
                        }
                        Close();
                    }
                }
                else
                {
                    MessageBox.Show("Ошибка записи");
                }
            }
        }
    }
}
