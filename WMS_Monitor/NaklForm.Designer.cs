
namespace WMS_Monitor
{
    partial class NaklForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this._bRefresh = new System.Windows.Forms.Button();
            this._bClose = new System.Windows.Forms.Button();
            this._lPlace = new System.Windows.Forms.Label();
            this._lDateCreated = new System.Windows.Forms.Label();
            this._lType = new System.Windows.Forms.Label();
            this._lNumber = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this._dgvTovar = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dgvTovar)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this._bRefresh);
            this.splitContainer1.Panel1.Controls.Add(this._bClose);
            this.splitContainer1.Panel1.Controls.Add(this._lPlace);
            this.splitContainer1.Panel1.Controls.Add(this._lDateCreated);
            this.splitContainer1.Panel1.Controls.Add(this._lType);
            this.splitContainer1.Panel1.Controls.Add(this._lNumber);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this._dgvTovar);
            this.splitContainer1.Size = new System.Drawing.Size(1904, 1041);
            this.splitContainer1.SplitterDistance = 232;
            this.splitContainer1.TabIndex = 0;
            // 
            // _bRefresh
            // 
            this._bRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this._bRefresh.Location = new System.Drawing.Point(1157, 3);
            this._bRefresh.Name = "_bRefresh";
            this._bRefresh.Size = new System.Drawing.Size(369, 225);
            this._bRefresh.TabIndex = 9;
            this._bRefresh.Text = "Оновити";
            this._bRefresh.UseVisualStyleBackColor = true;
            this._bRefresh.Click += new System.EventHandler(this._bRefresh_Click);
            // 
            // _bClose
            // 
            this._bClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this._bClose.Location = new System.Drawing.Point(1532, 3);
            this._bClose.Name = "_bClose";
            this._bClose.Size = new System.Drawing.Size(369, 225);
            this._bClose.TabIndex = 8;
            this._bClose.Text = "Закрити";
            this._bClose.UseVisualStyleBackColor = true;
            this._bClose.Click += new System.EventHandler(this._bClose_Click);
            // 
            // _lPlace
            // 
            this._lPlace.AutoSize = true;
            this._lPlace.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this._lPlace.Location = new System.Drawing.Point(814, 15);
            this._lPlace.Name = "_lPlace";
            this._lPlace.Size = new System.Drawing.Size(153, 55);
            this._lPlace.TabIndex = 7;
            this._lPlace.Text = "Місце";
            // 
            // _lDateCreated
            // 
            this._lDateCreated.AutoSize = true;
            this._lDateCreated.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this._lDateCreated.Location = new System.Drawing.Point(417, 153);
            this._lDateCreated.Name = "_lDateCreated";
            this._lDateCreated.Size = new System.Drawing.Size(382, 55);
            this._lDateCreated.TabIndex = 5;
            this._lDateCreated.Text = "Дата створення";
            // 
            // _lType
            // 
            this._lType.AutoSize = true;
            this._lType.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this._lType.Location = new System.Drawing.Point(312, 84);
            this._lType.Name = "_lType";
            this._lType.Size = new System.Drawing.Size(345, 55);
            this._lType.TabIndex = 4;
            this._lType.Text = "Тип накладної";
            // 
            // _lNumber
            // 
            this._lNumber.AutoSize = true;
            this._lNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this._lNumber.Location = new System.Drawing.Point(290, 15);
            this._lNumber.Name = "_lNumber";
            this._lNumber.Size = new System.Drawing.Size(412, 55);
            this._lNumber.TabIndex = 3;
            this._lNumber.Text = "Номер накладної";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(16, 153);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(395, 55);
            this.label3.TabIndex = 2;
            this.label3.Text = "Дата створення:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(12, 84);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(294, 55);
            this.label2.TabIndex = 1;
            this.label2.Text = "Тип клієнта:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(272, 55);
            this.label1.TabIndex = 0;
            this.label1.Text = "Накладна :";
            // 
            // _dgvTovar
            // 
            this._dgvTovar.BackgroundColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._dgvTovar.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this._dgvTovar.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("JetBrains Mono", 20F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._dgvTovar.DefaultCellStyle = dataGridViewCellStyle2;
            this._dgvTovar.Dock = System.Windows.Forms.DockStyle.Fill;
            this._dgvTovar.Location = new System.Drawing.Point(0, 0);
            this._dgvTovar.Name = "_dgvTovar";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._dgvTovar.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this._dgvTovar.RowHeadersVisible = false;
            this._dgvTovar.Size = new System.Drawing.Size(1904, 805);
            this._dgvTovar.TabIndex = 0;
            // 
            // NaklForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1904, 1041);
            this.Controls.Add(this.splitContainer1);
            this.Name = "NaklForm";
            this.Text = "NaklForm";
            this.Load += new System.EventHandler(this.NaklForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._dgvTovar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView _dgvTovar;
        private System.Windows.Forms.Label _lDateCreated;
        private System.Windows.Forms.Label _lType;
        private System.Windows.Forms.Label _lNumber;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label _lPlace;
        private System.Windows.Forms.Button _bClose;
        private System.Windows.Forms.Button _bRefresh;
    }
}