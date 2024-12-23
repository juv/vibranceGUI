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
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        private const string ErrorGraphicsAdapterUnknown = "Failed to determine your Graphic GraphicsAdapter type (NVIDIA/AMD). Make sure you have installed a proper GPU driver. Intel laptops are not supported as stated on the website. When installing your GPU driver did not work, please contact @juvlarN at twitter. Press Yes to open twitter in your browser now. Error: ";
        private const string ErrorGraphicsAdapterAmbiguous = "Both NVIDIA and AMD graphic drivers have been found on your system. This can happen when you recently switched your graphic card and did not uninstall the old drivers. Make sure to uninstall unused graphic drivers to keep your system safe and stable. Use the program \"Display Driver Uninstaller\" to uninstall your old drivers!\n\nIn case you want to do it manually: The related files are located in your Windows folder and are called \"nvapi.dll\" (NVIDIA) and \"atiadlxx.dll\" (AMD) and \"atiadlxy.dll\" (AMD). You are free to rename/delete the files that you no longer need but proceed with caution!\n\nPress Yes to open \"Display Driver Uninstaller\" download website in your Browser now.\nPress No to quit vibranceGUI.";
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

            if (Environment.OSVersion.Version.Major >= 6)
            {
                SetProcessDPIAware();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            NativeMethods.SetDllDirectory(CommonUtils.GetVibrance_GUI_AppDataPath());

            GraphicsAdapter adapter = GraphicsAdapterHelper.GetAdapter();
            Form vibranceGui = null;

            bool isForcedAmdAdapterExecution = args.Contains("--force-amd");
            bool isForcedNvidiaAdapterExecution = args.Contains("--force-nvidia");

            if (adapter == GraphicsAdapter.Amd || isForcedAmdAdapterExecution)
            {
                Func<List<ApplicationSetting>, Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>>, IVibranceProxy> getProxy = (x, y) => new AmdDynamicVibranceProxy(Environment.Is64BitOperatingSystem
                    ? new AmdAdapter64()
                    : (IAmdAdapter)new AmdAdapter32(), x, y);
                vibranceGui = new VibranceGUI(getProxy,
                    GraphicsAdapter.Amd,
                    AmdDynamicVibranceProxy.AmdDefaultLevel,
                    AmdDynamicVibranceProxy.AmdMinLevel,
                    AmdDynamicVibranceProxy.AmdMaxLevel,
                    AmdDynamicVibranceProxy.AmdDefaultLevel,
                    isForcedAmdAdapterExecution);
            }
            else if (adapter == GraphicsAdapter.Nvidia || isForcedNvidiaAdapterExecution)
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
                    GraphicsAdapter.Nvidia,
                    NvidiaDynamicVibranceProxy.NvapiDefaultLevel,
                    NvidiaDynamicVibranceProxy.NvapiDefaultLevel,
                    NvidiaDynamicVibranceProxy.NvapiMaxLevel,
                    NvidiaDynamicVibranceProxy.NvapiDefaultLevel,
                    isForcedNvidiaAdapterExecution);
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
            else if (adapter == GraphicsAdapter.Ambiguous)
            {
                if (MessageBox.Show(ErrorGraphicsAdapterAmbiguous, MessageBoxCaption, MessageBoxButtons.YesNo,
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
            vibranceGui.Text += buildFormTitleText(adapter, isForcedAmdAdapterExecution, isForcedNvidiaAdapterExecution);
            Application.Run(vibranceGui);

            GC.KeepAlive(mutex);
        }

        static string buildFormTitleText(GraphicsAdapter adapter, bool isForcedAmdAdapterExecution, bool isForcedNvidiaAdapterExecution)
        {
            string forcedExecution = "";
            if (isForcedAmdAdapterExecution)
            {
                forcedExecution = "*AMD forced*";
            }
            else if (isForcedNvidiaAdapterExecution)
            {
                forcedExecution = "*NVIDIA forced*";
            }
            return String.Format(" ({0}, {1}) {2}", adapter.ToString().ToUpper(), Application.ProductVersion, forcedExecution);
        }
    }
}
