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
    public partial class Staff : Form
    {
        List<HotelRooms> HR;

        public Staff()
        {
            InitializeComponent();
        }

        public Staff(List<HotelRooms> h) : this()
        {
            HR = h;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            byte[] cardbuf = new byte[250];
            string Bdate = DateTime.Now.ToString("yyMMddHHmm");

            string Edate = DateTime.Now.AddMonths(4).ToString("yyMMddHHmm");

            if (M1Enc.InitializeUSB(1) != 0)
            {
                MessageBox.Show("Проблемы с авторизатором");
                Close();
                return;
            }

            if ((M1Enc.ReadCard(1, cardbuf) == 0) && (cardbuf[5] != 48))
            {
                if (M1Enc.BuildingCard(1, Program.HotelId, 1, 1, 1, 1, 127, Bdate.ToCharArray(0, 10), "0001".ToCharArray(0, 4), (Edate.Substring(0, 6) + "2359").ToCharArray(0, 10), $"{HR[0].Building:00}".ToCharArray(0, 2), cardbuf) == 0)
                {
                    if (M1Enc.ReadCard(1, cardbuf) == 0)
                    {
                        // [todo] записывать в базу номер карты и держателя для последующего аннулирования карты
                    }
                    this.Close();
                }
                else
                    MessageBox.Show("Ошибка записи");
            }
        }

        private void Staff_Load(object sender, EventArgs e)
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
