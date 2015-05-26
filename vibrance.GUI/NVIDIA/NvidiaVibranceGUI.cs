using System.IO;
using gui.app.utils;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace vibrance.GUI
{
    public partial class NvidiaVibranceGUI : Form
    {
        private NvidiaVibranceProxy v;
        private RegistryController registryController;
        private AutoResetEvent resetEvent;
        public bool silenced = false;
        private const string appName = "vibranceGUI";
        private const string twitterLink = "https://twitter.com/juvlarN";
        private const string paypalDonationLink = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=JDQFNKNNEW356";

        private bool allowVisible;


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
                this.notifyIcon.ShowBalloonTip(250);
                this.Hide();
            }
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int vibranceIngameLevel = NvidiaVibranceProxy.NVAPI_MAX_LEVEL, vibranceWindowsLevel = NvidiaVibranceProxy.NVAPI_DEFAULT_LEVEL, refreshRate = 5000;
            bool keepActive = false, affectPrimaryMonitorOnly = false;

            while (!this.IsHandleCreated)
            {
                System.Threading.Thread.Sleep(500);
            }

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker) delegate
                {
                    readVibranceSettings(out vibranceIngameLevel, out vibranceWindowsLevel, out keepActive, out refreshRate, out affectPrimaryMonitorOnly);
                });
            }
            else
            {
                readVibranceSettings(out vibranceIngameLevel, out vibranceWindowsLevel, out keepActive, out refreshRate, out affectPrimaryMonitorOnly);
            }

            v = new NvidiaVibranceProxy(silenced);
            if (v.vibranceInfo.isInitialized)
            {
                backgroundWorker.ReportProgress(1);

                setGuiEnabledFlag(true);

                v.setShouldRun(true);
                v.setKeepActive(keepActive);
                v.setVibranceIngameLevel(vibranceIngameLevel);
                v.setVibranceWindowsLevel(vibranceWindowsLevel);
                v.setSleepInterval(refreshRate);
                v.setAffectPrimaryMonitorOnly(affectPrimaryMonitorOnly);
                v.handleDVC();
                bool unload = v.unloadLibraryEx();

                backgroundWorker.ReportProgress(2, unload);
                resetEvent.Set();
                Application.DoEvents();
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (v != null && v.vibranceInfo.isInitialized)
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
            bool keepActive = false, affectPrimaryMonitorOnly = false;
            this.Invoke((MethodInvoker)delegate
            {
                ingameLevel = trackBarIngameLevel.Value;
                windowsLevel = trackBarWindowsLevel.Value;
                keepActive = checkBoxKeepActive.Checked;
                refreshRate = int.Parse(textBoxRefreshRate.Text);
                affectPrimaryMonitorOnly = checkBoxPrimaryMonitorOnly.Checked;
            });
            saveVibranceSettings(ingameLevel, windowsLevel, keepActive, refreshRate, affectPrimaryMonitorOnly);
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
            allowVisible = true;
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
                    if (v != null)
                    {
                        v.setSleepInterval(refreshRate);
                        if (!settingsBackgroundWorker.IsBusy)
                        {
                            settingsBackgroundWorker.RunWorkerAsync();
                        }
                        listBoxLog.Items.Add("Refresh rate has been set to: " + refreshRate + " ms");
                    }
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
                    listBoxLog.Items.Add("VibranceGUI will only affect your primary monitor now.");
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
                        listBoxLog.Items.Add("Registered to Autostart!");
                    else
                        listBoxLog.Items.Add("Registering to Autostart failed!");
                }
                else if (!autostartController.isStartupPathUnchanged(appName, pathToExe))
                {
                    if(autostartController.registerProgram(appName, pathToExe))
                        listBoxLog.Items.Add("Updated Autostart Path!");
                    else
                        listBoxLog.Items.Add("Updating Autostart Path failed!");
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
                this.trackBarIngameLevel.Enabled = flag;
                this.textBoxRefreshRate.Enabled = flag;
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
                listBoxLog.Items.Add("Initiating observer thread exit... ");
                if (v != null && v.vibranceInfo.isInitialized)
                {
                    v.setShouldRun(false);
                    resetEvent.WaitOne();
                    listBoxLog.Items.Add("Unloading NVAPI... ");
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

        private void readVibranceSettings(out int vibranceIngameLevel, out int vibranceWindowsLevel, out bool keepActive, out int refreshRate, out bool affectPrimaryMonitorOnly)
        {
            registryController = new RegistryController();
            this.checkBoxAutostart.Checked = registryController.isProgramRegistered(appName);

            SettingsController settingsController = new SettingsController();
            settingsController.readVibranceSettings(GraphicsAdapter.NVIDIA, out vibranceIngameLevel, out vibranceWindowsLevel, out keepActive, out refreshRate, out affectPrimaryMonitorOnly);

            if (this.IsHandleCreated)
            {
                //no null check needed, SettingsController will always return matching values.
                labelWindowsLevel.Text = NvidiaSettingsWrapper.find(vibranceWindowsLevel).getPercentage;
                labelIngameLevel.Text = NvidiaSettingsWrapper.find(vibranceIngameLevel).getPercentage;

                trackBarWindowsLevel.Value = vibranceWindowsLevel;
                trackBarIngameLevel.Value = vibranceIngameLevel;
                checkBoxKeepActive.Checked = keepActive;
                textBoxRefreshRate.Text = refreshRate.ToString();
                checkBoxPrimaryMonitorOnly.Checked = affectPrimaryMonitorOnly;
            }
        }

        private void saveVibranceSettings(int ingameLevel, int windowsLevel, bool keepActive, int refreshRate, bool affectPrimaryMonitorOnly)
        {
            SettingsController settingsController = new SettingsController();

            settingsController.setVibranceSettings(
                ingameLevel.ToString(),
                windowsLevel.ToString(),
                keepActive.ToString(),
                refreshRate.ToString(),
                affectPrimaryMonitorOnly.ToString()
            );
        }

        private void buttonPaypal_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(NvidiaVibranceGUI.paypalDonationLink);
        }
    }
}