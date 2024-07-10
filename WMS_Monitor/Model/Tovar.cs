using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS_Monitor.Model
{
    public class Tovar
    {
        //public int codetvun { get; set; }
        public int codetv { get; set; }
        public string nametv { get; set; }
        public double countTovar { get; set; }
        public string operName { get; set; }
        public string resName { get; set; }
        public DateTime dateExec { get; set; }
    }
}
