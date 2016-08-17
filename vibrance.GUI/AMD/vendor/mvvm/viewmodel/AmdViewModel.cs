using System;
using System.IO;
using vibrance.GUI.AMD.vendor.utils;

namespace vibrance.GUI.AMD.vendor.mvvm.viewmodel
{
    public class AmdViewModel
    {
        public AmdViewModel(Action<string> addLogItem, IAmdAdapter gpuAdapter)
        {
            MinimumVibranceLevel = 100;
            MaximumVibranceLevel = 300;

            VibranceSettingsViewModel = new VibranceSettingsViewModel(addLogItem, gpuAdapter);

            if (VibranceSettingsViewModel.SettingsExists())
            {
                VibranceSettingsViewModel.LoadVibranceSettings();
            }
            else
            {
                VibranceSettingsViewModel.SaveVibranceSettings();
            }

            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = CommonUtils.GetVibrance_GUI_AppDataPath();
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = VibranceSettingsViewModel.SettingsName;
            watcher.Changed += (source, e) => VibranceSettingsViewModel.LoadVibranceSettings();
            watcher.EnableRaisingEvents = true;
        }

        public int MinimumVibranceLevel { get; private set; }

        public int MaximumVibranceLevel { get; private set; }

        public VibranceSettingsViewModel VibranceSettingsViewModel { get; private set; }
    }
}