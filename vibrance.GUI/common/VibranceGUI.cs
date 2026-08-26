using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.Forms.MessageBox;

namespace vibrance.GUI.common
{
    public partial class VibranceGUI : Form
    {
        private readonly int _defaultWindowsLevel;
        private readonly int _minTrackBarValue;
        private readonly int _maxTrackBarValue;
        private readonly int _defaultIngameValue;
        private readonly Func<int, string> _resolveLabelLevel;
        private readonly IVibranceProxy _v;
        private IRegistryController _registryController;
        private const string AppName = "vibranceGUI";
        private const string TwitterLink = "https://twitter.com/juvlarN";
        private const string PaypalDonationLink = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=JDQFNKNNEW356";

        private bool _allowVisible;
        private List<ApplicationSetting> _applicationSettings;
        private readonly List<ResolutionModeWrapper> _supportedResolutionList;
        private readonly Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>> _windowsResolutionSettings;

        public VibranceGUI(
            Func<List<ApplicationSetting>, Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>>, IVibranceProxy> getProxy, 
            int defaultWindowsLevel, 
            int minTrackBarValue,
            int maxTrackBarValue,
            int defaultIngameValue,
            Func<int, string> resolveLabelLevel)
        {
            _defaultWindowsLevel = defaultWindowsLevel;
            _minTrackBarValue = minTrackBarValue;
            _maxTrackBarValue = maxTrackBarValue;
            _defaultIngameValue = defaultIngameValue;
            _resolveLabelLevel = resolveLabelLevel;
            _allowVisible = true;

            InitializeComponent();

            trackBarWindowsLevel.Minimum = minTrackBarValue;
            trackBarWindowsLevel.Maximum = maxTrackBarValue;

            _windowsResolutionSettings = new Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>>();
            RebuildWindowsResolutionSettings(true);

            // _supportedResolutionList is readonly, so it can only be assigned from inside a
            // constructor body - not from a method the constructor merely calls, even a private
            // one - which is why this is pulled back out of RebuildWindowsResolutionSettings
            // (shared with the refresh path below) instead of living inside it. Equivalent to the
            // old "if (screen.Primary) { _supportedResolutionList = availableResolutions; }": if
            // the primary screen's own read failed, it never made it into the dictionary either,
            // and this is left null exactly as it was before.
            Screen primaryScreen = Screen.PrimaryScreen;
            Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>> primaryEntry;
            if (primaryScreen != null && _windowsResolutionSettings.TryGetValue(primaryScreen.DeviceName, out primaryEntry))
            {
                _supportedResolutionList = primaryEntry.Item2;
            }

            // Subscribed here - after _windowsResolutionSettings exists, before getProxy hands it
            // to the vendor proxy - and unsubscribed in CleanUp(). ResolutionChangeFailed lets the
            // resolution-change fix (see ResolutionHelper.ChangeResolutionEx) report a give-up
            // without ever showing a MessageBox from inside the WinEvent callback thread.
            // SystemEvents.DisplaySettingsChanged keeps _windowsResolutionSettings from going stale
            // when the user changes their desktop resolution directly in Windows - unsubscribing it
            // is mandatory (not just good practice): SystemEvents holds a strong reference to this
            // handler on its own dedicated thread, and leaving it subscribed leaks this form and can
            // fault at shutdown.
            ResolutionHelper.ResolutionChangeFailed += OnResolutionChangeFailed;
            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;

            _applicationSettings = new List<ApplicationSetting>();
            _v = getProxy(_applicationSettings, _windowsResolutionSettings);

            backgroundWorker.WorkerReportsProgress = true;
            settingsBackgroundWorker.WorkerReportsProgress = true;

            backgroundWorker.RunWorkerAsync();
        }

        protected override void SetVisibleCore(bool value)
        {
            if (!_allowVisible)
            {
                value = false;
                if (!this.IsHandleCreated)
                {
                    CreateHandle();
                }
            }
            base.SetVisibleCore(value);
        }

        public void SetAllowVisible(bool value)
        {
            _allowVisible = value;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetGuiEnabledFlag(false);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                //this.notifyIcon.Visible = true;
                //this.notifyIcon.BalloonTipText = "Running minimized... Like the program? Consider donating!";
                //this.notifyIcon.ShowBalloonTip(250);
                this.Hide();
            }
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int vibranceWindowsLevel = _defaultWindowsLevel;
            bool affectPrimaryMonitorOnly = false;
            bool neverSwitchResolution = false;

            while (!this.IsHandleCreated)
            {
                Thread.Sleep(500);
            }

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    ReadVibranceSettings(out vibranceWindowsLevel, out affectPrimaryMonitorOnly, out neverSwitchResolution);
                });
            }
            else
            {
                ReadVibranceSettings(out vibranceWindowsLevel, out affectPrimaryMonitorOnly, out neverSwitchResolution);
            }

            if (_v.GetVibranceInfo().isInitialized)
            {
                backgroundWorker.ReportProgress(1);

                SetGuiEnabledFlag(true);

                _v.SetApplicationSettings(_applicationSettings);
                _v.SetShouldRun(true);
                _v.SetVibranceWindowsLevel(vibranceWindowsLevel);
                _v.SetAffectPrimaryMonitorOnly(affectPrimaryMonitorOnly);
                _v.SetNeverSwitchResolution(neverSwitchResolution);
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (_v != null && _v.GetVibranceInfo().isInitialized)
            {
                SetGuiEnabledFlag(true);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            CleanUp();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void trackBarWindowsLevel_Scroll(object sender, EventArgs e)
        {
            _v.SetVibranceWindowsLevel(trackBarWindowsLevel.Value);
            labelWindowsLevel.Text = _resolveLabelLevel(trackBarWindowsLevel.Value);
            if (!settingsBackgroundWorker.IsBusy)
            {
                settingsBackgroundWorker.RunWorkerAsync();
            }
        }

        private void settingsBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(5000);
            ForceSaveVibranceSettings();
        }

        private void ForceSaveVibranceSettings()
        {
            int windowsLevel = 0;
            bool affectPrimaryMonitorOnly = false;
            bool neverSwitchResolution = false;
            this.Invoke((MethodInvoker)delegate
            {
                windowsLevel = trackBarWindowsLevel.Value;
                affectPrimaryMonitorOnly = checkBoxPrimaryMonitorOnly.Checked;
                neverSwitchResolution = checkBoxNeverChangeResolutions.Checked;
            });
            SaveVibranceSettings(windowsLevel, affectPrimaryMonitorOnly, neverSwitchResolution);
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
                this.statusLabel.Text = $"NVAPI Unloaded: {e.UserState}";
            }
        }

        private void settingsBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            _allowVisible = true;
            this.Show();

            this.WindowState = FormWindowState.Normal;
            this.Visible = true;

            this.Refresh();
            this.ShowInTaskbar = true;
        }

        private void checkBoxPrimaryMonitorOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (this._v == null)
            {
                return;
            }

            this._v.SetAffectPrimaryMonitorOnly(this.checkBoxPrimaryMonitorOnly.Checked);
            if (!this.settingsBackgroundWorker.IsBusy)
            {
                this.settingsBackgroundWorker.RunWorkerAsync();
            }
        }
        
        private void checkBoxNeverChangeResolutions_CheckedChanged(object sender, EventArgs e)
        {
            if (this._v == null)
            {
                return;
            }

            this._v.SetNeverSwitchResolution(this.checkBoxNeverChangeResolutions.Checked);
            if (!this.settingsBackgroundWorker.IsBusy)
            {
                this.settingsBackgroundWorker.RunWorkerAsync();
            }
        }

        private void checkBoxAutostart_CheckedChanged(object sender, EventArgs e)
        {
            RegistryController autostartController = new RegistryController();
            if (this.checkBoxAutostart.Checked)
            {
                string pathToExe = "\"" + Application.ExecutablePath + "\" -minimized";
                if (!autostartController.IsProgramRegistered(AppName))
                {
                    this.notifyIcon.BalloonTipText = autostartController.RegisterProgram(AppName, pathToExe) 
                        ? "Registered to Autostart!" 
                        : "Registering to Autostart failed!";
                }
                else if (!autostartController.IsStartupPathUnchanged(AppName, pathToExe))
                {
                    this.notifyIcon.BalloonTipText = autostartController.RegisterProgram(AppName, pathToExe)
                        ? "Updated Autostart Path!"
                        : "Updating Autostart Path failed!";
                }
                else
                {
                    return;
                }
            }
            else
            {
                this.notifyIcon.BalloonTipText = autostartController.UnregisterProgram(AppName) 
                    ? "Unregistered from Autostart!" 
                    : "Unregistering from Autostart failed!";
            }

            notifyIcon.ShowBalloonTip(250);
        }

        private void twitterToolStripTextBox_Click(object sender, EventArgs e)
        {
            Process.Start(TwitterLink);
        }

        private void linkLabelTwitter_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(TwitterLink);
        }

        private void SetGuiEnabledFlag(bool flag)
        {
            this.Invoke((MethodInvoker)delegate
            {
                this.trackBarWindowsLevel.Enabled = flag;
                this.checkBoxAutostart.Enabled = flag;
                this.checkBoxPrimaryMonitorOnly.Enabled = flag;
                this.buttonAddProgram.Enabled = flag;
                this.buttonProcessExplorer.Enabled = flag;
                this.buttonRemoveProgram.Enabled = flag;
            });
        }

        private void CleanUp()
        {
            try
            {
                this.statusLabel.Text = "Closing...";
                this.statusLabel.ForeColor = Color.Red;
                this.Update();
                if (_v != null && _v.GetVibranceInfo().isInitialized)
                {
                    _v.HandleDvcExit();
                    _v.SetShouldRun(false);
                    _v.UnloadLibraryEx();
                }
            }
            catch (Exception ex)
            {
                Log(ex);
            }
            finally
            {
                // In a finally, not just after the try: these must run even if the block above
                // throws. SystemEvents.DisplaySettingsChanged above all - see the ctor's own
                // comment for why leaving it subscribed leaks this form and can fault at shutdown.
                SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
                ResolutionHelper.ResolutionChangeFailed -= OnResolutionChangeFailed;
            }
        }

        /// <summary>
        /// (Re)populates _windowsResolutionSettings from the currently attached screens, mutating
        /// the existing Dictionary instance in place (Clear() then re-add) rather than replacing
        /// it - both proxies hold a reference to this very instance (NVIDIA's is static), so only
        /// in-place mutation is visible to them. Shared by the constructor (showFailureDialog:
        /// true) and OnDisplaySettingsChanged below (showFailureDialog: false) - see that method
        /// for why the dialog must never fire from the refresh path.
        /// </summary>
        private void RebuildWindowsResolutionSettings(bool showFailureDialog)
        {
            // This is the single most dangerous line in the resolution-change fix: if a refresh
            // runs while a game's resolution change is currently applied, a live read of "the
            // current mode" for the game's own screen returns the GAME's mode, not the desktop's.
            // Overwriting the captured "Windows resolution" (Item1) with that would strand the
            // desktop at the game's resolution forever - the revert path compares against Item1, so
            // once it has silently become the game's own mode, "reverting" turns into a no-op that
            // still reports success. A game going fullscreen is exactly the kind of change that
            // fires DisplaySettingsChanged, so this is not a rare interleaving to guard against.
            //
            // While a resolution change is applied, every screen this dictionary already has an
            // entry for keeps its previously captured Item1 untouched, and only Item2 (the
            // device's supported-mode list, a property of the device rather than of whichever mode
            // happens to be active right now) is refreshed. A screen with no previous entry still
            // needs one captured fresh - it cannot be the screen the game is running on, since that
            // one is already recorded.
            bool preserveCapturedMode = _v != null && _v.GetVibranceInfo().isResolutionChangeApplied;

            Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>> previous =
                new Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>>(_windowsResolutionSettings);

            _windowsResolutionSettings.Clear();
            foreach (Screen screen in Screen.AllScreens)
            {
                Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>> existing;
                bool hasExisting = previous.TryGetValue(screen.DeviceName, out existing);

                // Item2 is a property of the device's capability, not of whichever mode happens to
                // be active right now (the comment above already relies on that to justify reusing
                // it while a game's own change is applied) - so a device this dictionary already
                // has an entry for reuses that SAME List<ResolutionModeWrapper> instance rather than
                // re-enumerating. Two reasons this matters beyond the obvious P/Invoke cost (up to
                // several hundred EnumDisplaySettings calls per screen, on the UI thread): first,
                // vibranceGUI's OWN resolution changes also fire DisplaySettingsChanged, so an
                // unconditional re-enumerate here would run twice per alt-tab cycle; second, reusing
                // the identical instance (not a fresh copy) is what keeps _supportedResolutionList -
                // captured once, in the constructor, and readonly - from silently going stale after
                // a refresh, since it then still points at the very list being kept up to date here.
                List<ResolutionModeWrapper> availableResolutions = hasExisting
                    ? existing.Item2
                    : ResolutionHelper.EnumerateSupportedResolutionModes(screen.DeviceName);

                if (preserveCapturedMode && hasExisting)
                {
                    _windowsResolutionSettings.Add(screen.DeviceName,
                        new Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>(existing.Item1, availableResolutions));
                    continue;
                }

                Devmode currentResolutionMode;
                if (ResolutionHelper.GetCurrentResolutionSettings(out currentResolutionMode, screen.DeviceName))
                {
                    _windowsResolutionSettings.Add(screen.DeviceName,
                        new Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>(new ResolutionModeWrapper(currentResolutionMode), availableResolutions));
                }
                else if (showFailureDialog)
                {
                    MessageBox.Show("Current resolution mode could not be determined. Switching back to your Windows resolution will not work.");
                }
            }
        }

        /// <summary>
        /// Keeps _windowsResolutionSettings current when the user changes their desktop resolution
        /// (or a monitor is hot-plugged) outside of vibranceGUI itself - without this, the revert
        /// path drags the desktop back to whatever mode was active at startup, with every API call
        /// still reporting success.
        /// </summary>
        private void OnDisplaySettingsChanged(object sender, EventArgs e)
        {
            // Raised on SystemEvents' own dedicated thread, not necessarily the UI thread that owns
            // this form and that OnWinEventHook's callbacks arrive through - marshal onto it before
            // touching _windowsResolutionSettings, which that hook handler reads with no locking of
            // its own. InvokeRequired is NOT sufficient on its own: it returns false whenever the
            // form has no window handle yet (Control.InvokeRequired falls through to
            // FindMarshalingControl(), which returns false if !IsHandleCreated) - and the handle
            // genuinely does not exist for the whole span of the constructor's NvAPI/ADL
            // initialisation after this handler is subscribed (backgroundWorker_DoWork busy-waits
            // on !IsHandleCreated), which is exactly when a SystemEvents notification is likely at
            // autostart, as monitors settle. Without the explicit IsHandleCreated check this method
            // would run its dictionary mutation directly on the SystemEvents thread in that window.
            // IsDisposed also guards the symmetric case at shutdown: CleanUp()'s unsubscribe cannot
            // cover a notification already in flight, which could otherwise find the handle
            // destroyed (same wrong-thread mutation) or the form disposed (BeginInvoke throwing
            // ObjectDisposedException on the SystemEvents thread, with nothing there to catch it).
            if (this.IsDisposed || !this.IsHandleCreated)
            {
                return;
            }
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate { OnDisplaySettingsChanged(sender, e); });
                return;
            }

            // showFailureDialog: false - a MessageBox popping up on every hot-plug or resolution
            // change, potentially over a fullscreen game, is exactly the modal-on-the-callback-
            // thread mistake this whole fix removes. The constructor's own one-time build above
            // still shows it once, at startup, where the user is looking at the window and can act
            // on it immediately.
            RebuildWindowsResolutionSettings(false);
        }

        /// <summary>
        /// Reports a resolution change ChangeResolutionEx has given up on, via a balloon tip instead
        /// of the modal MessageBox the pre-fix code raised from inside the WinEvent callback thread
        /// (see ResolutionHelper.cs). ResolutionHelper only ever raises this once it has given up -
        /// never on a single transient failure - so there is no "still retrying" wording here; see
        /// ResolutionHelper.RecordFailure.
        /// </summary>
        private void OnResolutionChangeFailed(object sender, ResolutionFailureEventArgs e)
        {
            // Same reasoning as OnDisplaySettingsChanged above - this is also raised from
            // ResolutionHelper's own call stack, which for the WinEvent-driven cases below runs on
            // the UI thread already, but ResolutionHelper offers no guarantee of that in general.
            if (this.IsDisposed || !this.IsHandleCreated)
            {
                return;
            }
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate { OnResolutionChangeFailed(sender, e); });
                return;
            }

            this.notifyIcon.BalloonTipIcon = ToolTipIcon.Warning;
            // The desktop is now stuck at the game's resolution (revert) or the game never got its
            // requested resolution (apply) with nothing else in the program that will ever retry
            // it, so this has to name the device and, for a revert, point the user at where they
            // can fix it themselves.
            this.notifyIcon.BalloonTipText = e.IsRevert
                ? string.Format("vibranceGUI could not switch display {0} back to your Windows resolution and has stopped trying. Check Windows Display settings.", e.DeviceName)
                : string.Format("vibranceGUI could not change display {0} to this game's resolution and has stopped trying.", e.DeviceName);
            this.notifyIcon.ShowBalloonTip(250);
        }

        public static void Log(Exception ex)
        {
            using (StreamWriter w = File.AppendText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vibranceGUI.log")))
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

        public static void Log(string msg)
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

        private void ReadVibranceSettings(out int vibranceWindowsLevel, out bool affectPrimaryMonitorOnly, out bool neverSwitchResolution)
        {
            _registryController = new RegistryController();
            this.checkBoxAutostart.Checked = _registryController.IsProgramRegistered(AppName);

            SettingsController settingsController = new SettingsController();
            settingsController.ReadVibranceSettings(_v.GraphicsAdapter, out vibranceWindowsLevel, out affectPrimaryMonitorOnly, out neverSwitchResolution, out _applicationSettings);

            if (this.IsHandleCreated)
            {
                //no null check needed, SettingsController will always return matching values.
                labelWindowsLevel.Text = _resolveLabelLevel(vibranceWindowsLevel);

                trackBarWindowsLevel.Value = vibranceWindowsLevel;
                checkBoxPrimaryMonitorOnly.Checked = affectPrimaryMonitorOnly;
                checkBoxNeverChangeResolutions.Checked = neverSwitchResolution;
                foreach (ApplicationSetting application in _applicationSettings.ToList())
                {
                    if (!File.Exists(application.FileName))
                    {
                        _applicationSettings.Remove(application);
                        continue;
                    }                        

                    InitializeApplicationList();

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

        private void SaveVibranceSettings(int windowsLevel, bool affectPrimaryMonitorOnly, bool neverSwitchResolution)
        {
            SettingsController settingsController = new SettingsController();

            settingsController.SetVibranceSettings(
                windowsLevel.ToString(),
                affectPrimaryMonitorOnly.ToString(),
                neverSwitchResolution.ToString(),
                _applicationSettings
            );
        }

        private void buttonPaypal_Click(object sender, EventArgs e)
        {
            Process.Start(PaypalDonationLink);
        }

        private void buttonAddProgram_Click(object sender, EventArgs e)
        {
            InitializeApplicationList();

            OpenFileDialog fileDialog = new OpenFileDialog();
            DialogResult result = fileDialog.ShowDialog();
            if (result == DialogResult.OK && fileDialog.CheckFileExists && fileDialog.SafeFileName != null 
                && _applicationSettings.FirstOrDefault(x => x.FileName.ToLower() == fileDialog.FileName.ToLower()) == null)
            {
                Icon icon = Icon.ExtractAssociatedIcon(fileDialog.FileName);
                if (icon != null)
                {
                    ProcessExplorerEntry processExplorerEntry = new ProcessExplorerEntry(fileDialog.FileName, icon, Path.GetFileNameWithoutExtension(fileDialog.FileName));
                    AddProgramIntern(processExplorerEntry);
                }
            }
        }

        public void AddProgramExtern(ProcessExplorerEntry processExplorerEntry)
        {
            if(this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    AddProgramIntern(processExplorerEntry);
                });
            }
            else
            {
                AddProgramIntern(processExplorerEntry);
            }
        }

        private void AddProgramIntern(ProcessExplorerEntry processExplorerEntry)
        {
            InitializeApplicationList();
            
            if(!File.Exists(processExplorerEntry.Path) || _applicationSettings.FirstOrDefault(x => x.FileName.ToLower() == processExplorerEntry.Path.ToLower()) != null)
            {
                this.listApplications.SelectedIndices.Clear();
                return; 
            }

            Icon icon = processExplorerEntry.Icon;
            string path = processExplorerEntry.Path;
            if (icon != null)
            {
                this.listApplications.LargeImageList.Images.Add(icon);
                ListViewItem lvi = new ListViewItem(Path.GetFileNameWithoutExtension(path));
                lvi.ImageIndex = this.listApplications.Items.Count;
                lvi.Tag = path;
                this.listApplications.Items.Add(lvi);
                this.listApplications.SelectedIndices.Clear();
                lvi.Selected = true;
                listApplications_DoubleClick(this, EventArgs.Empty);
            }
        }

        private void InitializeApplicationList()
        {
            if (this.listApplications.LargeImageList == null)
            {
                ImageList imageList = new ImageList();
                imageList.ImageSize = new Size(48, 48);
                imageList.ColorDepth = ColorDepth.Depth32Bit;
                this.listApplications.LargeImageList = imageList;
                ListViewItem_SetSpacing(this.listApplications, 48 + 24, 48 + 6 + 16);
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
                ApplicationSetting actualSetting = _applicationSettings.FirstOrDefault(x => x.FileName == selectedItem.Tag.ToString());
                VibranceSettings settingsWindow = new VibranceSettings(_v, _minTrackBarValue, _maxTrackBarValue, _defaultIngameValue, selectedItem, actualSetting, _supportedResolutionList, _resolveLabelLevel);
                DialogResult result = settingsWindow.ShowDialog();
                if (result == DialogResult.OK)
                {
                    ApplicationSetting newSetting = settingsWindow.GetApplicationSetting();
                    if (_applicationSettings.FirstOrDefault(x => x.FileName == newSetting.FileName) != null)
                    {
                        _applicationSettings.Remove(_applicationSettings.First(x => x.FileName == newSetting.FileName));
                    }
                    _applicationSettings.Add(newSetting);
                    ForceSaveVibranceSettings();
                }
                else if(actualSetting == null)
                {
                    removeApplicationListItem(selectedItem);
                }
            }
        }

        private void buttonRemoveProgram_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem eachItem in listApplications.SelectedItems)
            {
                for (int i = eachItem.Index + 1; i < listApplications.Items.Count; i++)
                    listApplications.Items[i].ImageIndex--;

                removeApplicationListItem(eachItem);
                _applicationSettings.Remove(_applicationSettings.FirstOrDefault(x => x.FileName.Equals(eachItem.Tag.ToString())));
            }

            ForceSaveVibranceSettings();
        }

        private void removeApplicationListItem(ListViewItem item)
        {
            Image img = this.listApplications.LargeImageList.Images[item.ImageIndex];
            this.listApplications.LargeImageList.Images.RemoveAt(item.ImageIndex);
            img.Dispose();
            this.listApplications.Items.Remove(item);
        }

        private void buttonProcessExplorer_Click(object sender, EventArgs e)
        {
            ProcessExplorer ex = new ProcessExplorer(this);
            ex.Show();
        }
    }
}