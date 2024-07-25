using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMS_Monitor.Model;

namespace WMS_Monitor
{
    public partial class NaklForm : Form
    {
        private List<Tovar> ListTovar = new List<Tovar>();

        private NakladnaWMS _nakl;
        public NaklForm(NakladnaWMS nakl)
        {
            InitializeComponent();

            _dgvTovar.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            _dgvTovar.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            _nakl = nakl;
        }

        private void Refreshing()
        {
            ListTovar.Clear();
            LoadNakl(_nakl);

            _lNumber.Text = _nakl.Coden.ToString();
            _lType.Text = _nakl.NameDoc;
            _lPlace.Text = _nakl.PlaceERP;
            var timeSpan = DateTime.Now - _nakl.DateOpen;
            var textTimer = (timeSpan.TotalMinutes < 90) ? $"{(int)timeSpan.TotalMinutes}хв" : $"{(int)timeSpan.TotalHours}гд";
            _lDateCreated.Text = $"{_nakl.DateOpen.ToString("dd.MM.yy HH:mm:ss")} ({textTimer})";

            if (ListTovar.All(t => t.isPause == false))
                _dgvTovar.DataSource = ListTovar.Select(n => new { Кас_код = n.codetv, Назва_товару = n.nametv, Кільк = n.countTovar, Етап = n.operReal, Оператор = n.resReal.Length > 20 ? n.resReal.Substring(0, 20) : n.resReal, Час_захоплення = n.dateZahopl == DateTime.MinValue ? "" : n.dateZahopl.ToString("HH:mm:ss") }).ToList();
            else
                _dgvTovar.DataSource = ListTovar.Select(n => new { Кас_код = n.codetv, Назва_товару = n.nametv, Кільк = n.countTovar, Етап = n.operReal, Оператор = n.resReal.Length > 20 ? n.resReal.Substring(0, 20) : n.resReal, Час_захоплення = n.dateZahopl == DateTime.MinValue ? "" : n.dateZahopl.ToString("HH:mm:ss"), Пауза = n.isPause ? "Так" : "Ні" }).ToList();

            foreach (DataGridViewRow row in _dgvTovar.Rows)
            {
                if (_nakl.Type == NaklType.PokCM || _nakl.Type == NaklType.Zbut || _nakl.Type == NaklType.MP)
                {
                    if (row.Cells[3].Value.ToString() == "Видано" || row.Cells[3].Value.ToString() == "Видано")
                        row.DefaultCellStyle.BackColor = Color.FromArgb(0, 255, 0);
                    else if (row.Cells[5].Value.ToString() == "")
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 0, 0);
                    else 
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 0);
                }
            }

            _dgvTovar.ClearSelection();
            _dgvTovar.CurrentCell = null;
        }

        private void LoadNakl(NakladnaWMS nakl)
        {
            using (var connection = new SqlConnection(Program.connectionSql101sa))
            {
                string query = $"EXECUTE [us_MonitorNakl] {nakl.Coden}";
                var command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = null;
                try
                {
                    reader = command.ExecuteReader();
                    //reader.NextResult();
                    while (reader.Read())
                    {
                        var tovar = new Tovar
                        {
                            codetv = Convert.ToInt32(reader["codetv"]),
                            nametv = Convert.ToString(reader["nametv"]),
                            countTovar = Convert.ToDouble(reader["countTovar"]),
                            operName = Convert.ToString(reader["operName"]),
                            resName = reader["resName"] == System.DBNull.Value ? null : Convert.ToString(reader["resName"]),
                            dateExec = Convert.ToDateTime(reader["dateExec"]),
                            resRozp = reader["resRozp"] == System.DBNull.Value ? null : Convert.ToString(reader["resRozp"]),
                            dateZahopl = reader["dateZahopl"] == System.DBNull.Value ? DateTime.MinValue :  Convert.ToDateTime(reader["dateZahopl"]),
                            isPause = reader["isPause"] == System.DBNull.Value ? false : BitConverter.ToBoolean((byte[])reader["isPause"], 0)
                    };
                        if (tovar.nametv.Length > 40)
                            tovar.nametv = tovar.nametv.Substring(0, 40);
                        ListTovar.Add(tovar);
                    }
                }
                catch (Exception ex)
                {
#warning доробити errors!
                    //SaveErrorToSQL(connection, ex.Message, $"codetvun = {codetvun}");
                }
                finally
                {
                    reader?.Close();
                }
            }
        }

        private void _bClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void _bRefresh_Click(object sender, EventArgs e)
        {
            Refreshing();
        }

        private void NaklForm_Load(object sender, EventArgs e)
        {
            Refreshing();
        }
    }
}
