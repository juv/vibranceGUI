using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using vibrance.GUI.AMD.vendor;
using vibrance.GUI.AMD.vendor.adl32;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI.common
{
    public enum GraphicsAdapter
    {
        Unknown = 0,
        Nvidia = 1,
        Amd = 2,
        Ambiguous = 3
    }

    /// <summary>
    /// One graphics adapter, as Windows reports it through EnumDisplayDevices. Windows emits an
    /// adapter entry per display head, so several entries share a Name - they are folded into one
    /// instance here and DisplayNames lists every head the adapter owns.
    /// Only ever produced by GraphicsAdapterHelper.GetDisplayAdapters().
    /// </summary>
    public class DisplayAdapterInfo
    {
        public string Name { get; set; }                // DeviceString, e.g. "NVIDIA GeForce RTX 5070 Ti"
        public GraphicsAdapter Vendor { get; set; }     // Nvidia, Amd, or Unknown for anything else
        public bool IsAttachedToDesktop { get; set; }   // drives at least one display of the desktop
        public bool IsPrimary { get; set; }             // owns the primary display
        public List<string> DisplayNames { get; set; }  // "\\.\DISPLAY1", ...; diagnostics only

        public DisplayAdapterInfo()
        {
            DisplayNames = new List<string>();
        }
    }

    public class GraphicsAdapterHelper
    {

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DisplayDevice lpDisplayDevice, uint dwFlags);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct DisplayDevice
        {
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            public uint StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        private const uint DisplayDeviceAttachedToDesktop = 0x00000001;
        private const uint DisplayDevicePrimaryDevice = 0x00000004;

        // Nothing real comes close to this. The bound only exists so that a display driver which
        // never fails the enumeration cannot spin the loop forever before the window opens.
        private const uint MaxEnumeratedDisplayDevices = 64;

        // Matched case-insensitively as whole words in the adapter's DeviceString - see
        // ContainsAnyToken for why the word boundary is not optional. Anything that matches
        // neither list - Intel above all, and every virtual display driver - is Unknown and must
        // never be reported as one of the two supported vendors.
        private static readonly string[] NvidiaAdapterNameTokens = { "NVIDIA" };
        private static readonly string[] AmdAdapterNameTokens = { "AMD", "Radeon", "ATI" };

        private const string _nvidiaDllName = "nvapi.dll";
        private static readonly string _amdDllName = Environment.Is64BitOperatingSystem 
            ? AMD.vendor.adl64.AdlImport.AtiadlFileName
            : AMD.vendor.adl32.AdlImport.AtiadlFileName;


        public static GraphicsAdapter GetAdapter()
        {
            if (AreBothVendorDriversInstalled())
            {
                // A driver DLL sitting in the system folder says nothing about whether that GPU is
                // in use, so the file system alone cannot settle this. Ask Windows which adapter
                // actually drives a display first: on an AMD CPU with integrated graphics plus a
                // discrete NVIDIA card - both DLLs present, one GPU driving the monitors - there
                // is an unambiguous answer, and it is the difference between the application
                // starting and refusing to start at all.
                return GetAdapterFromAttachedDisplays();
            }
            if (IsAdapterAvailable(_amdDllName))
            {
                IAmdAdapter amdAdapter = Environment.Is64BitOperatingSystem ? (IAmdAdapter)new AmdAdapter64() :new AmdAdapter32();
                if (amdAdapter.IsAvailable())
                {
                    return GraphicsAdapter.Amd;
                }
            }
            if (IsAdapterAvailable(_nvidiaDllName))
            {
                return GraphicsAdapter.Nvidia;
            }
            return GraphicsAdapter.Unknown;
        }

        /// <summary>
        /// True when both vendors' driver DLLs are installed. That is the only case GetAdapter()
        /// cannot decide from the file system, and therefore the only case in which the display
        /// device detection, a stored preference or the chooser are allowed to have a say - a
        /// machine that resolves to a single vendor today keeps resolving exactly as it did.
        /// </summary>
        public static bool AreBothVendorDriversInstalled()
        {
            return IsVendorDriverInstalled(GraphicsAdapter.Amd) && IsVendorDriverInstalled(GraphicsAdapter.Nvidia);
        }

        /// <summary>
        /// True when the driver DLL of the given vendor is installed. Used to discard a stored
        /// preference that names hardware the user no longer has.
        /// </summary>
        public static bool IsVendorDriverInstalled(GraphicsAdapter graphicsAdapter)
        {
            string dllName;
            if (graphicsAdapter == GraphicsAdapter.Nvidia)
            {
                dllName = _nvidiaDllName;
            }
            else if (graphicsAdapter == GraphicsAdapter.Amd)
            {
                dllName = _amdDllName;
            }
            else
            {
                return false;
            }

            try
            {
                string windowsFolder = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);
                return File.Exists(Path.Combine(windowsFolder, dllName));
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// The vendor of the adapter that actually drives a display attached to the desktop. This
        /// is the signal that discriminates a hybrid machine: Win32_VideoController reports an AMD
        /// iGPU and a discrete NVIDIA card as equally OK, but only one of them has a monitor on
        /// it. Returns Ambiguous when both vendors drive a display, and also when neither does -
        /// an Intel-only desktop, an RDP session, or an enumeration that told us nothing. In those
        /// cases asking is better than guessing.
        /// </summary>
        public static GraphicsAdapter GetAdapterFromAttachedDisplays()
        {
            bool isNvidiaAttached = false;
            bool isAmdAttached = false;
            foreach (DisplayAdapterInfo adapter in GetAttachedDisplayAdapters())
            {
                if (adapter.Vendor == GraphicsAdapter.Nvidia)
                {
                    isNvidiaAttached = true;
                }
                else if (adapter.Vendor == GraphicsAdapter.Amd)
                {
                    isAmdAttached = true;
                }
            }

            if (isNvidiaAttached && !isAmdAttached)
            {
                return GraphicsAdapter.Nvidia;
            }
            if (isAmdAttached && !isNvidiaAttached)
            {
                return GraphicsAdapter.Amd;
            }
            return GraphicsAdapter.Ambiguous;
        }

        /// <summary>
        /// The adapters that drive at least one display attached to the desktop.
        /// </summary>
        public static List<DisplayAdapterInfo> GetAttachedDisplayAdapters()
        {
            List<DisplayAdapterInfo> attachedAdapters = new List<DisplayAdapterInfo>();
            foreach (DisplayAdapterInfo adapter in GetDisplayAdapters())
            {
                if (adapter.IsAttachedToDesktop)
                {
                    attachedAdapters.Add(adapter);
                }
            }
            return attachedAdapters;
        }

        /// <summary>
        /// Every graphics adapter Windows knows about, folded to one entry per adapter name.
        /// Never throws and never returns null: this runs before the main window exists, and a
        /// virtual display driver, an RDP session or a headless machine can make EnumDisplayDevices
        /// behave in ways no caller should have to anticipate. Every caller treats an empty list
        /// as "could not tell", which falls back to the behaviour that was there before.
        /// </summary>
        public static List<DisplayAdapterInfo> GetDisplayAdapters()
        {
            List<DisplayAdapterInfo> adapters = new List<DisplayAdapterInfo>();
            bool isEnumerationComplete = false;
            try
            {
                for (uint deviceIndex = 0; deviceIndex < MaxEnumeratedDisplayDevices; deviceIndex++)
                {
                    DisplayDevice device = new DisplayDevice();
                    device.cb = Marshal.SizeOf(typeof(DisplayDevice));
                    if (!EnumDisplayDevices(null, deviceIndex, ref device, 0))
                    {
                        isEnumerationComplete = true;
                        break;
                    }

                    string adapterName = device.DeviceString == null ? string.Empty : device.DeviceString.Trim();
                    if (adapterName.Length == 0)
                    {
                        continue;
                    }

                    DisplayAdapterInfo adapter = FindAdapterByName(adapters, adapterName);
                    if (adapter == null)
                    {
                        adapter = new DisplayAdapterInfo();
                        adapter.Name = adapterName;
                        adapter.Vendor = GetVendorFromAdapterName(adapterName);
                        adapters.Add(adapter);
                    }

                    adapter.IsAttachedToDesktop |= (device.StateFlags & DisplayDeviceAttachedToDesktop) != 0;
                    adapter.IsPrimary |= (device.StateFlags & DisplayDevicePrimaryDevice) != 0;
                    if (device.DeviceName != null && device.DeviceName.Trim().Length > 0)
                    {
                        adapter.DisplayNames.Add(device.DeviceName.Trim());
                    }
                }
            }
            catch (Exception)
            {
                // A half-read enumeration is worse than none: it could show one vendor and hide
                // the other. Report "could not tell" instead.
                return new List<DisplayAdapterInfo>();
            }

            if (!isEnumerationComplete)
            {
                // Ran out at the bound rather than at the end of the list. Same half-read hazard
                // as an exception, so it gets the same answer.
                return new List<DisplayAdapterInfo>();
            }
            return adapters;
        }

        /// <summary>
        /// The vendor an adapter name belongs to, or Unknown when it is neither of the two
        /// supported ones.
        /// </summary>
        public static GraphicsAdapter GetVendorFromAdapterName(string adapterName)
        {
            if (string.IsNullOrEmpty(adapterName))
            {
                return GraphicsAdapter.Unknown;
            }
            if (ContainsAnyToken(adapterName, NvidiaAdapterNameTokens))
            {
                return GraphicsAdapter.Nvidia;
            }
            if (ContainsAnyToken(adapterName, AmdAdapterNameTokens))
            {
                return GraphicsAdapter.Amd;
            }
            return GraphicsAdapter.Unknown;
        }

        /// <summary>
        /// One line per adapter, for vibranceGUI.log. This is the first thing to ask a user for
        /// when they report that the wrong GPU was picked.
        /// </summary>
        public static string DescribeDisplayAdapters()
        {
            StringBuilder description = new StringBuilder();
            List<DisplayAdapterInfo> adapters = GetDisplayAdapters();
            if (adapters.Count == 0)
            {
                return "No display adapters could be enumerated.";
            }

            foreach (DisplayAdapterInfo adapter in adapters)
            {
                description.AppendFormat("  {0} [vendor={1}, attached={2}, primary={3}, displays={4}]",
                    adapter.Name,
                    adapter.Vendor,
                    adapter.IsAttachedToDesktop,
                    adapter.IsPrimary,
                    string.Join(", ", adapter.DisplayNames.ToArray()));
                description.AppendLine();
            }
            return description.ToString().TrimEnd();
        }

        private static DisplayAdapterInfo FindAdapterByName(List<DisplayAdapterInfo> adapters, string adapterName)
        {
            foreach (DisplayAdapterInfo adapter in adapters)
            {
                if (string.Equals(adapter.Name, adapterName, StringComparison.OrdinalIgnoreCase))
                {
                    return adapter;
                }
            }
            return null;
        }

        /// <summary>
        /// True when one of the tokens appears in the adapter name as a whole word.
        /// The boundary check is not cosmetic. "ATI" occurs inside ordinary English words -
        /// workstation, application, cinematic, innovation - and a bare substring match turns
        /// "Workstation Virtual Display" into an AMD adapter. That would make the one branch which
        /// is supposed to refuse to guess guess confidently and wrongly, building an AMD proxy on
        /// a machine with no usable AMD GPU instead of showing the chooser.
        /// Only letters break a match, never digits: "ATI2VGA" and "AMD780G Integrated Graphics"
        /// are real adapter names and have to keep matching.
        /// </summary>
        private static bool ContainsAnyToken(string adapterName, string[] tokens)
        {
            foreach (string token in tokens)
            {
                int index = adapterName.IndexOf(token, StringComparison.OrdinalIgnoreCase);
                while (index >= 0)
                {
                    if (IsWordBoundary(adapterName, index - 1) &&
                        IsWordBoundary(adapterName, index + token.Length))
                    {
                        return true;
                    }
                    // Keep looking: an earlier glued occurrence must not hide a later real one,
                    // as in "Innovation Radeon Display".
                    index = adapterName.IndexOf(token, index + 1, StringComparison.OrdinalIgnoreCase);
                }
            }
            return false;
        }

        private static bool IsWordBoundary(string adapterName, int index)
        {
            return index < 0 || index >= adapterName.Length || !char.IsLetter(adapterName[index]);
        }

        private static bool IsAdapterAvailable(string dllName)
        {
            try
            {
                return LoadLibrary(dllName) != IntPtr.Zero;
            }
            catch (Exception)
            {
                return false;
            }    
        }
    }
}
