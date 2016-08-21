namespace vibrance.GUI.AMD
{
    partial class AmdVibranceGui
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AmdVibranceGui));
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.twitterToolStripTextBox = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.checkBoxAutostart = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxPrimaryMonitorOnly = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.labelWindowsLevel = new System.Windows.Forms.Label();
            this.trackBarWindowsLevel = new System.Windows.Forms.TrackBar();
            this.statusLabel = new System.Windows.Forms.Label();
            this.observerStatusLabel = new System.Windows.Forms.Label();
            this.labelTwitter = new System.Windows.Forms.Label();
            this.linkLabelTwitter = new System.Windows.Forms.LinkLabel();
            this.settingsBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.buttonPaypal = new System.Windows.Forms.Button();
            this.labelPaypal = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.buttonSteam = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.buttonRemoveProgram = new System.Windows.Forms.Button();
            this.listApplications = new System.Windows.Forms.ListView();
            this.buttonAddProgram = new System.Windows.Forms.Button();
            this.contextMenuStrip.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarWindowsLevel)).BeginInit();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon
            // 
            this.notifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon.BalloonTipText = "Running minimized... Like the program? Consider donating!";
            this.notifyIcon.BalloonTipTitle = "vibranceGUI";
            this.notifyIcon.ContextMenuStrip = this.contextMenuStrip;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "vibranceGUI";
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
            this.backgroundWorker.WorkerReportsProgress = true;
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
            this.groupBox1.Controls.Add(this.checkBoxPrimaryMonitorOnly);
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.checkBoxAutostart);
            this.groupBox1.Location = new System.Drawing.Point(13, 82);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(397, 143);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Settings";
            // 
            // checkBoxPrimaryMonitorOnly
            // 
            this.checkBoxPrimaryMonitorOnly.AutoSize = true;
            this.checkBoxPrimaryMonitorOnly.Location = new System.Drawing.Point(6, 42);
            this.checkBoxPrimaryMonitorOnly.Name = "checkBoxPrimaryMonitorOnly";
            this.checkBoxPrimaryMonitorOnly.Size = new System.Drawing.Size(151, 17);
            this.checkBoxPrimaryMonitorOnly.TabIndex = 15;
            this.checkBoxPrimaryMonitorOnly.Text = "Affect Primary Monitor only";
            this.toolTip.SetToolTip(this.checkBoxPrimaryMonitorOnly, "When checking this, VibranceGUI will only change vibrance values on your primary " +
        "monitor.");
            this.checkBoxPrimaryMonitorOnly.UseVisualStyleBackColor = true;
            this.checkBoxPrimaryMonitorOnly.CheckedChanged += new System.EventHandler(this.checkBoxPrimaryMonitorOnly_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.labelWindowsLevel);
            this.groupBox3.Controls.Add(this.trackBarWindowsLevel);
            this.groupBox3.Location = new System.Drawing.Point(7, 65);
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
            this.trackBarWindowsLevel.Maximum = 300;
            this.trackBarWindowsLevel.Name = "trackBarWindowsLevel";
            this.trackBarWindowsLevel.Size = new System.Drawing.Size(131, 45);
            this.trackBarWindowsLevel.TabIndex = 0;
            this.trackBarWindowsLevel.Scroll += new System.EventHandler(this.trackBarWindowsLevel_Scroll);
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(106, 502);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(61, 13);
            this.statusLabel.TabIndex = 14;
            this.statusLabel.Text = "Initializing...";
            // 
            // observerStatusLabel
            // 
            this.observerStatusLabel.AutoSize = true;
            this.observerStatusLabel.Location = new System.Drawing.Point(12, 502);
            this.observerStatusLabel.Name = "observerStatusLabel";
            this.observerStatusLabel.Size = new System.Drawing.Size(87, 13);
            this.observerStatusLabel.TabIndex = 13;
            this.observerStatusLabel.Text = "Observer status: ";
            // 
            // labelTwitter
            // 
            this.labelTwitter.AutoSize = true;
            this.labelTwitter.Location = new System.Drawing.Point(10, 11);
            this.labelTwitter.Name = "labelTwitter";
            this.labelTwitter.Size = new System.Drawing.Size(192, 13);
            this.labelTwitter.TabIndex = 11;
            this.labelTwitter.Text = "Follow @juvlarN on twitter for updates: ";
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
            // buttonPaypal
            // 
            this.buttonPaypal.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonPaypal.BackColor = System.Drawing.Color.Transparent;
            this.buttonPaypal.Image = ((System.Drawing.Image)(resources.GetObject("buttonPaypal.Image")));
            this.buttonPaypal.Location = new System.Drawing.Point(211, 28);
            this.buttonPaypal.Name = "buttonPaypal";
            this.buttonPaypal.Size = new System.Drawing.Size(90, 55);
            this.buttonPaypal.TabIndex = 16;
            this.toolTip.SetToolTip(this.buttonPaypal, "Click here to donate to vibranceGUI through Paypal");
            this.buttonPaypal.UseVisualStyleBackColor = false;
            this.buttonPaypal.Click += new System.EventHandler(this.buttonPaypal_Click);
            // 
            // labelPaypal
            // 
            this.labelPaypal.AutoSize = true;
            this.labelPaypal.Location = new System.Drawing.Point(10, 48);
            this.labelPaypal.Name = "labelPaypal";
            this.labelPaypal.Size = new System.Drawing.Size(183, 13);
            this.labelPaypal.TabIndex = 17;
            this.labelPaypal.Text = "Like the program? Consider donating:";
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 5000;
            this.toolTip.InitialDelay = 100;
            this.toolTip.IsBalloon = true;
            this.toolTip.ReshowDelay = 100;
            // 
            // buttonSteam
            // 
            this.buttonSteam.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonSteam.BackColor = System.Drawing.Color.Transparent;
            this.buttonSteam.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonSteam.Image = ((System.Drawing.Image)(resources.GetObject("buttonSteam.Image")));
            this.buttonSteam.Location = new System.Drawing.Point(307, 27);
            this.buttonSteam.Name = "buttonSteam";
            this.buttonSteam.Size = new System.Drawing.Size(90, 55);
            this.buttonSteam.TabIndex = 19;
            this.toolTip.SetToolTip(this.buttonSteam, "Click here to donate to vibranceGUI through Steam");
            this.buttonSteam.UseVisualStyleBackColor = false;
            this.buttonSteam.Click += new System.EventHandler(this.buttonSteam_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.buttonRemoveProgram);
            this.groupBox5.Controls.Add(this.listApplications);
            this.groupBox5.Controls.Add(this.buttonAddProgram);
            this.groupBox5.Location = new System.Drawing.Point(13, 231);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(397, 227);
            this.groupBox5.TabIndex = 18;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Program Settings";
            // 
            // buttonRemoveProgram
            // 
            this.buttonRemoveProgram.Location = new System.Drawing.Point(94, 19);
            this.buttonRemoveProgram.Name = "buttonRemoveProgram";
            this.buttonRemoveProgram.Size = new System.Drawing.Size(75, 23);
            this.buttonRemoveProgram.TabIndex = 2;
            this.buttonRemoveProgram.Text = "Remove";
            this.buttonRemoveProgram.UseVisualStyleBackColor = true;
            this.buttonRemoveProgram.Click += new System.EventHandler(this.buttonRemoveProgram_Click);
            // 
            // listApplications
            // 
            this.listApplications.Location = new System.Drawing.Point(7, 49);
            this.listApplications.Name = "listApplications";
            this.listApplications.Size = new System.Drawing.Size(384, 172);
            this.listApplications.TabIndex = 1;
            this.listApplications.UseCompatibleStateImageBehavior = false;
            this.listApplications.DoubleClick += new System.EventHandler(this.listApplications_DoubleClick);
            // 
            // buttonAddProgram
            // 
            this.buttonAddProgram.Location = new System.Drawing.Point(6, 19);
            this.buttonAddProgram.Name = "buttonAddProgram";
            this.buttonAddProgram.Size = new System.Drawing.Size(75, 23);
            this.buttonAddProgram.TabIndex = 0;
            this.buttonAddProgram.Text = "Add";
            this.buttonAddProgram.UseVisualStyleBackColor = true;
            this.buttonAddProgram.Click += new System.EventHandler(this.buttonAddProgram_Click);
            // 
            // AmdVibranceGui
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(419, 524);
            this.Controls.Add(this.buttonSteam);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.labelPaypal);
            this.Controls.Add(this.buttonPaypal);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.observerStatusLabel);
            this.Controls.Add(this.labelTwitter);
            this.Controls.Add(this.linkLabelTwitter);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AmdVibranceGui";
            this.Text = "vibranceGUI";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.contextMenuStrip.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarWindowsLevel)).EndInit();
            this.groupBox5.ResumeLayout(false);
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
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Label observerStatusLabel;
        private System.Windows.Forms.Label labelTwitter;
        private System.Windows.Forms.LinkLabel linkLabelTwitter;
        private System.ComponentModel.BackgroundWorker settingsBackgroundWorker;
        private System.Windows.Forms.Button buttonPaypal;
        private System.Windows.Forms.Label labelPaypal;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.CheckBox checkBoxPrimaryMonitorOnly;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button buttonRemoveProgram;
        private System.Windows.Forms.ListView listApplications;
        private System.Windows.Forms.Button buttonAddProgram;
        private System.Windows.Forms.Button buttonSteam;
    }
}

