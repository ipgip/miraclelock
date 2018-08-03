using Depo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace miraclelock
{
    public partial class P1 : Form
    {
        private int Building;
        private int Floor;
        private int Room;
        private List<HotelRooms> HR;

        public P1()
        {
            InitializeComponent();
        }

        public P1(List<HotelRooms> h, HotelRooms R) : this()
        {
            HR = h;
            Building = R.Building;
            Floor = R.Floor;
            Room = R.Room;
            label8.Text = $"{R.HumanReadableRoom}";
            //label5.Text = DateTime.Now.ToString("dd MMM yyyy HH:mm");
            dateTimePicker1.Enabled = false;
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
                Save.Enabled = false;
                return;
            }
            if (Convert.ToBoolean(Program.Config["Depo"] ?? false))
            {
                textBox2.Visible = Convert.ToBoolean(Program.Config["Depo"] ?? false);
            }
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            int ret;
            byte[] cardbuf = new byte[250];
            string Bdate = DateTime.Now.ToString("yyMMddHHmm");
            string Edate = dateTimePicker2.Value.ToString("yyMMddHHmm");


            if (M1Enc.InitializeUSB(1) != 0)
            {
                //MessageBox.Show("Проблемы с авторизатором");
                Save.Enabled = false;
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
                            if (int.TryParse(textBox7.Text, out int appartment)) { }
                            if (int.TryParse(textBox6.Text, out int house)) { }
                            Persone P = new Persone
                            {
                                FirstName = textBox1.Text,
                                SurName = textBox2.Text,
                                SecondName = textBox3.Text,
                                Country = (int)comboBox1.SelectedValue,
                                Town = textBox4.Text,
                                Street = textBox5.Text,
                                House = house,
                                Appartment = appartment,
                                DockType = (int)comboBox2.SelectedValue,
                                DockNumber = textBox8.Text
                            };
                            Db.Persone.InsertOnSubmit(P);
                            Db.SubmitChanges();
                            Cards C = new Cards
                            {
                                Ci = Ci,
                                Co = Co,
                                Card = Card,
                                RoomId = r.First().Id,
                                Holder = P.Id
                            };

                            //Db.Cards.InsertOnSubmit(C);
                            r.First().Cards.Add(C);
                            Db.SubmitChanges();
                            Db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues);
                        }
                        // работаем с депозитной системой
                        if (Convert.ToBoolean(Program.Config["Depo"] ?? false))
                        {
                            if (decimal.TryParse(textBox9.Text, out decimal Deposite))
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
            else
            {
                MessageBox.Show("нет карты");
            }
        }

        private void P1_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                using (LockDbDataContext db = new LockDbDataContext(Convert.ToString(Program.Config["ConnectionString"])))
                {
                    comboBox1.DisplayMember = "Title";
                    comboBox1.ValueMember = "Id";
                    comboBox1.DataSource = db.Countries.Select(x => new { Id = x.Id, Title = x.Title.Trim() }).ToList();

                    comboBox2.DisplayMember = "Title";
                    comboBox2.ValueMember = "Id";
                    comboBox2.DataSource = db.Docs.Select(x => new { Id = x.Id, Title = x.Title.Trim() }).ToList();
                }
                label10.Visible = textBox9.Visible = Convert.ToBoolean(Program.Config["Depo"] ?? false);
            }
        }

        private void textBox10_Leave(object sender, EventArgs e)
        {
            if (int.TryParse(textBox10.Text, out int d)) { }
            dateTimePicker2.Value = dateTimePicker1.Value.AddDays(d);

        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            textBox10.Text = (dateTimePicker2.Value - dateTimePicker1.Value).Days.ToString();
        }
    }
}
