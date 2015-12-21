using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using gui.app.utils;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using vibrance.GUI.common;
using vibrance.GUI.NVIDIA;
using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.Forms.MessageBox;

namespace vibrance.GUI
{
    public partial class NvidiaVibranceGUI : Form
    {
        private IVibranceProxy v;
        private IRegistryController registryController;
        private AutoResetEvent resetEvent;
        public bool silenced = false;
        private const string appName = "vibranceGUI";
        private const string twitterLink = "https://twitter.com/juvlarN";
        private const string paypalDonationLink = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=JDQFNKNNEW356";

        private bool allowVisible;

        List<NvidiaApplicationSetting> applicationSettings;


        public NvidiaVibranceGUI()
        {
            const string nvidiaAdapterName = "vibranceDLL.dll";
            string resourceName = string.Format("{0}.NVIDIA.{1}", typeof(Program).Namespace, nvidiaAdapterName);

            string dllPath = CommonUtils.LoadUnmanagedLibraryFromResource(
                Assembly.GetExecutingAssembly(),
                resourceName,
                nvidiaAdapterName);

            allowVisible = true;
            InitializeComponent();

            System.Runtime.InteropServices.Marshal.PrelinkAll(typeof(NvidiaVibranceProxy));

            applicationSettings = new List<NvidiaApplicationSetting>();
            v = new NvidiaDynamicVibranceProxy(ref applicationSettings);

            resetEvent = new AutoResetEvent(false);
            backgroundWorker.WorkerReportsProgress = true;
            settingsBackgroundWorker.WorkerReportsProgress = true;

            backgroundWorker.RunWorkerAsync();
        }

        protected override void SetVisibleCore(bool value)
        {
            if (!allowVisible)
            {
                value = false;
                if (!this.IsHandleCreated) CreateHandle();
            }
            base.SetVisibleCore(value);
        }

        public void SetAllowVisible(bool value)
        {
            allowVisible = value;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            setGuiEnabledFlag(false);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.notifyIcon.Visible = true;
                this.notifyIcon.BalloonTipText = "Running minimized... Like the program? Consider donating!";
                this.notifyIcon.ShowBalloonTip(250);
                this.Hide();
            }
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int vibranceWindowsLevel = NvidiaVibranceProxy.NVAPI_DEFAULT_LEVEL;
            bool keepActive = false, affectPrimaryMonitorOnly = false;

            while (!this.IsHandleCreated)
            {
                System.Threading.Thread.Sleep(500);
            }

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    readVibranceSettings(out vibranceWindowsLevel, out keepActive, out affectPrimaryMonitorOnly);
                });
            }
            else
            {
                readVibranceSettings(out vibranceWindowsLevel, out keepActive, out affectPrimaryMonitorOnly);
            }

            if (v.getVibranceInfo().isInitialized)
            {
                backgroundWorker.ReportProgress(1);

                setGuiEnabledFlag(true);

                v.setApplicationSettings(ref applicationSettings);
                v.setShouldRun(true);
                v.setKeepActive(keepActive);
                v.setVibranceWindowsLevel(vibranceWindowsLevel);
                v.setAffectPrimaryMonitorOnly(affectPrimaryMonitorOnly);
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (v != null && v.getVibranceInfo().isInitialized)
            {
                setGuiEnabledFlag(true);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            cleanUp();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void trackBarWindowsLevel_Scroll(object sender, EventArgs e)
        {
            NvidiaVibranceValueWrapper vibranceValue = NvidiaVibranceValueWrapper.find(trackBarWindowsLevel.Value);
            if (vibranceValue == null)
                return;
            v.setVibranceWindowsLevel(trackBarWindowsLevel.Value);
            labelWindowsLevel.Text = vibranceValue.getPercentage;
            if (!settingsBackgroundWorker.IsBusy)
            {
                settingsBackgroundWorker.RunWorkerAsync();
            }
        }

        private void settingsBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(5000);
            int windowsLevel = 0;
            bool keepActive = false, affectPrimaryMonitorOnly = false;
            this.Invoke((MethodInvoker)delegate
            {
                windowsLevel = trackBarWindowsLevel.Value;
                keepActive = checkBoxKeepActive.Checked;
                affectPrimaryMonitorOnly = checkBoxPrimaryMonitorOnly.Checked;
            });
            saveVibranceSettings(windowsLevel, keepActive, affectPrimaryMonitorOnly);
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 1)
            {
                this.statusLabel.Text = "Running!";
                this.statusLabel.ForeColor = Color.Green;
            }
            else if (e.ProgressPercentage == 2)
            {
                this.statusLabel.Text = "NVAPI Unloaded: " + e.UserState;
            }
        }

        private void settingsBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            allowVisible = true;
            this.Show();

            this.WindowState = FormWindowState.Normal;
            this.Visible = true;

            this.Refresh();
            this.ShowInTaskbar = true;
        }

        private void checkBoxKeepActive_CheckedChanged(object sender, EventArgs e)
        {
            if (v != null)
            {
                v.setKeepActive(checkBoxKeepActive.Checked);
                if (!settingsBackgroundWorker.IsBusy)
                {
                    settingsBackgroundWorker.RunWorkerAsync();
                }
                if (checkBoxKeepActive.Checked)
                {
                    notifyIcon.BalloonTipText = "Vibrance stays at ingame level when tabbed out.";
                    notifyIcon.ShowBalloonTip(250);
                }
            }
        }

        private void checkBoxPrimaryMonitorOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (v != null)
            {
                v.setAffectPrimaryMonitorOnly(checkBoxPrimaryMonitorOnly.Checked);
                if (!settingsBackgroundWorker.IsBusy)
                {
                    settingsBackgroundWorker.RunWorkerAsync();
                }
                if (checkBoxPrimaryMonitorOnly.Checked)
                {
                    notifyIcon.BalloonTipText = "vibranceGUI will only affect your primary monitor now.";
                    notifyIcon.ShowBalloonTip(250);
                }
            }
        }

        private void checkBoxAutostart_CheckedChanged(object sender, EventArgs e)
        {
            RegistryController autostartController = new RegistryController();
            if (this.checkBoxAutostart.Checked)
            {
                string pathToExe = "\"" + Application.ExecutablePath.ToString() + "\" -minimized";
                if (!autostartController.isProgramRegistered(appName))
                {
                    if (autostartController.registerProgram(appName, pathToExe))
                    {
                        notifyIcon.BalloonTipText = "Registered to Autostart!";
                    }
                    else
                    {
                        notifyIcon.BalloonTipText = "Registering to Autostart failed!";
                    }
                    notifyIcon.ShowBalloonTip(250);
                }
                else if (!autostartController.isStartupPathUnchanged(appName, pathToExe))
                {
                    if (autostartController.registerProgram(appName, pathToExe))
                        notifyIcon.BalloonTipText = "Updated Autostart Path!";
                    else
                        notifyIcon.BalloonTipText = "Updating Autostart Path failed!";
                    notifyIcon.ShowBalloonTip(250);
                }
            }
            else
            {
                if (autostartController.unregisterProgram(appName))
                    notifyIcon.BalloonTipText = "Unregistered from Autostart!";
                else
                    notifyIcon.BalloonTipText = "Unregistering from Autostart failed!";
                notifyIcon.ShowBalloonTip(250);
            }

        }

        private void twitterToolStripTextBox_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(NvidiaVibranceGUI.twitterLink);
        }

        private void linkLabelTwitter_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(NvidiaVibranceGUI.twitterLink);
        }

        private void setGuiEnabledFlag(bool flag)
        {
            this.Invoke((MethodInvoker)delegate
            {
                this.checkBoxKeepActive.Enabled = flag;
                this.trackBarWindowsLevel.Enabled = flag;
                this.checkBoxAutostart.Enabled = flag;
                this.checkBoxPrimaryMonitorOnly.Enabled = flag;
                //this.checkBoxMonitors.Enabled = flag;
            });
        }

        private void cleanUp()
        {
            try
            {
                this.statusLabel.Text = "Closing...";
                this.statusLabel.ForeColor = Color.Red;
                this.Update();
                if (v != null && v.getVibranceInfo().isInitialized)
                {
                    v.setShouldRun(false);
                    bool unload = v.unloadLibraryEx();
                }
            }
            catch (Exception ex)
            {
                Log(ex);
            }
        }

        public static void Log(Exception ex)
        {
            using (StreamWriter w = File.AppendText("vibranceGUI_log.txt"))
            {
                w.Write("\r\nLog Entry : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine("Exception Found:\nType: {0}", ex.GetType().FullName);
                w.WriteLine("Message: {0}", ex.Message);
                w.WriteLine("Source: {0}", ex.Source);
                w.WriteLine("Stacktrace: {0}", ex.StackTrace);
                w.WriteLine("Exception String: {0}", ex.ToString());

                w.WriteLine("-------------------------------");
            }
        }

        public static void Log(String msg)
        {
            using (StreamWriter w = File.AppendText("vibranceGUI_log.txt"))
            {
                w.Write("\r\nLog Entry : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine(msg);
                w.WriteLine("-------------------------------");
            }
        }

        private void readVibranceSettings(out int vibranceWindowsLevel, out bool keepActive, out bool affectPrimaryMonitorOnly)
        {
            registryController = new RegistryController();
            this.checkBoxAutostart.Checked = registryController.isProgramRegistered(appName);

            SettingsController settingsController = new SettingsController();
            settingsController.readVibranceSettings(GraphicsAdapter.NVIDIA, out vibranceWindowsLevel, out keepActive, out affectPrimaryMonitorOnly, out applicationSettings);

            if (this.IsHandleCreated)
            {
                //no null check needed, SettingsController will always return matching values.
                labelWindowsLevel.Text = NvidiaVibranceValueWrapper.find(vibranceWindowsLevel).getPercentage;

                trackBarWindowsLevel.Value = vibranceWindowsLevel;
                checkBoxKeepActive.Checked = keepActive;
                checkBoxPrimaryMonitorOnly.Checked = affectPrimaryMonitorOnly;
                foreach (NvidiaApplicationSetting application in applicationSettings)
                {
                    if (!File.Exists(application.FileName))
                        continue;
                    if (this.listApplications.LargeImageList == null)
                    {
                        ImageList imageList = new ImageList();
                        imageList.ImageSize = new Size(48, 48);
                        imageList.ColorDepth = ColorDepth.Depth32Bit;
                        this.listApplications.LargeImageList = imageList;
                        ListViewItem_SetSpacing(this.listApplications, 48 + 24, 48 + 6 + 16);
                    }

                    Icon icon = Icon.ExtractAssociatedIcon(application.FileName);
                    if (icon != null)
                    {
                        this.listApplications.LargeImageList.Images.Add(icon);
                        ListViewItem lvi = new ListViewItem(application.Name);
                        lvi.ImageIndex = this.listApplications.Items.Count;
                        lvi.Tag = application.FileName;
                        this.listApplications.Items.Add(lvi);
                    }
                }
            }
        }

        private void saveVibranceSettings(int windowsLevel, bool keepActive, bool affectPrimaryMonitorOnly)
        {
            SettingsController settingsController = new SettingsController();

            settingsController.setVibranceSettings(
                windowsLevel.ToString(),
                keepActive.ToString(),
                affectPrimaryMonitorOnly.ToString(),
                applicationSettings
            );
        }

        private void buttonPaypal_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(NvidiaVibranceGUI.paypalDonationLink);
        }

        private void buttonAddProgram_Click(object sender, EventArgs e)
        {
            if (this.listApplications.LargeImageList == null)
            {
                ImageList imageList = new ImageList();
                imageList.ImageSize = new Size(48, 48);
                imageList.ColorDepth = ColorDepth.Depth32Bit;
                this.listApplications.LargeImageList = imageList;
                ListViewItem_SetSpacing(this.listApplications, 48 + 24, 48 + 6 + 16);
            }
            OpenFileDialog fileDialog = new OpenFileDialog();
            DialogResult result = fileDialog.ShowDialog();
            if (result == DialogResult.OK && fileDialog.CheckFileExists && fileDialog.SafeFileName != null) // Test result.
            {
                Icon icon = Icon.ExtractAssociatedIcon(fileDialog.FileName);
                if (icon != null)
                {
                    this.listApplications.LargeImageList.Images.Add(icon);
                    ListViewItem lvi = new ListViewItem(Path.GetFileNameWithoutExtension(fileDialog.FileName));
                    lvi.ImageIndex = this.listApplications.Items.Count;
                    lvi.Tag = fileDialog.FileName;
                    this.listApplications.Items.Add(lvi);
                }
            }
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public int MakeLong(short lowPart, short highPart)
        {
            return (int)(((ushort)lowPart) | (uint)(highPart << 16));
        }

        public void ListViewItem_SetSpacing(ListView listview, short leftPadding, short topPadding)
        {
            const int LVM_FIRST = 0x1000;
            const int LVM_SETICONSPACING = LVM_FIRST + 53;
            SendMessage(listview.Handle, LVM_SETICONSPACING, IntPtr.Zero, (IntPtr)MakeLong(leftPadding, topPadding));
        }

        private void listApplications_DoubleClick(object sender, EventArgs e)
        {
            ListViewItem selectedItem = this.listApplications.SelectedItems[0];
            if (selectedItem != null)
            {
                NvidiaApplicationSetting actualSetting = applicationSettings.FirstOrDefault(x => x.FileName == selectedItem.Tag.ToString());
                VibranceSettings settingsWindow = new VibranceSettings(v, settingsBackgroundWorker, selectedItem, actualSetting);
                DialogResult result = settingsWindow.ShowDialog();
                if (result == DialogResult.OK)
                {
                    NvidiaApplicationSetting newSetting = settingsWindow.getApplicationSetting();
                    if (applicationSettings.FirstOrDefault(x => x.FileName == newSetting.FileName) != null)
                        applicationSettings.Remove(applicationSettings.First(x => x.FileName == newSetting.FileName));
                    applicationSettings.Add(newSetting);
                    if (!settingsBackgroundWorker.IsBusy)
                    {
                        settingsBackgroundWorker.RunWorkerAsync();
                    }
                }
            }
        }

        private void buttonRemoveProgram_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem eachItem in listApplications.SelectedItems)
            {
                for (int i = eachItem.Index + 1; i < listApplications.Items.Count; i++)
                    listApplications.Items[i].ImageIndex--;
                Image img = this.listApplications.LargeImageList.Images[eachItem.ImageIndex];
                this.listApplications.LargeImageList.Images.RemoveAt(eachItem.ImageIndex);
                img.Dispose();

                listApplications.Items.Remove(eachItem);

                applicationSettings.Remove(applicationSettings.FirstOrDefault(x => x.FileName.Equals(eachItem.Tag.ToString())));
            }

            if (!settingsBackgroundWorker.IsBusy)
            {
                settingsBackgroundWorker.RunWorkerAsync();
            }
        }
    }
}