using System.ComponentModel;
using System.Runtime.InteropServices;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using vibrance.GUI.AMD;
using vibrance.GUI.AMD.vendor;
using vibrance.GUI.common;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI
{
    static class Program
    {
        private const string ErrorGraphicsAdapterUnknown = "Failed to determine your Graphic Adapter type (NVIDIA/AMD). Please contact @juvlarN at twitter. Press Yes to open twitter in your browser now. Error: ";
        private const string MessageBoxCaption = "vibranceGUI Error";

        [STAThread]
        static void Main(string[] args)
        {
            bool result = false;
            Mutex mutex = new Mutex(true, "vibranceGUI~Mutex", out result);
            if (!result)
            {
                MessageBox.Show("You can run vibranceGUI only once at a time!", MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            GraphicsAdapter adapter = GraphicsAdapterHelper.GetAdapter();
            Form vibranceGui = null;

            if (adapter == GraphicsAdapter.Amd)
            {
                vibranceGui = new AmdVibranceGui(Environment.Is64BitOperatingSystem 
                    ? new AmdAdapter64() 
                    : (IAmdAdapter)new AmdAdapter32());
            }
            else if (adapter == GraphicsAdapter.Nvidia)
            {
                vibranceGui = new NvidiaVibranceGUI();
            }
            else if (adapter == GraphicsAdapter.Unknown)
            {
                string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                if (MessageBox.Show(ErrorGraphicsAdapterUnknown + errorMessage,
                    MessageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("https://twitter.com/juvlarN");
                }
                return;
            }
            if (args.Contains("-minimized"))
            {
                vibranceGui.WindowState = FormWindowState.Minimized;
                if (vibranceGui is AmdVibranceGui)
                {
                    ((AmdVibranceGui) vibranceGui).SetAllowVisible(false);
                }
                else
                {
                    ((NvidiaVibranceGUI) vibranceGui).SetAllowVisible(false);
                }
            }
            Application.Run(vibranceGui);

            GC.KeepAlive(mutex);
        }
    }
}
