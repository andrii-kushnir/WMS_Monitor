using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS_Monitor.Model
{
    public class Discharge
    {
        public int DischargeCode { get; set; }
        public int Coden { get; set; }
        public string PlaceWMS { get; set; }
        public string PlaceERP { get; set; }
        public DateTime DateOpen { get; set; }
        public DateTime DateClose { get; set; }
        public TimeSpan Timer { get; set; }
        public bool Sound { get; set; }
        public double Waiting { get; set; }
    }
}
