using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace vibrance.GUI.common
{
    class VersionCheckHelper
    {
        /*private static readonly Uri url = new Uri("http://vibrancegui.com/update");
  
        public async static void PerformVersionCheck()
        {
            WebClient webClient = new WebClient();
            webClient.QueryString.Add("GUID", Guid.NewGuid().ToString());
            webClient.DownloadStringCompleted += WebClient_DownloadStringCompleted;
            webClient.DownloadStringAsync(url);
        }

        private static void WebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            Version retrievedVersion = new Version(e.Result.Trim());
            if(retrievedVersion != null && NewVersionAvailable(retrievedVersion))
            {
                
            }
        }

        private static bool NewVersionAvailable(Version retrievedVersion)
        {
            Assembly assem = Assembly.GetEntryAssembly();
            AssemblyName assemName = assem.GetName();
            Version currentVersion = assemName.Version;

            //is currentVersion lower than the retrievedVersion?
            if(currentVersion.CompareTo(retrievedVersion) == -1)
            {
                return true;
            }
            return false;
        }*/
    }
}
