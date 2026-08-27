using System.ComponentModel;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using vibrance.GUI.AMD;
using vibrance.GUI.AMD.vendor;
using vibrance.GUI.AMD.vendor.utils;
using vibrance.GUI.common;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI
{
    static class Program
    {
        private const string ErrorGraphicsAdapterUnknown = "Failed to determine your Graphic GraphicsAdapter type (NVIDIA/AMD). Make sure you have installed a proper GPU driver. Intel laptops are not supported as stated on the website. When installing your GPU driver did not work, please contact @juvlarN at twitter. Press Yes to open twitter in your browser now. Error: ";
        private const string ErrorGraphicsAdapterAmbiguous = "Both NVIDIA and AMD graphic drivers have been found on your system. This can happen when you recently switched your graphic card and did not uninstall the old drivers. Make sure to uninstall unused graphic drivers to keep your system safe and stable. Use the program \"Display Driver Uninstaller\" to uninstall your old drivers!\n\nPress Yes to open \"Display Driver Uninstaller\" download website now.\nPress No to quit vibranceGUI.";
        private const string MessageBoxCaption = "vibranceGUI Error";
        private const string VibranceSelfTestMessageBoxCaption = "vibranceGUI vibrance restore self test";

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

            // Placed before NativeMethods.SetDllDirectory/GraphicsAdapterHelper.GetAdapter() below
            // on purpose: VibranceRestoreFixture only ever drives the NVIDIA/AMD apply and restore
            // logic through INvidiaVibranceDevice/IAmdAdapter fakes, so it needs no GPU driver and
            // must stay runnable on a machine (or a build agent) that GetAdapter() cannot resolve
            // and would exit from. There is deliberately no hardware variant of this self test, and
            // there must never be one - a fixture that could change a real display's vibrance would
            // be the exact bug issues #60/#36/#144/#95 are about.
            if (args.Contains("--selftest-vibrance"))
            {
                MessageBox.Show(string.Join(Environment.NewLine, VibranceRestoreFixture.Run().ToArray()),
                    VibranceSelfTestMessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            NativeMethods.SetDllDirectory(CommonUtils.GetVibrance_GUI_AppDataPath());

            GraphicsAdapter adapter = GraphicsAdapterHelper.GetAdapter();
            Form vibranceGui = null;

            if (adapter == GraphicsAdapter.Amd)
            {
                Func<List<ApplicationSetting>, Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>>, IVibranceProxy> getProxy = (x, y) => new AmdDynamicVibranceProxy(Environment.Is64BitOperatingSystem
                    ? new AmdAdapter64()
                    : (IAmdAdapter)new AmdAdapter32(), x, y);
                vibranceGui = new VibranceGUI(getProxy,
                    100,
                    0,
                    300,
                    100,
                    x => x.ToString());
            }
            else if (adapter == GraphicsAdapter.Nvidia)
            {
                const string nvidiaAdapterName = "vibranceDLL.dll";
                string resourceName = $"{typeof(Program).Namespace}.NVIDIA.{nvidiaAdapterName}";
                CommonUtils.LoadUnmanagedLibraryFromResource(
                    Assembly.GetExecutingAssembly(),
                    resourceName,
                    nvidiaAdapterName);
                Marshal.PrelinkAll(typeof(NvidiaDynamicVibranceProxy));

                vibranceGui = new VibranceGUI(
                    (x, y) => new NvidiaDynamicVibranceProxy(x, y),
                    NvidiaDynamicVibranceProxy.NvapiDefaultLevel,
                    NvidiaDynamicVibranceProxy.NvapiDefaultLevel,
                    NvidiaDynamicVibranceProxy.NvapiMaxLevel,
                    NvidiaDynamicVibranceProxy.NvapiDefaultLevel,
                    x => NvidiaVibranceValueWrapper.Find(x).Percentage);
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
            else if(adapter == GraphicsAdapter.Ambiguous)
            {
                if(MessageBox.Show(ErrorGraphicsAdapterAmbiguous, MessageBoxCaption, MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("http://www.guru3d.com/files-details/display-driver-uninstaller-download.html");
                }
                return;
            }
            if (args.Contains("-minimized"))
            {
                vibranceGui.WindowState = FormWindowState.Minimized;
                ((VibranceGUI)vibranceGui).SetAllowVisible(false);
            }
            vibranceGui.Text += String.Format(" ({0}, {1})", adapter.ToString().ToUpper(), Application.ProductVersion);
            Application.Run(vibranceGui);

            GC.KeepAlive(mutex);
        }

        // Internal rather than private: the vibrance restore path (see
        // NVIDIA\NvidiaDynamicVibranceProxy.cs's LogDisplayFailureOnce, reached from the
        // WINEVENT_OUTOFCONTEXT foreground-change callback) reuses this so a broken log write
        // (e.g. File.AppendText failing, the log file being locked by another process) cannot
        // itself throw an exception back across that native callback frame. VibranceGUI.Log's own
        // File.AppendText calls are otherwise unguarded.
        internal static void LogSafely(string message)
        {
            try
            {
                VibranceGUI.Log(message);
            }
            catch (Exception)
            {
                // Logging must never be the reason a foreground event handler throws.
            }
        }
    }
}
