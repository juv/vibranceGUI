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
        private BackgroundWorker settingsBackgroundWorker;
        private IVibranceProxy v;
        private ListViewItem sender;

        public VibranceSettings(IVibranceProxy v, BackgroundWorker settingsBackgroundWorker, ListViewItem sender)
        {
            InitializeComponent();
            this.settingsBackgroundWorker = settingsBackgroundWorker;
            this.sender = sender;
            this.v = v;
            this.labelTitle.Text += "\"" + sender.Text + "\"";
            this.pictureBox.Image = this.sender.ListView.LargeImageList.Images[this.sender.ImageIndex];
        }

        private void trackBarIngameLevel_Scroll(object sender, EventArgs e)
        {
            NvidiaVibranceValueWrapper vibranceValue = NvidiaVibranceValueWrapper.find(trackBarIngameLevel.Value);
            if (vibranceValue == null)
                return;
            v.setVibranceIngameLevel(trackBarIngameLevel.Value);
            labelIngameLevel.Text = vibranceValue.getPercentage;
            if (!settingsBackgroundWorker.IsBusy)
            {
                settingsBackgroundWorker.RunWorkerAsync();
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public NvidiaApplicationSetting getApplicationSetting()
        {
            return new NvidiaApplicationSetting(sender.Text, sender.Tag.ToString(), this.trackBarIngameLevel.Value);
        }
    }
}
