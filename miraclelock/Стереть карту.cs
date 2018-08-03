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
    public partial class Стереть_карту : Form
    {
        public Стереть_карту()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            byte[] cardbuf = new byte[250];
            if (Program.HotelId != 0)
            {
                if (M1Enc.InitializeUSB(1) != 0)
                {
                    MessageBox.Show("Проблемы с авторизатором");
                    Close();
                    return;
                }

                if ((M1Enc.ReadCard(1, cardbuf) == 0) && (cardbuf[5] != 48))
                {
                    string Card = Encoding.ASCII.GetString(cardbuf).Substring(24, 8);
                    if (Card == "FFFFFFFF")
                    {
                        MessageBox.Show("Карта уже чистая!");
                        return;
                    }
                    else
                    {
                        if (M1Enc.CardErase(1, Program.HotelId, cardbuf) == 0)
                        {
                            MessageBox.Show("Карта стерта");
                            DialogResult = DialogResult.OK;
                            //Close();
                        }
                    }
                }
            }
        }

        private void Стереть_карту_Load(object sender, EventArgs e)
        {
            if (M1Enc.InitializeUSB(1) != 0)
            {
                MessageBox.Show("Проблемы с авторизатором");
                Close();
                return;
            }

        }
    }
}
