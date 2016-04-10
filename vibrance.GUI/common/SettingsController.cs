using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI.common
{

    class SettingsController : ISettingsController
    {
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
        const string szKeyNameRefreshRate = "refreshRate";
        const string szKeyNameAffectPrimaryMonitorOnly = "affectPrimaryMonitorOnly";

        private string fileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + "\\vibranceGUI\\vibranceGUI.ini";
        private string fileNameApplicationSettings = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + "\\vibranceGUI\\applicationData.xml";


        public bool setVibranceSettings(string windowsLevel, string affectPrimaryMonitorOnly, List<NvidiaApplicationSetting> applicationSettings)
        {
            if (!prepareFile())
            {
                return false;
            }

            WritePrivateProfileString(szSectionName, szKeyNameInactive, windowsLevel, fileName);
            WritePrivateProfileString(szSectionName, szKeyNameAffectPrimaryMonitorOnly, affectPrimaryMonitorOnly, fileName);

            try
            {
                var writer = System.Xml.XmlWriter.Create(fileNameApplicationSettings);
                if (writer.WriteState != WriteState.Start)
                    return false;
                XmlSerializer serializer = new XmlSerializer(typeof(List<NvidiaApplicationSetting>));
                serializer.Serialize(writer, applicationSettings);
                writer.Flush();
                writer.Close();
            }
            catch (Exception)
            {
                return false;
            }

            return (Marshal.GetLastWin32Error() == 0);
        }

        public bool setVibranceSetting(string szKeyName, string value)
        {
            if (!prepareFile())
            {
                return false;
            }

            WritePrivateProfileString(szSectionName, szKeyName, value.ToString(), fileName);

            return (Marshal.GetLastWin32Error() == 0);
        }

        private bool prepareFile()
        {
            if (!isFileExisting(fileName))
            {
                StreamWriter sw = new StreamWriter(fileName);
                sw.Close();
                if (!isFileExisting(fileName))
                {
                    return false;
                }
            }

            return true;
        }

        public void readVibranceSettings(GraphicsAdapter graphicsAdapter, out int vibranceWindowsLevel, out bool affectPrimaryMonitorOnly, out List<NvidiaApplicationSetting> applicationSettings)
        {
            int defaultLevel = 0; 
            int maxLevel = 0;
            if (graphicsAdapter == GraphicsAdapter.NVIDIA)
            {
                defaultLevel = NvidiaVibranceProxy.NVAPI_DEFAULT_LEVEL;
                maxLevel = NvidiaVibranceProxy.NVAPI_MAX_LEVEL;
            }


            if (!isFileExisting(fileName) || !isFileExisting(fileNameApplicationSettings))
            {
                vibranceWindowsLevel = defaultLevel;
                affectPrimaryMonitorOnly = false;
                applicationSettings = new List<NvidiaApplicationSetting>();
                return;
            }

            string szDefault = "";

            StringBuilder szValueInactive = new StringBuilder(1024);
            GetPrivateProfileString(szSectionName,
                szKeyNameInactive,
                szDefault,
                szValueInactive,
                Convert.ToUInt32(szValueInactive.Capacity),
                fileName);

            StringBuilder szValueRefreshRate = new StringBuilder(1024);
            GetPrivateProfileString(szSectionName,
                szKeyNameRefreshRate,
                szDefault,
                szValueRefreshRate,
                Convert.ToUInt32(szValueRefreshRate.Capacity),
                fileName);

            StringBuilder szValueAffectPrimaryMonitorOnly = new StringBuilder(1024);
            GetPrivateProfileString(szSectionName,
                szKeyNameAffectPrimaryMonitorOnly,
                szDefault,
                szValueAffectPrimaryMonitorOnly,
                Convert.ToUInt32(szValueAffectPrimaryMonitorOnly.Capacity),
                fileName);

            try
            {
                vibranceWindowsLevel = int.Parse(szValueInactive.ToString());
                affectPrimaryMonitorOnly = bool.Parse(szValueAffectPrimaryMonitorOnly.ToString());
            }
            catch (Exception)
            {
                vibranceWindowsLevel = defaultLevel;
                affectPrimaryMonitorOnly = false;
                applicationSettings = new List<NvidiaApplicationSetting>();
                return;
            }

            if (vibranceWindowsLevel < defaultLevel || vibranceWindowsLevel > maxLevel)
                vibranceWindowsLevel = defaultLevel;

            try
            {
                var reader = System.Xml.XmlReader.Create(fileNameApplicationSettings);
                XmlSerializer serializer = new XmlSerializer(typeof(List<NvidiaApplicationSetting>));
                applicationSettings = (List<NvidiaApplicationSetting>)serializer.Deserialize(reader);
                reader.Close();
            }
            catch (Exception)
            {
                applicationSettings = new List<NvidiaApplicationSetting>();
            }
        }

        private bool isFileExisting(string szFilename)
        {
            return File.Exists(szFilename);
        }
    }
}
