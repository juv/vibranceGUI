namespace vibrance.GUI.NVIDIA
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
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarIngameLevel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.trackBarIngameLevel);
            this.groupBox2.Controls.Add(this.labelIngameLevel);
            this.groupBox2.Location = new System.Drawing.Point(12, 63);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(246, 72);
            this.groupBox2.TabIndex = 13;
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
            this.labelIngameLevel.Location = new System.Drawing.Point(149, 26);
            this.labelIngameLevel.Name = "labelIngameLevel";
            this.labelIngameLevel.Size = new System.Drawing.Size(27, 13);
            this.labelIngameLevel.TabIndex = 10;
            this.labelIngameLevel.Text = "50%";
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(100, 231);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 14;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Location = new System.Drawing.Point(66, 9);
            this.labelTitle.MaximumSize = new System.Drawing.Size(150, 0);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(63, 13);
            this.labelTitle.TabIndex = 15;
            this.labelTitle.Text = "Settings for ";
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(12, 9);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(48, 48);
            this.pictureBox.TabIndex = 16;
            this.pictureBox.TabStop = false;
            // 
            // cBoxResolution
            // 
            this.cBoxResolution.Enabled = false;
            this.cBoxResolution.FormattingEnabled = true;
            this.cBoxResolution.Location = new System.Drawing.Point(6, 57);
            this.cBoxResolution.Name = "cBoxResolution";
            this.cBoxResolution.Size = new System.Drawing.Size(234, 21);
            this.cBoxResolution.TabIndex = 17;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.labelResolution);
            this.groupBox1.Controls.Add(this.checkBoxResolution);
            this.groupBox1.Controls.Add(this.cBoxResolution);
            this.groupBox1.Location = new System.Drawing.Point(12, 141);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(246, 84);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Ingame Resolution";
            // 
            // labelResolution
            // 
            this.labelResolution.AutoSize = true;
            this.labelResolution.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelResolution.Location = new System.Drawing.Point(4, 18);
            this.labelResolution.Name = "labelResolution";
            this.labelResolution.Size = new System.Drawing.Size(225, 13);
            this.labelResolution.TabIndex = 19;
            this.labelResolution.Text = "For (Borderless) Windowed Mode players only!";
            // 
            // checkBoxResolution
            // 
            this.checkBoxResolution.AutoSize = true;
            this.checkBoxResolution.Location = new System.Drawing.Point(6, 34);
            this.checkBoxResolution.Name = "checkBoxResolution";
            this.checkBoxResolution.Size = new System.Drawing.Size(183, 17);
            this.checkBoxResolution.TabIndex = 18;
            this.checkBoxResolution.Text = "Change Resolution when Ingame";
            this.checkBoxResolution.UseVisualStyleBackColor = true;
            this.checkBoxResolution.CheckedChanged += new System.EventHandler(this.checkBoxResolution_CheckedChanged);
            // 
            // VibranceSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(270, 266);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.groupBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "VibranceSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "vibranceGUI";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarIngameLevel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
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
    }
}