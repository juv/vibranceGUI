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
        private const string SelfTestMessageBoxCaption = "vibranceGUI graphics adapter self test";
        private const string DisplayDriverUninstallerUrl = "http://www.guru3d.com/files-details/display-driver-uninstaller-download.html";

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

            // Runs before the adapter detection below on purpose: the vendor matching it covers is
            // pure, so the self test stays runnable on a build agent or a reviewer's machine that
            // has neither driver installed, where GetAdapter() shows an error and exits.
            if (args.Contains("--selftest-gpu"))
            {
                MessageBox.Show(string.Join(Environment.NewLine, GraphicsAdapterFixture.Run().ToArray()),
                    SelfTestMessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            NativeMethods.SetDllDirectory(CommonUtils.GetVibrance_GUI_AppDataPath());

            GraphicsAdapter adapter = GraphicsAdapterHelper.GetAdapter();

            // Captured here, while it still means something. The File.Exists calls below overwrite
            // the thread's last error, degrading 126 "the specified module could not be found",
            // which tells a user their driver DLL is missing, into 2 "the system cannot find the
            // file specified". That message is what users are asked to paste into a bug report.
            int adapterDetectionWin32Error = Marshal.GetLastWin32Error();

            Form vibranceGui = null;

            // Both drivers installed is the one case the driver files cannot settle, and the case
            // vibranceGUI used to refuse to start on. A stored choice wins first, then whichever
            // adapter actually drives an attached display, and only then is the user asked. A
            // machine that resolves to a single vendor never reaches any of this and keeps
            // resolving exactly as it did.
            if (GraphicsAdapterHelper.AreBothVendorDriversInstalled())
            {
                adapter = ResolveInstalledDriverConflict(adapter);
                if (adapter != GraphicsAdapter.Amd && adapter != GraphicsAdapter.Nvidia)
                {
                    // Cancelled, or the fallback dialog has already had its say.
                    return;
                }
            }

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
                string errorMessage = new Win32Exception(adapterDetectionWin32Error).Message;
                if (MessageBox.Show(ErrorGraphicsAdapterUnknown + errorMessage,
                    MessageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("https://twitter.com/juvlarN");
                }
                return;
            }
            else if(adapter == GraphicsAdapter.Ambiguous)
            {
                // Not reachable through ResolveInstalledDriverConflict, which never lets an
                // unresolved Ambiguous past it. Kept so the vendor cannot silently go unhandled.
                ShowLegacyAmbiguousDriverDialog();
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

        /// <summary>
        /// Settles the case the driver files alone cannot: both vendors' DLLs are installed, so
        /// the old code gave up here and told the user to uninstall a driver. A stored choice wins
        /// first, then whatever adapter actually drives an attached display, and only when both of
        /// those come up empty is the user asked.
        /// Returns Ambiguous when the user declined to choose, which means "quit".
        /// </summary>
        static GraphicsAdapter ResolveInstalledDriverConflict(GraphicsAdapter detectedAdapter)
        {
            GraphicsAdapter storedAdapter = GraphicsAdapter.Unknown;
            try
            {
                storedAdapter = new SettingsController().ReadGraphicsAdapterPreference();
            }
            catch (Exception ex)
            {
                LogSafely(ex.ToString());
            }

            // A stored choice is honoured only while the hardware it names is still around,
            // otherwise a user who swapped cards would be stuck on last year's answer.
            if (storedAdapter != GraphicsAdapter.Unknown && GraphicsAdapterHelper.IsVendorDriverInstalled(storedAdapter))
            {
                LogAdapterResolution("the stored preference", storedAdapter);
                return storedAdapter;
            }

            if (detectedAdapter == GraphicsAdapter.Amd || detectedAdapter == GraphicsAdapter.Nvidia)
            {
                LogAdapterResolution("the attached display devices", detectedAdapter);
                return detectedAdapter;
            }

            return AskUserForGraphicsAdapter();
        }

        static GraphicsAdapter AskUserForGraphicsAdapter()
        {
            try
            {
                using (GraphicsAdapterChooser chooser = new GraphicsAdapterChooser(GraphicsAdapterHelper.GetDisplayAdapters()))
                {
                    if (chooser.ShowDialog() != DialogResult.OK ||
                        (chooser.SelectedAdapter != GraphicsAdapter.Amd && chooser.SelectedAdapter != GraphicsAdapter.Nvidia))
                    {
                        return GraphicsAdapter.Ambiguous;
                    }

                    if (chooser.ShouldRememberChoice)
                    {
                        try
                        {
                            new SettingsController().SetGraphicsAdapterPreference(chooser.SelectedAdapter);
                        }
                        catch (Exception ex)
                        {
                            // Not being able to remember the answer is no reason not to act on it.
                            LogSafely(ex.ToString());
                        }
                    }

                    LogAdapterResolution("the chooser dialog", chooser.SelectedAdapter);
                    return chooser.SelectedAdapter;
                }
            }
            catch (Exception ex)
            {
                LogSafely(ex.ToString());
                return ShowLegacyAmbiguousDriverDialog();
            }
        }

        /// <summary>
        /// The pre-existing dialog, now only the fallback for when the chooser itself cannot be
        /// shown. Its DDU advice is right for a leftover driver and wrong for hybrid hardware,
        /// which is why it is no longer the first thing a user meets.
        /// </summary>
        static GraphicsAdapter ShowLegacyAmbiguousDriverDialog()
        {
            if (MessageBox.Show(ErrorGraphicsAdapterAmbiguous, MessageBoxCaption, MessageBoxButtons.YesNo,
                MessageBoxIcon.Error) == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start(DisplayDriverUninstallerUrl);
            }
            return GraphicsAdapter.Ambiguous;
        }

        /// <summary>
        /// The line to ask a user for when they report that vibranceGUI picked the wrong GPU.
        /// </summary>
        static void LogAdapterResolution(string source, GraphicsAdapter adapter)
        {
            LogSafely(String.Format("Both GPU drivers are installed. Resolved to {0} from {1}.{2}{3}",
                adapter, source, Environment.NewLine, GraphicsAdapterHelper.DescribeDisplayAdapters()));
        }

        static void LogSafely(string message)
        {
            try
            {
                VibranceGUI.Log(message);
            }
            catch (Exception)
            {
                // Logging must never be the reason startup fails.
            }
        }
    }
}
