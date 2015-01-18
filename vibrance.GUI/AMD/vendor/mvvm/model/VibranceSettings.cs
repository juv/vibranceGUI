using System;
using System.ComponentModel;
using GalaSoft.MvvmLight;

namespace gui.app.mvvm.model
{
    public class VibranceSettings : ObservableObject
    {
        private int _windowsVibranceLevel;
        private int _ingameVibranceLevel;
        private bool _autostartVibranceGui;
        private bool _keepVibranceOnWhenCsGoIsStarted;
        private bool _useMultipleMonitors;
        private int _refreshRate;

        public VibranceSettings()
        {
            _windowsVibranceLevel = 100;
            _ingameVibranceLevel = 200;
            _autostartVibranceGui = false;
            _keepVibranceOnWhenCsGoIsStarted = false;
            _useMultipleMonitors = false;
            _refreshRate = 5000;
        }

        public bool AutostartVibranceGui
        {
            get
            {
                return _autostartVibranceGui;
            }

            set
            {
                this.Set(() => AutostartVibranceGui, ref _autostartVibranceGui, value);
            }
        }

        public bool KeepVibranceOnWhenCsGoIsStarted
        {
            get
            {
                return _keepVibranceOnWhenCsGoIsStarted;
            }

            set
            {
                this.Set(() => KeepVibranceOnWhenCsGoIsStarted, ref _keepVibranceOnWhenCsGoIsStarted, value);
            }
        }

        public bool UseMultipleMonitors
        {
            get
            {
                return _useMultipleMonitors;
            }

            set
            {
                this.Set(() => UseMultipleMonitors, ref _useMultipleMonitors, value);
            }
        }

        public int WindowsVibranceLevel
        {
            get
            {
                return _windowsVibranceLevel;
            }

            set
            {
                this.Set(() => WindowsVibranceLevel, ref _windowsVibranceLevel, value);
            }
        }

        public int IngameVibranceLevel
        {
            get
            {
                return _ingameVibranceLevel;
            }

            set
            {
                this.Set(() => IngameVibranceLevel, ref _ingameVibranceLevel, value);
            }
        }

        public int RefreshRate
        {
            get
            {
                return _refreshRate;
            }

            set
            {
                this.Set(() => RefreshRate, ref _refreshRate, value);
            }
        }
    }
}