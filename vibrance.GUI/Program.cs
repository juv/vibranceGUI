using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace vibrance.GUI
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            bool result = false;
            Mutex mutex = new Mutex(true, "vibranceGUI~Mutex", out result);
            if (!result)
            {
                MessageBox.Show("You can run vibranceGUI only once at a time!", "vibranceGUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            vibranceGUI vibrance = new vibranceGUI();
            if (args.Contains("-besteastereggever")){
                MessageBox.Show("You unlocked the secret easter egg which does absolutely nothing!", "vibranceGUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            if (args.Contains("-minimized"))
            {
                vibrance.WindowState = FormWindowState.Minimized;
                vibrance.ShowInTaskbar = false;
            }
            vibrance.silenced = args.Contains("-silenced");
            Application.Run(vibrance);

            GC.KeepAlive(mutex);
        }
    }
}
