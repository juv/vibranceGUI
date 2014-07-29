using System.Reflection;
using System.Windows;
using GalaSoft.MvvmLight.Threading;
using gui.app.utils;

namespace gui.app
{
    public partial class App : Application
    {
        static App()
        {
            DispatcherHelper.Initialize();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            const string nvidiaAdapterName = "nvidia.adapter.dll";
            string resourceName = string.Format("{0}.{1}", typeof(App).Namespace, nvidiaAdapterName);

            CommonUtils.LoadUnmanagedLibraryFromResource(
                Assembly.GetExecutingAssembly(),
                resourceName,
                nvidiaAdapterName);

            base.OnStartup(e);
        }
    }
}
