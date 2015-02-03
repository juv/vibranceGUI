using Microsoft.Win32;
using System;

namespace vibrance.GUI
{
    class RegistryController : IRegistryController
    {
        private const string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private const string vcKey = "SOFTWARE\\Microsoft\\DevDiv\\VC\\Servicing\\11.0\\RuntimeMinimum";

        private RegistryKey startupKey;

        public RegistryController()
        {

        }

        public bool registerProgram(string appName, string pathToExe)
        {
            try
            {
                startupKey = Registry.CurrentUser.OpenSubKey(runKey, true);
                if (startupKey == null)
                    return false;
                startupKey.SetValue(appName, pathToExe);
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                startupKey.Close();
            }
            return true;
        }

        public bool unregisterProgram(string appName)
        {
            try
            {
                startupKey = Registry.CurrentUser.OpenSubKey(runKey, true);
                startupKey.DeleteValue(appName, true);
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                startupKey.Close();
            }
            return true;
        }

        public bool isProgramRegistered(string appName)
        {
            try
            {
                startupKey = Registry.CurrentUser.OpenSubKey(runKey, true);
                if (startupKey.GetValue(appName) != null)
                    return true;
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

        public bool isStartupPathUnchanged(string appName, string pathToExe)
        {
            try
            {
                startupKey = Registry.CurrentUser.OpenSubKey(runKey);
                if (startupKey == null)
                {
                    return false;
                }

                string startUpValue = startupKey.GetValue(appName).ToString();
                if (startUpValue == pathToExe)
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                startupKey.Close();
            }
        }
    }
}
