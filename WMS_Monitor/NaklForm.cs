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
        private const string connectionSql101 = @"Server=192.168.4.101; Database=erp; uid=sa; pwd=Yi*7tg8tc=t?PjM;";

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
            _lDateCreated.Text = $"{_nakl.DateOpen} ({textTimer})";

            var listTovar = ListTovar.Select(n => new { Кас_код = n.codetv, Назва_товару = n.nametv, Кільк = n.countTovar, Оператор = n.resName.Length > 20 ? n.resName.Substring(0, 20) : n.resName, Операція = n.dateExec == new DateTime(0001, 01, 01) ? n.operName : "Видано"}).ToList();
            _dgvTovar.DataSource = listTovar;

            foreach (DataGridViewRow row in _dgvTovar.Rows)
            {
                if (_nakl.Type == NaklType.PokCM || _nakl.Type == NaklType.Zbut || _nakl.Type == NaklType.MP)
                {
                    var tovar = ListTovar.Find(n => n.codetv == listTovar[row.Index].Кас_код);
                    if (listTovar[row.Index].Оператор == "-=Не взято=-")
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 0, 0);
                    else if (listTovar[row.Index].Операція == "Відвантаження відбір")
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 0);
                    else if (listTovar[row.Index].Операція == "Контейнер")
                        row.DefaultCellStyle.BackColor = Color.FromArgb(220, 220, 0);
                    else if (listTovar[row.Index].Операція == "Відвантаження завершення")
                        row.DefaultCellStyle.BackColor = Color.FromArgb(100, 200, 0);
                    else if (listTovar[row.Index].Операція == "Видано")
                        row.DefaultCellStyle.BackColor = Color.FromArgb(0, 255, 0);
                }
            }

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
                            codetv = Convert.ToInt32(reader["codetv"]),
                            nametv = Convert.ToString(reader["nametv"]),
                            countTovar = Convert.ToDouble(reader["countTovar"]),
                            operName = Convert.ToString(reader["operName"]),
                            resName = reader["resName"] == System.DBNull.Value ? "-=Не взято=-" : Convert.ToString(reader["resName"]),
                            dateExec = Convert.ToDateTime(reader["dateExec"])
                        };
                        if (tovar.nametv.Length > 44)
                            tovar.nametv = tovar.nametv.Substring(0, 44);
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
