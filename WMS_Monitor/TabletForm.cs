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
    public partial class TabletForm : Form
    {
        private Dictionary<string, KomirkaVisual> ListKomirka = new Dictionary<string, KomirkaVisual>();
        private List<NakladnaWMS> ListNakladna = new List<NakladnaWMS>();

        private readonly System.Threading.Timer _timer;
        private const int intervalUpdate = 20 * 1000;
        private DateTime lastExecute = DateTime.MinValue;

        private const int timeWaiting15 = 15 * 60;

        public TabletForm()
        {
            InitializeComponent();
            KomirkaInit();

            _timer = new System.Threading.Timer(new TimerCallback(UpdateMonitor), null, 3 * intervalUpdate, intervalUpdate);
        }

        private void UpdateMonitor(object obj)
        {
            UpdateFromBD();
            UpdateTimingNakl();
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
                        var nakl = ListNakladna.FirstOrDefault(n => n.Coden == coden);
                        if (nakl == null)
                        {
                            var place = Convert.ToString(reader["place"]);
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
                                if ((DateTime.Now - nakl.DateOpen).TotalHours > 27) continue;
                                switch (nakl.NameDoc)
                                {
                                    case "":
                                    case "Тернопіль СМ":
                                    case "Господарські витрати":
                                    case "Повернення покупців":
                                    case "Філії":
                                    case "Внутрішній прихід":
                                    case "Прихід від постачальника":
                                        continue;
                                    case "Покупець СМ":
                                        nakl.Type = NaklType.PokCM;
                                        nakl.NameDoc = "ПокупСМ";
                                        break;
                                    case "Збут":
                                        nakl.Type = NaklType.Zbut;
                                        break;
                                    case "Покупець Маркетплейс":
                                        nakl.Type = NaklType.MP;
                                        nakl.NameDoc = "ПокупМП";
                                        break;
                                    default:
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
                                komirka.Number.BackColor = Color.WhiteSmoke;
                            }));
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

        private void UpdateTimingNakl()
        {
            ListNakladna = ListNakladna.OrderByDescending(n => n.DateOpen).ToList(); /*.ThenBy(n => n.FirstName)*/
            foreach (var nakl in ListNakladna)
            {
                var komirka = ListKomirka[nakl.PlaceWMS];
                nakl.Timer = DateTime.Now - nakl.DateOpen;
                var textTimer = (nakl.Timer.TotalMinutes < 100) ? $"{(int)nakl.Timer.TotalMinutes:00}хв" : $"{(int)nakl.Timer.TotalHours:00}гд";
                this.Invoke(new Action(() =>
                {
                    switch (nakl.Type)
                    {
                        case NaklType.PokCM:
                            komirka.Text.Text = $"ПокупСМ {nakl.Coden} {textTimer}";
                            break;
                        case NaklType.Zbut:
                            komirka.Text.Text = $"Збут    {nakl.Coden} {textTimer}";
                            break;
                        case NaklType.MP:
                            komirka.Text.Text = $"ПокупМП {nakl.Coden} {textTimer}";
                            break;
                    }
                }));
                if (!komirka.Text.Visible)
                    this.Invoke(new Action(() =>
                    {
                        komirka.Text.Visible = true;
                    }));
                if (komirka.Text.BackColor != Color.WhiteSmoke)
                    this.Invoke(new Action(() =>
                    {
                        komirka.Text.BackColor = Color.WhiteSmoke;
                    }));

                nakl.Waiting = nakl.Timer.TotalSeconds / timeWaiting15;

                if (nakl.Waiting < 0) nakl.Waiting = 0;
                if (nakl.Waiting < 0.4)
                    this.Invoke(new Action(() =>
                    {
                        komirka.Number.BackColor = Color.FromArgb(0, 255, 0);
                        komirka.Text.BackColor = Color.FromArgb(0, 255, 0);
                    }));
                else
                {
                    if (nakl.Waiting < 1)
                        this.Invoke(new Action(() =>
                        {
                            komirka.Number.BackColor = Color.FromArgb(255, 255, 0);
                            komirka.Text.BackColor = Color.FromArgb(255, 255, 0);
                        }));
                    else
                        this.Invoke(new Action(() =>
                        {
                            komirka.Number.BackColor = Color.FromArgb(255, 0, 0);
                            komirka.Text.BackColor = Color.FromArgb(255, 0, 0);
                            //if (!nakl.Sound && (nakl.Type == NaklType.PokCM || nakl.Type == NaklType.Zbut || nakl.Type == NaklType.MP))
                            //{
                            //    (new SoundPlayer("Long.wav")).Play();
                            //    nakl.Sound = true;
                            //}
                        }));
                }
            }
        }

        private void Place_Click(object sender, EventArgs e)
        {
            var place = ListKomirka.FirstOrDefault(k => k.Value.Number == sender || k.Value.Text == sender).Key;
            var nakl = ListNakladna.LastOrDefault(n => n.PlaceWMS == place);
            if (nakl != null)
            {
                var naklForm = new NaklForm(nakl);
                naklForm.Show();
            }
        }

        private void TabletForm_Load(object sender, EventArgs e)
        {
            UpdateMonitor(null);
        }

        private void KomirkaInit()
        {
            var komirka01 = new KomirkaVisual()
            {
                Number = this._lKomirka01,
                //Picture = this._pbKomirka01,
                Text = this._lText01
            };
            ListKomirka.Add("EXI.01", komirka01);
            var komirka02 = new KomirkaVisual()
            {
                Number = this._lKomirka02,
                //Picture = this._pbKomirka02,
                Text = this._lText02
            };
            ListKomirka.Add("EXI.02", komirka02);
            var komirka03 = new KomirkaVisual()
            {
                Number = this._lKomirka03,
                //Picture = this._pbKomirka03,
                Text = this._lText03
            };
            ListKomirka.Add("EXI.03", komirka03);
            var komirka04 = new KomirkaVisual()
            {
                Number = this._lKomirka04,
                //Picture = this._pbKomirka04,
                Text = this._lText04
            };
            ListKomirka.Add("EXI.04", komirka04);
            var komirka05 = new KomirkaVisual()
            {
                Number = this._lKomirka05,
                //Picture = this._pbKomirka05,
                Text = this._lText05
            };
            ListKomirka.Add("EXI.05", komirka05);
            var komirka06 = new KomirkaVisual()
            {
                Number = this._lKomirka06,
                //Picture = this._pbKomirka06,
                Text = this._lText06
            };
            ListKomirka.Add("EXI.06", komirka06);
            var komirka07 = new KomirkaVisual()
            {
                Number = this._lKomirka07,
                //Picture = this._pbKomirka07,
                Text = this._lText07
            };
            ListKomirka.Add("EXI.07", komirka07);
            var komirka08 = new KomirkaVisual()
            {
                Number = this._lKomirka08,
                //Picture = this._pbKomirka08,
                Text = this._lText08
            };
            ListKomirka.Add("EXI.08", komirka08);
            var komirka09 = new KomirkaVisual()
            {
                Number = this._lKomirka09,
                //Picture = this._pbKomirka09,
                Text = this._lText09
            };
            ListKomirka.Add("EXI.09", komirka09);
            var komirka10 = new KomirkaVisual()
            {
                Number = this._lKomirka10,
                //Picture = this._pbKomirka10,
                Text = this._lText10
            };
            ListKomirka.Add("EXI.10", komirka10);
            var komirka11 = new KomirkaVisual()
            {
                Number = this._lKomirka11,
                //Picture = this._pbKomirka11,
                Text = this._lText11
            };
            ListKomirka.Add("EXI.11", komirka11);
            var komirka12 = new KomirkaVisual()
            {
                Number = this._lKomirka12,
                //Picture = this._pbKomirka12,
                Text = this._lText12
            };
            ListKomirka.Add("EXI.12", komirka12);
            var komirka13 = new KomirkaVisual()
            {
                Number = this._lKomirka13,
                //Picture = this._pbKomirka13,
                Text = this._lText13
            };
            ListKomirka.Add("EXI.13", komirka13);
            var komirka14 = new KomirkaVisual()
            {
                Number = this._lKomirka14,
                //Picture = this._pbKomirka14,
                Text = this._lText14
            };
            ListKomirka.Add("EXI.14", komirka14);
            var komirka15 = new KomirkaVisual()
            {
                Number = this._lKomirka15,
                //Picture = this._pbKomirka15,
                Text = this._lText15
            };
            ListKomirka.Add("EXI.15", komirka15);
            var komirka16 = new KomirkaVisual()
            {
                Number = this._lKomirka16,
                //Picture = this._pbKomirka16,
                Text = this._lText16
            };
            ListKomirka.Add("EXI.16", komirka16);
            var komirka17 = new KomirkaVisual()
            {
                Number = this._lKomirka17,
                //Picture = this._pbKomirka17,
                Text = this._lText17
            };
            ListKomirka.Add("EXI.17", komirka17);
            var komirka18 = new KomirkaVisual()
            {
                Number = this._lKomirka18,
                //Picture = this._pbKomirka18,
                Text = this._lText18
            };
            ListKomirka.Add("EXI.18", komirka18);
            var komirka19 = new KomirkaVisual()
            {
                Number = this._lKomirka19,
                //Picture = this._pbKomirka19,
                Text = this._lText19
            };
            ListKomirka.Add("EXI.19", komirka19);
            var komirka20 = new KomirkaVisual()
            {
                Number = this._lKomirka20,
                //Picture = this._pbKomirka20,
                Text = this._lText20
            };
            ListKomirka.Add("EXI.20", komirka20);
            var komirka21 = new KomirkaVisual()
            {
                Number = this._lKomirka21,
                //Picture = this._pbKomirka21,
                Text = this._lText21
            };
            ListKomirka.Add("EXI.21", komirka21);
            var komirka22 = new KomirkaVisual()
            {
                Number = this._lKomirka22,
                //Picture = this._pbKomirka22,
                Text = this._lText22
            };
            ListKomirka.Add("EXI.22", komirka22);
            var komirka23 = new KomirkaVisual()
            {
                Number = this._lKomirka23,
                //Picture = this._pbKomirka23,
                Text = this._lText23
            };
            ListKomirka.Add("EXI.23", komirka23);
            var komirka24 = new KomirkaVisual()
            {
                Number = this._lKomirka24,
                //Picture = this._pbKomirka24,
                Text = this._lText24
            };
            ListKomirka.Add("EXI.24", komirka24);
            var komirka25 = new KomirkaVisual()
            {
                Number = this._lKomirka25,
                //Picture = this._pbKomirka25,
                Text = this._lText25
            };
            ListKomirka.Add("EXI.25", komirka25);
            var komirka26 = new KomirkaVisual()
            {
                Number = this._lKomirka26,
                //Picture = this._pbKomirka26,
                Text = this._lText26
            };
            ListKomirka.Add("EXI.26", komirka26);
            var komirka27 = new KomirkaVisual()
            {
                Number = this._lKomirka27,
                //Picture = this._pbKomirka27,
                Text = this._lText27
            };
            ListKomirka.Add("EXI.27", komirka27);

            //var komirka31 = new KomirkaVisual()
            //{
            //    Number = this._lKomirka31,
            //    Picture = this._pbKomirka31,
            //    Text = this._lText31
            //};
            //ListKomirka.Add("ENT.31", komirka31);
            //var komirka32 = new KomirkaVisual()
            //{
            //    Number = this._lKomirka32,
            //    Picture = this._pbKomirka32,
            //    Text = this._lText32
            //};
            //ListKomirka.Add("ENT.32", komirka32);
            //var komirka33 = new KomirkaVisual()
            //{
            //    Number = this._lKomirka33,
            //    Picture = this._pbKomirka33,
            //    Text = this._lText33
            //};
            //ListKomirka.Add("ENT.33", komirka33);
            //var komirka34 = new KomirkaVisual()
            //{
            //    Number = this._lKomirka34,
            //    Picture = this._pbKomirka34,
            //    Text = this._lText34
            //};
            //ListKomirka.Add("ENT.34", komirka34);
            //var komirka35 = new KomirkaVisual()
            //{
            //    Number = this._lKomirka35,
            //    Picture = this._pbKomirka35,
            //    Text = this._lText35
            //};
            //ListKomirka.Add("ENT.35", komirka35);
            //var komirka36 = new KomirkaVisual()
            //{
            //    Number = this._lKomirka36,
            //    Picture = this._pbKomirka36,
            //    Text = this._lText36
            //};
            //ListKomirka.Add("ENT.36", komirka36);

            //var komirka55 = new KomirkaVisual()
            //{
            //    Number = this._lKomirka55,
            //    Picture = this._pbKomirka55,
            //    Text = this._lText55
            //};
            //ListKomirka.Add("ENT.55", komirka55);
            //var komirka56 = new KomirkaVisual()
            //{
            //    Number = this._lKomirka56,
            //    Picture = this._pbKomirka56,
            //    Text = this._lText56
            //};
            //ListKomirka.Add("ENT.56", komirka56);
            var komirka57 = new KomirkaVisual()
            {
                Number = this._lKomirka57,
                //Picture = this._pbKomirka57,
                Text = this._lText57
            };
            ListKomirka.Add("ENT.57", komirka57);
            var komirka58 = new KomirkaVisual()
            {
                Number = this._lKomirka58,
                //Picture = this._pbKomirka58,
                Text = this._lText58
            };
            ListKomirka.Add("ENT.58", komirka58);
            var komirka59 = new KomirkaVisual()
            {
                Number = this._lKomirka59,
                //Picture = this._pbKomirka59,
                Text = this._lText59
            };
            ListKomirka.Add("ENT.59", komirka59);
            var komirka60 = new KomirkaVisual()
            {
                Number = this._lKomirka60,
                //Picture = this._pbKomirka60,
                Text = this._lText60
            };
            ListKomirka.Add("ENT.60", komirka60);
            var komirka61 = new KomirkaVisual()
            {
                Number = this._lKomirka61,
                //Picture = this._pbKomirka61,
                Text = this._lText61
            };
            ListKomirka.Add("ENT.61", komirka61);
            var komirka62 = new KomirkaVisual()
            {
                Number = this._lKomirka62,
                //Picture = this._pbKomirka62,
                Text = this._lText62
            };
            ListKomirka.Add("ENT.62", komirka62);

            //var komirka101 = new KomirkaVisual()
            //{
            //    Number = this._lKomirka101,
            //    Picture = this._pbKomirka101,
            //    Text = this._lText101
            //};
            //ListKomirka.Add("ENT.101", komirka101);
            //var komirka102 = new KomirkaVisual()
            //{
            //    Number = this._lKomirka102,
            //    Picture = this._pbKomirka102,
            //    Text = this._lText102
            //};
            //ListKomirka.Add("ENT.102", komirka102);
            var komirkaB2 = new KomirkaVisual()
            {
                Number = this._lKomirkaB2,
                //Picture = this._pbKomirkaB2,
                Text = this._lTextB2
            };
            ListKomirka.Add("EXI.V2", komirkaB2);
            var komirkaLAV = new KomirkaVisual()
            {
                Number = this._lKomirkaLAV,
                //Picture = this._pbKomirkaLAV,
                Text = this._lTextLAV
            };
            ListKomirka.Add("EXI.LAV", komirkaLAV);
            var komirkaKIM = new KomirkaVisual()
            {
                Number = this._lKomirkaKIM,
                //Picture = this._pbKomirkaKIM,
                Text = this._lTextKIM
            };
            ListKomirka.Add("EXI.KIM", komirkaKIM);
            //var komirkaMP = new KomirkaVisual()
            //{
            //    Number = this._lKomirkaMP,
            //    Picture = this._pbKomirkaMP,
            //    Text = this._lTextMP
            //};
            //ListKomirka.Add("ENT.MP", komirkaMP);
            //var komirkaCM1 = new KomirkaVisual()
            //{
            //    Number = this._lKomirkaCM1,
            //    Picture = this._pbKomirkaCM1,
            //    Text = this._lTextCM1
            //};
            //ListKomirka.Add("ENT.CM1", komirkaCM1);
            //var komirkaCM2 = new KomirkaVisual()
            //{
            //    Number = this._lKomirkaCM2,
            //    Picture = this._pbKomirkaCM2,
            //    Text = this._lTextCM2
            //};
            //ListKomirka.Add("ENT.CM2", komirkaCM2);
            //var komirkaCM3 = new KomirkaVisual()
            //{
            //    Number = this._lKomirkaCM3,
            //    Picture = this._pbKomirkaCM3,
            //    Text = this._lTextCM3
            //};
            //ListKomirka.Add("ENT.CM3", komirkaCM3);
            //var komirkaHOL = new KomirkaVisual()
            //{
            //    Number = this._lKomirkaHOL,
            //    Picture = this._pbKomirkaHOL,
            //    Text = this._lTextHOL
            //};
            //ListKomirka.Add("EXI.HOL", komirkaHOL);
        }
    }
}
