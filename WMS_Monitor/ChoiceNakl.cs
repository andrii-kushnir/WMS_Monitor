using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMS_Monitor.Model;

namespace WMS_Monitor
{
    public partial class ChoiceNakl : Form
    {
        private List<NakladnaWMS> _listNakl;

        public ChoiceNakl(List<NakladnaWMS> listNakl)
        {
            InitializeComponent();

            _dgvNakl.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            _dgvNakl.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            _listNakl = listNakl;
            _dgvNakl.DataSource = _listNakl.Select(n => new { Накладна = n.Coden}).OrderBy(n => n.Накладна).ToList();
        }

        private void _dgvNakl_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var nakl = _listNakl.LastOrDefault(n => n.Coden == (int)_dgvNakl.Rows[e.RowIndex].Cells[0].Value);
            Close();
            var naklForm = new NaklForm(nakl);
            naklForm.Show();
        }

        private void ChoiceNakl_Load(object sender, EventArgs e)
        {
            _dgvNakl.ScrollBars = ScrollBars.None;
            var totalHeight = _dgvNakl.Rows.GetRowsHeight(DataGridViewElementStates.None);
            var totalWidth = _dgvNakl.Columns.GetColumnsWidth(DataGridViewElementStates.None);
            _dgvNakl.ClientSize = new Size(totalWidth, totalHeight);
            this.ClientSize = new Size(totalWidth + 2, totalHeight + 2);
        }

        private void ChoiceNakl_Deactivate(object sender, EventArgs e)
        {
            Close();
        }
    }
}
