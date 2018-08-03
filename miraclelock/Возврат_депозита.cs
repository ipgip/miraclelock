using Depo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace miraclelock
{
    public partial class Возврат_депозита : Form
    {
        string Card = string.Empty;

        public Возврат_депозита()
        {
            InitializeComponent();
        }

        public Возврат_депозита(string C) : this()
        {
            Card = C;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (decimal.TryParse(label1.Text, NumberStyles.Currency, CultureInfo.CreateSpecificCulture("ru-RU"), out decimal Deposite))
            {
                string address = Convert.ToString(Program.Config["DepoServerAddress"]);
                IDepo c = (new Depo.Depo()).Connect(address);
                if (c.Minus(Card, Deposite) == Results.Succsess)
                {
                    MessageBox.Show("Возврат выполнен");
                }
                else
                {
                    MessageBox.Show("Ошибка возврата");
                }
            }
            Close();
        }

        private void Возврат_депозита_Load(object sender, EventArgs e)
        {
            //byte[] cardbuf = new byte[250];
            //byte[] lockno = new byte[8];
            //string R;
            //byte[] Edate = new byte[10];

            //if (M1Enc.InitializeUSB(1) != 0)
            //{
            //    MessageBox.Show("Проблемы с авторизатором");
            //    Close();
            //    return;
            //}
            //if (M1Enc.ReadCard(1, cardbuf) == 0)
            //{
            //    string Signature = Encoding.ASCII.GetString(cardbuf).Substring(0, 6);
            //    if (Signature == "551501")
            //    {
            //        Card = Encoding.ASCII.GetString(cardbuf).Substring(24, 8);

            // прверяем существование счета пользователя
            if (Convert.ToBoolean(Program.Config["Depo"] ?? false))
            {
                string address = Convert.ToString(Program.Config["DepoServerAddress"]);
                IDepo c = (new Depo.Depo()).Connect(address);
                try
                {
                    if (c.CheckAmount(Card, out decimal Amount) == Results.Succsess)
                    {
                        // Счет существует
                        label1.Text = Amount.ToString("C2");
                    }
                    else
                    {
                        MessageBox.Show($"С картой {Card} не связано ни одного счета");
                        Close();
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show($"{err.Message}");
                }

            }
            //}
            //}
            //else
            //{
            //    MessageBox.Show("Нет карты в автризаторе");
            //}

        }
    }
}
