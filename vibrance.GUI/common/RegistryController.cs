using System;
using Microsoft.Win32;

namespace vibrance.GUI.common
{
    class RegistryController : IRegistryController
    {
        private const string RunKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

        private RegistryKey _startupKey;

        public RegistryController()
        {

        }

        public bool RegisterProgram(string appName, string pathToExe)
        {
            try
            {
                _startupKey = Registry.CurrentUser.OpenSubKey(RunKey, true);
                if (_startupKey == null)
                    return false;
                _startupKey.SetValue(appName, pathToExe);
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                _startupKey.Close();
            }
            return true;
        }

        public bool UnregisterProgram(string appName)
        {
            try
            {
                _startupKey = Registry.CurrentUser.OpenSubKey(RunKey, true);
                _startupKey.DeleteValue(appName, true);
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                _startupKey.Close();
            }
            return true;
        }

        public bool IsProgramRegistered(string appName)
        {
            try
            {
                _startupKey = Registry.CurrentUser.OpenSubKey(RunKey, true);
                if (_startupKey.GetValue(appName) != null)
                    return true;
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

        public bool IsStartupPathUnchanged(string appName, string pathToExe)
        {
            try
            {
                _startupKey = Registry.CurrentUser.OpenSubKey(RunKey);
                if (_startupKey == null)
                {
                    return false;
                }

                string startUpValue = _startupKey.GetValue(appName).ToString();
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
                _startupKey.Close();
            }
        }
    }
}
