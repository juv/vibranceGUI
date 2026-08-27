using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace vibrance.GUI.common
{
    /// <summary>
    /// The vibrance path's counterpart to DeviceGammaRampHelper's restore work-list
    /// (_devicesHoldingGameRamp). Neither vendor proxy previously kept any record of which
    /// display(s) it had actually written a GAME vibrance level to - NVIDIA hijacked a single
    /// "defaultHandle" field that a later game overwrote and a resolution-scoped restore never
    /// touched (issue #144), and the restore itself was gated on "is the currently focused screen
    /// the one the game used" instead of "which screen(s) actually need restoring" (issue #95).
    /// Both proxies share this one work-list because exactly one vendor proxy is ever constructed
    /// per process (see Program.cs's vendor selection) - there is never a question of which
    /// proxy's records these are.
    /// </summary>
    internal static class VibranceRestoreHelper
    {
        // Screen.DeviceName of every display this application has written a GAME vibrance level to
        // and has not yet written the Windows level back to. Same role and rule as
        // DeviceGammaRampHelper._devicesHoldingGameRamp: added only when a write actually landed,
        // drained by restore. Static and shared by whichever vendor proxy this process constructed -
        // exactly one is ever built. Everything runs on the UI thread (proxy construction,
        // WINEVENT_OUTOFCONTEXT callbacks, Form1_FormClosing), so deliberately unsynchronised, like
        // the _gameScreen/_vibranceInfo statics beside it. Keys are \\.\DISPLAYn, a namespace the OS
        // bounds.
        private static readonly HashSet<string> _displaysHoldingGameLevel = new HashSet<string>();

        /// <summary>
        /// Marks deviceName as owing a restore to the Windows vibrance level. A no-op for null or
        /// empty - never adds a key that ComposeRestoreTargets or a foreach over the set could not
        /// meaningfully act on.
        /// </summary>
        internal static void RecordGameLevelApplied(string deviceName)
        {
            if (string.IsNullOrEmpty(deviceName))
            {
                return;
            }
            _displaysHoldingGameLevel.Add(deviceName);
        }

        /// <summary>
        /// Drops deviceName from the work-list, normally once its restore has actually landed (or
        /// is confirmed unnecessary via a read-back). A no-op for null, empty, or a name not
        /// currently held.
        /// </summary>
        internal static void ClearGameLevelRecord(string deviceName)
        {
            if (string.IsNullOrEmpty(deviceName))
            {
                return;
            }
            _displaysHoldingGameLevel.Remove(deviceName);
        }

        /// <summary>
        /// The "affectPrimaryMonitorOnly == false" exit: that path restores every display through
        /// its own all-displays call, not one at a time through ComposeRestoreTargets, so the
        /// work-list is cleared in one step by whoever just did that restore.
        /// </summary>
        internal static void ClearAllGameLevelRecords()
        {
            _displaysHoldingGameLevel.Clear();
        }

        internal static int HoldingCount
        {
            get { return _displaysHoldingGameLevel.Count; }
        }

        /// <summary>
        /// The set of displays a restore must visit: every display currently on the work-list,
        /// plus - only when affectPrimaryMonitorOnly is true - the primary display, appended once,
        /// even if it is not itself on the work-list (the Windows Vibrance Level always owns the
        /// primary; it does not need to have been "applied to" first to be due a restore). Never
        /// null, never contains a null or empty entry, never contains a duplicate.
        ///
        /// When affectPrimaryMonitorOnly is false the primary is deliberately NOT appended - that
        /// scope is the pre-existing "every enumerated display handle" restore, which the caller
        /// drives through its own list, not through this one. The caller must call
        /// ClearAllGameLevelRecords() itself after that restore; this method does not mutate
        /// anything, so it does not do that on the caller's behalf when the flag is false.
        /// </summary>
        internal static List<string> ComposeRestoreTargets(bool affectPrimaryMonitorOnly, string primaryDeviceName)
        {
            List<string> targets = new List<string>(_displaysHoldingGameLevel);
            if (affectPrimaryMonitorOnly && !string.IsNullOrEmpty(primaryDeviceName) && !targets.Contains(primaryDeviceName))
            {
                targets.Add(primaryDeviceName);
            }
            return targets;
        }

        /// <summary>
        /// Screen.PrimaryScreen.DeviceName, or null when there is no primary screen (not reachable
        /// on a real machine, but Screen.PrimaryScreen is itself documented as nullable).
        /// </summary>
        internal static string GetPrimaryDeviceName()
        {
            Screen primary = Screen.PrimaryScreen;
            return primary == null ? null : primary.DeviceName;
        }

        // Exists for VibranceRestoreFixture only - production code never needs to blank this
        // out mid-run. Mirrors DeviceGammaRampHelper.ResetForTests, which exists for the same
        // reason.
        internal static void ResetForTests()
        {
            _displaysHoldingGameLevel.Clear();
        }
    }
}
