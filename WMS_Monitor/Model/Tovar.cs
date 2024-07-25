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
        public DateTime dateZahopl { get; set; }
        public bool isPause { get; set; }
        public string resRozp { get; set; }
        public string operReal
        {
            get
            {
                //if (dateExec == new DateTime(0001, 01, 01))
                if (operName == "Відвантаження завершення")
                    return "Видано";
                else
                    return operName;
            }
        }

        public string resReal 
        {
            get 
            {
                if (resRozp == null)
                {
                    if (operName == "Відвантаження завершення")
                        if (resName == null)
                            return "";
                        else
                            return resName;
                    else
                        return "-= Не взято =-";
                }
                return resRozp; 
            }
        }
    }
}
