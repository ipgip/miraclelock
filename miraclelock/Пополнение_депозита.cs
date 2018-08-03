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
    public partial class Пополнение_депозита : Form
    {
        string Card = string.Empty;

        public Пополнение_депозита()
        {
            InitializeComponent();
        }

        public Пополнение_депозита(string C) : this()
        {
            Card = C;

            if (Convert.ToBoolean(Program.Config["Depo"] ?? false))
            {
                // прверяем существование счета пользователя
                string address = Convert.ToString(Program.Config["DepoServerAddress"]);
                IDepo c = (new Depo.Depo()).Connect(address);
                if (c.Holder(Card, out string Holder) == Results.Succsess)
                {
                    textBox2.Text = Holder;
                    textBox2.Enabled = false;
                }
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (Convert.ToBoolean(Program.Config["Depo"] ?? false))
            {
                if (decimal.TryParse(textBox1.Text, out decimal Deposite))
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
                                //MessageBox.Show("Пополнение успешно");
                            }
                            else
                            {
                                MessageBox.Show("Ошибка ополнения счета");
                            }
                        }
                        else
                        {
                            // Счета нет, будем создвать его
                            // 1. записываем карту на номер 999999
                            if (Program.IshueCard(99, 99, 99, DateTime.Today.AddYears(1)) == 0)
                            {
                                // 2. читаем номер карты
                                byte[] cardbuf = new byte[250];
                                Card = Program.ReadCard(out cardbuf);
                            }
                            // 3. создаем депозитный счет
                            if (c.CreateAccount(Card, textBox2.Text, Deposite) == Results.Succsess)
                            {
                                //MessageBox.Show("Счет создан успешно");
                                if (textBox2.Text != string.Empty)
                                {
                                    using (LockDbDataContext Db = new LockDbDataContext(Convert.ToString(Program.Config["ConnectionString"])))
                                    {
                                        Db.Cards.InsertOnSubmit(new Cards
                                        {
                                            Card = Card,
                                            Holder = textBox2.Text,
                                            Ci=DateTime.Now,
                                            Co=DateTime.Today.AddYears(1)
                                        });
                                        Db.SubmitChanges();
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("Ошибка создания счета");
                            }
                            //MessageBox.Show($"С картой {Card} не связано ни одного счета");
                        }
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"{err.Message}");
                    }
                }

            }            //    }
            //}
            //else
            //{
            //    MessageBox.Show("Нет карты в автризаторе");
            //}
            Close();
        }
    }
}
