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
    public partial class Продление : Form
    {
        string Card = string.Empty;
        string Card1 = string.Empty;
        string HumanReadNumber = string.Empty;
        public Продление()
        {
            InitializeComponent();
        }

        public Продление(string C, string H) : this()
        {
            Card = C;
            HumanReadNumber = H ?? string.Empty;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (Card == string.Empty)
                return;
            if (Program.IshueCard(dateTimePicker1.Value) == 0)
            {
                Card1 = Program.ReadCard(out byte[] cardbuf);
                if (Convert.ToBoolean(Program.Config["Depo"] ?? false))
                {
                    // Проверяем наличие депозита
                    string address = Convert.ToString(Program.Config["DepoServerAddress"]);
                    IDepo c = (new Depo.Depo()).Connect(address);
                    if ((c.CheckAmount(Card, out decimal Amount) == Results.Succsess) && (Amount > 0))
                    {
                        // Депозит есть - меняем номер карты
                        if (Card1 != string.Empty)
                        {
                            if (c.ChangeCard(Card, Card1) != Results.Succsess)
                            {
                                MessageBox.Show("Ошибка изменения номера депозитной карты");
                            }
                        }
                    }
                    //else
                    //{
                    //    // Депозита нет
                    //}
                }
                Close();
            }
        }

        private void Продление_Load(object sender, EventArgs e)
        {
            if (HumanReadNumber != string.Empty)
            {
                label2.Text = HumanReadNumber;
            }
            else
            {
                MessageBox.Show("Карта не от номера, продлевать нечего");
                Close();
            }
        }
    }
}
