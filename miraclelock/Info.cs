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
    public partial class Info : Form
    {
        List<HotelRooms> HR;
        private int Building;
        private int Floor;
        private int Room;
        HotelRooms _GP;
        string Card = string.Empty;
        byte[] cardbuf = new byte[250];

        public Info()
        {
            InitializeComponent();
            label2.Text = string.Empty;
            label7.Text = string.Empty;
            label8.Text = string.Empty;
            label3.Text = string.Empty;
        }

        public Info(List<HotelRooms> h, string C, byte[] cb) : this()
        {
            HR = h;
            Card = C;
            cardbuf = cb;
        }

        private void Info_Load(object sender, EventArgs e)
        {
            label5.Visible = Convert.ToBoolean(Program.Config["Depo"] ?? false);
            byte[] lockno = new byte[8];
            string R;
            byte[] Edate = new byte[10];
            byte[] cardtype = new byte[16];

            if (M1Enc.GetGuestLockNoByCardDataStr(Program.HotelId, cardbuf, lockno) == 0)
            {
                R = Encoding.ASCII.GetString(lockno).Substring(0, 6);
                Building = int.Parse(R.Substring(0, 2));
                Floor = int.Parse(R.Substring(2, 2));
                Room = int.Parse(R.Substring(4, 2));
                _GP = Find(R);
                string r = (_GP == null) ? string.Empty : _GP.HumanReadableRoom;
                label2.Text = r;
            }
            if (M1Enc.GetCardTypeByCardDataStr(cardbuf, cardtype) == 0)
                label3.Text = M1Enc.Card_type(cardtype[0]);

            if (M1Enc.GetGuestETimeByCardDataStr(Program.HotelId, cardbuf, Edate) == 0)
            {
                label7.Text = GetTime(Encoding.ASCII.GetString(Edate)).ToString();
            }
            using (LockDbDataContext Db = new LockDbDataContext(Convert.ToString(Program.Config["ConnectionString"])))
            {
                DateTime Now = DateTime.Now;
                var H = Db.Rooms.Where(x => x.Building == Building && x.Floor == Floor && x.Room == Room).OrderByDescending(x => x.Id);
                string h = string.Empty;
                if (H.Count() > 0)
                {
                    var H1 = Db.Cards.Where(x => x.Ci.Value <= Now && Now <= x.Co.Value && x.RoomId == H.First().Id);
                    foreach (var h1 in H1)
                    {
                        dataGridView1.Rows.Add(new object[] { h1.Id, h1.Holder, h1.Card, h1.Ci.Value, h1.Co.Value });
                        if (h1.Card.Trim().ToUpper() == Card.Trim().ToUpper())
                        {
                            if (dataGridView1.Rows.Count > 0)
                                dataGridView1.Rows[0].Selected = false;
                            if (dataGridView1.Rows.Count > 0)
                                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Selected = true;
                        }
                    }
                }
            }

            if (Convert.ToBoolean(Program.Config["Depo"] ?? false))
            {
                try
                {
                    label5.Visible = false;
                    label8.Text = string.Empty;

                    IDepo c = (new Depo.Depo()).Connect(Convert.ToString(Program.Config["DepoServerAddress"]));
                    if (c.Holder(Card, out string Holder) == Results.Succsess)
                    {
                        CiCol.Visible = false;
                        CoCol.Visible = false;
                        dataGridView1.Rows.Add(new object[] { 0, Holder, Card });
                    }
                    if (c.CheckAmount(Card, out decimal Amount) == Results.Succsess)
                    {
                        label5.Visible = (Amount > 0);
                        label8.Text = $"{Amount:C2}";
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show($"{err.Message}");
                }
            }
            timer2.Start();
        }

        private HotelRooms Find(string room)
        {
            foreach (HotelRooms r in HR)
            {
                if ($"{r.Building:00}{r.Floor:00}{r.Room:00}" == room)
                {
                    return r;
                }
            }
            return null;
        }

        private DateTime GetTime(string p)
        {
            DateTime d = new DateTime();
            try
            {
                d = new DateTime(2000 + Int16.Parse(p.Substring(0, 2)), Int16.Parse(p.Substring(2, 2)), Int16.Parse(p.Substring(4, 2)), Int16.Parse(p.Substring(6, 2)), Int16.Parse(p.Substring(8, 2)), 0);//, Int16.Parse(p.Substring(10)));
            }
            catch (Exception) { };
            return d;
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            Close();
        }

        // поселить
        private void Button1_Click(object sender, EventArgs e)
        {
            (new Поселение(_GP, HR) { StartPosition = FormStartPosition.CenterScreen }).Show();
        }

        // выселить
        private void Button2_Click(object sender, EventArgs e)
        {
            (new Выселение(HR, _GP) { StartPosition = FormStartPosition.CenterScreen }).Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            (new Стереть_карту() { StartPosition = FormStartPosition.CenterScreen }).Show();
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {

        }
    }
}
