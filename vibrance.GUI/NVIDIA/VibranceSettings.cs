using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using vibrance.GUI.common;

namespace vibrance.GUI.NVIDIA
{
    public partial class VibranceSettings : Form
    {
        private IVibranceProxy v;
        private ListViewItem sender;

        public VibranceSettings(IVibranceProxy v, ListViewItem sender, NvidiaApplicationSetting setting, List<ResolutionModeWrapper> supportedResolutionList)
        {
            InitializeComponent();
            this.sender = sender;
            this.v = v;
            this.labelTitle.Text += "\"" + sender.Text + "\"";
            this.pictureBox.Image = this.sender.ListView.LargeImageList.Images[this.sender.ImageIndex];
            this.cBoxResolution.DataSource = supportedResolutionList;
            // If the setting is new, we don't need to set the progress bar value
            if (setting != null)
            {
                // Sets the progress bar value to the Ingame Vibrance setting
                this.trackBarIngameLevel.Value = setting.IngameLevel;
                this.cBoxResolution.SelectedItem = setting.ResolutionSettings;
                // Necessary to reload the label which tells the percentage
                trackBarIngameLevel_Scroll(null, null); 
            }
        }

        private void trackBarIngameLevel_Scroll(object sender, EventArgs e)
        {
            NvidiaVibranceValueWrapper vibranceValue = NvidiaVibranceValueWrapper.find(trackBarIngameLevel.Value);
            if (vibranceValue == null)
                return;
            v.setVibranceIngameLevel(trackBarIngameLevel.Value);
            labelIngameLevel.Text = vibranceValue.getPercentage;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public NvidiaApplicationSetting getApplicationSetting()
        {
            return new NvidiaApplicationSetting(sender.Text, sender.Tag.ToString(), this.trackBarIngameLevel.Value, 
                (ResolutionModeWrapper)this.cBoxResolution.SelectedItem, this.checkBoxResolution.Checked);
        }

        private void checkBoxResolution_CheckedChanged(object sender, EventArgs e)
        {
            this.cBoxResolution.Enabled = this.checkBoxResolution.Checked;
        }
    }
}
