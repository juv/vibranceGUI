using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace vibrance.GUI
{
    public partial class vibranceGUI : Form
    {
        private VibranceProxy v;
        private RegistryController registryController;
        private AutoResetEvent resetEvent;
        public bool silenced = false;
        private const string appName = "vibranceGUI";
        private const string twitterLink = "https://twitter.com/juvlarN";

        public vibranceGUI()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            setGuiEnabledFlag(false);
            System.Runtime.InteropServices.Marshal.PrelinkAll(typeof(VibranceProxy));
            resetEvent = new AutoResetEvent(false);
            backgroundWorker.WorkerReportsProgress = true;
            settingsBackgroundWorker.WorkerReportsProgress = true;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.notifyIcon.Visible = true;
                this.notifyIcon.ShowBalloonTip(250);
                this.Hide();
            }
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int vibranceIngameLevel = VibranceProxy.NVAPI_MAX_LEVEL, vibranceWindowsLevel = VibranceProxy.NVAPI_DEFAULT_LEVEL, refreshRate = 5000;
            bool keepActive = false, multipleMonitors = false;

            this.Invoke((MethodInvoker)delegate
            {
                readVibranceSettings(out vibranceIngameLevel, out vibranceWindowsLevel, out keepActive, out refreshRate, out multipleMonitors);
            });

            v = new VibranceProxy(multipleMonitors, silenced);
            if (v.vibranceInfo.isInitialized)
            {
                backgroundWorker.ReportProgress(1);

                setGuiEnabledFlag(true);

                v.setShouldRun(true);
                v.setKeepActive(keepActive);
                v.setVibranceIngameLevel(vibranceIngameLevel);
                v.setVibranceWindowsLevel(vibranceWindowsLevel);
                v.setSleepInterval(refreshRate);
                v.handleDVC();
                bool unload = v.unloadLibraryEx();

                backgroundWorker.ReportProgress(2, unload);
                resetEvent.Set();
                Application.DoEvents();
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            backgroundWorker.RunWorkerAsync();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            cleanUp();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void trackBarIngameLevel_Scroll(object sender, EventArgs e)
        {
            NvidiaSettingsWrapper setting = NvidiaSettingsWrapper.find(trackBarIngameLevel.Value);
            if (setting == null)
                return;
            v.setVibranceIngameLevel(trackBarIngameLevel.Value);
            labelIngameLevel.Text = setting.getPercentage;
            if (!settingsBackgroundWorker.IsBusy)
            {
                settingsBackgroundWorker.RunWorkerAsync();
            }
        }

        private void trackBarWindowsLevel_Scroll(object sender, EventArgs e)
        {
            NvidiaSettingsWrapper setting = NvidiaSettingsWrapper.find(trackBarWindowsLevel.Value);
            if (setting == null)
                return;
            v.setVibranceWindowsLevel(trackBarWindowsLevel.Value);
            labelWindowsLevel.Text = setting.getPercentage;
            if (!settingsBackgroundWorker.IsBusy)
            {
                settingsBackgroundWorker.RunWorkerAsync();
            }
        }

        private void settingsBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(1000);
            int ingameLevel = 0, windowsLevel = 0, refreshRate = 0;
            bool keepActive = false, multipleMonitors = false;
            this.Invoke((MethodInvoker)delegate
            {
                ingameLevel = trackBarIngameLevel.Value;
                windowsLevel = trackBarWindowsLevel.Value;
                keepActive = checkBoxKeepActive.Checked;
                refreshRate = int.Parse(textBoxRefreshRate.Text);
                multipleMonitors = checkBoxMonitors.Checked;
            });
            saveVibranceSettings(ingameLevel, windowsLevel, keepActive, refreshRate, multipleMonitors);
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 1)
            {
                listBoxLog.Items.Add("vibranceInfo.isInitialized: " + v.vibranceInfo.isInitialized);
                listBoxLog.Items.Add("vibranceInfo.szGpuName: " + v.vibranceInfo.szGpuName);
                listBoxLog.Items.Add("vibranceInfo.activeOutput: " + v.vibranceInfo.activeOutput);
                listBoxLog.Items.Add("vibranceInfo.defaultHandle: " + v.vibranceInfo.defaultHandle);

                listBoxLog.Items.Add("vibranceInfo.userVibranceSettingActive: " + v.vibranceInfo.userVibranceSettingActive);
                listBoxLog.Items.Add("vibranceInfo.userVibranceSettingDefault: " + v.vibranceInfo.userVibranceSettingDefault);
                listBoxLog.Items.Add("");
                listBoxLog.Items.Add("");
                this.statusLabel.Text = "Running!";
                this.statusLabel.ForeColor = Color.Green;
            }
            else if (e.ProgressPercentage == 2)
            {
                listBoxLog.Items.Add("NVAPI Unloaded: " + e.UserState);
            }
        }

        private void settingsBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            this.Show();

			this.WindowState = FormWindowState.Normal;
			this.Visible = true;

			this.Refresh();
			this.ShowInTaskbar = true;
        }

        private void textBoxRefreshRate_TextChanged(object sender, EventArgs e)
        {
            if (v != null)
            {
                int refreshRate = -1;

                if (!int.TryParse(textBoxRefreshRate.Text, out refreshRate))
                {
                    textBoxRefreshRate.Text = "5000";
                }
                else if (refreshRate > 200)
                {
                    v.setSleepInterval(refreshRate);
                    if (!settingsBackgroundWorker.IsBusy)
                    {
                        settingsBackgroundWorker.RunWorkerAsync();
                    }
                    listBoxLog.Items.Add("Refresh rate has been set to: " + refreshRate + " ms");
                }
                else
                {
                    listBoxLog.Items.Add("The refresh rate must be greater than 200 ms!");
                }
            }
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
                    listBoxLog.Items.Add("Vibrance stays at ingame level when tabbed out.");
            }
        }

        private void checkBoxMonitors_CheckedChanged(object sender, EventArgs e)
        {
            if (v != null)
            {
                v.adjustMultipleMonitorsSetting(checkBoxMonitors.Checked);
                if (!settingsBackgroundWorker.IsBusy)
                {
                    settingsBackgroundWorker.RunWorkerAsync();
                }
                if (checkBoxKeepActive.Checked)
                    listBoxLog.Items.Add("Reading monitor information..");
            }
        }

        private void checkBoxAutostart_CheckedChanged(object sender, EventArgs e)
        {
            RegistryController autostartController = new RegistryController();
            if (this.checkBoxAutostart.Checked)
            {
                if (!autostartController.isProgramRegistered(appName))
                {
                    if (autostartController.registerProgram(appName, "\"" + Application.ExecutablePath.ToString() + "\" -minimized"))
                        listBoxLog.Items.Add("Registered to Autostart!");
                    else
                        listBoxLog.Items.Add("Registering to Autostart failed!");       
                }
            }
            else
            {
                if (autostartController.unregisterProgram(appName))
                    listBoxLog.Items.Add("Unregistered from Autostart!");
                else
                    listBoxLog.Items.Add("Unregistering from Autostart failed!");
            }
        }

        private void twitterToolStripTextBox_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(vibranceGUI.twitterLink);
        }

        private void linkLabelTwitter_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(vibranceGUI.twitterLink);
        }

        private void setGuiEnabledFlag(bool flag)
        {
            this.Invoke((MethodInvoker)delegate
            {
                this.checkBoxKeepActive.Enabled = flag;
                this.trackBarWindowsLevel.Enabled = flag;
                this.trackBarIngameLevel.Enabled = flag;
                this.textBoxRefreshRate.Enabled = flag;
                this.checkBoxAutostart.Enabled = flag;
                this.checkBoxMonitors.Enabled = flag;
            });
        }

        private void cleanUp()
        {
            this.statusLabel.Text = "Closing...";
            this.statusLabel.ForeColor = Color.Red;
            this.Update();
            listBoxLog.Items.Add("Initiating observer thread exit... ");
            if (v != null && v.vibranceInfo.isInitialized)
            {
                v.setShouldRun(false);
                resetEvent.WaitOne();
                listBoxLog.Items.Add("Unloading NVAPI... ");
            }
        }

        private void readVibranceSettings(out int vibranceIngameLevel, out int vibranceWindowsLevel, out bool keepActive, out int refreshRate, out bool multipleMonitors)
        {
            registryController = new RegistryController();
            this.checkBoxAutostart.Checked = registryController.isProgramRegistered(appName);

            SettingsController settingsController = new SettingsController();
            settingsController.readVibranceSettings(out vibranceIngameLevel, out vibranceWindowsLevel, out keepActive, out refreshRate, out multipleMonitors);

            //no null check needed, SettingsController will always return matching values.
            labelWindowsLevel.Text = NvidiaSettingsWrapper.find(vibranceWindowsLevel).getPercentage;
            labelIngameLevel.Text = NvidiaSettingsWrapper.find(vibranceIngameLevel).getPercentage;

            trackBarWindowsLevel.Value = vibranceWindowsLevel;
            trackBarIngameLevel.Value = vibranceIngameLevel;
            checkBoxKeepActive.Checked = keepActive;
            checkBoxMonitors.Checked = multipleMonitors;
            textBoxRefreshRate.Text = refreshRate.ToString();
        }

        private void saveVibranceSettings(int ingameLevel, int windowsLevel, bool keepActive, int refreshRate, bool multipleMonitors)
        {
            SettingsController settingsController = new SettingsController();

            settingsController.setVibranceSettings(
                ingameLevel.ToString(),
                windowsLevel.ToString(),
                keepActive.ToString(),
                refreshRate.ToString(),
                multipleMonitors.ToString()
            );
        }
    }
}