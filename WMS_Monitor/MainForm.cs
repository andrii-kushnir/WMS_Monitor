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
        private const string connectionSql101 = @"Server=192.168.4.101; Database=erp; uid=КушнірА; pwd=зщшфтв;";

        private Image car = Image.FromFile("car.png");
        private Image shopping = Image.FromFile("shopping.png");
        private const int timeWaiting15 = 15 * 60;
        private const int timeWaiting120 = 120 * 60;

        private int kLAV = 0;
        private int kKIM = 0;

        private Dictionary<string, KomirkaVisual> ListKomirka = new Dictionary<string, KomirkaVisual>();
        private List<Nakladna> ListNakladna = new List<Nakladna>();

        private readonly System.Threading.Timer _timer;
        private const int intervalUpdate = 15 * 1000;
        private DateTime lastExecute = DateTime.MinValue;

        //private readonly System.Threading.Timer _timerBlinking;

        public MainForm()
        {
            InitializeComponent();
            KomirkaInit();

            _timer = new System.Threading.Timer(new TimerCallback(UpdateMonitor), null, 0, intervalUpdate);
            //_timerBlinking = new System.Threading.Timer(new TimerCallback(OnTimerBlink), null, 2000, 300);

            _naklGrid.DefaultCellStyle.Font = new Font("Times New Roman", 18, FontStyle.Bold);
            _naklGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            _naklGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        }

        private void UpdateMonitor(object obj)
        {
            UpdateFromBD();
            UpdateTiming();
            ShowDouble();
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
            using (var connection = new SqlConnection(connectionSql101))
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
                        var nakl = ListNakladna.FirstOrDefault(n => n.Coden == coden);
                        if (nakl == null)
                        {
                            var place = Convert.ToString(reader["place"]);
                            if (ListKomirka.ContainsKey(place))
                            {
                                nakl = new Nakladna
                                {
                                    Coden = coden,
                                    PlaceWMS = place,
                                    PlaceERP = reader["nameERP"] == System.DBNull.Value ? "Невідомо" : Convert.ToString(reader["nameERP"]),
                                    Text = Convert.ToString(reader["error"]),
                                    DateOpen = Convert.ToDateTime(reader["date_log"]),
                                    Dostavka = reader["codepdost"] == System.DBNull.Value ? 0 : Convert.ToInt32(reader["codepdost"]),
                                    NameDoc = reader["NameDoc"] == System.DBNull.Value ? "" : Convert.ToString(reader["NameDoc"])
                                };
                                switch (nakl.NameDoc)
                                {
                                    case "":
                                        nakl.Type = NaklType.Empty;
                                        break;
                                    case "Покупець СМ":
                                        nakl.Type = NaklType.PokCM;
                                        ListNakladna.Add(nakl);
                                        break;
                                    case "Тернопіль СМ":
                                        nakl.Type = NaklType.TerCM;
                                        ListNakladna.Add(nakl);
                                        break;
                                    case "Господарські витрати":
                                        nakl.Type = NaklType.Gosp;
                                        break;
                                    case "Збут":
                                        nakl.Type = NaklType.Zbut;
                                        ListNakladna.Add(nakl);
                                        break;
                                    case "Повернення покупців":
                                        nakl.Type = NaklType.Povern;
                                        break;
                                    case "Покупець Маркетплейс":
                                        nakl.Type = NaklType.MP;
                                        nakl.NameDoc = "Покупець МП";
                                        ListNakladna.Add(nakl);
                                        break;
                                    case "Філії":
                                        nakl.Type = NaklType.Filii;
                                        ListNakladna.Add(nakl);
                                        break;
                                    case "Внутрішній прихід":
                                        nakl.Type = NaklType.VnPr;
                                        break;
                                    case "Прихід від постачальника":
                                        nakl.Type = NaklType.Post;
                                        break;
                                    default:
                                        nakl.Type = NaklType.Empty;
                                        break;
                                }
                                //Покищо додаю тільки розхідні
                                //ListNakladna.Add(nakl);
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
                            //if (close > komirka.LastChange)
                            //{
                                komirka.blink = false;
                                this.Invoke(new Action(() =>
                                {
                                    komirka.Text.Visible = false;
                                    komirka.Picture.Visible = false;
                                }));
                            //    komirka.LastChange = close;
                            //}

                            ListNakladna.Remove(nakl);
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

        private void UpdateTiming()
        {
            ListNakladna = ListNakladna.OrderByDescending(n => n.DateOpen).ToList();
            this.Invoke(new Action(() =>
            {
                _naklGrid.DataSource = ListNakladna.Where(n => n.Type == NaklType.PokCM || n.Type == NaklType.Zbut || n.Type == NaklType.MP).Select(n => new { NameDoc = n.NameDoc, Coden = n.Coden, Place = n.PlaceERP, Date = n.DateOpen }).OrderBy(n => n.Date).ToList();
            }));
            foreach (var nakl in ListNakladna)
            {
                var komirka = ListKomirka[nakl.PlaceWMS];
                nakl.Timer = DateTime.Now - nakl.DateOpen;
                this.Invoke(new Action(() =>
                {
                    //komirka.Text.Text = $"ПокупМП {nakl.Coden} ⌚{nakl.Timer.ToString(@"h\:mm")}";
                    switch (nakl.Type)
                    {
                        case NaklType.PokCM:
                            komirka.Text.Text = $"ПокупСМ {nakl.Coden}   {(int)nakl.Timer.TotalMinutes}хв";
                            break;
                        case NaklType.Zbut:
                            komirka.Text.Text = $"Збут {nakl.Coden}      {(int)nakl.Timer.TotalMinutes}хв";
                            break;
                        case NaklType.MP:
                            komirka.Text.Text = $"ПокупМП {nakl.Coden}   {(int)nakl.Timer.TotalMinutes}хв";
                            break;
                        case NaklType.TerCM:
                            komirka.Text.Text = $"ТернСМ {nakl.Coden} ⌚{nakl.Timer.ToString(@"hh\:mm")}";
                            break;
                        case NaklType.Filii:
                            komirka.Text.Text = $"Доставка {nakl.Dostavka} {(int)nakl.Timer.TotalHours}год";
                            break;
                    }
                    komirka.Text.Visible = true;
                    komirka.Picture.Visible = true;
                }));

                Double waiting = 0;
                switch (nakl.Type)
                {
                    case NaklType.PokCM:
                    case NaklType.Zbut:
                    case NaklType.MP:
                        waiting = nakl.Timer.TotalSeconds / timeWaiting15;
                        break;
                    case NaklType.TerCM:
                    case NaklType.Filii:
                        waiting = nakl.Timer.TotalSeconds / timeWaiting120;
                        break;
                }
                if (waiting < 0) waiting = 0;
                if (waiting < 0.66)
                    this.Invoke(new Action(() =>
                    {
                        //int red = (int)(waiting * 255);
                        //int green = (int)(255 - (waiting * 255));
                        komirka.Picture.BackColor = Color.FromArgb(0, 255, 0);
                    }));
                else
                {
                    if (waiting < 1)
                        this.Invoke(new Action(() =>
                        {
                            komirka.Picture.BackColor = Color.FromArgb(255, 255, 0);
                        }));
                    else
                        this.Invoke(new Action(() =>
                        {
                            komirka.Picture.BackColor = Color.FromArgb(255, 0, 0);
                            if (!nakl.Sound && (nakl.Type == NaklType.PokCM || nakl.Type == NaklType.Zbut || nakl.Type == NaklType.MP))
                            {
                                (new SoundPlayer("Long.wav")).Play();
                                nakl.Sound = true;
                            }
                            //if (nakl.Place == "EXI.KIM" && kKIM < 5)
                            //{
                            //    (new SoundPlayer("KIM.wav")).Play();
                            //    kKIM++;
                            //}
                            //if (nakl.Place == "EXI.LAV" && kLAV < 5)
                            //{
                            //    (new SoundPlayer("LAV.wav")).Play();
                            //    kLAV++;
                            //}
                        }));
                }
                //if (waiting > 1.4)
                //    komirka.blink = true;
            }
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

        private void CountShow(PictureBox box, int count)
        {
            using (Font myFont = new Font("Arial", 30))
            {
                box.CreateGraphics().DrawString(count.ToString(), myFont, Brushes.White, new Point(16, -4));
            }
        }

        private void CountClear(PictureBox box)
        {
            box.Image = car;
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
            ListKomirka.Add("EXI.31", komirka31);
            var komirka32 = new KomirkaVisual()
            {
                Number = this._lKomirka32,
                Picture = this._pbKomirka32,
                Text = this._lText32
            };
            komirka32.Picture.Image = car;
            ListKomirka.Add("EXI.32", komirka32);
            var komirka33 = new KomirkaVisual()
            {
                Number = this._lKomirka33,
                Picture = this._pbKomirka33,
                Text = this._lText33
            };
            komirka33.Picture.Image = car;
            ListKomirka.Add("EXI.33", komirka33);
            var komirka34 = new KomirkaVisual()
            {
                Number = this._lKomirka34,
                Picture = this._pbKomirka34,
                Text = this._lText34
            };
            komirka34.Picture.Image = car;
            ListKomirka.Add("EXI.34", komirka34);
            var komirka35 = new KomirkaVisual()
            {
                Number = this._lKomirka35,
                Picture = this._pbKomirka35,
                Text = this._lText35
            };
            komirka35.Picture.Image = car;
            ListKomirka.Add("EXI.35", komirka35);
            var komirka36 = new KomirkaVisual()
            {
                Number = this._lKomirka36,
                Picture = this._pbKomirka36,
                Text = this._lText36
            };
            komirka36.Picture.Image = car;
            ListKomirka.Add("EXI.36", komirka36);

            var komirka55 = new KomirkaVisual()
            {
                Number = this._lKomirka55,
                Picture = this._pbKomirka55,
                Text = this._lText55
            };
            komirka55.Picture.Image = car;
            ListKomirka.Add("EXI.55", komirka55);
            var komirka56 = new KomirkaVisual()
            {
                Number = this._lKomirka56,
                Picture = this._pbKomirka56,
                Text = this._lText56
            };
            komirka56.Picture.Image = car;
            ListKomirka.Add("EXI.56", komirka56);
            var komirka57 = new KomirkaVisual()
            {
                Number = this._lKomirka57,
                Picture = this._pbKomirka57,
                Text = this._lText57
            };
            komirka57.Picture.Image = car;
            ListKomirka.Add("EXI.57", komirka57);
            var komirka58 = new KomirkaVisual()
            {
                Number = this._lKomirka58,
                Picture = this._pbKomirka58,
                Text = this._lText58
            };
            komirka58.Picture.Image = car;
            ListKomirka.Add("EXI.58", komirka58);
            var komirka59 = new KomirkaVisual()
            {
                Number = this._lKomirka59,
                Picture = this._pbKomirka59,
                Text = this._lText59
            };
            komirka59.Picture.Image = car;
            ListKomirka.Add("EXI.59", komirka59);
            var komirka60 = new KomirkaVisual()
            {
                Number = this._lKomirka60,
                Picture = this._pbKomirka60,
                Text = this._lText60
            };
            komirka60.Picture.Image = car;
            ListKomirka.Add("EXI.60", komirka60);
            var komirka61 = new KomirkaVisual()
            {
                Number = this._lKomirka61,
                Picture = this._pbKomirka61,
                Text = this._lText61
            };
            komirka61.Picture.Image = car;
            ListKomirka.Add("EXI.61", komirka61);
            var komirka62 = new KomirkaVisual()
            {
                Number = this._lKomirka62,
                Picture = this._pbKomirka62,
                Text = this._lText62
            };
            komirka62.Picture.Image = car;
            ListKomirka.Add("EXI.62", komirka62);

            var komirka101 = new KomirkaVisual()
            {
                Number = this._lKomirka101,
                Picture = this._pbKomirka101,
                Text = this._lText101
            };
            komirka101.Picture.Image = car;
            ListKomirka.Add("EXI.101", komirka101);
            var komirka102 = new KomirkaVisual()
            {
                Number = this._lKomirka102,
                Picture = this._pbKomirka102,
                Text = this._lText102
            };
            komirka102.Picture.Image = car;
            ListKomirka.Add("EXI.102", komirka102);

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
            ListKomirka.Add("EXI.MP", komirkaMP);
            var komirkaCM1 = new KomirkaVisual()
            {
                Number = this._lKomirkaCM1,
                Picture = this._pbKomirkaCM1,
                Text = this._lTextCM1
            };
            komirkaCM1.Picture.Image = car;
            ListKomirka.Add("EXI.CM1", komirkaCM1);
            var komirkaCM2 = new KomirkaVisual()
            {
                Number = this._lKomirkaCM2,
                Picture = this._pbKomirkaCM2,
                Text = this._lTextCM2
            };
            komirkaCM2.Picture.Image = car;
            ListKomirka.Add("EXI.CM2", komirkaCM2);
            var komirkaCM3 = new KomirkaVisual()
            {
                Number = this._lKomirkaCM3,
                Picture = this._pbKomirkaCM3,
                Text = this._lTextCM3
            };
            komirkaCM3.Picture.Image = car;
            ListKomirka.Add("EXI.CM3", komirkaCM3);
            var komirkaHOL = new KomirkaVisual()
            {
                Number = this._lKomirkaHOL,
                Picture = this._pbKomirkaHOL,
                Text = this._lTextHOL
            };
            komirkaHOL.Picture.Image = car;
            ListKomirka.Add("EXI.HOL", komirkaHOL);
        }
    }
}
