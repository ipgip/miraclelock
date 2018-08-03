namespace miraclelock
{
    partial class Выбор_карты
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.IdCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HolderCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CardCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CiCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CoCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.IdCol,
            this.HolderCol,
            this.CardCol,
            this.CiCol,
            this.CoCol});
            this.dataGridView1.Location = new System.Drawing.Point(1, 27);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(282, 232);
            this.dataGridView1.TabIndex = 24;
            this.dataGridView1.Click += new System.EventHandler(this.DataGridView1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-2, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 25;
            this.label1.Text = "Номер";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(59, 0);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 21);
            this.comboBox1.TabIndex = 26;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.ComboBox1_SelectedIndexChanged);
            // 
            // IdCol
            // 
            this.IdCol.DataPropertyName = "Id";
            this.IdCol.HeaderText = "Id";
            this.IdCol.Name = "IdCol";
            this.IdCol.Visible = false;
            // 
            // HolderCol
            // 
            this.HolderCol.DataPropertyName = "Holder";
            this.HolderCol.HeaderText = "Гость";
            this.HolderCol.Name = "HolderCol";
            // 
            // CardCol
            // 
            this.CardCol.DataPropertyName = "Card";
            this.CardCol.HeaderText = "Карта";
            this.CardCol.Name = "CardCol";
            this.CardCol.Visible = false;
            // 
            // CiCol
            // 
            this.CiCol.DataPropertyName = "Ci";
            this.CiCol.HeaderText = "С";
            this.CiCol.Name = "CiCol";
            // 
            // CoCol
            // 
            this.CoCol.DataPropertyName = "Co";
            this.CoCol.HeaderText = "По";
            this.CoCol.Name = "CoCol";
            // 
            // Выбор_карты
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dataGridView1);
            this.Name = "Выбор_карты";
            this.Text = "Выбор_карты";
            this.Load += new System.EventHandler(this.Выбор_карты_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.DataGridViewTextBoxColumn IdCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn HolderCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn CardCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn CiCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn CoCol;
    }
}