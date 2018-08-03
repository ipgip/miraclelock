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
    public partial class LostForm : Form
    {
        public LostForm()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            byte[] cardbuf = new byte[250];
            string Card = Convert.ToString(Tag);
            if ((M1Enc.ReadCard(1, cardbuf) == 0) && (cardbuf[5] != 48))
            {
                if (M1Enc.LimitCard(1, Program.HotelId, 1, 0, DateTime.Now.AddDays(1).ToString("yyMMddHHmm").ToCharArray(0, 10), Card_Pack(Card.Trim()), cardbuf) == 0)
                {
                    DialogResult = DialogResult.OK;
                }
            }
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
    }
}
