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
    public partial class Выбор_карты : Form
    {
        int Id;
        LockDbDataContext Db = new LockDbDataContext(Convert.ToString(Program.Config["ConnectionString"]));

        public Выбор_карты()
        {
            InitializeComponent();
        }

        private void Выбор_карты_Load(object sender, EventArgs e)
        {
            if (Tag != null)
            {
                Id = Convert.ToInt32(Tag);
                label1.Visible = false;
                comboBox1.Visible = false;
                Text += $" {Id}";
                LoadNumber();
            }
            else
            {
                comboBox1.ValueMember = "Id";
                comboBox1.DisplayMember = "HumanRoomNumber";
                comboBox1.DataSource = Db.Rooms.Where(x => x.State.Value == (int)RoomStates.Busy).OrderBy(x => x.HumanRoomNumber.Trim());
                label1.Visible = true;
                comboBox1.Visible = true;
            }
        }

        private void LoadNumber()
        {
            DateTime Today = DateTime.Now;
            var H = Db.Rooms.Where(x => x.Id == Id);
            string h = string.Empty;
            if (H.Count() > 0)
            {
                var H1 = Db.Cards.Where(x => x.Ci.Value <= Today && Today <= x.Co.Value);
                List<Cards> r = H1.ToList();
                foreach(var r1 in r)
                {
                    dataGridView1.Rows.Add(new object[] { r1.Id, r1.Holder, r1.Card, r1.Ci, r1.Co });
                }
            }
        }

        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            //if (dataGridView1.SelectedRows.Count > 0)
            //{
            //    Tag = dataGridView1.SelectedRows[0].Cells["IdCol"].Value;
            //    DialogResult = DialogResult.OK;
            //}
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadNumber();

        }

        private void DataGridView1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                Tag = dataGridView1.SelectedRows[0].Cells["IdCol"].Value;
                DialogResult = DialogResult.OK;
            }

        }
    }
}
