using System;
using System.Diagnostics;

namespace vibrance.GUI.common
{
    class WinEventHookEventArgs : EventArgs
    {
        public uint ProcessId { get; set; }
        public Process Process { get; set; }
        public string WindowText { get; set; }
        public string ProcessName { get; set; }
        public string MainWindowTitle { get; set; }
        public IntPtr Handle { get; set; }
    }
}
