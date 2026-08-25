using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace vibrance.GUI.common
{
    /// <summary>
    /// Asked once, and only when it is genuinely unclear which GPU vibranceGUI should drive: both
    /// vendors' drivers are installed AND the attached display devices did not settle it, either
    /// because both vendors drive a display or because neither of them does.
    /// It lists the adapter names Windows reports rather than bare vendor names, because a user
    /// recognises "NVIDIA GeForce RTX 5070 Ti" and does not necessarily know which chip is in
    /// their laptop. This runs before the main form exists, so it has no proxy to ask anything of.
    /// </summary>
    public partial class GraphicsAdapterChooser : Form
    {
        private const string DisplayDriverUninstallerUrl = "http://www.guru3d.com/files-details/display-driver-uninstaller-download.html";

        private const string StatusPrimaryDisplay = "Drives your main display";
        private const string StatusAttached = "Drives a display";
        private const string StatusNotAttached = "No display attached";

        // Shown only when Windows lists no display device at all for a vendor whose driver is
        // installed. Naming the vendor is the best that can be done in that case, and it is still
        // better than an empty list, which would be the dead end this dialog exists to remove.
        private const string FallbackNvidiaAdapterName = "NVIDIA graphics card";
        private const string FallbackAmdAdapterName = "AMD graphics card";

        private readonly List<DisplayAdapterInfo> _adapters;

        // The pick is tracked here rather than read back from listViewAdapters.SelectedItems,
        // which stays empty until the list has a window handle. Reading it straight would leave
        // the accept button disabled while a row was visibly highlighted.
        private ListViewItem _selectedItem;

        public GraphicsAdapterChooser(List<DisplayAdapterInfo> displayAdapters)
        {
            InitializeComponent();

            listViewAdapters.Columns.Add("Graphics adapter", 360, HorizontalAlignment.Left);
            listViewAdapters.Columns.Add("Status", 190, HorizontalAlignment.Left);

            SelectedAdapter = GraphicsAdapter.Unknown;
            ShouldRememberChoice = checkBoxRemember.Checked;

            _adapters = BuildCandidates(displayAdapters);
            FillList();

            try
            {
                this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch (Exception)
            {
                // The window icon is not worth failing a startup dialog over.
            }
        }

        /// <summary>
        /// The vendor the user picked, or Unknown when the dialog was cancelled or closed.
        /// </summary>
        public GraphicsAdapter SelectedAdapter { get; private set; }

        /// <summary>
        /// Whether the choice should be written to the INI so the question is asked only once.
        /// </summary>
        public bool ShouldRememberChoice { get; private set; }

        /// <summary>
        /// The adapters worth offering: the supported ones that drive a display, or - when none of
        /// them do - every supported one Windows knows about. Both vendors always end up pickable,
        /// because this dialog only opens when both vendors' drivers are installed.
        /// </summary>
        private static List<DisplayAdapterInfo> BuildCandidates(List<DisplayAdapterInfo> displayAdapters)
        {
            List<DisplayAdapterInfo> supportedAdapters = new List<DisplayAdapterInfo>();
            List<DisplayAdapterInfo> attachedAdapters = new List<DisplayAdapterInfo>();
            if (displayAdapters != null)
            {
                foreach (DisplayAdapterInfo adapter in displayAdapters)
                {
                    if (adapter == null ||
                        (adapter.Vendor != GraphicsAdapter.Nvidia && adapter.Vendor != GraphicsAdapter.Amd))
                    {
                        continue;
                    }

                    supportedAdapters.Add(adapter);
                    if (adapter.IsAttachedToDesktop)
                    {
                        attachedAdapters.Add(adapter);
                    }
                }
            }

            List<DisplayAdapterInfo> candidates = attachedAdapters.Count > 0 ? attachedAdapters : supportedAdapters;
            AddFallbackAdapter(candidates, GraphicsAdapter.Nvidia, FallbackNvidiaAdapterName);
            AddFallbackAdapter(candidates, GraphicsAdapter.Amd, FallbackAmdAdapterName);
            return candidates;
        }

        private static void AddFallbackAdapter(List<DisplayAdapterInfo> candidates, GraphicsAdapter vendor, string adapterName)
        {
            foreach (DisplayAdapterInfo candidate in candidates)
            {
                if (candidate.Vendor == vendor)
                {
                    return;
                }
            }

            DisplayAdapterInfo fallbackAdapter = new DisplayAdapterInfo();
            fallbackAdapter.Name = adapterName;
            fallbackAdapter.Vendor = vendor;
            candidates.Add(fallbackAdapter);
        }

        private void FillList()
        {
            ListViewItem defaultItem = null;
            foreach (DisplayAdapterInfo adapter in _adapters)
            {
                ListViewItem listItem = new ListViewItem(adapter.Name);
                listItem.Tag = adapter;
                listItem.SubItems.Add(DescribeStatus(adapter));
                listViewAdapters.Items.Add(listItem);

                // The adapter that owns the primary display is the one the user is looking at, so
                // it is the safe default.
                if (adapter.IsPrimary)
                {
                    defaultItem = listItem;
                }
            }

            // Otherwise the first entry, so the dialog never opens with nothing picked.
            if (defaultItem == null && listViewAdapters.Items.Count > 0)
            {
                defaultItem = listViewAdapters.Items[0];
            }
            if (defaultItem != null)
            {
                _selectedItem = defaultItem;
                defaultItem.Selected = true;
            }

            listViewAdapters.Select();
            UpdateAcceptButton();
        }

        private static string DescribeStatus(DisplayAdapterInfo adapter)
        {
            if (adapter.IsPrimary)
            {
                return StatusPrimaryDisplay;
            }
            if (adapter.IsAttachedToDesktop)
            {
                return StatusAttached;
            }
            return StatusNotAttached;
        }

        private DisplayAdapterInfo GetSelectedAdapter()
        {
            if (_selectedItem == null)
            {
                return null;
            }
            return _selectedItem.Tag as DisplayAdapterInfo;
        }

        private void UpdateAcceptButton()
        {
            buttonUse.Enabled = GetSelectedAdapter() != null;
        }

        private void listViewAdapters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewAdapters.SelectedItems.Count == 1)
            {
                _selectedItem = listViewAdapters.SelectedItems[0];
            }
            else if (_selectedItem != null && listViewAdapters.IsHandleCreated)
            {
                // Clicking past the last row clears the highlight. Put it back rather than leave
                // an enabled accept button pointing at a row the user can no longer see.
                _selectedItem.Selected = true;
            }
            UpdateAcceptButton();
        }

        private void listViewAdapters_DoubleClick(object sender, EventArgs e)
        {
            Accept();
        }

        private void buttonUse_Click(object sender, EventArgs e)
        {
            Accept();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void Accept()
        {
            DisplayAdapterInfo adapter = GetSelectedAdapter();
            if (adapter == null)
            {
                return;
            }

            SelectedAdapter = adapter.Vendor;
            ShouldRememberChoice = checkBoxRemember.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void linkLabelDdu_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(DisplayDriverUninstallerUrl);
            }
            catch (Exception ex)
            {
                try
                {
                    VibranceGUI.Log(ex);
                }
                catch (Exception)
                {
                    // No browser and no log file is still no reason to take the dialog down.
                }
            }
        }
    }
}
