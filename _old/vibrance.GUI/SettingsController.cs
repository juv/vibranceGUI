using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace vibrance.GUI
{

    class SettingsController
    {
        private readonly VibranceProxy _vibranceProxy;

        public SettingsController(VibranceProxy vibranceProxy)
        {
            _vibranceProxy = vibranceProxy;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern uint GetPrivateProfileString(
           string lpAppName,
           string lpKeyName,
           string lpDefault,
           StringBuilder lpReturnedString,
           uint nSize,
           string lpFileName);


        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileString")]
        private static extern bool WritePrivateProfileString(string lpAppName,
          string lpKeyName, string lpString, string lpFileName);

        const string szSectionName = "Settings";
        const string szKeyNameInactive = "inactiveValue";
        const string szKeyNameActive = "activeValue";
        const string szKeyNameKeepActive = "keepActive";
        const string szKeyNameRefreshRate = "refreshRate";
        const string szKeyNameMultipleMonitors = "multipleMonitors";

        public bool setVibranceSettings(string szKeyName, string value)
        {
            string szFilename = "\\vibranceGUI.ini";
            string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            szFilename = appdataPath + szFilename;

            if (!isFileExisting(szFilename))
            {
                StreamWriter sw = new StreamWriter(szFilename);
                sw.Close();
                if (!isFileExisting(szFilename))
                {
                    return false;
                }
            }

            WritePrivateProfileString(szSectionName, szKeyName, value.ToString(), szFilename);
            if (Marshal.GetLastWin32Error() == 0)
            {
                return true;
            }
            return false;
        }


        public void readVibranceSettings(out int vibranceIngameLevel, out int vibranceWindowsLevel, out bool keepActive, out int refreshRate, out bool multipleMonitors)
        {
            string szFilename = "\\vibranceGUI.ini";
            string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            szFilename = appdataPath + szFilename;
            if (!isFileExisting(szFilename))
            {
                vibranceIngameLevel = _vibranceProxy.NVAPI_DEFAULT_LEVEL;
                vibranceWindowsLevel = _vibranceProxy.NVAPI_DEFAULT_LEVEL;
                refreshRate = _vibranceProxy.NVAPI_DEFAULT_REFRESH_RATE;
                keepActive = false;
                multipleMonitors = false;
                return;
            }

            string szDefault = "";

            StringBuilder szValueActive = new StringBuilder(1024);
            
            GetPrivateProfileString(szSectionName,
                szKeyNameActive,
                szDefault,
                szValueActive,
                Convert.ToUInt32(szValueActive.Capacity),
                szFilename);

            StringBuilder szValueInactive = new StringBuilder(1024);
            GetPrivateProfileString(szSectionName,
                szKeyNameInactive,
                szDefault,
                szValueInactive,
                Convert.ToUInt32(szValueInactive.Capacity),
                szFilename);

            StringBuilder szValueRefreshRate = new StringBuilder(1024);
            GetPrivateProfileString(szSectionName,
                szKeyNameRefreshRate,
                szDefault,
                szValueRefreshRate,
                Convert.ToUInt32(szValueRefreshRate.Capacity),
                szFilename);


            StringBuilder szValueKeepActive = new StringBuilder(1024);
            GetPrivateProfileString(szSectionName,
                szKeyNameKeepActive,
                szDefault,
                szValueKeepActive,
                Convert.ToUInt32(szValueKeepActive.Capacity),
                szFilename);

            StringBuilder szValueMultipleMonitors = new StringBuilder(1024);
            GetPrivateProfileString(szSectionName,
                szKeyNameMultipleMonitors,
                szDefault,
                szValueMultipleMonitors,
                Convert.ToUInt32(szValueMultipleMonitors.Capacity),
                szFilename);

            try
            {
                vibranceWindowsLevel = int.Parse(szValueInactive.ToString());
                vibranceIngameLevel = int.Parse(szValueActive.ToString());
                refreshRate = int.Parse(szValueRefreshRate.ToString());
                keepActive = bool.Parse(szValueKeepActive.ToString());
                multipleMonitors = bool.Parse(szValueMultipleMonitors.ToString());
            }
            catch (Exception)
            {
                vibranceIngameLevel = _vibranceProxy.NVAPI_DEFAULT_LEVEL;
                vibranceWindowsLevel = _vibranceProxy.NVAPI_DEFAULT_LEVEL;
                refreshRate = _vibranceProxy.NVAPI_DEFAULT_REFRESH_RATE;
                keepActive = false;
                multipleMonitors = false;
                return;
            }

            if (vibranceWindowsLevel < _vibranceProxy.NVAPI_DEFAULT_LEVEL || vibranceWindowsLevel > _vibranceProxy.NVAPI_MAX_LEVEL)
                vibranceWindowsLevel = _vibranceProxy.NVAPI_DEFAULT_LEVEL;
            if (vibranceIngameLevel < _vibranceProxy.NVAPI_DEFAULT_LEVEL || vibranceIngameLevel > _vibranceProxy.NVAPI_MAX_LEVEL)
                vibranceIngameLevel = _vibranceProxy.NVAPI_MAX_LEVEL;
            if (refreshRate < _vibranceProxy.NVAPI_MIN_REFRESH_RATE)
                refreshRate = _vibranceProxy.NVAPI_DEFAULT_REFRESH_RATE;
        }

        private bool isFileExisting(string szFilename)
        {
            return File.Exists(szFilename);
        }
    }
}
