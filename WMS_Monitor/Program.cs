using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WMS_Monitor
{
    static class Program
    {
        internal const string connectionSql101 = @"Server=192.168.4.101; Database=erp; uid=КушнірА; pwd=зщшфтв;";
        internal const string connectionSql101sa = @"Server=192.168.4.101; Database=erp; uid=sa; pwd=Yi*7tg8tc=t?PjM;";
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length == 0)
            {
                MessageBox.Show("Увага! Ця програма запускається з параметрами. -Т - для планшету, -М - для монітору, -О - для логіста/оператора");
            }
            else
                switch (args[0])
                {
                    case "-T":    /*Планшет*/
                    case "-t":
                        Application.Run(new TabletForm());
                        break;
                    case "-M":    /*Монітор*/
                    case "-m":
                        Application.Run(new MainForm());
                        break;
                    case "-O":    /*Логіст*/
                    case "-o":
                        Application.Run(new MainForm(true));
                        break;
                }
        }
    }
}
