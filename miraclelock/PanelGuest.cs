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
    public partial class PanelGuest : UserControl
    {
        private int Room = 0;

        private event EventHandler<GuestInfo> _GuestNew;
        private event EventHandler<GuestInfo> _GuestSave;

        [Category("IGP"), Description("Событие по записи гостя")]
        public event EventHandler<GuestInfo> GuestSave
        {
            add { _GuestSave += value; }
            remove { _GuestSave -= value; }
        }

        [Category("IGP"), Description("Событие по созданию нового гостя")]
        public event EventHandler<GuestInfo> GuestNew
        {
            add { _GuestNew += value; }
            remove { _GuestNew -= value; }
        }


        public PanelGuest()
        {
            InitializeComponent();
        }

        public PanelGuest(int R) : this()
        {
            Room = R;
        }

        private void NewBTN_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBox6.Text, out int H)) { }
            if (int.TryParse(textBox7.Text, out int App)) { }
            _GuestNew?.Invoke(this, new GuestInfo(
                textBox1.Text,
                textBox2.Text,
                textBox3.Text,
                (int)comboBox1.SelectedValue,
                textBox4.Text,
                textBox5.Text,
                H,
                App,
                (int)comboBox2.SelectedValue,
                textBox8.Text
                ));
        }

        private void SaveBTN_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBox6.Text, out int H)) { }
            if (int.TryParse(textBox7.Text, out int App)) { }
            _GuestSave?.Invoke(this, new GuestInfo(
                textBox1.Text,
                textBox2.Text,
                textBox3.Text,
                (int)comboBox1.SelectedValue,
                textBox4.Text,
                textBox5.Text,
                H,
                App,
                (int)comboBox2.SelectedValue,
                textBox8.Text
                ));

        }

        private void PanelGuest_Load(object sender, EventArgs e)
        {
            NewBTN.Visible = false;// (_GuestNew != null);
            SaveBTN.Visible = false;// (_GuestSave != null);
            AddButton1.Visible = true;
            //toolStrip1.Visible = (NewBTN.Visible || SaveBTN.Visible);
            Text += $" {Room}";
            if (!DesignMode)
            {
                using (LockDbDataContext db = new LockDbDataContext(Convert.ToString(Program.Config["ConnectionString"])))
                {
                    comboBox1.DisplayMember = "Title";
                    comboBox1.ValueMember = "Id";
                    comboBox1.DataSource = db.Countries.Select(x=>x.Title.Trim()).ToList();

                    comboBox2.DisplayMember = "Title";
                    comboBox2.ValueMember = "Id";
                    comboBox2.DataSource = db.Docs.Select(x => x.Title.Trim()).ToList();
                }

            }        }

        private void AddButton1_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBox6.Text, out int H)) { }
            if (int.TryParse(textBox7.Text, out int App)) { }
            _GuestSave?.Invoke(this, new GuestInfo(
                textBox1.Text,
                textBox2.Text,
                textBox3.Text,
                (int)comboBox1.SelectedValue,
                textBox4.Text,
                textBox5.Text,
                H,
                App,
                (int)comboBox2.SelectedValue,
                textBox8.Text
                ));
        }
    }

    public class GuestInfo: EventArgs
    {
        public string FirstName;
        public string SurnameName;
        public string SecondName;
        public int Country;
        public string Town;
        public string Street;
        public int House;
        public int Appartment;
        public int DocType;
        public string DocNumber;

        public GuestInfo(string FN, string SurName, string SN, int C, string T, string Str, int H, int App, int DT, string DN)
        {
            FirstName = FN;
        }
    }
}
