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
        private const string connectionSql101 = @"Server=192.168.4.101; Database=erp; uid=КушнірА; pwd=зщшфтв;";

        private List<Tovar> ListTovar = new List<Tovar>();

        private NakladnaWMS _nakl;
        public NaklForm(NakladnaWMS nakl)
        {
            InitializeComponent();

            _dgvTovar.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            _dgvTovar.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            _nakl = nakl;
            LoadNakl(_nakl);

            _lNumber.Text = _nakl.Coden.ToString();
            _lType.Text = _nakl.NameDoc;
            _lPlace.Text = _nakl.PlaceERP;
            var timeSpan = DateTime.Now - nakl.DateOpen;
            var textTimer = (timeSpan.TotalMinutes < 90) ? $"{(int)timeSpan.TotalMinutes}хв" : $"{(int)timeSpan.TotalHours}гд";
            _lDateCreated.Text = $"{_nakl.DateOpen} ({textTimer})";

            var listTovar = ListTovar.Select(n => new { Кас_код = n.codetv, Назва_товару = n.nametv, Кільк = n.kol, Ов = n.ov }).ToList();
            _dgvTovar.DataSource = listTovar;
            _dgvTovar.ClearSelection();
            _dgvTovar.CurrentCell = null;
        }

        private void LoadNakl(NakladnaWMS nakl)
        {
            using (var connection = new SqlConnection(connectionSql101))
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
                            codetvun = Convert.ToInt32(reader["codetvun"]),
                            codetv = Convert.ToInt32(reader["codetv"]),
                            nametv = Convert.ToString(reader["nametv"]),
                            kol = Convert.ToDouble(reader["kol"]),
                            ov = Convert.ToString(reader["ov"])
                        };
                        if (tovar.nametv.Length > 50)
                            tovar.nametv = tovar.nametv.Substring(0, 50);
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
    }
}
