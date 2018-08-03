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
    public partial class SelectDepo : Form
    {
        public SelectDepo()
        {
            InitializeComponent();
        }

        private void SelectDepo_Load(object sender, EventArgs e)
        {
            foreach (var i in (Tag as IQueryable<Cards>))
            {
                dataGridView1.Rows.Add(new object[] { i.Id, i.Card, i.Holder });
            }

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
