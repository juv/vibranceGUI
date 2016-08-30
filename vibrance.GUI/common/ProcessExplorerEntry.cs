using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

namespace vibrance.GUI.common
{

    public class ProcessExplorerEntry
    {
        public string Path { get; set; }

        public Icon Icon { get; set; }

        public string ProcessName { get; set; }

        public ProcessExplorerEntry(string path, Icon icon, Process process)
        {
            this.Path = path;
            this.Icon = icon;
            this.ProcessName = process.ProcessName;
        }

        public ProcessExplorerEntry(string path, Icon icon, string processName)
        {
            this.Path = path;
            this.Icon = icon;
            this.ProcessName = processName;
        }
    }
}
