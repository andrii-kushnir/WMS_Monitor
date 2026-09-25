using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WMS_Monitor
{
    static class Program
    {
        public static string connectionSql;
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
                MessageBox.Show("Увага! Ця програма запускається з параметрами. -Т - для планшету, -М - для монітору, -О - для логіста/оператора, -Rххх - для району");
                return;
            }

            var connPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WMS", "conn.dat");
            try
            {
                byte[] encrypted = File.ReadAllBytes(connPath);
                connectionSql = Encoding.UTF8.GetString(ProtectedData.Unprotect(encrypted, null, DataProtectionScope.LocalMachine));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не вдалося прочитати підключення до SQL.\n\n" +
                    "Файл має бути на цьому комп'ютері:\n" + connPath + "\n\n" +
                    "Його не можна просто скопіювати з іншого ПК.\n\n" +
                    ex.Message,
                    "WMS Monitor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

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
                case string arg when arg.Length > 2
                    && arg.StartsWith("-R", StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(arg.Substring(2), out int sklad):    /*Райони*/
                    Application.Run(new RayonForm(sklad));
                    break;
                default:
                    MessageBox.Show("Невідомий параметр. -Т - планшет, -М - монітор, -О - логіст/оператор, -Rххх - район");
                    break;
            }
        }
    }
}
