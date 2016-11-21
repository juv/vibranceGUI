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
        private const string ErrorGraphicsAdapterUnknown = "Falha em determinar o tipo de sua GPU (NVIDIA/AMD). Porvaor entrar em contato com @juvlarN no twitter. Pressione sim para abrir o twitter em seu navegador. Erro: ";
        private const string MessageBoxCaption = "vibranceGUI Erro";

        [STAThread]
        static void Main(string[] args)
        {
            bool result = false;
            Mutex mutex = new Mutex(true, "vibranceGUI~Mutex", out result);
            if (!result)
            {
                MessageBox.Show("Você só pode executar uma instância de cada vez do vibranceGUI!", MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            NativeMethods.SetDllDirectory(CommonUtils.GetVibrance_GUI_AppDataPath());

            GraphicsAdapter adapter = GraphicsAdapterHelper.GetAdapter();
            Form vibranceGui = null;

            if (adapter == GraphicsAdapter.Amd)
            {
                Func<List<ApplicationSetting>, ResolutionModeWrapper, IVibranceProxy> getProxy = (x, y) => new AmdDynamicVibranceProxy(Environment.Is64BitOperatingSystem
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
            if (args.Contains("-minimized"))
            {
                vibranceGui.WindowState = FormWindowState.Minimized;
                ((VibranceGUI)vibranceGui).SetAllowVisible(false);
            }
            Application.Run(vibranceGui);

            GC.KeepAlive(mutex);
        }
    }
}
