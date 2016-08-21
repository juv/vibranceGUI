using System;
using System.Collections.Generic;
using System.Windows.Forms;
using vibrance.GUI.common;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI.AMD
{
    public partial class VibranceSettings : Form
    {
        private IVibranceProxy _v;
        private ListViewItem _sender;

        public VibranceSettings(IVibranceProxy v, ListViewItem sender, ApplicationSetting setting, List<ResolutionModeWrapper> supportedResolutionList)
        {
            InitializeComponent();
            this._sender = sender;
            this._v = v;
            this.labelTitle.Text += "\"" + sender.Text + "\"";
            this.pictureBox.Image = this._sender.ListView.LargeImageList.Images[this._sender.ImageIndex];
            this.cBoxResolution.DataSource = supportedResolutionList;
            // If the setting is new, we don't need to set the progress bar value
            if (setting != null)
            {
                // Sets the progress bar value to the Ingame Vibrance setting
                this.trackBarIngameLevel.Value = setting.IngameLevel;
                this.cBoxResolution.SelectedItem = setting.ResolutionSettings;
                this.checkBoxResolution.Checked = setting.IsResolutionChangeNeeded;
                // Necessary to reload the label which tells the percentage
                trackBarIngameLevel_Scroll(null, null); 
            }
        }

        private void trackBarIngameLevel_Scroll(object sender, EventArgs e)
        {
            _v.SetVibranceIngameLevel(trackBarIngameLevel.Value);
            labelIngameLevel.Text = trackBarIngameLevel.Value.ToString();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public ApplicationSetting GetApplicationSetting()
        {
            return new ApplicationSetting(_sender.Text, _sender.Tag.ToString(), this.trackBarIngameLevel.Value, 
                (ResolutionModeWrapper)this.cBoxResolution.SelectedItem, this.checkBoxResolution.Checked);
        }

        private void checkBoxResolution_CheckedChanged(object sender, EventArgs e)
        {
            this.cBoxResolution.Enabled = this.checkBoxResolution.Checked;
        }
    }
}
