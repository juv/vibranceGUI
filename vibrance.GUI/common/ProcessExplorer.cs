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

            backgroundWorker.RunWorkerAsync();
        }

        private void GetAllProcesses()
        {
            var activeProcceses = Process.GetProcesses();
            foreach(Process process in activeProcceses)
            {
                if(process.MainWindowHandle != IntPtr.Zero)
                {
                    Console.WriteLine(String.Format("ProcessName: {0}\tMainWindowHandle {1}: ", process.ProcessName, process.MainWindowHandle));
                    string path = string.Empty; 
                    try
                    {
                        path = GetPathFromProcessId(process);
                        if (path != string.Empty)
                        {
                            iconList.Images.Add(Icon.ExtractAssociatedIcon(path));
                            var listItem = new ListViewItem(process.ProcessName, iconList.Images.Count - 1);
                            listItem.Tag = process;
                            listItem.SubItems.Add(path);
                            listView.Items.Add(listItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
        }

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
            if(listView.SelectedItems.Count == 1)
            {
                Console.WriteLine(listView.SelectedItems[0].Text);
                Console.WriteLine(listView.SelectedItems[0].Tag);

            }
        }

        private void button_Click(object sender, EventArgs e)
        {
            listView.Items.Clear();
            GetAllProcesses();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if(this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    GetAllProcesses();
                });
            }
            else
            {
                GetAllProcesses();
            }
        }
    }
}
