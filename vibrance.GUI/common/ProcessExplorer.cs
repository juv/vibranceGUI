using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Windows.Forms;

namespace vibrance.GUI.common
{
    public partial class ProcessExplorer : Form
    {
        public ProcessExplorer()
        {
            InitializeComponent();

            listView.Columns.Add("Programs", 130, HorizontalAlignment.Left);
            listView.Columns.Add("Full Path", 320, HorizontalAlignment.Left);
            listView.LargeImageList = iconList;
            listView.FullRowSelect = true;
            listView.View = View.Tile;
            listView.HeaderStyle = ColumnHeaderStyle.Nonclickable;

            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.RunWorkerAsync();
        }

        private void GetAllProcesses()
        {
            var activeProcceses = Process.GetProcesses();
            int activeApplicationCount = 0;
            foreach (Process process in activeProcceses)
            {
                if (process.MainWindowHandle != IntPtr.Zero)
                {
                    Console.WriteLine(String.Format("ProcessName: {0}\tMainWindowHandle {1}: ", process.ProcessName, process.MainWindowHandle));
                    string path = string.Empty;
                    try
                    {
                        path = GetPathFromProcessId(process);
                        if (path != string.Empty)
                        {

                            ProcessExplorerEntry processEntry = new ProcessExplorerEntry(path, Icon.ExtractAssociatedIcon(path), process);
                            backgroundWorker.ReportProgress(++activeApplicationCount, processEntry);
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Safely gets the executable path from the specified process.
        /// Process.MainModule.FileName crashes when called on a x64 process because vibranceGUI is running as x86 process.
        /// </summary>
        /// <param name="process">the process to read out the executable path of</param>
        /// <returns>the fully qualified executable path</returns>
        private string GetPathFromProcessId(Process process)
        {
            var query = "SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id;

            using (var searcher = new ManagementObjectSearcher(query))
                foreach (ManagementObject item in searcher.Get())
                {
                    object id = item["ProcessID"];
                    object path = item["ExecutablePath"];

                    if (path != null && id.ToString() == process.Id.ToString())
                    {
                        return path.ToString();
                    }
                }
            return string.Empty;
        }

        private void listView_DoubleClick(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 1)
            {
                Console.WriteLine(listView.SelectedItems[0].Text);
                Console.WriteLine(listView.SelectedItems[0].Tag);

            }
        }

        private void button_Click(object sender, EventArgs e)
        {
            listView.Items.Clear();
            backgroundWorker.RunWorkerAsync();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            GetAllProcesses();
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if(!(e.UserState is ProcessExplorerEntry))
            {
                return;
            }
            ProcessExplorerEntry processEntry = (ProcessExplorerEntry)e.UserState;
            iconList.Images.Add(processEntry.Icon);
            var listItem = new ListViewItem(processEntry.Process.ProcessName, iconList.Images.Count - 1);
            listItem.Tag = processEntry.Process;
            listItem.SubItems.Add(processEntry.Path);
            listView.Items.Add(listItem);
        }
    }

    class ProcessExplorerEntry
    {
        public string Path { get; set; }

        public Icon Icon { get; set; }

        public Process Process { get; set; }

        public ProcessExplorerEntry(string path, Icon icon, Process process)
        {
            this.Path = path;
            this.Icon = icon;
            this.Process = process;
        }
    }
}
