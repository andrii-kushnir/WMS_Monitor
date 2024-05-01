using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS_Monitor.Model
{
    public class Nakladna
    {
        public int Coden { get; set; }
        public string PlaceWMS { get; set; }
        public string PlaceERP { get; set; }
        public string Text { get; set; }
        public NaklType Type { get; set; }
        public DateTime DatePrepare { get; set; }
        public DateTime DateOpen { get; set; }
        public DateTime DateClose { get; set; }
        public int Dostavka { get; set; }
        public string NameDoc { get; set; }
        public TimeSpan Timer { get; set; }
        public bool Sound { get; set; }
    }

    public enum NaklType
    {
        Empty = 0,
        PokCM = 1,
        TerCM = 2,
        Gosp = 3,
        Zbut = 4,
        Povern = 5,
        MP = 6,
        Filii = 7,
        VnPr = 8,
        Post = 9
    }
}
