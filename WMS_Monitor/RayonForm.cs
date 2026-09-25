using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows.Forms;
using WMS_Monitor.Model;

namespace WMS_Monitor
{
    public partial class RayonForm : Form
    {
        private readonly int _sklad;
        private const int timeWaiting15 = 15 * 60;
        private const string OtherPlaceKey = "OTHER";

        private Dictionary<string, KomirkaVisual> ListKomirka = new Dictionary<string, KomirkaVisual>();
        private List<NakladnaWMS> ListNakladna = new List<NakladnaWMS>();
        private List<NakladnaWMS> ErrorNakladna = new List<NakladnaWMS>();
        private readonly HashSet<int> _cancelledNakl = new HashSet<int>();

        private System.Threading.Timer _timer;
        private int intervalUpdateMonitor = 30 * 1000;
        private DateTime lastExecute = DateTime.MinValue;

        private System.Windows.Forms.Timer timerRefreshButton = new System.Windows.Forms.Timer() { Interval = 1000 };
        private System.Windows.Forms.Timer timerRefresh = new System.Windows.Forms.Timer() { Interval = 1000 };
        private DateTime _startTime;

        public RayonForm(int sklad)
        {
            _sklad = sklad;
            InitializeComponent();
            KomirkaInit();

            Text = $"МОНІТОР діяльності складу {_sklad}";
            WindowState = FormWindowState.Maximized;

            _naklGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            _naklGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            _problemGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            _problemGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            _startTime = DateTime.Now.AddSeconds(1 - (intervalUpdateMonitor / 1000));
            timerRefreshButton.Enabled = true;
            timerRefreshButton.Tick += timerRefreshButton_Tick;
            timerRefresh.Enabled = true;
            timerRefresh.Tick += timerRefresh_Tick;
            _timer = new System.Threading.Timer(new TimerCallback(UpdateMonitor), null, intervalUpdateMonitor, intervalUpdateMonitor);
            Resize += RayonForm_Resize;
        }

        private void UpdateMonitor(object obj)
        {
            UpdateFromBD();
            UpdateTimingNakl();
            ShowErrorNakl();
            _startTime = DateTime.Now;
            InvokeUi(() =>
            {
                _bRefresh.Text = "Оновлено";
                _bRefresh.Enabled = false;
            });
        }

        private void UpdateFromBD()
        {
            using (var connection = new SqlConnection(Program.connectionSql))
            {
                string query;
                if (lastExecute == DateTime.MinValue)
                    query = $"EXECUTE [us_MonitorUpdateRayon] {_sklad}";
                else
                    query = $"EXECUTE [us_MonitorUpdateRayon] {_sklad}, '{lastExecute:yyyy-MM-dd HH:mm:ss.fff}'";

                var command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = null;
                bool notifyNewNakl = lastExecute != DateTime.MinValue;
                bool hasNewNakl = false;
                try
                {
                    reader = command.ExecuteReader();
                    reader.Read();
                    lastExecute = Convert.ToDateTime(reader["dateNow"]);
                    var namesk = reader["namesk"] == DBNull.Value || string.IsNullOrWhiteSpace(Convert.ToString(reader["namesk"]))
                        ? _sklad.ToString()
                        : Convert.ToString(reader["namesk"]);
                    InvokeUi(() => { Text = $"МОНІТОР діяльності складу {namesk}"; });

                    reader.NextResult();
                    while (reader.Read())
                    {
                        var coden = Convert.ToInt32(reader["coden"]);
                        var guidnakl = Convert.ToString(reader["guid"]);
                        if (String.IsNullOrWhiteSpace(guidnakl)) guidnakl = coden.ToString();
                        ErrorNakladna.RemoveAll(n => n.Coden == coden);
                        if (_cancelledNakl.Contains(coden))
                            continue;
                        var nakl = ListNakladna.FirstOrDefault(n => n.Coden == coden && n.GuidNakl == guidnakl);
                        if (nakl == null)
                        {
                            var place = Convert.ToString(reader["place"])?.Trim();
                            var nameErp = reader["nameERP"] == DBNull.Value ? null : Convert.ToString(reader["nameERP"]);
                            if (String.IsNullOrWhiteSpace(place))
                                continue;
                            if (!ListKomirka.ContainsKey(place))
                            {
                                if (string.IsNullOrWhiteSpace(nameErp))
                                    nameErp = string.IsNullOrWhiteSpace(place) ? "Інші" : place;
                                place = OtherPlaceKey;
                            }

                            nakl = new NakladnaWMS
                            {
                                Coden = coden,
                                Codesk = _sklad,
                                GuidNakl = guidnakl,
                                PlaceWMS = place,
                                PlaceERP = nameErp,
                                Text = Convert.ToString(reader["error"]),
                                DateOpen = Convert.ToDateTime(reader["date_log"]),
                                Dostavka = reader["codepdost"] == System.DBNull.Value ? 0 : Convert.ToInt32(reader["codepdost"]),
                                NameDoc = reader["NameDoc"] == System.DBNull.Value ? "" : Convert.ToString(reader["NameDoc"])
                            };
                            if ((DateTime.Now - nakl.DateOpen).TotalHours > 72) continue;
                            switch (nakl.NameDoc)
                            {
                                case "Відбір - Покупець СМ":
                                    nakl.Type = NaklType.PokCM;
                                    break;
                                case "Відбір - Супермаркет":
                                    nakl.Type = NaklType.RayonMarket;
                                    continue;
                                case "Відбір - Центральний Склад":
                                    nakl.Type = NaklType.RayonCS;
                                    continue;
                                case "Господарські витрати":
                                    nakl.Type = NaklType.Gosp;
                                    nakl.NameDoc = "Господарськ";
                                    continue;
                                case "":
                                    nakl.Type = NaklType.Empty;
                                    continue;
                                case "Повернення покупців":
                                    nakl.Type = NaklType.Povern;
                                    continue;
                                case "Внутрішній прихід":
                                    nakl.Type = NaklType.VnPr;
                                    continue;
                                case "Прихід від постачальника":
                                    nakl.Type = NaklType.Post;
                                    continue;
                                default:
                                    nakl.Type = NaklType.Empty;
                                    continue;
                            }
                            ListNakladna.Add(nakl);
                            if (notifyNewNakl)
                                hasNewNakl = true;
                        }
                    }

                    if (hasNewNakl)
                        PlayNewNaklSound();

                    reader.NextResult();
                    while (reader.Read())
                    {
                        var coden = Convert.ToInt32(reader["coden"]);
                        var guidnakl = Convert.ToString(reader["guid"]);
                        var nakl = ListNakladna.FirstOrDefault(n => n.GuidNakl == guidnakl);
                        if (nakl != null)
                        {
                            var close = Convert.ToDateTime(reader["datenlog"]);
                            nakl.DateClose = close;
                            if (ListKomirka.ContainsKey(nakl.PlaceWMS))
                                ListKomirka[nakl.PlaceWMS].blink = false;
                            ListNakladna.Remove(nakl);
                        }
                        nakl = ErrorNakladna.FirstOrDefault(n => n.Coden == coden);
                        if (nakl != null)
                        {
                            ErrorNakladna.Remove(nakl);
                        }
                    }

                    reader.NextResult();
                    while (reader.Read())
                    {
                        var coden = Convert.ToInt32(reader["coden"]);
                        var nakl = ErrorNakladna.FirstOrDefault(n => n.Coden == coden);
                        if (nakl == null)
                        {
                            nakl = new NakladnaWMS
                            {
                                Coden = coden,
                                Text = Convert.ToString(reader["error"]),
                                DateOpen = Convert.ToDateTime(reader["date_log"])
                            };
                            if ((DateTime.Now - nakl.DateOpen).TotalHours > 11) continue;
                            ErrorNakladna.Add(nakl);
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    reader?.Close();
                }
            }
        }

        private void UpdateTimingNakl()
        {
            foreach (var k in ListKomirka.Values)
            {
                InvokeUi(() =>
                {
                    k.Text.Visible = false;
                    k.Text.Text = "";
                    k.Text.ForeColor = Color.Black;
                    k.Text.BackColor = Color.White;
                    k.Number.BackColor = Color.White;
                });
            }

            ListNakladna = ListNakladna.OrderByDescending(n => n.DateOpen).ToList();
            foreach (var nakl in ListNakladna.ToList())
            {
                bool inWork = false;
                bool isEnd = false;
                bool cancelled = false;
                using (var connection = new SqlConnection(Program.connectionSql))
                {
                    var query = $"EXECUTE [us_MonitorNakl] {nakl.Coden}, {_sklad}";
                    var command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = null;
                    try
                    {
                        inWork = true;
                        isEnd = true;
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            if (Convert.ToInt32(reader["is_cancelled"]) == 1)
                            {
                                _cancelledNakl.Add(nakl.Coden);
                                ListNakladna.Remove(nakl);
                                cancelled = true;
                                break;
                            }
                            var operId = Convert.ToInt32(reader["operId"]);
                            if (operId == 10)
                                continue;
                            else
                                isEnd = false;
                            var resRozp = reader["resRozp"] == System.DBNull.Value ? null : Convert.ToString(reader["resRozp"]);
                            if (resRozp == null)
                                inWork = false;
                            else
                                nakl.Worker = resRozp;
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        reader?.Close();
                    }
                }

                if (cancelled) continue;
                var boardPlace = GetBoardPlace(nakl.PlaceWMS);
                if (!ListKomirka.ContainsKey(boardPlace)) continue;
                if (nakl.Type != NaklType.PokCM && nakl.Type != NaklType.Zbut && nakl.Type != NaklType.Gosp && ListNakladna.Any(n => GetBoardPlace(n.PlaceWMS) == boardPlace && (n.Type == NaklType.PokCM || n.Type == NaklType.Zbut || n.Type == NaklType.Gosp)))
                    continue;

                var komirka = ListKomirka[boardPlace];
                nakl.Timer = DateTime.Now - nakl.DateOpen;
                var textTimer = (nakl.Timer.TotalMinutes < 100) ? $"{(int)nakl.Timer.TotalMinutes}хв" : $"{(int)nakl.Timer.TotalHours}гд";
                InvokeUi(() =>
                {
                    switch (nakl.Type)
                    {
                        case NaklType.PokCM:
                            komirka.Text.Text = $"СМ {nakl.Coden} {textTimer}";
                            break;
                        case NaklType.RayonMarket:
                            komirka.Text.Text = $"Маркет {nakl.Coden} {nakl.Timer.ToString(@"hh\:mm")}";
                            break;
                    }
                });
                if (!komirka.Text.Visible)
                    InvokeUi(() => { komirka.Text.Visible = true; });
                if (komirka.Text.BackColor != Color.White)
                    InvokeUi(() =>
                    {
                        komirka.Text.BackColor = Color.White;
                        komirka.Text.ForeColor = Color.Black;
                    });

                nakl.Waiting = nakl.Timer.TotalSeconds / timeWaiting15;
                switch (nakl.Waiting)
                {
                    case var _ when (isEnd):
                        nakl.Color = Color.FromArgb(0, 255, 0);
                        break;
                    case var _ when (inWork && nakl.Waiting < 1):
                        nakl.Color = Color.FromArgb(0, 255, 0);
                        break;
                    case var _ when nakl.Waiting < 0.33:
                        nakl.Color = Color.FromArgb(255, 255, 0);
                        break;
                    case var _ when nakl.Waiting < 0.66:
                        nakl.Color = Color.FromArgb(245, 90, 180);
                        if (!nakl.Sound5 && nakl.Type == NaklType.PokCM)
                            InvokeUi(() =>
                            {
                                (new SoundPlayer("NotWork.wav")).Play();
                                nakl.Sound5 = true;
                            });
                        break;
                    case var _ when nakl.Waiting < 1:
                        nakl.Color = Color.FromArgb(245, 90, 180);
                        break;
                    case var _ when nakl.Waiting > 1:
                        nakl.Color = Color.FromArgb(255, 0, 0);
                        if (!nakl.Sound15 && nakl.Type == NaklType.PokCM)
                            InvokeUi(() =>
                            {
                                (new SoundPlayer("Long.wav")).Play();
                                nakl.Sound15 = true;
                            });
                        break;
                }

                InvokeUi(() =>
                {
                    komirka.Number.BackColor = nakl.Color;
                    komirka.Text.BackColor = nakl.Color;
                    komirka.Text.ForeColor = Color.Black;
                    if (nakl.Worker != null)
                    {
                        komirka.Text.Text += " " + nakl.Worker;
                        komirka.Text.Text = komirka.Text.Text.Substring(0, Math.Min(21, komirka.Text.Text.Length));
                    }
                });
            }

            InvokeUi(() =>
            {
                var grid = ListNakladna.Where(n => (n.Type == NaklType.PokCM || n.Type == NaklType.Zbut || n.Type == NaklType.Gosp)).Select(n => new { Тип_документа = n.NameDoc, Накладна = n.Coden, Місце = n.PlaceERP.Substring(0, Math.Min(9, n.PlaceERP.Length)), Час = n.DateOpen.ToString("HH:mm") }).OrderBy(n => n.Час).ToList();
                _naklGrid.DataSource = grid;
                foreach (DataGridViewRow row in _naklGrid.Rows)
                {
                    var color = ListNakladna.Find(n => n.Coden == grid[row.Index].Накладна).Color;
                    row.DefaultCellStyle.BackColor = color;
                }
                _naklGrid.ClearSelection();
                _naklGrid.CurrentCell = null;
            });
        }

        private void ShowErrorNakl()
        {
            InvokeUi(() =>
            {
                var grid = ErrorNakladna.Where(n => n.DateOpen > DateTime.Now.AddMinutes(-90) && (n.Text != "Немає залишку для товарів" || n.DateOpen > DateTime.Now.AddMinutes(-30))).Select(n => new { Накладна = n.Coden, Час = n.DateOpen.ToString("HH:mm"), Помилка = n.Text.Substring(0, Math.Min(50, n.Text.Length)), }).OrderBy(n => n.Час).ToList();
                _problemGrid.DataSource = grid;
                foreach (DataGridViewRow row in _problemGrid.Rows)
                {
                    var nakl = ErrorNakladna.Find(n => n.Coden == grid[row.Index].Накладна);
                    if (nakl.Text != "Немає залишку для товарів")
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 0, 0);
                }
                _problemGrid.ClearSelection();
                _problemGrid.CurrentCell = null;
            });
        }

        private void timerRefreshButton_Tick(object sender, EventArgs e)
        {
            TimeSpan t = DateTime.Now - _startTime;
            if (t.TotalSeconds > 10 && !_bRefresh.Enabled)
            {
                _bRefresh.Enabled = true;
            }
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            TimeSpan t = DateTime.Now - _startTime;
            if (t.TotalSeconds < 10)
            {
                _bRefresh.Text = "Оновлено";
                return;
            }
            int left = (intervalUpdateMonitor / 1000) - (int)t.TotalSeconds;
            if (left < 0) left = 0;
            _bRefresh.Text = $"Оновити({left})";
        }

        private void _bRefresh_Click(object sender, EventArgs e)
        {
            UpdateMonitor(null);
        }

        private void KomirkaInit()
        {
            const int rowHeight = 72;
            const int startY = 280;
            AddKomirka("EXI.06", "6", 20, startY);
            AddKomirka("EXI.05", "5", 20, startY + rowHeight);
            AddKomirka("EXI.04", "4", 20, startY + rowHeight * 2);
            AddKomirka("EXI.03", "3", 20, startY + rowHeight * 3);
            AddKomirka("EXI.02", "2", 20, startY + rowHeight * 4);
            AddKomirka("EXI.01", "1", 20, startY + rowHeight * 5);
            AddKomirka("EXI.BT", "Видача", 20, startY + rowHeight * 6);

            AddKomirka("EXI.12", "12", 620, startY);
            AddKomirka("EXI.11", "11", 620, startY + rowHeight);
            AddKomirka("EXI.10", "10", 620, startY + rowHeight * 2);
            AddKomirka("EXI.09", "9", 620, startY + rowHeight * 3);
            AddKomirka("EXI.08", "8", 620, startY + rowHeight * 4);
            AddKomirka("EXI.07", "7", 620, startY + rowHeight * 5);
            AddKomirka("EXI.BV", "Ворота", 620, startY + rowHeight * 6);
            AddKomirka(OtherPlaceKey, "Інші", 20, startY + rowHeight * 7, 1020);
        }

        private string GetBoardPlace(string placeWms)
        {
            if (string.IsNullOrWhiteSpace(placeWms) || !ListKomirka.ContainsKey(placeWms))
                return OtherPlaceKey;
            return placeWms;
        }

        private void AddKomirka(string placeKey, string caption, int x, int y, int textWidth = 0)
        {
            int numberWidth = caption.Length > 2 ? 140 : 80;
            if (textWidth <= 0)
                textWidth = 560 - numberWidth;
            float numberFontSize = caption.Length > 2 ? 16F : 24F;
            var number = new Label
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Microsoft Sans Serif", numberFontSize, FontStyle.Bold, GraphicsUnit.Point, 204),
                Location = new Point(x, y),
                Name = "_lKomirka" + placeKey.Replace(".", ""),
                Size = new Size(numberWidth, 64),
                Text = caption,
                TextAlign = ContentAlignment.MiddleCenter
            };
            var text = new Label
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("JetBrains Mono Medium", 18.75F, FontStyle.Regular, GraphicsUnit.Point, 204),
                Location = new Point(x + numberWidth, y),
                Name = "_lText" + placeKey.Replace(".", ""),
                Size = new Size(textWidth, 64),
                TextAlign = ContentAlignment.MiddleLeft,
                Visible = false
            };
            var picture = new PictureBox
            {
                Location = new Point(x + numberWidth, y),
                Name = "_pbKomirka" + placeKey.Replace(".", ""),
                Size = new Size(70, 36),
                Visible = false
            };

            number.Click += Place_Click;
            text.Click += Place_Click;
            _pSklad.Controls.Add(text);
            _pSklad.Controls.Add(number);
            _pSklad.Controls.Add(picture);
            ListKomirka.Add(placeKey, new KomirkaVisual
            {
                Number = number,
                Picture = picture,
                Text = text
            });
        }

        private void RayonForm_Load(object sender, EventArgs e)
        {
            LayoutForScreen();
            UpdateMonitor(null);
        }

        private void RayonForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                return;
            LayoutForScreen();
        }

        private bool _layoutBusy;
        private Size _lastLayoutSize;

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

                int split = fullHd ? 764 : Math.Max(360, w * 764 / 1904);
                split = Math.Min(split, w - 400);
                if (split < 280)
                    split = Math.Max(280, w / 2);
                splitContainer1.Panel1MinSize = 280;
                splitContainer1.Panel2MinSize = 280;
                if (split > 0 && split < w)
                    splitContainer1.SplitterDistance = split;

                LayoutLeftPanel(fullHd);
                if (fullHd)
                    LayoutSkladDesign();
                else
                    LayoutSkladCompact();
            }
            catch
            {
            }
            finally
            {
                _layoutBusy = false;
            }
        }

        private void LayoutLeftPanel(bool fullHd)
        {
            int h = splitContainer1.Panel1.ClientSize.Height;
            int w = splitContainer1.Panel1.ClientSize.Width;
            if (h < 50 || w < 50)
                return;

            int problemH = fullHd ? 238 : Math.Max(90, Math.Min(180, h * 20 / 100));
            int labelH = Math.Max(20, label1.Height);
            int problemY = h - problemH;
            int labelY = Math.Max(24, problemY - labelH - 4);
            int naklH = Math.Max(40, labelY);

            _naklGrid.Anchor = AnchorStyles.None;
            _problemGrid.Anchor = AnchorStyles.None;
            label1.Anchor = AnchorStyles.None;
            _naklGrid.Bounds = new Rectangle(0, 0, w, naklH);
            label1.Location = new Point(3, labelY);
            _problemGrid.Bounds = new Rectangle(0, problemY, w, problemH);
        }

        private void LayoutSkladDesign()
        {
            _bRefresh.Location = new Point(411, 40);
            _bRefresh.Size = new Size(381, 101);
            SetControlFont(_bRefresh, "Microsoft Sans Serif", 27.75F, FontStyle.Regular);
            LayoutPlaces(20, 620, 280, 72, 64, 1F, 560, 1020);
            SetGridFonts(22F, 16F, 15.75F);
        }

        private void LayoutSkladCompact()
        {
            int pw = Math.Max(1, _pSklad.ClientSize.Width);
            int ph = Math.Max(1, _pSklad.ClientSize.Height);
            float scale = Math.Min(pw / 1139F, ph / 1001F);
            scale = Math.Max(0.5F, Math.Min(1F, scale));

            int btnW = Math.Max(160, (int)(381 * scale));
            int btnH = Math.Max(48, (int)(101 * scale));
            _bRefresh.Size = new Size(Math.Min(btnW, pw - 16), btnH);
            _bRefresh.Location = new Point(Math.Max(8, (pw - _bRefresh.Width) / 2), Math.Max(6, (int)(12 * scale)));
            SetControlFont(_bRefresh, "Microsoft Sans Serif", 27.75F * scale, FontStyle.Regular);

            int pad = Math.Max(8, (int)(16 * scale));
            int startY = _bRefresh.Bottom + Math.Max(6, (int)(10 * scale));
            int rowH = (ph - startY) / 8;
            if (rowH > 72) rowH = 72;
            if (rowH < 40) rowH = 40;
            int cellH = Math.Min(64, Math.Max(32, rowH - 4));
            int gap = pad;
            int colW = Math.Max(80, (pw - pad * 2 - gap) / 2);
            int otherW = Math.Max(80, pw - pad * 2);
            LayoutPlaces(pad, pad + colW + gap, startY, rowH, cellH, scale, colW, otherW);
            SetGridFonts(Math.Max(12F, 22F * scale), Math.Max(10F, 16F * scale), Math.Max(10F, 15.75F * scale));
        }

        private void LayoutPlaces(int leftX, int rightX, int startY, int rowH, int cellH, float scale, int colW, int otherW)
        {
            ApplyPlace("EXI.06", leftX, startY, colW, cellH, scale);
            ApplyPlace("EXI.05", leftX, startY + rowH, colW, cellH, scale);
            ApplyPlace("EXI.04", leftX, startY + rowH * 2, colW, cellH, scale);
            ApplyPlace("EXI.03", leftX, startY + rowH * 3, colW, cellH, scale);
            ApplyPlace("EXI.02", leftX, startY + rowH * 4, colW, cellH, scale);
            ApplyPlace("EXI.01", leftX, startY + rowH * 5, colW, cellH, scale);
            ApplyPlace("EXI.BT", leftX, startY + rowH * 6, colW, cellH, scale);

            ApplyPlace("EXI.12", rightX, startY, colW, cellH, scale);
            ApplyPlace("EXI.11", rightX, startY + rowH, colW, cellH, scale);
            ApplyPlace("EXI.10", rightX, startY + rowH * 2, colW, cellH, scale);
            ApplyPlace("EXI.09", rightX, startY + rowH * 3, colW, cellH, scale);
            ApplyPlace("EXI.08", rightX, startY + rowH * 4, colW, cellH, scale);
            ApplyPlace("EXI.07", rightX, startY + rowH * 5, colW, cellH, scale);
            ApplyPlace("EXI.BV", rightX, startY + rowH * 6, colW, cellH, scale);

            ApplyPlace(OtherPlaceKey, leftX, startY + rowH * 7, otherW, cellH, scale);
        }

        private void ApplyPlace(string key, int x, int y, int colW, int cellH, float scale)
        {
            if (!ListKomirka.ContainsKey(key))
                return;
            var k = ListKomirka[key];
            bool wideCaption = k.Number.Text.Length > 2;
            int numberWidth = wideCaption ? (int)(140 * scale) : (int)(80 * scale);
            if (numberWidth < 40) numberWidth = 40;
            if (numberWidth > colW / 2) numberWidth = Math.Max(40, colW / 3);
            int textW = Math.Max(40, colW - numberWidth);
            k.Number.Bounds = new Rectangle(x, y, numberWidth, cellH);
            k.Text.Bounds = new Rectangle(x + numberWidth, y, textW, cellH);
            k.Picture.Location = new Point(x + numberWidth, y);
            SetControlFont(k.Number, "Microsoft Sans Serif", wideCaption ? 16F * scale : 24F * scale, FontStyle.Bold);
            SetControlFont(k.Text, "JetBrains Mono Medium", 18.75F * scale, FontStyle.Regular);
        }

        private void SetGridFonts(float naklSize, float problemSize, float labelSize)
        {
            _naklGrid.DefaultCellStyle.Font = MakeFont("Microsoft Sans Serif", naklSize, FontStyle.Bold);
            _problemGrid.DefaultCellStyle.Font = MakeFont("Times New Roman", problemSize, FontStyle.Bold);
            SetControlFont(label1, "Microsoft Sans Serif", labelSize, FontStyle.Regular);
        }

        private static void SetControlFont(Control control, string family, float size, FontStyle style)
        {
            var old = control.Font;
            control.Font = MakeFont(family, size, style);
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

        private void Place_Click(object sender, EventArgs e)
        {
            var place = ListKomirka.FirstOrDefault(k => k.Value.Number == sender || k.Value.Text == sender).Key;
            var nakls = ListNakladna.Where(n => GetBoardPlace(n.PlaceWMS) == place).ToList();
            if (nakls.Count == 0)
                return;
            if (nakls.Count == 1)
            {
                var naklForm = new NaklForm(nakls[0]);
                naklForm.Show();
            }
            else
            {
                var choiceNakl = new ChoiceNakl(nakls);
                choiceNakl.Location = Cursor.Position;
                choiceNakl.Show();
            }
        }

        private void _naklGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            var nakl = ListNakladna.LastOrDefault(n => n.Coden == (int)_naklGrid.Rows[e.RowIndex].Cells[1].Value);
            if (nakl != null)
            {
                var naklForm = new NaklForm(nakl);
                naklForm.Show();
            }
        }

        private void вихідToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void PlayNewNaklSound()
        {
            InvokeUi(() =>
            {
                try
                {
                    (new SoundPlayer("NewOrder.wav")).Play();
                }
                catch
                {
                    SystemSounds.Exclamation.Play();
                }
            });
        }

        private void InvokeUi(Action action)
        {
            if (IsDisposed || !IsHandleCreated)
                return;
            if (InvokeRequired)
                Invoke(action);
            else
                action();
        }
    }
}
