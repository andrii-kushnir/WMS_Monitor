namespace WMS_Monitor
{
    partial class RayonForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this._problemGrid = new System.Windows.Forms.DataGridView();
            this._naklGrid = new System.Windows.Forms.DataGridView();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.вихідToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._pSklad = new System.Windows.Forms.Panel();
            this._lTimer = new System.Windows.Forms.Label();
            this._bRefresh = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._problemGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._naklGrid)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this._pSklad.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.ForeColor = System.Drawing.Color.Black;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this._problemGrid);
            this.splitContainer1.Panel1.Controls.Add(this._naklGrid);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this._pSklad);
            this.splitContainer1.Size = new System.Drawing.Size(1904, 1001);
            this.splitContainer1.SplitterDistance = 764;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(3, 735);
            this.label1.Name = "label1";
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.Size = new System.Drawing.Size(495, 25);
            this.label1.TabIndex = 52;
            this.label1.Text = "Невдалі спроби потрапляння накладних у WMS:";
            // 
            // _problemGrid
            // 
            this._problemGrid.BackgroundColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._problemGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this._problemGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Times New Roman", 16F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._problemGrid.DefaultCellStyle = dataGridViewCellStyle2;
            this._problemGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._problemGrid.Location = new System.Drawing.Point(0, 763);
            this._problemGrid.Name = "_problemGrid";
            this._problemGrid.RowHeadersVisible = false;
            this._problemGrid.Size = new System.Drawing.Size(764, 238);
            this._problemGrid.TabIndex = 51;
            this._problemGrid.TabStop = false;
            // 
            // _naklGrid
            // 
            this._naklGrid.BackgroundColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._naklGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this._naklGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 22F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._naklGrid.DefaultCellStyle = dataGridViewCellStyle4;
            this._naklGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._naklGrid.Location = new System.Drawing.Point(0, 0);
            this._naklGrid.Name = "_naklGrid";
            this._naklGrid.RowHeadersVisible = false;
            this._naklGrid.Size = new System.Drawing.Size(764, 732);
            this._naklGrid.TabIndex = 50;
            this._naklGrid.TabStop = false;
            this._naklGrid.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this._naklGrid_CellClick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.вихідToolStripMenuItem});
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.Top;
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1904, 24);
            this.menuStrip1.TabIndex = 53;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // вихідToolStripMenuItem
            // 
            this.вихідToolStripMenuItem.Name = "вихідToolStripMenuItem";
            this.вихідToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.вихідToolStripMenuItem.Text = "Вихід";
            this.вихідToolStripMenuItem.Click += new System.EventHandler(this.вихідToolStripMenuItem_Click);
            // 
            // _pSklad
            // 
            this._pSklad.Controls.Add(this._lTimer);
            this._pSklad.Controls.Add(this._bRefresh);
            this._pSklad.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pSklad.AutoScroll = true;
            this._pSklad.Location = new System.Drawing.Point(0, 0);
            this._pSklad.Name = "_pSklad";
            this._pSklad.Size = new System.Drawing.Size(1139, 1001);
            this._pSklad.TabIndex = 0;
            // 
            // _lTimer
            // 
            this._lTimer.AutoSize = true;
            this._lTimer.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this._lTimer.Location = new System.Drawing.Point(800, 48);
            this._lTimer.Name = "_lTimer";
            this._lTimer.Size = new System.Drawing.Size(105, 73);
            this._lTimer.TabIndex = 154;
            this._lTimer.Visible = false;
            // 
            // _bRefresh
            // 
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RayonForm));
            this._bRefresh.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_bRefresh.BackgroundImage")));
            this._bRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this._bRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this._bRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 27.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this._bRefresh.Location = new System.Drawing.Point(411, 40);
            this._bRefresh.Name = "_bRefresh";
            this._bRefresh.Size = new System.Drawing.Size(381, 101);
            this._bRefresh.TabIndex = 153;
            this._bRefresh.Text = "Оновити(30)";
            this._bRefresh.UseVisualStyleBackColor = false;
            this._bRefresh.Click += new System.EventHandler(this._bRefresh_Click);
            // 
            // RayonForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1904, 1001);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "RayonForm";
            this.Text = "МОНІТОР діяльності складу";
            this.Load += new System.EventHandler(this.RayonForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._problemGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._naklGrid)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this._pSklad.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel _pSklad;
        private System.Windows.Forms.DataGridView _naklGrid;
        private System.Windows.Forms.DataGridView _problemGrid;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button _bRefresh;
        private System.Windows.Forms.Label _lTimer;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem вихідToolStripMenuItem;
    }
}
