namespace vibrance.GUI
{
    partial class vibranceGUI
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(vibranceGUI));
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.twitterToolStripTextBox = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.checkBoxAutostart = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxMonitors = new System.Windows.Forms.CheckBox();
            this.checkBoxKeepActive = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.labelRefreshRate = new System.Windows.Forms.Label();
            this.textBoxRefreshRate = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.labelWindowsLevel = new System.Windows.Forms.Label();
            this.trackBarWindowsLevel = new System.Windows.Forms.TrackBar();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.trackBarIngameLevel = new System.Windows.Forms.TrackBar();
            this.labelIngameLevel = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.observerStatusLabel = new System.Windows.Forms.Label();
            this.listBoxLog = new System.Windows.Forms.ListBox();
            this.twitterLabel = new System.Windows.Forms.Label();
            this.linkLabelTwitter = new System.Windows.Forms.LinkLabel();
            this.settingsBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.contextMenuStrip.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarWindowsLevel)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarIngameLevel)).BeginInit();
            this.SuspendLayout();
            // 
            // notifyIcon
            // 
            this.notifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon.BalloonTipText = "Running minimized!";
            this.notifyIcon.BalloonTipTitle = "Vibrance Control";
            this.notifyIcon.ContextMenuStrip = this.contextMenuStrip;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "Vibrance Control";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseClick);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.twitterToolStripTextBox,
            this.exitToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(217, 48);
            this.contextMenuStrip.Text = "Vibrance Control";
            // 
            // twitterToolStripTextBox
            // 
            this.twitterToolStripTextBox.Font = new System.Drawing.Font("Segoe UI", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Italic | System.Drawing.FontStyle.Underline))));
            this.twitterToolStripTextBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(6)))), ((int)(((byte)(69)))), ((int)(((byte)(173)))));
            this.twitterToolStripTextBox.Image = ((System.Drawing.Image)(resources.GetObject("twitterToolStripTextBox.Image")));
            this.twitterToolStripTextBox.Name = "twitterToolStripTextBox";
            this.twitterToolStripTextBox.Size = new System.Drawing.Size(216, 22);
            this.twitterToolStripTextBox.Text = "https://twitter.com/juvlarN";
            this.twitterToolStripTextBox.Click += new System.EventHandler(this.twitterToolStripTextBox_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("exitToolStripMenuItem.Image")));
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // backgroundWorker
            // 
            this.backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_DoWork);
            this.backgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker_ProgressChanged);
            // 
            // checkBoxAutostart
            // 
            this.checkBoxAutostart.AutoSize = true;
            this.checkBoxAutostart.Location = new System.Drawing.Point(6, 19);
            this.checkBoxAutostart.Name = "checkBoxAutostart";
            this.checkBoxAutostart.Size = new System.Drawing.Size(131, 17);
            this.checkBoxAutostart.TabIndex = 8;
            this.checkBoxAutostart.Text = "Autostart vibranceGUI";
            this.checkBoxAutostart.UseVisualStyleBackColor = true;
            this.checkBoxAutostart.CheckedChanged += new System.EventHandler(this.checkBoxAutostart_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxMonitors);
            this.groupBox1.Controls.Add(this.checkBoxKeepActive);
            this.groupBox1.Controls.Add(this.groupBox4);
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.checkBoxAutostart);
            this.groupBox1.Location = new System.Drawing.Point(13, 27);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(397, 245);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Settings";
            // 
            // checkBoxMonitors
            // 
            this.checkBoxMonitors.AutoSize = true;
            this.checkBoxMonitors.Location = new System.Drawing.Point(6, 65);
            this.checkBoxMonitors.Name = "checkBoxMonitors";
            this.checkBoxMonitors.Size = new System.Drawing.Size(125, 17);
            this.checkBoxMonitors.TabIndex = 15;
            this.checkBoxMonitors.Text = "Use multiple monitors";
            this.checkBoxMonitors.UseVisualStyleBackColor = true;
            this.checkBoxMonitors.CheckedChanged += new System.EventHandler(this.checkBoxMonitors_CheckedChanged);
            // 
            // checkBoxKeepActive
            // 
            this.checkBoxKeepActive.AutoSize = true;
            this.checkBoxKeepActive.Location = new System.Drawing.Point(6, 42);
            this.checkBoxKeepActive.Name = "checkBoxKeepActive";
            this.checkBoxKeepActive.Size = new System.Drawing.Size(221, 17);
            this.checkBoxKeepActive.TabIndex = 14;
            this.checkBoxKeepActive.Text = "Keep Vibrance on when CSGO is running";
            this.checkBoxKeepActive.UseVisualStyleBackColor = true;
            this.checkBoxKeepActive.CheckedChanged += new System.EventHandler(this.checkBoxKeepActive_CheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.labelRefreshRate);
            this.groupBox4.Controls.Add(this.textBoxRefreshRate);
            this.groupBox4.Location = new System.Drawing.Point(198, 88);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(193, 72);
            this.groupBox4.TabIndex = 13;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Refresh Rate";
            // 
            // labelRefreshRate
            // 
            this.labelRefreshRate.AutoSize = true;
            this.labelRefreshRate.Location = new System.Drawing.Point(7, 23);
            this.labelRefreshRate.Name = "labelRefreshRate";
            this.labelRefreshRate.Size = new System.Drawing.Size(110, 13);
            this.labelRefreshRate.TabIndex = 1;
            this.labelRefreshRate.Text = "Interval (milliseconds):";
            // 
            // textBoxRefreshRate
            // 
            this.textBoxRefreshRate.Location = new System.Drawing.Point(120, 20);
            this.textBoxRefreshRate.Name = "textBoxRefreshRate";
            this.textBoxRefreshRate.Size = new System.Drawing.Size(67, 20);
            this.textBoxRefreshRate.TabIndex = 0;
            this.textBoxRefreshRate.Text = "5000";
            this.textBoxRefreshRate.TextChanged += new System.EventHandler(this.textBoxRefreshRate_TextChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.labelWindowsLevel);
            this.groupBox3.Controls.Add(this.trackBarWindowsLevel);
            this.groupBox3.Location = new System.Drawing.Point(7, 166);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(182, 72);
            this.groupBox3.TabIndex = 12;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Windows Vibrance Level";
            // 
            // labelWindowsLevel
            // 
            this.labelWindowsLevel.AutoSize = true;
            this.labelWindowsLevel.Location = new System.Drawing.Point(148, 22);
            this.labelWindowsLevel.Name = "labelWindowsLevel";
            this.labelWindowsLevel.Size = new System.Drawing.Size(0, 13);
            this.labelWindowsLevel.TabIndex = 1;
            // 
            // trackBarWindowsLevel
            // 
            this.trackBarWindowsLevel.Location = new System.Drawing.Point(15, 22);
            this.trackBarWindowsLevel.Maximum = 63;
            this.trackBarWindowsLevel.Name = "trackBarWindowsLevel";
            this.trackBarWindowsLevel.Size = new System.Drawing.Size(131, 45);
            this.trackBarWindowsLevel.TabIndex = 0;
            this.trackBarWindowsLevel.Scroll += new System.EventHandler(this.trackBarWindowsLevel_Scroll);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.trackBarIngameLevel);
            this.groupBox2.Controls.Add(this.labelIngameLevel);
            this.groupBox2.Location = new System.Drawing.Point(6, 88);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(183, 72);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Ingame Vibrance Level";
            // 
            // trackBarIngameLevel
            // 
            this.trackBarIngameLevel.Location = new System.Drawing.Point(16, 23);
            this.trackBarIngameLevel.Maximum = 63;
            this.trackBarIngameLevel.Name = "trackBarIngameLevel";
            this.trackBarIngameLevel.Size = new System.Drawing.Size(131, 45);
            this.trackBarIngameLevel.TabIndex = 9;
            this.trackBarIngameLevel.Scroll += new System.EventHandler(this.trackBarIngameLevel_Scroll);
            // 
            // labelIngameLevel
            // 
            this.labelIngameLevel.AutoSize = true;
            this.labelIngameLevel.Location = new System.Drawing.Point(148, 23);
            this.labelIngameLevel.Name = "labelIngameLevel";
            this.labelIngameLevel.Size = new System.Drawing.Size(0, 13);
            this.labelIngameLevel.TabIndex = 10;
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(103, 288);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(61, 13);
            this.statusLabel.TabIndex = 14;
            this.statusLabel.Text = "Initializing...";
            // 
            // observerStatusLabel
            // 
            this.observerStatusLabel.AutoSize = true;
            this.observerStatusLabel.Location = new System.Drawing.Point(9, 288);
            this.observerStatusLabel.Name = "observerStatusLabel";
            this.observerStatusLabel.Size = new System.Drawing.Size(87, 13);
            this.observerStatusLabel.TabIndex = 13;
            this.observerStatusLabel.Text = "Observer status: ";
            // 
            // listBoxLog
            // 
            this.listBoxLog.FormattingEnabled = true;
            this.listBoxLog.Location = new System.Drawing.Point(12, 304);
            this.listBoxLog.Name = "listBoxLog";
            this.listBoxLog.Size = new System.Drawing.Size(397, 225);
            this.listBoxLog.TabIndex = 12;
            // 
            // twitterLabel
            // 
            this.twitterLabel.AutoSize = true;
            this.twitterLabel.Location = new System.Drawing.Point(10, 11);
            this.twitterLabel.Name = "twitterLabel";
            this.twitterLabel.Size = new System.Drawing.Size(192, 13);
            this.twitterLabel.TabIndex = 11;
            this.twitterLabel.Text = "Follow @juvlarN on twitter for updates: ";
            // 
            // linkLabelTwitter
            // 
            this.linkLabelTwitter.AutoSize = true;
            this.linkLabelTwitter.Location = new System.Drawing.Point(208, 11);
            this.linkLabelTwitter.Name = "linkLabelTwitter";
            this.linkLabelTwitter.Size = new System.Drawing.Size(132, 13);
            this.linkLabelTwitter.TabIndex = 10;
            this.linkLabelTwitter.TabStop = true;
            this.linkLabelTwitter.Text = "https://twitter.com/juvlarN";
            this.linkLabelTwitter.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelTwitter_LinkClicked);
            // 
            // settingsBackgroundWorker
            // 
            this.settingsBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.settingsBackgroundWorker_DoWork);
            this.settingsBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.settingsBackgroundWorker_RunWorkerCompleted);
            // 
            // vibranceGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(421, 541);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.observerStatusLabel);
            this.Controls.Add(this.listBoxLog);
            this.Controls.Add(this.twitterLabel);
            this.Controls.Add(this.linkLabelTwitter);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "vibranceGUI";
            this.Text = "vibranceGUI";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.contextMenuStrip.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarWindowsLevel)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarIngameLevel)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem twitterToolStripTextBox;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker backgroundWorker;
        private System.Windows.Forms.CheckBox checkBoxAutostart;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label labelWindowsLevel;
        private System.Windows.Forms.TrackBar trackBarWindowsLevel;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TrackBar trackBarIngameLevel;
        private System.Windows.Forms.Label labelIngameLevel;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Label observerStatusLabel;
        private System.Windows.Forms.ListBox listBoxLog;
        private System.Windows.Forms.Label twitterLabel;
        private System.Windows.Forms.LinkLabel linkLabelTwitter;
        private System.ComponentModel.BackgroundWorker settingsBackgroundWorker;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label labelRefreshRate;
        private System.Windows.Forms.TextBox textBoxRefreshRate;
        private System.Windows.Forms.CheckBox checkBoxKeepActive;
        private System.Windows.Forms.CheckBox checkBoxMonitors;

    }
}

