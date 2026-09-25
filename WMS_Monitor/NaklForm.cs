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
        private bool _layoutBusy;
        private Size _lastLayoutSize;

        public NaklForm(NakladnaWMS nakl)
        {
            InitializeComponent();

            _dgvTovar.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            _dgvTovar.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            _nakl = nakl;
            WindowState = FormWindowState.Maximized;
            Resize += NaklForm_Resize;
        }

        private void Refreshing()
        {
            ListTovar.Clear();
            LoadNakl(_nakl);

            _lNumber.Text = _nakl.GuidNakl;
            _lType.Text = _nakl.NameDoc;
            _lPlace.Text = _nakl.PlaceERP;
            var timeSpan = DateTime.Now - _nakl.DateOpen;
            var textTimer = (timeSpan.TotalMinutes < 90) ? $"{(int)timeSpan.TotalMinutes}хв" : $"{(int)timeSpan.TotalHours}гд";
            _lDateCreated.Text = $"{_nakl.DateOpen.ToString("dd.MM.yy HH:mm:ss")} ({textTimer})";

            if (ListTovar.All(t => t.isPause == false))
                _dgvTovar.DataSource = ListTovar.Select(n => new { Кас_код = n.codetv, Категорія = n.categor, Назва_товару = n.nametv, Кільк = n.countTovar, Етап = n.operReal, Оператор = n.resReal.Length > 20 ? n.resReal.Substring(0, 20) : n.resReal, Час_захоплення = n.dateZahopl == DateTime.MinValue ? "" : n.dateZahopl.ToString("HH:mm:ss") }).ToList();
            else
                _dgvTovar.DataSource = ListTovar.Select(n => new { Кас_код = n.codetv, Категорія = n.categor, Назва_товару = n.nametv, Кільк = n.countTovar, Етап = n.operReal, Оператор = n.resReal.Length > 20 ? n.resReal.Substring(0, 20) : n.resReal, Час_захоплення = n.dateZahopl == DateTime.MinValue ? "" : n.dateZahopl.ToString("HH:mm:ss"), Пауза = n.isPause ? "Так" : "Ні" }).ToList();

            foreach (DataGridViewRow row in _dgvTovar.Rows)
            {
                if (_nakl.Type == NaklType.PokCM || _nakl.Type == NaklType.Zbut || _nakl.Type == NaklType.MP)
                {
                    if (row.Cells[4].Value.ToString() == "Видано" || row.Cells[4].Value.ToString() == "Видано")
                        row.DefaultCellStyle.BackColor = Color.FromArgb(0, 255, 0);
                    else if (row.Cells[6].Value.ToString() == "")
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
            using (var connection = new SqlConnection(Program.connectionSql))
            {
                //Поміняти на nakl.GuidNakl тут і в SQL коли буду поновляти нову версію усім:
                string query = (nakl.Codesk == null || nakl.Codesk == 4)
                    ? $"EXECUTE [us_MonitorNakl] {nakl.Coden}"
                    : $"EXECUTE [us_MonitorNakl] {nakl.Coden}, {nakl.Codesk}";
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
                            categor = Convert.ToString(reader["categor"]),
                            nametv = Convert.ToString(reader["nametv"]),
                            countTovar = Convert.ToDouble(reader["countTovar"]),
                            operName = Convert.ToString(reader["operName"]),
                            resName = reader["resName"] == System.DBNull.Value ? null : Convert.ToString(reader["resName"]),
                            dateExec = Convert.ToDateTime(reader["dateExec"]),
                            resRozp = reader["resRozp"] == System.DBNull.Value ? null : Convert.ToString(reader["resRozp"]),
                            dateZahopl = reader["dateZahopl"] == System.DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["dateZahopl"]),
                            isPause = reader["isPause"] == System.DBNull.Value ? false : BitConverter.ToBoolean((byte[])reader["isPause"], 0)
                        };
                        if (tovar.nametv.Length > 40)
                            tovar.nametv = tovar.nametv.Substring(0, 40);
                        ListTovar.Add(tovar);
                    }
                }
                catch
                {
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
            _lastLayoutSize = Size.Empty;
            LayoutForScreen();
        }

        private void NaklForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                return;
            LayoutForScreen();
        }

        private void LayoutForScreen()
        {
            if (_layoutBusy || splitContainer1.ClientSize.Width < 100 || splitContainer1.ClientSize.Height < 100)
                return;
            if (splitContainer1.ClientSize == _lastLayoutSize)
                return;

            _layoutBusy = true;
            try
            {
                _lastLayoutSize = splitContainer1.ClientSize;
                int w = splitContainer1.ClientSize.Width;
                int h = splitContainer1.ClientSize.Height;
                bool fullHd = w >= 1800 && h >= 950;
                if (fullHd)
                    LayoutDesign();
                else
                    LayoutCompact(w, h);
            }
            catch
            {
            }
            finally
            {
                _layoutBusy = false;
            }
        }

        private void LayoutDesign()
        {
            splitContainer1.Panel1MinSize = 80;
            splitContainer1.Panel2MinSize = 80;
            if (splitContainer1.Height > 232)
                splitContainer1.SplitterDistance = 232;

            SetControlFont(label1, 36F);
            SetControlFont(label2, 36F);
            SetControlFont(label3, 36F);
            SetControlFont(_lNumber, 36F);
            SetControlFont(_lType, 36F);
            SetControlFont(_lPlace, 36F);
            SetControlFont(_lDateCreated, 36F);
            SetControlFont(_bRefresh, 48F);
            SetControlFont(_bClose, 48F);

            label1.Location = new Point(12, 15);
            _lNumber.Location = new Point(290, 15);
            _lPlace.Location = new Point(857, 15);
            label2.Location = new Point(12, 84);
            _lType.Location = new Point(312, 84);
            label3.Location = new Point(16, 153);
            _lDateCreated.Location = new Point(417, 153);
            _bRefresh.Bounds = new Rectangle(1157, 3, 369, 225);
            _bClose.Bounds = new Rectangle(1532, 3, 369, 225);
            SetGridFonts(20F, 16F);
        }

        private void LayoutCompact(int w, int h)
        {
            float scale = Math.Min(w / 1904F, h / 1041F);
            scale = Math.Max(0.5F, Math.Min(1F, scale));

            int headerH = Math.Max(96, Math.Min(232, (int)(232 * scale)));
            if (headerH > h / 3)
                headerH = Math.Max(96, h / 3);

            splitContainer1.Panel1MinSize = 80;
            splitContainer1.Panel2MinSize = 80;
            if (headerH < splitContainer1.Height)
                splitContainer1.SplitterDistance = headerH;

            float labelSize = Math.Max(14F, 36F * scale);
            float buttonSize = Math.Max(16F, 48F * scale);
            SetControlFont(label1, labelSize);
            SetControlFont(label2, labelSize);
            SetControlFont(label3, labelSize);
            SetControlFont(_lNumber, labelSize);
            SetControlFont(_lType, labelSize);
            SetControlFont(_lPlace, labelSize);
            SetControlFont(_lDateCreated, labelSize);
            SetControlFont(_bRefresh, buttonSize);
            SetControlFont(_bClose, buttonSize);

            int pad = Math.Max(6, (int)(12 * scale));
            int btnW = Math.Max(90, (int)(369 * scale));
            int btnH = Math.Max(40, headerH - pad * 2);
            if (btnW * 2 + pad * 3 > w / 2)
                btnW = Math.Max(80, (w / 2 - pad * 3) / 2);

            _bClose.Bounds = new Rectangle(w - pad - btnW, pad, btnW, btnH);
            _bRefresh.Bounds = new Rectangle(_bClose.Left - pad - btnW, pad, btnW, btnH);

            int rowH = Math.Max(label1.Height + 4, (headerH - pad) / 3);
            int xCaption = pad;
            label1.Location = new Point(xCaption, pad);
            label2.Location = new Point(xCaption, pad + rowH);
            label3.Location = new Point(xCaption, pad + rowH * 2);

            int xValue = Math.Max(label1.Right, Math.Max(label2.Right, label3.Right)) + pad;
            _lNumber.Location = new Point(xValue, pad);
            _lType.Location = new Point(xValue, pad + rowH);
            _lDateCreated.Location = new Point(xValue, pad + rowH * 2);
            _lPlace.Location = new Point(_lNumber.Right + pad, pad);

            int maxLabelRight = _bRefresh.Left - pad;
            if (_lPlace.Right > maxLabelRight)
                _lPlace.Location = new Point(Math.Max(xCaption, maxLabelRight - _lPlace.Width), _lPlace.Top);
            if (_lType.Right > maxLabelRight)
                _lType.Location = new Point(Math.Max(xCaption, maxLabelRight - _lType.Width), _lType.Top);
            if (_lDateCreated.Right > maxLabelRight)
                _lDateCreated.Location = new Point(Math.Max(xCaption, maxLabelRight - _lDateCreated.Width), _lDateCreated.Top);

            SetGridFonts(Math.Max(11F, 20F * scale), Math.Max(10F, 16F * scale));
        }

        private void SetGridFonts(float cellSize, float headerSize)
        {
            _dgvTovar.DefaultCellStyle.Font = MakeFont("JetBrains Mono", cellSize, FontStyle.Regular);
            _dgvTovar.ColumnHeadersDefaultCellStyle.Font = MakeFont("Microsoft Sans Serif", headerSize, FontStyle.Regular);
            _dgvTovar.RowHeadersDefaultCellStyle.Font = MakeFont("Microsoft Sans Serif", headerSize, FontStyle.Regular);
        }

        private static void SetControlFont(Control control, float size)
        {
            var old = control.Font;
            control.Font = MakeFont(old != null ? old.FontFamily.Name : "Microsoft Sans Serif", size, old != null ? old.Style : FontStyle.Regular);
            if (old != null && old != control.Font)
                old.Dispose();
        }

        private static Font MakeFont(string family, float size, FontStyle style)
        {
            size = Math.Max(8F, size);
            try
            {
                return new Font(family, size, style, GraphicsUnit.Point, 204);
            }
            catch
            {
                return new Font("Microsoft Sans Serif", size, style, GraphicsUnit.Point, 204);
            }
        }
    }
}
