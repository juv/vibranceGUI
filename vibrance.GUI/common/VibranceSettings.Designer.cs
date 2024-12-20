namespace vibrance.GUI.common
{
    partial class VibranceSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VibranceSettings));
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.trackBarIngameLevel = new System.Windows.Forms.TrackBar();
            this.labelIngameLevel = new System.Windows.Forms.Label();
            this.buttonSave = new System.Windows.Forms.Button();
            this.labelTitle = new System.Windows.Forms.Label();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.cBoxResolution = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelResolution = new System.Windows.Forms.Label();
            this.checkBoxResolution = new System.Windows.Forms.CheckBox();
            this.groupBoxBrightness = new System.Windows.Forms.GroupBox();
            this.trackBarBrightness = new System.Windows.Forms.TrackBar();
            this.labelBrightness = new System.Windows.Forms.Label();
            this.groupBoxContrast = new System.Windows.Forms.GroupBox();
            this.trackBarContrast = new System.Windows.Forms.TrackBar();
            this.labelContrast = new System.Windows.Forms.Label();
            this.groupBoxGamma = new System.Windows.Forms.GroupBox();
            this.trackBarGamma = new System.Windows.Forms.TrackBar();
            this.labelGamma = new System.Windows.Forms.Label();
            this.buttonReset = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarIngameLevel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBoxBrightness.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarBrightness)).BeginInit();
            this.groupBoxContrast.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarContrast)).BeginInit();
            this.groupBoxGamma.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarGamma)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.trackBarIngameLevel);
            this.groupBox2.Controls.Add(this.labelIngameLevel);
            this.groupBox2.Location = new System.Drawing.Point(18, 97);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox2.Size = new System.Drawing.Size(369, 111);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Ingame Vibrance Level";
            // 
            // trackBarIngameLevel
            // 
            this.trackBarIngameLevel.Location = new System.Drawing.Point(24, 35);
            this.trackBarIngameLevel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.trackBarIngameLevel.Maximum = 63;
            this.trackBarIngameLevel.Name = "trackBarIngameLevel";
            this.trackBarIngameLevel.Size = new System.Drawing.Size(196, 69);
            this.trackBarIngameLevel.TabIndex = 9;
            this.trackBarIngameLevel.Scroll += new System.EventHandler(this.trackBarIngameLevel_Scroll);
            // 
            // labelIngameLevel
            // 
            this.labelIngameLevel.AutoSize = true;
            this.labelIngameLevel.Location = new System.Drawing.Point(224, 40);
            this.labelIngameLevel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelIngameLevel.Name = "labelIngameLevel";
            this.labelIngameLevel.Size = new System.Drawing.Size(41, 20);
            this.labelIngameLevel.TabIndex = 10;
            this.labelIngameLevel.Text = "50%";
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(247, 718);
            this.buttonSave.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(140, 37);
            this.buttonSave.TabIndex = 14;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Location = new System.Drawing.Point(99, 14);
            this.labelTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTitle.MaximumSize = new System.Drawing.Size(225, 0);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(95, 20);
            this.labelTitle.TabIndex = 15;
            this.labelTitle.Text = "Settings for ";
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(18, 14);
            this.pictureBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(72, 74);
            this.pictureBox.TabIndex = 16;
            this.pictureBox.TabStop = false;
            // 
            // cBoxResolution
            // 
            this.cBoxResolution.Enabled = false;
            this.cBoxResolution.FormattingEnabled = true;
            this.cBoxResolution.Location = new System.Drawing.Point(9, 88);
            this.cBoxResolution.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cBoxResolution.Name = "cBoxResolution";
            this.cBoxResolution.Size = new System.Drawing.Size(349, 28);
            this.cBoxResolution.TabIndex = 17;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.labelResolution);
            this.groupBox1.Controls.Add(this.checkBoxResolution);
            this.groupBox1.Controls.Add(this.cBoxResolution);
            this.groupBox1.Location = new System.Drawing.Point(18, 581);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Size = new System.Drawing.Size(369, 131);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Ingame Resolution";
            // 
            // labelResolution
            // 
            this.labelResolution.AutoSize = true;
            this.labelResolution.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelResolution.Location = new System.Drawing.Point(6, 28);
            this.labelResolution.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelResolution.Name = "labelResolution";
            this.labelResolution.Size = new System.Drawing.Size(360, 20);
            this.labelResolution.TabIndex = 19;
            this.labelResolution.Text = "For (Borderless) Windowed Mode players only!";
            // 
            // checkBoxResolution
            // 
            this.checkBoxResolution.AutoSize = true;
            this.checkBoxResolution.Location = new System.Drawing.Point(9, 52);
            this.checkBoxResolution.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxResolution.Name = "checkBoxResolution";
            this.checkBoxResolution.Size = new System.Drawing.Size(271, 24);
            this.checkBoxResolution.TabIndex = 18;
            this.checkBoxResolution.Text = "Change Resolution when Ingame";
            this.checkBoxResolution.UseVisualStyleBackColor = true;
            this.checkBoxResolution.CheckedChanged += new System.EventHandler(this.checkBoxResolution_CheckedChanged);
            // 
            // groupBoxBrightness
            // 
            this.groupBoxBrightness.Controls.Add(this.trackBarBrightness);
            this.groupBoxBrightness.Controls.Add(this.labelBrightness);
            this.groupBoxBrightness.Location = new System.Drawing.Point(18, 218);
            this.groupBoxBrightness.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxBrightness.Name = "groupBoxBrightness";
            this.groupBoxBrightness.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxBrightness.Size = new System.Drawing.Size(369, 111);
            this.groupBoxBrightness.TabIndex = 21;
            this.groupBoxBrightness.TabStop = false;
            this.groupBoxBrightness.Text = "Ingame Brightness (EXPERIMENTAL)";
            // 
            // trackBarBrightness
            // 
            this.trackBarBrightness.Location = new System.Drawing.Point(24, 35);
            this.trackBarBrightness.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.trackBarBrightness.Maximum = 100;
            this.trackBarBrightness.Name = "trackBarBrightness";
            this.trackBarBrightness.Size = new System.Drawing.Size(196, 69);
            this.trackBarBrightness.TabIndex = 9;
            this.trackBarBrightness.Value = 50;
            this.trackBarBrightness.Scroll += new System.EventHandler(this.trackBarBrightness_Scroll);
            // 
            // labelBrightness
            // 
            this.labelBrightness.AutoSize = true;
            this.labelBrightness.Location = new System.Drawing.Point(224, 40);
            this.labelBrightness.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelBrightness.Name = "labelBrightness";
            this.labelBrightness.Size = new System.Drawing.Size(41, 20);
            this.labelBrightness.TabIndex = 10;
            this.labelBrightness.Text = "50%";
            // 
            // groupBoxContrast
            // 
            this.groupBoxContrast.Controls.Add(this.trackBarContrast);
            this.groupBoxContrast.Controls.Add(this.labelContrast);
            this.groupBoxContrast.Location = new System.Drawing.Point(18, 339);
            this.groupBoxContrast.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxContrast.Name = "groupBoxContrast";
            this.groupBoxContrast.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxContrast.Size = new System.Drawing.Size(369, 111);
            this.groupBoxContrast.TabIndex = 22;
            this.groupBoxContrast.TabStop = false;
            this.groupBoxContrast.Text = "Ingame Contrast (EXPERIMENTAL)";
            // 
            // trackBarContrast
            // 
            this.trackBarContrast.Location = new System.Drawing.Point(24, 35);
            this.trackBarContrast.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.trackBarContrast.Maximum = 100;
            this.trackBarContrast.Name = "trackBarContrast";
            this.trackBarContrast.Size = new System.Drawing.Size(196, 69);
            this.trackBarContrast.TabIndex = 9;
            this.trackBarContrast.Value = 50;
            this.trackBarContrast.Scroll += new System.EventHandler(this.trackBarContrast_Scroll);
            // 
            // labelContrast
            // 
            this.labelContrast.AutoSize = true;
            this.labelContrast.Location = new System.Drawing.Point(224, 40);
            this.labelContrast.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelContrast.Name = "labelContrast";
            this.labelContrast.Size = new System.Drawing.Size(41, 20);
            this.labelContrast.TabIndex = 10;
            this.labelContrast.Text = "50%";
            // 
            // groupBoxGamma
            // 
            this.groupBoxGamma.Controls.Add(this.trackBarGamma);
            this.groupBoxGamma.Controls.Add(this.labelGamma);
            this.groupBoxGamma.Location = new System.Drawing.Point(18, 460);
            this.groupBoxGamma.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxGamma.Name = "groupBoxGamma";
            this.groupBoxGamma.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxGamma.Size = new System.Drawing.Size(369, 111);
            this.groupBoxGamma.TabIndex = 23;
            this.groupBoxGamma.TabStop = false;
            this.groupBoxGamma.Text = "Ingame Gamma (EXPERIMENTAL)";
            // 
            // trackBarGamma
            // 
            this.trackBarGamma.Location = new System.Drawing.Point(24, 35);
            this.trackBarGamma.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.trackBarGamma.Maximum = 240;
            this.trackBarGamma.Minimum = 30;
            this.trackBarGamma.Name = "trackBarGamma";
            this.trackBarGamma.Size = new System.Drawing.Size(196, 69);
            this.trackBarGamma.TabIndex = 9;
            this.trackBarGamma.Value = 100;
            this.trackBarGamma.Scroll += new System.EventHandler(this.trackBarGamma_Scroll);
            // 
            // labelGamma
            // 
            this.labelGamma.AutoSize = true;
            this.labelGamma.Location = new System.Drawing.Point(224, 40);
            this.labelGamma.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelGamma.Name = "labelGamma";
            this.labelGamma.Size = new System.Drawing.Size(18, 20);
            this.labelGamma.TabIndex = 10;
            this.labelGamma.Text = "1";
            // 
            // buttonReset
            // 
            this.buttonReset.Location = new System.Drawing.Point(18, 718);
            this.buttonReset.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(140, 37);
            this.buttonReset.TabIndex = 24;
            this.buttonReset.Text = "Reset values";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // VibranceSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(405, 769);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.groupBoxGamma);
            this.Controls.Add(this.groupBoxContrast);
            this.Controls.Add(this.groupBoxBrightness);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.groupBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.Name = "VibranceSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "vibranceGUI";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarIngameLevel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBoxBrightness.ResumeLayout(false);
            this.groupBoxBrightness.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarBrightness)).EndInit();
            this.groupBoxContrast.ResumeLayout(false);
            this.groupBoxContrast.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarContrast)).EndInit();
            this.groupBoxGamma.ResumeLayout(false);
            this.groupBoxGamma.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarGamma)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TrackBar trackBarIngameLevel;
        private System.Windows.Forms.Label labelIngameLevel;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.ComboBox cBoxResolution;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBoxResolution;
        private System.Windows.Forms.Label labelResolution;
        private System.Windows.Forms.GroupBox groupBoxBrightness;
        private System.Windows.Forms.TrackBar trackBarBrightness;
        private System.Windows.Forms.Label labelBrightness;
        private System.Windows.Forms.GroupBox groupBoxContrast;
        private System.Windows.Forms.TrackBar trackBarContrast;
        private System.Windows.Forms.Label labelContrast;
        private System.Windows.Forms.GroupBox groupBoxGamma;
        private System.Windows.Forms.TrackBar trackBarGamma;
        private System.Windows.Forms.Label labelGamma;
        private System.Windows.Forms.Button buttonReset;
    }
}