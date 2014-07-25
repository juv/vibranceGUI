using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace vibrance.GUI
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            vibranceGUI vibrance = new vibranceGUI();
            if (args.Length == 1 && "-minimized".Equals(args[0]))
            {
                vibrance.WindowState = FormWindowState.Minimized;
                vibrance.ShowInTaskbar = false;
            }
            Application.Run(vibrance);
        }
    }
}
