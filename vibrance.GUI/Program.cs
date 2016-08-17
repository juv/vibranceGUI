using System.ComponentModel;
using System.Runtime.InteropServices;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using vibrance.GUI.AMD;
using vibrance.GUI.common;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI
{
    static class Program
    {
        private const string errorGraphicsAdapterUnknown = "Failed to determine your Graphic Adapter type (NVIDIA/AMD). Please contact @juvlarN at twitter. Press Yes to open twitter in your browser now. Error: ";
        private const string messageBoxCaption = "vibranceGUI Error";

        [STAThread]
        static void Main(string[] args)
        {
            bool result = false;
            Mutex mutex = new Mutex(true, "vibranceGUI~Mutex", out result);
            if (!result)
            {
                MessageBox.Show("You can run vibranceGUI only once at a time!", messageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            GraphicsAdapter adapter = GraphicsAdapterHelper.getAdapter();
            Form vibranceGUI = null;

            if (adapter == GraphicsAdapter.AMD)
            {
                vibranceGUI = new AmdVibranceGUI();
            }
            else if (adapter == GraphicsAdapter.NVIDIA)
            {
                vibranceGUI = new NvidiaVibranceGUI();
            }
            else if (adapter == GraphicsAdapter.UNKNOWN)
            {
                string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                if (MessageBox.Show(errorGraphicsAdapterUnknown + errorMessage,
                    messageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("https://twitter.com/juvlarN");
                }
                return;
            }
            if (args.Contains("-minimized"))
            {
                vibranceGUI.WindowState = FormWindowState.Minimized;
                vibranceGUI.ShowInTaskbar = false;
                if (vibranceGUI is AmdVibranceGUI)
                {
                    ((AmdVibranceGUI) vibranceGUI).SetAllowVisible(false);
                }
                else
                {
                    ((NvidiaVibranceGUI) vibranceGUI).SetAllowVisible(false);
                }
            }
            Application.Run(vibranceGUI);

            GC.KeepAlive(mutex);
        }
    }
}
