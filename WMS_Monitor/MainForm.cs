using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMS_Monitor.Model;

namespace WMS_Monitor
{
    public partial class MainForm : Form
    {
        private bool _operatorMode = false;

        private Image car = Image.FromFile("car.png");
        private Image shopping = Image.FromFile("shopping.png");
        private const int timeWaiting15 = 15 * 60;
        private const int timeWaiting120 = 120 * 60;

        private Dictionary<string, KomirkaVisual> ListKomirka = new Dictionary<string, KomirkaVisual>();
        private List<NakladnaWMS> ListNakladna = new List<NakladnaWMS>();
        private List<Discharge> ListDischarge = new List<Discharge>();
        private List<NakladnaWMS> ErrorNakladna = new List<NakladnaWMS>();

        private System.Threading.Timer _timer;
        private int intervalUpdateMonitor = 60 * 1000;
        private int intervalUpdateOperator = 120 * 1000;
        private DateTime lastExecute = DateTime.MinValue;

        private System.Windows.Forms.Timer timerRefreshButton = new System.Windows.Forms.Timer() { Interval = 1000};
        private System.Windows.Forms.Timer timerRefresh = new System.Windows.Forms.Timer() { Interval = 1000 };
        private DateTime _startTime;

        //private readonly System.Threading.Timer _timerBlinking;

        public MainForm()
        {
            InitializeDefault();
        }

        public MainForm(bool operatorMode)
        {
            _operatorMode = operatorMode;
            InitializeDefault();
        }

        private void InitializeDefault()
        {
            InitializeComponent();
            KomirkaInit();

            _naklGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            _naklGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            _problemGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            _problemGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            if (_operatorMode)
            {
                _startTime = DateTime.Now.AddSeconds(1 - (intervalUpdateOperator/1000));
                _bRefresh.Visible = true;
                //_lTimer.Visible = false;
                this.timerRefreshButton.Enabled = true;
                this.timerRefreshButton.Tick += new System.EventHandler(timerRefreshButton_Tick);
            }
            else
            {
                _startTime = DateTime.Now.AddSeconds(1 - (intervalUpdateMonitor/1000));
                _bRefresh.Visible = false;
                //_lTimer.Visible = false;
                //this.timerRefresh.Enabled = true;
                //this.timerRefresh.Tick += new System.EventHandler(timerRefresh_Tick);
                _timer = new System.Threading.Timer(new TimerCallback(UpdateMonitor), null, intervalUpdateMonitor, intervalUpdateMonitor);
            }
            //_timerBlinking = new System.Threading.Timer(new TimerCallback(OnTimerBlink), null, 2000, 300);
        }

        private void UpdateMonitor(object obj)
        {
            UpdateFromBD();
            UpdateTimingDischarge();
            UpdateTimingNakl();
            //ShowDouble();
            ShowErrorNakl();
            _startTime = DateTime.Now;
        }

        private void OnTimerBlink(object obj)
        {
            foreach (var komirka in ListKomirka)
            {
                if (komirka.Value.blink)
                {
                    this.Invoke(new Action(() =>
                    {
                        komirka.Value.Picture.Visible = !komirka.Value.Picture.Visible;
                    }));
                }
            }
        }

        private void UpdateFromBD()
        {
            using (var connection = new SqlConnection(Program.connectionSql101))
            {
                string query;
                if (lastExecute == DateTime.MinValue)
                    query = $"EXECUTE [us_MonitorUpdate]";
                else
                    query = $"EXECUTE [us_MonitorUpdate] '{lastExecute:yyyy-MM-dd HH:mm:ss.fff}'";

                var command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = null;
                try
                {
                    reader = command.ExecuteReader();
                    reader.Read();
                    lastExecute = Convert.ToDateTime(reader["dateNow"]);

                    reader.NextResult();
                    while (reader.Read())
                    {
                        var coden = Convert.ToInt32(reader["coden"]);
                        ErrorNakladna.RemoveAll(n => n.Coden == coden);
                        var nakl = ListNakladna.FirstOrDefault(n => n.Coden == coden);
                        if (nakl == null)
                        {
                            var place = Convert.ToString(reader["place"]);
                            if (String.IsNullOrWhiteSpace(place))
                                continue;
                            if (ListKomirka.ContainsKey(place))
                            {
                                nakl = new NakladnaWMS
                                {
                                    Coden = coden,
                                    PlaceWMS = place,
                                    PlaceERP = reader["nameERP"] == System.DBNull.Value ? "Невідомо" : Convert.ToString(reader["nameERP"]),
                                    Text = Convert.ToString(reader["error"]),
                                    DateOpen = Convert.ToDateTime(reader["date_log"]),
                                    Dostavka = reader["codepdost"] == System.DBNull.Value ? 0 : Convert.ToInt32(reader["codepdost"]),
                                    NameDoc = reader["NameDoc"] == System.DBNull.Value ? "" : Convert.ToString(reader["NameDoc"])
                                };
                                if ((DateTime.Now - nakl.DateOpen).TotalHours > 72) continue;
                                switch (nakl.NameDoc)
                                {
                                    case "Покупець СМ":
                                        nakl.Type = NaklType.PokCM;
                                        break;
                                    case "Тернопіль СМ":
                                        nakl.Type = NaklType.TerCM;
                                        break;
                                    case "Збут":
                                        nakl.Type = NaklType.Zbut;
                                        break;
                                    case "Покупець Маркетплейс":
                                        nakl.Type = NaklType.MP;
                                        nakl.NameDoc = "Покупець МП";
                                        break;
                                    case "Філії":
                                        nakl.Type = NaklType.Filii;
                                        break;

                                    case "":
                                        nakl.Type = NaklType.Empty;
                                        continue;
                                    case "Господарські витрати":
                                        nakl.Type = NaklType.Gosp;
                                        continue;
                                    case "Повернення покупців":
                                        nakl.Type = NaklType.Povern;
                                        continue;
                                    case "Внутрішній прихід":
                                        nakl.Type = NaklType.VnPr;
                                        continue;
                                    case "Прихід від постачальника":
                                        //Приходи обробляються через Discharge
                                        nakl.Type = NaklType.Post;
                                        continue;
                                    default:
                                        nakl.Type = NaklType.Empty;
                                        continue;
                                }
                                ListNakladna.Add(nakl);
                            }
                        }
                        else
                        {
                            //Така накладна вже пішла у ВМС. Що робити поки не знаю, тому ігнорую.
                        }
                    }

                    reader.NextResult();
                    while (reader.Read())
                    {
                        var coden = Convert.ToInt32(reader["coden"]);
                        var nakl = ListNakladna.FirstOrDefault(n => n.Coden == coden);
                        if (nakl != null)
                        {
                            var close = Convert.ToDateTime(reader["datenlog"]);
                            nakl.DateClose = close;

                            var komirka = ListKomirka[nakl.PlaceWMS];
                            komirka.blink = false;
                            this.Invoke(new Action(() =>
                            {
                                komirka.Text.Visible = false;
                                komirka.Text.ForeColor = Color.Black;
                                komirka.Number.BackColor = Color.White;
                                //komirka.Picture.Visible = false;
                            }));
                            ListNakladna.Remove(nakl);
                        }
                    }

                    var newListDischarge = new List<Discharge>();
                    reader.NextResult();
                    while (reader.Read())
                    {
                        var place = Convert.ToString(reader["place"]);
                        if (ListKomirka.ContainsKey(place))
                        {
                            var nakl = new Discharge()
                            {
                                Coden = Convert.ToInt32(reader["coden"]),
                                PlaceWMS = place,
                                PlaceERP = Convert.ToString(reader["nameERP"]),
                                DateOpen = Convert.ToDateTime(reader["dateOpen"]),
                                DischargeCode = Convert.ToInt32(reader["discharge"])
                            };
                            newListDischarge.Add(nakl);

                            if (!ListDischarge.Exists(n => n.Coden == nakl.Coden))
                                ListDischarge.Add(nakl);
                        }
                    }
                    foreach(var nakl in ListDischarge.ToList())
                    {
                        if (!newListDischarge.Exists(n => n.Coden == nakl.Coden))
                        {
                            var komirka = ListKomirka[nakl.PlaceWMS];
                            this.Invoke(new Action(() =>
                            {
                                komirka.Text.Visible = false;
                                komirka.Text.ForeColor = Color.Black;
                                komirka.Number.BackColor = Color.White;
                            }));
                            ListDischarge.Remove(nakl);
                        }
                    }
                    newListDischarge.Clear();

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

        private void UpdateTimingDischarge()
        {
            ListDischarge = ListDischarge.OrderByDescending(n => n.DateOpen).ToList();
            foreach (var nakl in ListDischarge)
            {
                var komirka = ListKomirka[nakl.PlaceWMS];
                nakl.Timer = DateTime.Now - nakl.DateOpen;
                var textTimer = (nakl.Timer.TotalMinutes < 60) ? $"{(int)nakl.Timer.TotalMinutes}хв" : $"{(int)nakl.Timer.TotalHours}гд";
                if (!komirka.Text.Visible)
                    this.Invoke(new Action(() =>
                        {
                            komirka.Text.Visible = true;
                        }));
                this.Invoke(new Action(() =>
                {
                    komirka.Text.Text = $"Постач {nakl.DischargeCode} ⌚{nakl.Timer.ToString(@"hh\:mm")}";
                }));

                nakl.Waiting = nakl.Timer.TotalSeconds / timeWaiting120;

                if (nakl.Waiting < 1)
                {
                    if (komirka.Text.BackColor != Color.White)
                        this.Invoke(new Action(() =>
                            {
                                komirka.Text.BackColor = Color.White;
                            }));
                }
                else
                {
                    if (komirka.Number.BackColor != Color.DarkGray)
                        this.Invoke(new Action(() =>
                            {
                                komirka.Number.BackColor = Color.DarkGray;
                            }));
                    if (komirka.Text.BackColor != Color.DarkGray)
                        this.Invoke(new Action(() =>
                            {
                                komirka.Text.BackColor = Color.DarkGray;
                            }));
                    if (!_operatorMode)
                        this.Invoke(new Action(() =>
                            {
                                if (!nakl.Sound && nakl.PlaceWMS == "ENT.59")
                                {
                                    (new SoundPlayer("59.wav")).Play();
                                    nakl.Sound = true;
                                }
                                if (!nakl.Sound && nakl.PlaceWMS == "ENT.58")
                                {
                                    (new SoundPlayer("58.wav")).Play();
                                    nakl.Sound = true;
                                }
                            }));
                }
            }
        }

        private void UpdateTimingNakl()
        {
            ListNakladna = ListNakladna.OrderByDescending(n => n.DateOpen).ToList(); /*.ThenBy(n => n.FirstName)*/
            foreach (var nakl in ListNakladna)
            {
                if (nakl.Type != NaklType.PokCM && nakl.Type != NaklType.Zbut && nakl.Type != NaklType.MP && ListNakladna.Any(n => n.PlaceWMS == nakl.PlaceWMS && (n.Type == NaklType.PokCM || n.Type == NaklType.Zbut || n.Type == NaklType.MP)))
                    continue;
                var komirka = ListKomirka[nakl.PlaceWMS];
                nakl.Timer = DateTime.Now - nakl.DateOpen;
                var textTimer = (nakl.Timer.TotalMinutes < 100) ? $"{(int)nakl.Timer.TotalMinutes}хв": $"{(int)nakl.Timer.TotalHours}гд";
                this.Invoke(new Action(() =>
                {
                    switch (nakl.Type)
                    {
                        case NaklType.PokCM:
                            komirka.Text.Text = $"СМ {nakl.Coden} {textTimer}";
                            break;
                        case NaklType.Zbut:
                            komirka.Text.Text = $"Зб {nakl.Coden} {textTimer}";
                            break;
                        case NaklType.MP:
                            komirka.Text.Text = $"МП {nakl.Coden} {textTimer}";
                            break;
                        case NaklType.TerCM:
                            komirka.Text.Text = $"ТернСМ {nakl.Coden} {nakl.Timer.ToString(@"hh\:mm")}";
                            break;
                        case NaklType.Filii:
                            komirka.Text.Text = $"Доставка {nakl.Dostavka} {textTimer}";
                            break;
                        case NaklType.Post:
                            komirka.Text.Text = $"Постач  {nakl.Coden} {textTimer}";
                            break;
                    }
                }));
                if (!komirka.Text.Visible)
                    this.Invoke(new Action(() =>
                        {
                            komirka.Text.Visible = true;
                        }));
                if (komirka.Text.BackColor != Color.White)
                    this.Invoke(new Action(() =>
                        {
                            komirka.Text.BackColor = Color.White;
                            komirka.Text.ForeColor = Color.Black;
                        }));

                if (nakl.PlaceWMS == "ENT.MP" || nakl.PlaceWMS == "ENT.CM1" || nakl.PlaceWMS == "ENT.CM2" || nakl.PlaceWMS == "ENT.CM3" || nakl.Type == NaklType.Post || nakl.Type == NaklType.Filii || nakl.Type == NaklType.TerCM)
                {
                    if (komirka.Number.BackColor != Color.White)
                        this.Invoke(new Action(() =>
                            {
                                komirka.Number.BackColor = Color.White;
                            }));
                    continue;
                }

                bool inWork = false;
                bool isEnd = false;
                if (!_operatorMode)
                {
                    using (var connection = new SqlConnection(Program.connectionSql101sa))
                    {
                        var query = $"EXECUTE [us_MonitorNakl] '{nakl.Coden}'";

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
                        catch (Exception ex)
                        {
                            //SaveErrorToSQL(connection, ex.Message, $"codetvun = {codetvun}");
                        }
                        finally
                        {
                            reader?.Close();
                        }
                    }
                }

                //switch (nakl.Type)
                //{
                //    case NaklType.PokCM:
                //    case NaklType.Zbut:
                //    case NaklType.MP:
                //        nakl.Waiting = nakl.Timer.TotalSeconds / timeWaiting15;
                //        break;
                //    case NaklType.TerCM:
                //    case NaklType.Filii:
                //        nakl.Waiting = nakl.Timer.TotalSeconds / timeWaiting120;
                //        break;
                //    case NaklType.Post:
                //        nakl.Waiting = nakl.Timer.TotalSeconds / timeWaiting120;
                //        break;
                //}

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
                        if (_operatorMode)
                            nakl.Color = Color.FromArgb(0, 255, 0);
                        else
                            nakl.Color = Color.FromArgb(255, 255, 0);
                        break;
                    case var _ when nakl.Waiting < 0.66:
                        if (_operatorMode)
                            nakl.Color = Color.FromArgb(0, 255, 0);
                        else
                        {
                            nakl.Color = Color.FromArgb(245, 90, 180);
                            if (!nakl.Sound5 && (nakl.Type == NaklType.PokCM || nakl.Type == NaklType.Zbut || nakl.Type == NaklType.MP))
                                this.Invoke(new Action(() =>
                                {
                                    (new SoundPlayer("NotWork.wav")).Play();
                                    nakl.Sound5 = true;
                                }));
                        }
                        break;
                    case var _ when nakl.Waiting < 1:
                        if (_operatorMode)
                            nakl.Color = Color.FromArgb(255, 255, 0);
                        else
                            nakl.Color = Color.FromArgb(245, 90, 180);
                        break;
                    case var _ when nakl.Waiting > 1:
                        nakl.Color = Color.FromArgb(255, 0, 0);
                        if (!_operatorMode)
                            if (!nakl.Sound15 && (nakl.Type == NaklType.PokCM || nakl.Type == NaklType.Zbut || nakl.Type == NaklType.MP))
                                this.Invoke(new Action(() =>
                                {
                                    (new SoundPlayer("Long.wav")).Play();
                                    nakl.Sound15 = true;
                                }));
                        break;
                }

                this.Invoke(new Action(() =>
                {
                    komirka.Number.BackColor = nakl.Color;
                    komirka.Text.BackColor = nakl.Color;
                    komirka.Text.ForeColor = Color.Black;
                    if (nakl.Worker != null)
                    {
                        komirka.Text.Text += " " + nakl.Worker;  //.Substring(0, Math.Min(7, nakl.Worker.Length));
                        komirka.Text.Text = komirka.Text.Text.Substring(0, Math.Min(21, komirka.Text.Text.Length));
                    }
                }));
            }

            this.Invoke(new Action(() =>
            {
                var grid = ListNakladna.Where(n => (n.Type == NaklType.PokCM || n.Type == NaklType.Zbut || n.Type == NaklType.MP) && n.PlaceWMS != "ENT.MP").Select(n => new { Тип_документа = n.NameDoc, Накладна = n.Coden, Місце = n.PlaceERP.Substring(0, Math.Min(9, n.PlaceERP.Length)), Час = n.DateOpen.ToString("HH:mm") }).OrderBy(n => n.Час).ToList();
                _naklGrid.DataSource = grid;
                foreach (DataGridViewRow row in _naklGrid.Rows)
                {
                    var color = ListNakladna.Find(n => n.Coden == grid[row.Index].Накладна).Color;
                    row.DefaultCellStyle.BackColor = color;
                }
                _naklGrid.ClearSelection();
                _naklGrid.CurrentCell = null;
            }));
        }

        private void ShowErrorNakl()
        {
            this.Invoke(new Action(() =>
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
            }));
        }

        private void ShowDouble()
        {
            var dounleNakl = ListNakladna.GroupBy(n => n.PlaceWMS).Select(g => new { Place = g.Key, Count = g.Count() }).Where(g => g.Count > 1).ToList();
            foreach (var group in dounleNakl)
            {
                if (ListKomirka.ContainsKey(group.Place))
                {
                    var komirka = ListKomirka[group.Place];
                    this.Invoke(new Action(() =>
                        {
                            CountShow(komirka.Picture, group.Count);
                        }));
                }
            }
        }

        private void timerRefreshButton_Tick(object sender, EventArgs e)
        {
            TimeSpan t = DateTime.Now - _startTime;
            if (t.TotalSeconds > intervalUpdateOperator / 1000)
            {
                UpdateMonitor(null);
                _bRefresh.Text = "Обновлено";
            }
            else
            {
                if (t.TotalSeconds < intervalUpdateOperator / 6000)
                    _bRefresh.Text = "Обновлено";
                else
                    _bRefresh.Text = $"Обновити({(intervalUpdateOperator / 1000) - t.TotalSeconds:N0})";
            }
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            TimeSpan t = DateTime.Now - _startTime;
            _lTimer.Text = $"{(intervalUpdateMonitor / 1000) - t.TotalSeconds:N0}";
        }

        private void _bRefresh_Click(object sender, EventArgs e)
        {
            UpdateMonitor(null);
            _bRefresh.Text = "Обновлено";
        }

        private void CountShow(PictureBox box, int count)
        {
            using (Font myFont = new Font("Arial", 30))
            {
                box.CreateGraphics().DrawString(count.ToString(), myFont, Brushes.White, new Point(16, -4));
            }
        }



        private void KomirkaInit()
        {
            var komirka01 = new KomirkaVisual()
            {
                Number = this._lKomirka01,
                Picture = this._pbKomirka01,
                Text = this._lText01
            };
            komirka01.Picture.Image = car;
            ListKomirka.Add("EXI.01", komirka01);
            var komirka02 = new KomirkaVisual()
            {
                Number = this._lKomirka02,
                Picture = this._pbKomirka02,
                Text = this._lText02
            };
            komirka02.Picture.Image = car;
            ListKomirka.Add("EXI.02", komirka02);
            var komirka03 = new KomirkaVisual()
            {
                Number = this._lKomirka03,
                Picture = this._pbKomirka03,
                Text = this._lText03
            };
            komirka03.Picture.Image = car;
            ListKomirka.Add("EXI.03", komirka03);
            var komirka04 = new KomirkaVisual()
            {
                Number = this._lKomirka04,
                Picture = this._pbKomirka04,
                Text = this._lText04
            };
            komirka04.Picture.Image = car;
            ListKomirka.Add("EXI.04", komirka04);
            var komirka05 = new KomirkaVisual()
            {
                Number = this._lKomirka05,
                Picture = this._pbKomirka05,
                Text = this._lText05
            };
            komirka05.Picture.Image = car;
            ListKomirka.Add("EXI.05", komirka05);
            var komirka06 = new KomirkaVisual()
            {
                Number = this._lKomirka06,
                Picture = this._pbKomirka06,
                Text = this._lText06
            };
            komirka06.Picture.Image = car;
            ListKomirka.Add("EXI.06", komirka06);
            var komirka07 = new KomirkaVisual()
            {
                Number = this._lKomirka07,
                Picture = this._pbKomirka07,
                Text = this._lText07
            };
            komirka07.Picture.Image = car;
            ListKomirka.Add("EXI.07", komirka07);
            var komirka08 = new KomirkaVisual()
            {
                Number = this._lKomirka08,
                Picture = this._pbKomirka08,
                Text = this._lText08
            };
            komirka08.Picture.Image = car;
            ListKomirka.Add("EXI.08", komirka08);
            var komirka09 = new KomirkaVisual()
            {
                Number = this._lKomirka09,
                Picture = this._pbKomirka09,
                Text = this._lText09
            };
            komirka09.Picture.Image = car;
            ListKomirka.Add("EXI.09", komirka09);
            var komirka10 = new KomirkaVisual()
            {
                Number = this._lKomirka10,
                Picture = this._pbKomirka10,
                Text = this._lText10
            };
            komirka10.Picture.Image = car;
            ListKomirka.Add("EXI.10", komirka10);
            var komirka11 = new KomirkaVisual()
            {
                Number = this._lKomirka11,
                Picture = this._pbKomirka11,
                Text = this._lText11
            };
            komirka11.Picture.Image = car;
            ListKomirka.Add("EXI.11", komirka11);
            var komirka12 = new KomirkaVisual()
            {
                Number = this._lKomirka12,
                Picture = this._pbKomirka12,
                Text = this._lText12
            };
            komirka12.Picture.Image = car;
            ListKomirka.Add("EXI.12", komirka12);
            var komirka13 = new KomirkaVisual()
            {
                Number = this._lKomirka13,
                Picture = this._pbKomirka13,
                Text = this._lText13
            };
            komirka13.Picture.Image = car;
            ListKomirka.Add("EXI.13", komirka13);
            var komirka14 = new KomirkaVisual()
            {
                Number = this._lKomirka14,
                Picture = this._pbKomirka14,
                Text = this._lText14
            };
            komirka14.Picture.Image = car;
            ListKomirka.Add("EXI.14", komirka14);

            var komirka15 = new KomirkaVisual()
            {
                Number = this._lKomirka15,
                Picture = this._pbKomirka15,
                Text = this._lText15
            };
            komirka15.Picture.Image = car;
            ListKomirka.Add("EXI.15", komirka15);
            var komirka16 = new KomirkaVisual()
            {
                Number = this._lKomirka16,
                Picture = this._pbKomirka16,
                Text = this._lText16
            };
            komirka16.Picture.Image = car;
            ListKomirka.Add("EXI.16", komirka16);
            var komirka17 = new KomirkaVisual()
            {
                Number = this._lKomirka17,
                Picture = this._pbKomirka17,
                Text = this._lText17
            };
            komirka17.Picture.Image = car;
            ListKomirka.Add("EXI.17", komirka17);
            var komirka18 = new KomirkaVisual()
            {
                Number = this._lKomirka18,
                Picture = this._pbKomirka18,
                Text = this._lText18
            };
            komirka18.Picture.Image = car;
            ListKomirka.Add("EXI.18", komirka18);
            var komirka19 = new KomirkaVisual()
            {
                Number = this._lKomirka19,
                Picture = this._pbKomirka19,
                Text = this._lText19
            };
            komirka19.Picture.Image = car;
            ListKomirka.Add("EXI.19", komirka19);
            var komirka20 = new KomirkaVisual()
            {
                Number = this._lKomirka20,
                Picture = this._pbKomirka20,
                Text = this._lText20
            };
            komirka20.Picture.Image = car;
            ListKomirka.Add("EXI.20", komirka20);
            var komirka21 = new KomirkaVisual()
            {
                Number = this._lKomirka21,
                Picture = this._pbKomirka21,
                Text = this._lText21
            };
            komirka21.Picture.Image = car;
            ListKomirka.Add("EXI.21", komirka21);
            var komirka22 = new KomirkaVisual()
            {
                Number = this._lKomirka22,
                Picture = this._pbKomirka22,
                Text = this._lText22
            };
            komirka22.Picture.Image = car;
            ListKomirka.Add("EXI.22", komirka22);
            var komirka23 = new KomirkaVisual()
            {
                Number = this._lKomirka23,
                Picture = this._pbKomirka23,
                Text = this._lText23
            };
            komirka23.Picture.Image = car;
            ListKomirka.Add("EXI.23", komirka23);

            var komirka24 = new KomirkaVisual()
            {
                Number = this._lKomirka24,
                Picture = this._pbKomirka24,
                Text = this._lText24
            };
            komirka24.Picture.Image = car;
            ListKomirka.Add("EXI.24", komirka24);
            var komirka25 = new KomirkaVisual()
            {
                Number = this._lKomirka25,
                Picture = this._pbKomirka25,
                Text = this._lText25
            };
            komirka25.Picture.Image = car;
            ListKomirka.Add("EXI.25", komirka25);
            var komirka26 = new KomirkaVisual()
            {
                Number = this._lKomirka26,
                Picture = this._pbKomirka26,
                Text = this._lText26
            };
            komirka26.Picture.Image = car;
            ListKomirka.Add("EXI.26", komirka26);
            var komirka27 = new KomirkaVisual()
            {
                Number = this._lKomirka27,
                Picture = this._pbKomirka27,
                Text = this._lText27
            };
            komirka27.Picture.Image = car;
            ListKomirka.Add("EXI.27", komirka27);

            var komirka31 = new KomirkaVisual()
            {
                Number = this._lKomirka31,
                Picture = this._pbKomirka31,
                Text = this._lText31
            };
            komirka31.Picture.Image = car;
            ListKomirka.Add("ENT.31", komirka31);
            var komirka32 = new KomirkaVisual()
            {
                Number = this._lKomirka32,
                Picture = this._pbKomirka32,
                Text = this._lText32
            };
            komirka32.Picture.Image = car;
            ListKomirka.Add("ENT.32", komirka32);
            var komirka33 = new KomirkaVisual()
            {
                Number = this._lKomirka33,
                Picture = this._pbKomirka33,
                Text = this._lText33
            };
            komirka33.Picture.Image = car;
            ListKomirka.Add("ENT.33", komirka33);
            var komirka34 = new KomirkaVisual()
            {
                Number = this._lKomirka34,
                Picture = this._pbKomirka34,
                Text = this._lText34
            };
            komirka34.Picture.Image = car;
            ListKomirka.Add("ENT.34", komirka34);
            var komirka35 = new KomirkaVisual()
            {
                Number = this._lKomirka35,
                Picture = this._pbKomirka35,
                Text = this._lText35
            };
            komirka35.Picture.Image = car;
            ListKomirka.Add("ENT.35", komirka35);
            var komirka36 = new KomirkaVisual()
            {
                Number = this._lKomirka36,
                Picture = this._pbKomirka36,
                Text = this._lText36
            };
            komirka36.Picture.Image = car;
            ListKomirka.Add("ENT.36", komirka36);

            var komirka55 = new KomirkaVisual()
            {
                Number = this._lKomirka55,
                Picture = this._pbKomirka55,
                Text = this._lText55
            };
            komirka55.Picture.Image = car;
            ListKomirka.Add("ENT.55", komirka55);
            var komirka56 = new KomirkaVisual()
            {
                Number = this._lKomirka56,
                Picture = this._pbKomirka56,
                Text = this._lText56
            };
            komirka56.Picture.Image = car;
            ListKomirka.Add("ENT.56", komirka56);
            var komirka57 = new KomirkaVisual()
            {
                Number = this._lKomirka57,
                Picture = this._pbKomirka57,
                Text = this._lText57
            };
            komirka57.Picture.Image = car;
            ListKomirka.Add("ENT.57", komirka57);
            var komirka58 = new KomirkaVisual()
            {
                Number = this._lKomirka58,
                Picture = this._pbKomirka58,
                Text = this._lText58
            };
            komirka58.Picture.Image = car;
            ListKomirka.Add("ENT.58", komirka58);
            var komirka59 = new KomirkaVisual()
            {
                Number = this._lKomirka59,
                Picture = this._pbKomirka59,
                Text = this._lText59
            };
            komirka59.Picture.Image = car;
            ListKomirka.Add("ENT.59", komirka59);
            var komirka60 = new KomirkaVisual()
            {
                Number = this._lKomirka60,
                Picture = this._pbKomirka60,
                Text = this._lText60
            };
            komirka60.Picture.Image = car;
            ListKomirka.Add("ENT.60", komirka60);
            var komirka61 = new KomirkaVisual()
            {
                Number = this._lKomirka61,
                Picture = this._pbKomirka61,
                Text = this._lText61
            };
            komirka61.Picture.Image = car;
            ListKomirka.Add("ENT.61", komirka61);
            var komirka62 = new KomirkaVisual()
            {
                Number = this._lKomirka62,
                Picture = this._pbKomirka62,
                Text = this._lText62
            };
            komirka62.Picture.Image = car;
            ListKomirka.Add("ENT.62", komirka62);

            var komirka101 = new KomirkaVisual()
            {
                Number = this._lKomirka101,
                Picture = this._pbKomirka101,
                Text = this._lText101
            };
            komirka101.Picture.Image = car;
            ListKomirka.Add("ENT.101", komirka101);
            var komirka102 = new KomirkaVisual()
            {
                Number = this._lKomirka102,
                Picture = this._pbKomirka102,
                Text = this._lText102
            };
            komirka102.Picture.Image = car;
            ListKomirka.Add("ENT.102", komirka102);

            var komirkaB2 = new KomirkaVisual()
            {
                Number = this._lKomirkaB2,
                Picture = this._pbKomirkaB2,
                Text = this._lTextB2
            };
            komirkaB2.Picture.Image = car;
            ListKomirka.Add("EXI.V2", komirkaB2);
            var komirkaLAV = new KomirkaVisual()
            {
                Number = this._lKomirkaLAV,
                Picture = this._pbKomirkaLAV,
                Text = this._lTextLAV
            };
            komirkaLAV.Picture.Image = shopping;
            ListKomirka.Add("EXI.LAV", komirkaLAV);
            var komirkaKIM = new KomirkaVisual()
            {
                Number = this._lKomirkaKIM,
                Picture = this._pbKomirkaKIM,
                Text = this._lTextKIM
            };
            komirkaKIM.Picture.Image = shopping;
            ListKomirka.Add("EXI.KIM", komirkaKIM);
            var komirkaMP = new KomirkaVisual()
            {
                Number = this._lKomirkaMP,
                Picture = this._pbKomirkaMP,
                Text = this._lTextMP
            };
            komirkaMP.Picture.Image = car;
            ListKomirka.Add("ENT.MP", komirkaMP);
            var komirkaCM1 = new KomirkaVisual()
            {
                Number = this._lKomirkaCM1,
                Picture = this._pbKomirkaCM1,
                Text = this._lTextCM1
            };
            komirkaCM1.Picture.Image = car;
            ListKomirka.Add("ENT.CM1", komirkaCM1);
            var komirkaCM2 = new KomirkaVisual()
            {
                Number = this._lKomirkaCM2,
                Picture = this._pbKomirkaCM2,
                Text = this._lTextCM2
            };
            komirkaCM2.Picture.Image = car;
            ListKomirka.Add("ENT.CM2", komirkaCM2);
            var komirkaCM3 = new KomirkaVisual()
            {
                Number = this._lKomirkaCM3,
                Picture = this._pbKomirkaCM3,
                Text = this._lTextCM3
            };
            komirkaCM3.Picture.Image = car;
            ListKomirka.Add("ENT.CM3", komirkaCM3);
            var komirkaHOL = new KomirkaVisual()
            {
                Number = this._lKomirkaHOL,
                Picture = this._pbKomirkaHOL,
                Text = this._lTextHOL
            };
            komirkaHOL.Picture.Image = car;
            ListKomirka.Add("EXI.HOL", komirkaHOL);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateMonitor(null);
        }

        private void Place_Click(object sender, EventArgs e)
        {
            var place = ListKomirka.FirstOrDefault(k => k.Value.Number == sender || k.Value.Text == sender).Key;
            var nakls = ListNakladna.Where(n => n.PlaceWMS == place).ToList();
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
            var nakl = ListNakladna.LastOrDefault(n => n.Coden == (int)_naklGrid.Rows[e.RowIndex].Cells[1].Value);
            if (nakl != null)
            {
                var naklForm = new NaklForm(nakl);
                naklForm.Show();
            }
        }
    }
}
