using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace vibrance.GUI.common
{
    public partial class VibranceSettings : Form
    {
        private IVibranceProxy _v;
        private GraphicsAdapter _graphicsAdapter;
        private ListViewItem _sender;
        private int _vibranceDefaultValue;

        public VibranceSettings(IVibranceProxy v, int minValue, int maxValue, int defaultValue, ListViewItem sender, ApplicationSetting setting, List<ResolutionModeWrapper> supportedResolutionList, GraphicsAdapter graphicsAdapter)
        {
            InitializeComponent();
            this._vibranceDefaultValue = defaultValue;
            this.trackBarIngameLevel.Minimum = minValue;
            this.trackBarIngameLevel.Maximum = maxValue;
            this.trackBarIngameLevel.Value = defaultValue;
            this._sender = sender;
            this._graphicsAdapter = graphicsAdapter;
            this._v = v;
            labelIngameLevel.Text = TrackbarLabelHelper.ResolveVibranceLabelLevel(_graphicsAdapter, trackBarIngameLevel.Value);
            this.labelTitle.Text += $@"""{sender.Text}""";
            this.pictureBox.Image = this._sender.ListView.LargeImageList.Images[this._sender.ImageIndex];
            this.cBoxResolution.DataSource = supportedResolutionList;

            if(_v.GetVibranceInfo().neverChangeColorSettings)
            {
                this.trackBarBrightness.Enabled = false;
                this.trackBarContrast.Enabled = false;
                this.trackBarGamma.Enabled = false;
            }

            if(_v.GetVibranceInfo().neverChangeResolution)
            {
                this.cBoxResolution.Enabled = false;
                this.checkBoxResolution.Enabled = false;
                this.checkBoxResolution.Checked = false;
            }

            // If the setting is new, we don't need to set the progress bar value
            if (setting != null)
            {
                // Sets the progress bar value to the Ingame Vibrance setting
                this.trackBarIngameLevel.Value = setting.IngameLevel;
                this.trackBarBrightness.Value = setting.Brightness;
                this.trackBarContrast.Value = setting.Contrast;
                this.trackBarGamma.Value = setting.Gamma;
                this.cBoxResolution.SelectedItem = setting.ResolutionSettings;
                this.checkBoxResolution.Checked = setting.IsResolutionChangeNeeded;
                reloadTrackbarLabels();
            }
        }

        private void trackBarIngameLevel_Scroll(object sender, EventArgs e)
        {
            _v.SetVibranceIngameLevel(trackBarIngameLevel.Value);
            labelIngameLevel.Text = TrackbarLabelHelper.ResolveVibranceLabelLevel(_graphicsAdapter, trackBarIngameLevel.Value);
        }

        private void trackBarBrightness_Scroll(object sender, EventArgs e)
        {
            labelBrightness.Text = TrackbarLabelHelper.ResolveBrightnessLabelLevel(trackBarBrightness.Value);
        }

        private void trackBarContrast_Scroll(object sender, EventArgs e)
        {
            labelContrast.Text = TrackbarLabelHelper.ResolveContrastLabelLevel(trackBarContrast.Value);
        }

        private void trackBarGamma_Scroll(object sender, EventArgs e)
        {
            labelGamma.Text = TrackbarLabelHelper.ResolveGammaLabelLevel(trackBarGamma.Value);
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public ApplicationSetting GetApplicationSetting()
        {
            return new ApplicationSetting(_sender.Text, _sender.Tag.ToString(), this.trackBarIngameLevel.Value, 
                (ResolutionModeWrapper)this.cBoxResolution.SelectedItem, this.checkBoxResolution.Checked, 
                this.trackBarBrightness.Value, this.trackBarContrast.Value, this.trackBarGamma.Value);
        }

        private void checkBoxResolution_CheckedChanged(object sender, EventArgs e)
        {
            this.cBoxResolution.Enabled = this.checkBoxResolution.Checked;
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            this.trackBarIngameLevel.Value = this._vibranceDefaultValue;
            this.trackBarBrightness.Value = 50;
            this.trackBarContrast.Value = 50;
            this.trackBarGamma.Value = 100;
            this.checkBoxResolution.Checked = false;
            this.cBoxResolution.SelectedIndex = 0;

            reloadTrackbarLabels();
        }

        private void reloadTrackbarLabels()
        {
            // Fake a scroll event, to reload the label which tells the percentage
            trackBarIngameLevel_Scroll(null, null);
            trackBarBrightness_Scroll(null, null);
            trackBarContrast_Scroll(null, null);
            trackBarGamma_Scroll(null, null);
        }
    }
}
