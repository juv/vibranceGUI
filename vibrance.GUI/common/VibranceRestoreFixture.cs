using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using vibrance.GUI.AMD;
using vibrance.GUI.AMD.vendor;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI.common
{
    /// <summary>
    /// Regression coverage for the vibrance restore fix: issues #60/#36 ("Affect Primary Monitor
    /// only" resetting a second monitor's level on every launch or restore) and #144/#95 (vibrance
    /// surviving the game closing, or an alt-tab to another monitor, because restore was scoped to
    /// whichever screen currently has focus instead of to whatever this application actually
    /// changed). No GUI, no live GPU driver - the NVIDIA half is driven entirely through
    /// INvidiaVibranceDevice and NvidiaDynamicVibranceProxy's ResetForTests seam, and the AMD half
    /// through IAmdAdapter, a fake implementation of the same interface AmdDynamicVibranceProxy's
    /// constructor already takes. Run by vibrance.GUI.exe --selftest-vibrance.
    ///
    /// Deliberately has no hardware variant and must never grow one: a fixture that actually
    /// changed a real display's vibrance would be changing exactly the state these four issues are
    /// about, on whatever machine happens to run the regression suite.
    /// </summary>
    public static class VibranceRestoreFixture
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        public static List<string> Run()
        {
            Checklist checklist = new Checklist();
            checklist.Lines.Add("vibranceGUI vibrance restore self test");
            checklist.Lines.Add(string.Empty);

            RunPureChecks(checklist);
            RunNvidiaChecks(checklist);
            RunAmdChecks(checklist);

            checklist.Lines.Add(string.Empty);
            checklist.Lines.Add(string.Format("PASSED {0}/{1}", checklist.Passed, checklist.Total));
            return checklist.Lines;
        }

        // ------------------------------------------------------------------
        // Pure - VibranceRestoreHelper.ComposeRestoreTargets, no fakes at all.
        // ------------------------------------------------------------------

        private static void RunPureChecks(Checklist checklist)
        {
            checklist.Lines.Add("ComposeRestoreTargets (pure):");

            CheckComposeAppendsPrimaryOnce(checklist);
            CheckComposeDoesNotDuplicatePrimaryAlreadyOnWorkList(checklist);
            CheckComposeToleratesMissingPrimary(checklist);
            CheckComposeAppendsPrimaryEvenWithAnEmptyWorkList(checklist);
            CheckComposeNeverAppendsPrimaryWithFlagOff(checklist);

            checklist.Lines.Add(string.Empty);
        }

        // V1. Mutation this guards: don't append the primary at all.
        private static void CheckComposeAppendsPrimaryOnce(Checklist checklist)
        {
            VibranceRestoreHelper.ResetForTests();
            VibranceRestoreHelper.RecordGameLevelApplied("D2");
            VibranceRestoreHelper.RecordGameLevelApplied("D3");

            List<string> targets = VibranceRestoreHelper.ComposeRestoreTargets(true, "D1");

            checklist.Check(SequenceEqual(targets, new List<string> { "D2", "D3", "D1" }),
                string.Format("V1: work-list {{D2,D3}} + primary D1, flag on -> [D2,D3,D1], got [{0}]",
                    string.Join(",", targets)));
        }

        // V2. Mutation this guards: append the primary unconditionally, even when it duplicates a
        // work-list entry.
        private static void CheckComposeDoesNotDuplicatePrimaryAlreadyOnWorkList(Checklist checklist)
        {
            VibranceRestoreHelper.ResetForTests();
            VibranceRestoreHelper.RecordGameLevelApplied("D1");

            List<string> targets = VibranceRestoreHelper.ComposeRestoreTargets(true, "D1");

            checklist.Check(targets.Count == 1 && targets[0] == "D1",
                string.Format("V2: work-list {{D1}} + primary D1 -> [D1] (length 1, no duplicate), got [{0}]",
                    string.Join(",", targets)));
        }

        // V3. Mutation this guards: append primaryDeviceName unguarded, even when it is null or
        // empty.
        private static void CheckComposeToleratesMissingPrimary(Checklist checklist)
        {
            VibranceRestoreHelper.ResetForTests();
            VibranceRestoreHelper.RecordGameLevelApplied("D2");

            List<string> nullPrimaryTargets = VibranceRestoreHelper.ComposeRestoreTargets(true, null);
            List<string> emptyPrimaryTargets = VibranceRestoreHelper.ComposeRestoreTargets(true, string.Empty);

            bool ok = SequenceEqual(nullPrimaryTargets, new List<string> { "D2" }) &&
                SequenceEqual(emptyPrimaryTargets, new List<string> { "D2" });
            checklist.Check(ok, string.Format(
                "V3: a null or empty primary leaves the work-list alone, no null/empty entry - got null=[{0}], empty=[{1}]",
                string.Join(",", nullPrimaryTargets), string.Join(",", emptyPrimaryTargets)));
        }

        // V4. Mutation this guards: return early (an empty list) when the work-list itself is
        // empty, skipping the primary append.
        private static void CheckComposeAppendsPrimaryEvenWithAnEmptyWorkList(Checklist checklist)
        {
            VibranceRestoreHelper.ResetForTests();

            List<string> targets = VibranceRestoreHelper.ComposeRestoreTargets(true, "D1");

            checklist.Check(targets.Count == 1 && targets[0] == "D1",
                string.Format("V4: empty work-list + primary D1, flag on -> [D1], got [{0}]", string.Join(",", targets)));
        }

        // V5. Mutation this guards: append the primary regardless of affectPrimaryMonitorOnly.
        private static void CheckComposeNeverAppendsPrimaryWithFlagOff(Checklist checklist)
        {
            VibranceRestoreHelper.ResetForTests();
            VibranceRestoreHelper.RecordGameLevelApplied("D2");

            List<string> targets = VibranceRestoreHelper.ComposeRestoreTargets(false, "D1");

            checklist.Check(SequenceEqual(targets, new List<string> { "D2" }),
                string.Format("V5: flag off -> work-list alone, primary never appended, got [{0}]", string.Join(",", targets)));
        }

        // ------------------------------------------------------------------
        // NVIDIA - ApplyGameVibranceLevel/RestoreWindowsVibranceLevel via ResetForTests + a fake
        // device that records every (handle, level) it is asked to write.
        // ------------------------------------------------------------------

        private static void RunNvidiaChecks(Checklist checklist)
        {
            checklist.Lines.Add("NVIDIA apply/restore (via NvidiaDynamicVibranceProxy's fixture seam):");

            CheckNvidiaRestoreIgnoresDisplayHandlesWithFlagOn(checklist);
            CheckNvidiaRestoreReachesEveryHeldDisplay(checklist);
            CheckNvidiaRestoreDrainsWorkList(checklist);
            CheckNvidiaRestoreRetriesFailedWrite(checklist);
            CheckNvidiaRestoreGuardsUnresolvableHandle(checklist);
            CheckNvidiaRestoreSkipsWriteWhenAlreadyAtLevel(checklist);
            CheckNvidiaApplyOnlyRecordsALandedWrite(checklist);
            CheckNvidiaRestoreAllDisplaysBranchUnchanged(checklist);
            CheckNvidiaHandleDvcExitRestoresPrimaryGuardedByReadBack(checklist);
            CheckNvidiaRestoreReachesRealCallSiteRegardlessOfGameScreen(checklist);
            CheckNvidiaRestoreIsNoOpBeforeWindowsLevelKnown(checklist);
            CheckNvidiaRestoreSkipsInvalidHandlesInAllDisplaysBranch(checklist);

            checklist.Lines.Add(string.Empty);
        }

        // N13 (NB2). Mutation this guards: drop the "handle == -1 || handle == 0" skip from the
        // flag-off branch's write loop and/or AllDisplaysAtLevel. EnumerateDisplayHandles's loop
        // stops as soon as enumerateNvidiaDisplayHandle returns -1, but nothing filters a 0 it may
        // still hand back as one of the earlier entries, so this is reachable in production, not
        // hypothetical. N9 below seeds only valid handles and so cannot catch this on its own.
        private static void CheckNvidiaRestoreSkipsInvalidHandlesInAllDisplaysBranch(Checklist checklist)
        {
            FakeNvidiaVibranceDevice device = new FakeNvidiaVibranceDevice();
            NvidiaDynamicVibranceProxy.ResetForTests(device, new VibranceInfo(), new List<ApplicationSetting>());

            List<int> displayHandles = new List<int> { -1, 0, 501 };
            const int windowsLevel = 33;

            NvidiaDynamicVibranceProxy.RestoreWindowsVibranceLevel(device, false, null, displayHandles, windowsLevel, true);

            bool onlyValidHandleWritten = device.SetLevelCalls.Count == 1 && device.SetLevelCalls[0] == 501;
            checklist.Check(onlyValidHandleWritten, string.Format(
                "N13: SetLevel is only called for the valid handle (501), never for -1 or 0, got [{0}]",
                string.Join(",", device.SetLevelCalls)));
        }

        // N12 (NB1). Mutation this guards: drop the isWindowsLevelKnown guard from
        // RestoreWindowsVibranceLevel (or otherwise let it write while the level is still
        // unknown) - reopening the arbitrary-write window between the hook subscribing and
        // SetVibranceWindowsLevel actually running (see VibranceInfo.isWindowsLevelKnown and
        // InitializeProxy's comment). A fresh VibranceInfo()'s isWindowsLevelKnown is false (the
        // struct default), matching real state before SetVibranceWindowsLevel has ever run once.
        private static void CheckNvidiaRestoreIsNoOpBeforeWindowsLevelKnown(Checklist checklist)
        {
            FakeNvidiaVibranceDevice device = new FakeNvidiaVibranceDevice();
            NvidiaDynamicVibranceProxy.ResetForTests(device, new VibranceInfo(), new List<ApplicationSetting>());

            const string deviceName = "\\\\.\\DISPLAY_TESTONLY_N12";
            const string primary = "\\\\.\\DISPLAY_TESTONLY_N12_PRIMARY";
            VibranceRestoreHelper.RecordGameLevelApplied(deviceName);

            NvidiaDynamicVibranceProxy.RestoreWindowsVibranceLevel(device, true, primary, new List<int>(), 30, false);

            checklist.Check(device.SetLevelCalls.Count == 0,
                "N12: RestoreWindowsVibranceLevel is a no-op when isWindowsLevelKnown is false, even with a non-empty work-list and a primary target");
            checklist.Check(VibranceRestoreHelper.HoldingCount == 1,
                "N12: the work-list is left untouched too - nothing was drained by a call that wrote nothing");
        }

        // N2 (#60/#36). Mutation this guards: take the pre-existing "displayHandles" all-displays
        // branch even though affectPrimaryMonitorOnly is true.
        private static void CheckNvidiaRestoreIgnoresDisplayHandlesWithFlagOn(Checklist checklist)
        {
            FakeNvidiaVibranceDevice device = new FakeNvidiaVibranceDevice();
            NvidiaDynamicVibranceProxy.ResetForTests(device, new VibranceInfo(), new List<ApplicationSetting>());

            const string deviceName = "\\\\.\\DISPLAY_TESTONLY_N2";
            const int windowsLevel = 40;
            VibranceRestoreHelper.RecordGameLevelApplied(deviceName);
            string primaryDeviceName = VibranceRestoreHelper.GetPrimaryDeviceName();

            // Three decoy handles that would be written if this fell back to the all-displays
            // branch - it must not, with the flag on.
            List<int> decoyDisplayHandles = new List<int> { 9001, 9002, 9003 };

            NvidiaDynamicVibranceProxy.RestoreWindowsVibranceLevel(device, true, primaryDeviceName, decoyDisplayHandles, windowsLevel, true);

            checklist.Check(device.SetLevelCalls.Count == 2,
                string.Format("N2: exactly two SetLevel calls (the work-list entry + the primary), got {0}", device.SetLevelCalls.Count));

            bool noDecoyTouched = !device.SetLevelCalls.Contains(9001) && !device.SetLevelCalls.Contains(9002) && !device.SetLevelCalls.Contains(9003);
            checklist.Check(noDecoyTouched, "N2: none of the three decoy displayHandles entries were written to");
        }

        // N3 (defaultHandle hijack). Mutation this guards: restore only the last-applied handle
        // (the pre-fix "_vibranceInfo.defaultHandle = displayHandle" hijack), not every display
        // actually holding a game level.
        private static void CheckNvidiaRestoreReachesEveryHeldDisplay(Checklist checklist)
        {
            FakeNvidiaVibranceDevice device = new FakeNvidiaVibranceDevice();
            NvidiaDynamicVibranceProxy.ResetForTests(device, new VibranceInfo(), new List<ApplicationSetting>());

            const string d1 = "\\\\.\\DISPLAY_TESTONLY_N3_D1";
            const string d2 = "\\\\.\\DISPLAY_TESTONLY_N3_D2";
            const string d3 = "\\\\.\\DISPLAY_TESTONLY_N3_D3";
            const int windowsLevel = 25;

            VibranceRestoreHelper.RecordGameLevelApplied(d2);
            VibranceRestoreHelper.RecordGameLevelApplied(d3);

            NvidiaDynamicVibranceProxy.RestoreWindowsVibranceLevel(device, true, d1, new List<int>(), windowsLevel, true);

            int h1 = device.HandleFor(d1);
            int h2 = device.HandleFor(d2);
            int h3 = device.HandleFor(d3);

            bool distinctHandles = h1 != h2 && h2 != h3 && h1 != h3;
            bool allWritten = device.IsAtLevel(h1, windowsLevel) && device.IsAtLevel(h2, windowsLevel) && device.IsAtLevel(h3, windowsLevel);
            checklist.Check(distinctHandles && allWritten,
                "N3: all three distinct displays (two held from the work-list, one the primary) are written to the Windows level in a single restore");
        }

        // N4 (drain). Mutation this guards: never clear a display from the work-list once it is
        // restored.
        private static void CheckNvidiaRestoreDrainsWorkList(Checklist checklist)
        {
            FakeNvidiaVibranceDevice device = new FakeNvidiaVibranceDevice();
            NvidiaDynamicVibranceProxy.ResetForTests(device, new VibranceInfo(), new List<ApplicationSetting>());

            const string primary = "\\\\.\\DISPLAY_TESTONLY_N4_PRIMARY";
            const string worklistDevice = "\\\\.\\DISPLAY_TESTONLY_N4";
            const int windowsLevel = 12;

            VibranceRestoreHelper.RecordGameLevelApplied(worklistDevice);
            NvidiaDynamicVibranceProxy.RestoreWindowsVibranceLevel(device, true, primary, new List<int>(), windowsLevel, true);

            checklist.Check(VibranceRestoreHelper.HoldingCount == 0,
                string.Format("N4: HoldingCount is 0 after a fully successful restore, got {0}", VibranceRestoreHelper.HoldingCount));

            device.ResolvedDeviceNames.Clear();
            NvidiaDynamicVibranceProxy.RestoreWindowsVibranceLevel(device, true, primary, new List<int>(), windowsLevel, true);

            checklist.Check(device.ResolvedDeviceNames.Count == 1 && device.ResolvedDeviceNames[0] == primary,
                string.Format("N4: the second restore's scope is the primary alone - worklistDevice was already drained, got [{0}]",
                    string.Join(",", device.ResolvedDeviceNames)));
        }

        // N5 (retry). Mutation this guards: drain a display from the work-list unconditionally,
        // even when its write failed.
        private static void CheckNvidiaRestoreRetriesFailedWrite(Checklist checklist)
        {
            FakeNvidiaVibranceDevice device = new FakeNvidiaVibranceDevice();
            NvidiaDynamicVibranceProxy.ResetForTests(device, new VibranceInfo(), new List<ApplicationSetting>());

            const string d2 = "\\\\.\\DISPLAY_TESTONLY_N5";
            const int windowsLevel = 8;

            VibranceRestoreHelper.RecordGameLevelApplied(d2);
            device.FailNextSetLevel(d2);

            NvidiaDynamicVibranceProxy.RestoreWindowsVibranceLevel(device, true, null, new List<int>(), windowsLevel, true);

            checklist.Check(VibranceRestoreHelper.HoldingCount == 1,
                string.Format("N5: a display whose write failed is still on the work-list, got HoldingCount={0}", VibranceRestoreHelper.HoldingCount));

            // The underlying failure is gone now (FailNextSetLevel only failed the one call) - the
            // next restore (as the next foreground event would trigger) must retry and succeed.
            NvidiaDynamicVibranceProxy.RestoreWindowsVibranceLevel(device, true, null, new List<int>(), windowsLevel, true);
            checklist.Check(VibranceRestoreHelper.HoldingCount == 0,
                "N5: a later restore, with the underlying failure gone, drains it");
        }

        // N6 (unresolvable). Mutation this guards: skip the -1/0 guard and pass an unresolved
        // handle straight to SetLevel.
        private static void CheckNvidiaRestoreGuardsUnresolvableHandle(Checklist checklist)
        {
            FakeNvidiaVibranceDevice device = new FakeNvidiaVibranceDevice();
            NvidiaDynamicVibranceProxy.ResetForTests(device, new VibranceInfo(), new List<ApplicationSetting>());

            const string d2 = "\\\\.\\DISPLAY_TESTONLY_N6";
            const int windowsLevel = 5;

            VibranceRestoreHelper.RecordGameLevelApplied(d2);
            device.SetUnresolvable(d2);

            NvidiaDynamicVibranceProxy.RestoreWindowsVibranceLevel(device, true, null, new List<int>(), windowsLevel, true);

            bool neverCalledWithInvalidHandle = !device.SetLevelCalls.Contains(-1) && !device.SetLevelCalls.Contains(0);
            checklist.Check(neverCalledWithInvalidHandle && device.SetLevelCalls.Count == 0,
                "N6: SetLevel was never called - not with -1, not with 0 - for a display NvAPI cannot resolve");
            checklist.Check(VibranceRestoreHelper.HoldingCount == 1, "N6: the unresolvable display stays on the work-list");
        }

        // N7 (read-back). Mutation this guards: drop the IsAtLevel check and always call SetLevel.
        private static void CheckNvidiaRestoreSkipsWriteWhenAlreadyAtLevel(Checklist checklist)
        {
            FakeNvidiaVibranceDevice device = new FakeNvidiaVibranceDevice();
            NvidiaDynamicVibranceProxy.ResetForTests(device, new VibranceInfo(), new List<ApplicationSetting>());

            const string d2 = "\\\\.\\DISPLAY_TESTONLY_N7";
            const int windowsLevel = 17;

            VibranceRestoreHelper.RecordGameLevelApplied(d2);
            device.SeedLevel(d2, windowsLevel);

            NvidiaDynamicVibranceProxy.RestoreWindowsVibranceLevel(device, true, null, new List<int>(), windowsLevel, true);

            checklist.Check(device.SetLevelCalls.Count == 0, "N7: no SetLevel call was made for a display already at the Windows level");
            checklist.Check(VibranceRestoreHelper.HoldingCount == 0, "N7: it is still drained from the work-list via the read-back alone");
        }

        // N8 (record only what landed). Drives the REAL, private static OnWinEventHook via
        // reflection (ResetForTests swaps in the fake device and a matched ApplicationSetting, no
        // constructor and so no initializeLibrary() involved) rather than re-implementing
        // OnWinEventHook's own "if (ApplyGameVibranceLevel(...)) RecordGameLevelApplied(...)" gate
        // inline here - a regression that made that call site record unconditionally would not be
        // caught by testing ApplyGameVibranceLevel's return value alone. neverChangeResolution is
        // forced true so this reaches only the vibrance apply branch, not a real resolution change
        // on the machine running this fixture. Mutation this guards: record a display as owing a
        // restore before checking ApplyGameVibranceLevel's result (e.g. moving
        // RecordGameLevelApplied outside the "if").
        private static void CheckNvidiaApplyOnlyRecordsALandedWrite(Checklist checklist)
        {
            FakeNvidiaVibranceDevice device = new FakeNvidiaVibranceDevice();

            const int ingameLevel = 60;
            ApplicationSetting matchingSetting = new ApplicationSetting();
            matchingSetting.Name = "TestGameN8";
            matchingSetting.IngameLevel = ingameLevel;
            List<ApplicationSetting> settings = new List<ApplicationSetting> { matchingSetting };

            VibranceInfo vibranceInfo = new VibranceInfo();
            vibranceInfo.neverChangeResolution = true;
            NvidiaDynamicVibranceProxy.ResetForTests(device, vibranceInfo, settings);

            IntPtr desktop = GetDesktopWindow();
            string gameDeviceName = Screen.FromHandle(desktop).DeviceName;
            device.FailNextSetLevel(gameDeviceName);

            MethodInfo onWinEventHook = typeof(NvidiaDynamicVibranceProxy).GetMethod(
                "OnWinEventHook", BindingFlags.NonPublic | BindingFlags.Static);
            WinEventHookEventArgs args = new WinEventHookEventArgs
            {
                Handle = desktop,
                ProcessName = "TestGameN8"
            };
            onWinEventHook.Invoke(null, new object[] { null, args });

            checklist.Check(device.SetLevelCalls.Count == 1,
                "N8: the apply branch actually attempted the (failing) write");
            checklist.Check(VibranceRestoreHelper.HoldingCount == 0,
                string.Format("N8: OnWinEventHook's apply branch never records a display as owing a restore when the write failed, got HoldingCount={0}", VibranceRestoreHelper.HoldingCount));
        }

        // N9 (flag off unchanged). PIN, not regression evidence for this fix - it passes against a
        // correct pre-fix implementation too. The affectPrimaryMonitorOnly == false branch was
        // never gated on focus and is not part of issues #60/#36/#95/#144; this only proves
        // RestoreWindowsVibranceLevel's refactor left it byte-for-byte equivalent to the pre-fix
        // "displayHandles.TrueForAll(...)/.ForEach(...)" code.
        private static void CheckNvidiaRestoreAllDisplaysBranchUnchanged(Checklist checklist)
        {
            FakeNvidiaVibranceDevice device = new FakeNvidiaVibranceDevice();
            NvidiaDynamicVibranceProxy.ResetForTests(device, new VibranceInfo(), new List<ApplicationSetting>());

            const string leftoverWorkListDevice = "\\\\.\\DISPLAY_TESTONLY_N9_LEFTOVER";
            VibranceRestoreHelper.RecordGameLevelApplied(leftoverWorkListDevice);

            List<int> displayHandles = new List<int> { 501, 502, 503 };
            const int windowsLevel = 33;

            NvidiaDynamicVibranceProxy.RestoreWindowsVibranceLevel(device, false, null, displayHandles, windowsLevel, true);

            bool allWrittenOnce = device.SetLevelCalls.Count == 3 &&
                device.SetLevelCalls.Contains(501) && device.SetLevelCalls.Contains(502) && device.SetLevelCalls.Contains(503);
            checklist.Check(allWrittenOnce, string.Format(
                "N9 (pin, passes pre-fix too): all three seeded displayHandles entries were written exactly once each, got [{0}]",
                string.Join(",", device.SetLevelCalls)));

            checklist.Check(VibranceRestoreHelper.HoldingCount == 0,
                "N9 (pin, passes pre-fix too): the work-list is dropped once the all-displays branch runs, even though nothing on it was individually restored");
        }

        // N10 (HandleDvcExit). Mutation this guards: write the Windows level unconditionally, as
        // the pre-fix "setDVCLevel(_vibranceInfo.defaultHandle, ...)" in HandleDvcExit did, instead
        // of consulting the read-back first.
        private static void CheckNvidiaHandleDvcExitRestoresPrimaryGuardedByReadBack(Checklist checklist)
        {
            FakeNvidiaVibranceDevice device = new FakeNvidiaVibranceDevice();
            NvidiaDynamicVibranceProxy.ResetForTests(device, new VibranceInfo(), new List<ApplicationSetting>());

            const string primary = "\\\\.\\DISPLAY_TESTONLY_N10_PRIMARY";
            const int windowsLevel = 21;

            // Stands in for HandleDvcExit's own call, with an empty work-list and the flag on.
            NvidiaDynamicVibranceProxy.RestoreWindowsVibranceLevel(device, true, primary, new List<int>(), windowsLevel, true);
            checklist.Check(device.SetLevelCalls.Count == 1 && device.SetLevelCalls[0] == device.HandleFor(primary),
                "N10: the first call (empty work-list, flag on) writes the primary exactly once");

            // A later call - as a second HandleDvcExit-shaped restore, or the next foreground event
            // - with the primary already at the Windows level, must make no SetLevel call at all:
            // the read-back decides, not an unconditional write.
            device.SetLevelCalls.Clear();
            NvidiaDynamicVibranceProxy.RestoreWindowsVibranceLevel(device, true, primary, new List<int>(), windowsLevel, true);
            checklist.Check(device.SetLevelCalls.Count == 0,
                "N10: a later call, with the primary already at the Windows level, makes no SetLevel call");
        }

        // N11 (#95/#144). N2-N10 above all call RestoreWindowsVibranceLevel DIRECTLY - that seam
        // has no "current screen" parameter at all, so none of them can see a gate wrapped AROUND
        // the call, which is exactly where the pre-fix
        // "if (_vibranceInfo.affectPrimaryMonitorOnly && !equalsDVCLevel(defaultHandle,
        // userVibranceSettingDefault)) { if (_gameScreen == null ||
        // _gameScreen.DeviceName.Equals(currentScreen.DeviceName)) { ... } else { return; } }"
        // gate sat, in OnWinEventHook itself (immediately above the call this replaced) - upstream's
        // actual pre-fix shape inverts that inner condition into "if (_gameScreen != null &&
        // !_gameScreen.DeviceName.Equals(currentScreen.DeviceName)) { return; }", an early return
        // out of the whole handler, but the effect on this check is the same: a mismatch between
        // _gameScreen and the restore event's own screen must not block the write. This is the only
        // NVIDIA check that reaches the real call site: it reflects into the actual private static
        // OnWinEventHook (as N8 already does for the apply branch) with _gameScreen forced - via
        // reflection, Screen has no public constructor - to a real screen DIFFERENT from the one the
        // restore event itself resolves to, then asserts the write still happens. A version of this
        // fixture that called RestoreWindowsVibranceLevel directly with a synthetic DeviceName
        // instead, asserting the same "gets written" outcome, could never have caught a regression
        // that reinstated the gate around the call site - only reflecting into OnWinEventHook itself
        // can. Confirmed by mutation: temporarily reinstating upstream's original gate around the
        // RestoreWindowsVibranceLevel call site turns this specific check red while N2-N10 stay
        // green.
        //
        // Needs two real, distinct monitors to force a mismatch - Skip on a single-monitor machine,
        // the convention the AMD checks below already use for their own real-focus dependency.
        private static void CheckNvidiaRestoreReachesRealCallSiteRegardlessOfGameScreen(Checklist checklist)
        {
            IntPtr desktop = GetDesktopWindow();
            Screen currentScreen = Screen.FromHandle(desktop);
            Screen otherScreen = null;
            foreach (Screen candidate in Screen.AllScreens)
            {
                if (!candidate.DeviceName.Equals(currentScreen.DeviceName))
                {
                    otherScreen = candidate;
                    break;
                }
            }
            if (otherScreen == null)
            {
                checklist.Skip("N11: the real OnWinEventHook restore branch reaches RestoreWindowsVibranceLevel regardless of _gameScreen - only one real monitor is attached, cannot force _gameScreen to differ from the current screen");
                return;
            }

            FakeNvidiaVibranceDevice device = new FakeNvidiaVibranceDevice();
            VibranceInfo vibranceInfo = new VibranceInfo();
            vibranceInfo.affectPrimaryMonitorOnly = true;
            vibranceInfo.neverChangeResolution = true;
            // Otherwise RestoreWindowsVibranceLevel's own isWindowsLevelKnown guard (added for NB1)
            // would make this a no-op regardless of the gate under test.
            vibranceInfo.isWindowsLevelKnown = true;
            // See CreateNonMatchingApplicationSetting's comment below (AMD section) - NVIDIA's
            // OnWinEventHook gates its whole body on "_applicationSettings.Count > 0" the same
            // way, so an empty list here would never reach the restore branch this check
            // exercises.
            List<ApplicationSetting> nonMatchingSettings = new List<ApplicationSetting> { CreateNonMatchingApplicationSetting() };
            NvidiaDynamicVibranceProxy.ResetForTests(device, vibranceInfo, nonMatchingSettings);

            // _gameScreen has no production setter reachable without a live game event - reflection
            // is the only way to force it here, same justification as N8 reflecting into
            // OnWinEventHook itself.
            FieldInfo gameScreenField = typeof(NvidiaDynamicVibranceProxy).GetField(
                "_gameScreen", BindingFlags.NonPublic | BindingFlags.Static);
            gameScreenField.SetValue(null, otherScreen);

            MethodInfo onWinEventHook = typeof(NvidiaDynamicVibranceProxy).GetMethod(
                "OnWinEventHook", BindingFlags.NonPublic | BindingFlags.Static);
            // GetDesktopWindow() is a stable handle - unlike AMD's restore branch, NVIDIA's checks
            // isWindowActive through the fake device (always true here), never the real foreground
            // window, so there is no focus race to guard against for this one.
            WinEventHookEventArgs args = new WinEventHookEventArgs
            {
                Handle = desktop,
                ProcessName = "doesnotmatter"
            };
            onWinEventHook.Invoke(null, new object[] { null, args });

            checklist.Check(device.SetLevelCalls.Count > 0,
                "N11: the restore branch's real call site still restores the Windows level when _gameScreen names a different real screen than the current one - 0 SetLevel calls means a focus/current-screen gate has been reinstated around the call");
        }

        // ------------------------------------------------------------------
        // AMD - the real OnWinEventHook via reflection, plus a FakeAmdAdapter. These would run
        // against a build with no VibranceRestoreHelper recording/restore (i.e. this repository
        // before the fix commit) and FAIL there - see each check's own comment for which ones are
        // pins instead.
        // ------------------------------------------------------------------

        private static void RunAmdChecks(Checklist checklist)
        {
            checklist.Lines.Add("AMD apply/restore (real OnWinEventHook via reflection + FakeAmdAdapter):");

            CheckAmdRestoreConsultsAffectPrimaryMonitorOnly(checklist);
            CheckAmdRestoreTargetsGamesScreenAfterApply(checklist);
            CheckAmdRestoreDrainsWorkList(checklist);
            CheckAmdRestoreAllDisplaysBranchUnchanged(checklist);
            CheckAmdRestoreWritesEveryDisplayRecordedByAnAllDisplaysApply(checklist);
            CheckAmdRestoreReachesRealCallSiteRegardlessOfGameScreen(checklist);

            checklist.Lines.Add(string.Empty);
        }

        // A1 (#60 on AMD). Mutation this guards: restore back to the unconditional
        // SetSaturationOnAllDisplays(...) call AmdDynamicVibranceProxy made regardless of
        // affectPrimaryMonitorOnly.
        private static void CheckAmdRestoreConsultsAffectPrimaryMonitorOnly(Checklist checklist)
        {
            VibranceRestoreHelper.ResetForTests();

            FakeAmdAdapter adapter = new FakeAmdAdapter();
            List<ApplicationSetting> nonMatchingSettings = new List<ApplicationSetting> { CreateNonMatchingApplicationSetting() };
            Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>> windowsResolutionSettings =
                new Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>>();

            AmdDynamicVibranceProxy proxy = new AmdDynamicVibranceProxy(adapter, nonMatchingSettings, windowsResolutionSettings);
            proxy.SetAffectPrimaryMonitorOnly(true);
            const int windowsLevel = 44;
            proxy.SetVibranceWindowsLevel(windowsLevel);

            MethodInfo onWinEventHook = GetOnWinEventHook();

            // This test is dependent on the real foreground window (GetForegroundWindow() is what
            // AmdDynamicVibranceProxy's own restore branch gates on, not a mockable seam), so its
            // precondition - the foreground window not changing mid test - is checked before and
            // after and the check is Skipped, not failed, if it was disturbed by something outside
            // this fixture's control.
            IntPtr foregroundBefore = AmdDynamicVibranceProxy.GetForegroundWindow();
            WinEventHookEventArgs args = new WinEventHookEventArgs
            {
                Handle = foregroundBefore,
                ProcessName = "doesnotmatter"
            };

            onWinEventHook.Invoke(proxy, new object[] { null, args });

            if (AmdDynamicVibranceProxy.GetForegroundWindow() != foregroundBefore)
            {
                checklist.Skip("A1: AMD restore honours affectPrimaryMonitorOnly - the foreground window changed mid test, its precondition was destroyed by a real focus change");
                return;
            }

            checklist.Check(adapter.SetSaturationOnAllDisplaysCallCount == 0,
                string.Format("A1: SetSaturationOnAllDisplays was never called with the flag on, got {0} calls", adapter.SetSaturationOnAllDisplaysCallCount));

            string expectedPrimary = VibranceRestoreHelper.GetPrimaryDeviceName();
            bool wroteThePrimaryOnce = adapter.SetSaturationOnDisplayNames.Count == 1 &&
                adapter.SetSaturationOnDisplayNames[0] == expectedPrimary &&
                adapter.SetSaturationOnDisplayLevels[0] == windowsLevel;
            checklist.Check(wroteThePrimaryOnce, string.Format(
                "A1: SetSaturationOnDisplay({0}, {1}) ran exactly once, got [{2}]", windowsLevel, expectedPrimary,
                string.Join(",", adapter.SetSaturationOnDisplayNames)));
        }

        // A2. Mutation this guards: restore the game's own screen from _gameScreen instead of from
        // the work-list (would still happen to pass while the work-list and _gameScreen agree, but
        // is checked here against ComposeRestoreTargets' actual scope, not the field).
        private static void CheckAmdRestoreTargetsGamesScreenAfterApply(Checklist checklist)
        {
            VibranceRestoreHelper.ResetForTests();

            const int ingameLevel = 250;
            const int windowsLevel = 44;
            ApplicationSetting matchingSetting = new ApplicationSetting();
            matchingSetting.Name = "TestGameA2";
            matchingSetting.IngameLevel = ingameLevel;

            List<ApplicationSetting> settings = new List<ApplicationSetting> { matchingSetting };
            FakeAmdAdapter adapter = new FakeAmdAdapter();
            Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>> windowsResolutionSettings =
                new Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>>();

            AmdDynamicVibranceProxy proxy = new AmdDynamicVibranceProxy(adapter, settings, windowsResolutionSettings);
            proxy.SetAffectPrimaryMonitorOnly(true);
            proxy.SetVibranceWindowsLevel(windowsLevel);
            // Not what this test checks; left on, it could touch this machine's real desktop
            // resolution, since ApplicationSetting.IsResolutionChangeNeeded defaults to false but
            // this guards against it regardless.
            proxy.SetNeverSwitchResolution(true);

            MethodInfo onWinEventHook = GetOnWinEventHook();

            IntPtr desktop = GetDesktopWindow();
            string gameDeviceName = Screen.FromHandle(desktop).DeviceName;

            WinEventHookEventArgs applyArgs = new WinEventHookEventArgs
            {
                Handle = desktop,
                ProcessName = "TestGameA2"
            };
            onWinEventHook.Invoke(proxy, new object[] { null, applyArgs });

            checklist.Check(adapter.SetSaturationOnDisplayNames.Count == 1 &&
                adapter.SetSaturationOnDisplayNames[0] == gameDeviceName &&
                adapter.SetSaturationOnDisplayLevels[0] == ingameLevel,
                "A2: the apply half wrote the ingame level to the game's own screen");

            // Independent of which physical monitor happens to be primary on the machine
            // running this fixture (the restore assertion below cannot tell "recorded, then
            // restored" apart from "never recorded, but restored anyway because it coincided
            // with the primary" when the game's own screen and the primary are the same
            // display) - this checks the recording mechanism itself, directly.
            checklist.Check(VibranceRestoreHelper.HoldingCount == 1, string.Format(
                "A2: exactly the game's own screen is recorded as owing a restore after the apply, got HoldingCount={0}",
                VibranceRestoreHelper.HoldingCount));

            IntPtr foregroundBefore = AmdDynamicVibranceProxy.GetForegroundWindow();
            WinEventHookEventArgs restoreArgs = new WinEventHookEventArgs
            {
                Handle = foregroundBefore,
                ProcessName = "doesnotmatter"
            };
            onWinEventHook.Invoke(proxy, new object[] { null, restoreArgs });

            if (AmdDynamicVibranceProxy.GetForegroundWindow() != foregroundBefore)
            {
                checklist.Skip("A2: AMD restore targets the game's screen - the foreground window changed mid test, its precondition was destroyed by a real focus change");
                return;
            }

            bool restoredGamesScreen = adapter.SetSaturationOnDisplayNames.Count == 2 &&
                adapter.SetSaturationOnDisplayNames[1] == gameDeviceName &&
                adapter.SetSaturationOnDisplayLevels[1] == windowsLevel;
            checklist.Check(restoredGamesScreen, string.Format(
                "A2: the restore half wrote the Windows level back to the same screen the game applied to, got [{0}]",
                string.Join(",", adapter.SetSaturationOnDisplayNames)));
        }

        // A3 (drain). Mutation this guards: never clear a display from the work-list once it is
        // restored (AMD's counterpart to N4/N5 - AMD has no read-back to retry against, so only
        // the drain itself is checked here).
        private static void CheckAmdRestoreDrainsWorkList(Checklist checklist)
        {
            VibranceRestoreHelper.ResetForTests();

            FakeAmdAdapter adapter = new FakeAmdAdapter();
            List<ApplicationSetting> nonMatchingSettings = new List<ApplicationSetting> { CreateNonMatchingApplicationSetting() };
            Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>> windowsResolutionSettings =
                new Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>>();

            AmdDynamicVibranceProxy proxy = new AmdDynamicVibranceProxy(adapter, nonMatchingSettings, windowsResolutionSettings);
            proxy.SetAffectPrimaryMonitorOnly(true);
            const int windowsLevel = 60;
            proxy.SetVibranceWindowsLevel(windowsLevel);

            const string extraDevice = "\\\\.\\DISPLAY_TESTONLY_A3_EXTRA";
            VibranceRestoreHelper.RecordGameLevelApplied(extraDevice);

            MethodInfo onWinEventHook = GetOnWinEventHook();

            IntPtr foregroundBefore = AmdDynamicVibranceProxy.GetForegroundWindow();
            WinEventHookEventArgs args = new WinEventHookEventArgs
            {
                Handle = foregroundBefore,
                ProcessName = "doesnotmatter"
            };

            onWinEventHook.Invoke(proxy, new object[] { null, args });

            if (AmdDynamicVibranceProxy.GetForegroundWindow() != foregroundBefore)
            {
                checklist.Skip("A3: AMD restore drains the work-list - the foreground window changed mid test, its precondition was destroyed by a real focus change");
                return;
            }

            checklist.Check(adapter.SetSaturationOnDisplayNames.Count == 2,
                string.Format("A3: the first restore wrote both the extra work-list entry and the primary, got {0} calls", adapter.SetSaturationOnDisplayNames.Count));
            checklist.Check(VibranceRestoreHelper.HoldingCount == 0, "A3: the work-list is empty after the first restore");

            onWinEventHook.Invoke(proxy, new object[] { null, args });

            if (AmdDynamicVibranceProxy.GetForegroundWindow() != foregroundBefore)
            {
                checklist.Skip("A3: AMD restore drains the work-list (second call) - the foreground window changed mid test, its precondition was destroyed by a real focus change");
                return;
            }

            bool secondCallOnlyPrimary = adapter.SetSaturationOnDisplayNames.Count == 3 &&
                adapter.SetSaturationOnDisplayNames[2] == VibranceRestoreHelper.GetPrimaryDeviceName();
            checklist.Check(secondCallOnlyPrimary, string.Format(
                "A3: the second restore only re-touches the primary, not the already-drained extra entry, got [{0}]",
                string.Join(",", adapter.SetSaturationOnDisplayNames)));
        }

        // A4 (flag off unchanged). PIN, not regression evidence - see N9's comment for the
        // convention. The affectPrimaryMonitorOnly == false branch already called
        // SetSaturationOnAllDisplays unconditionally before this fix too (both in the restore
        // branch and in HandleDvcExit); this only proves the refactor into
        // RestoreWindowsVibranceLevel left that branch equivalent.
        private static void CheckAmdRestoreAllDisplaysBranchUnchanged(Checklist checklist)
        {
            VibranceRestoreHelper.ResetForTests();

            FakeAmdAdapter adapter = new FakeAmdAdapter();
            List<ApplicationSetting> nonMatchingSettings = new List<ApplicationSetting> { CreateNonMatchingApplicationSetting() };
            Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>> windowsResolutionSettings =
                new Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>>();

            AmdDynamicVibranceProxy proxy = new AmdDynamicVibranceProxy(adapter, nonMatchingSettings, windowsResolutionSettings);
            // affectPrimaryMonitorOnly defaults to false - never set, so this exercises that
            // branch, not a chosen one.
            const int windowsLevel = 70;
            proxy.SetVibranceWindowsLevel(windowsLevel);

            MethodInfo onWinEventHook = GetOnWinEventHook();

            IntPtr foregroundBefore = AmdDynamicVibranceProxy.GetForegroundWindow();
            WinEventHookEventArgs args = new WinEventHookEventArgs
            {
                Handle = foregroundBefore,
                ProcessName = "doesnotmatter"
            };

            onWinEventHook.Invoke(proxy, new object[] { null, args });

            if (AmdDynamicVibranceProxy.GetForegroundWindow() != foregroundBefore)
            {
                checklist.Skip("A4: AMD restore with the flag off - the foreground window changed mid test, its precondition was destroyed by a real focus change");
                return;
            }

            checklist.Check(adapter.SetSaturationOnAllDisplaysCallCount == 1 && adapter.LastSetSaturationOnAllDisplaysLevel == windowsLevel,
                string.Format("A4 (pin, passes pre-fix too): SetSaturationOnAllDisplays ran exactly once with the Windows level ({0})", windowsLevel));
            checklist.Check(adapter.SetSaturationOnDisplayNames.Count == 0,
                "A4 (pin, passes pre-fix too): SetSaturationOnDisplay was never called with the flag off");
        }

        // A5 (toggle strand). Guard on the new recording mechanism itself, not regression evidence
        // for this fix - AMD's all-displays apply branch happens to cover every attached display by
        // accident even before the fix (it always called SetSaturationOnAllDisplays regardless of
        // the flag). The real, flag-scoped counterpart to this is N3 on the NVIDIA side. This exists
        // so a future change to the recording in the unchecked apply branch cannot silently start
        // recording (and therefore restoring) fewer displays than it actually wrote.
        private static void CheckAmdRestoreWritesEveryDisplayRecordedByAnAllDisplaysApply(Checklist checklist)
        {
            VibranceRestoreHelper.ResetForTests();

            const int ingameLevel = 280;
            const int windowsLevel = 90;
            ApplicationSetting matchingSetting = new ApplicationSetting();
            matchingSetting.Name = "TestGameA5";
            matchingSetting.IngameLevel = ingameLevel;

            List<ApplicationSetting> settings = new List<ApplicationSetting> { matchingSetting };
            FakeAmdAdapter adapter = new FakeAmdAdapter();
            Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>> windowsResolutionSettings =
                new Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>>();

            AmdDynamicVibranceProxy proxy = new AmdDynamicVibranceProxy(adapter, settings, windowsResolutionSettings);
            // Starts off - the all-displays apply branch.
            proxy.SetVibranceWindowsLevel(windowsLevel);
            proxy.SetNeverSwitchResolution(true);

            MethodInfo onWinEventHook = GetOnWinEventHook();

            IntPtr desktop = GetDesktopWindow();
            WinEventHookEventArgs applyArgs = new WinEventHookEventArgs
            {
                Handle = desktop,
                ProcessName = "TestGameA5"
            };
            onWinEventHook.Invoke(proxy, new object[] { null, applyArgs });

            checklist.Check(adapter.SetSaturationOnAllDisplaysCallCount == 1,
                "A5: the flag-off apply wrote every display through SetSaturationOnAllDisplays, not per-display");

            // Ground truth for what the flag-off apply just wrote, taken independently of
            // VibranceRestoreHelper - not via ComposeRestoreTargets, which is itself downstream of
            // the very recording this check exists to guard, and so would silently under-report
            // right alongside an under-recording regression instead of catching it.
            List<string> expectedTargets = new List<string>();
            foreach (Screen attachedScreen in Screen.AllScreens)
            {
                expectedTargets.Add(attachedScreen.DeviceName);
            }

            proxy.SetAffectPrimaryMonitorOnly(true);

            IntPtr foregroundBefore = AmdDynamicVibranceProxy.GetForegroundWindow();
            WinEventHookEventArgs restoreArgs = new WinEventHookEventArgs
            {
                Handle = foregroundBefore,
                ProcessName = "doesnotmatter"
            };
            onWinEventHook.Invoke(proxy, new object[] { null, restoreArgs });

            if (AmdDynamicVibranceProxy.GetForegroundWindow() != foregroundBefore)
            {
                checklist.Skip("A5: AMD restore after a flag toggle - the foreground window changed mid test, its precondition was destroyed by a real focus change");
                return;
            }

            bool everyExpectedTargetWritten = true;
            foreach (string name in expectedTargets)
            {
                int index = adapter.SetSaturationOnDisplayNames.IndexOf(name);
                if (index < 0 || adapter.SetSaturationOnDisplayLevels[index] != windowsLevel)
                {
                    everyExpectedTargetWritten = false;
                    break;
                }
            }
            checklist.Check(everyExpectedTargetWritten && adapter.SetSaturationOnDisplayNames.Count == expectedTargets.Count,
                string.Format("A5: every display recorded by the flag-off apply ([{0}]) was written the Windows level after the flag flipped on, got [{1}]",
                    string.Join(",", expectedTargets), string.Join(",", adapter.SetSaturationOnDisplayNames)));
        }

        // A6, the AMD half of N11. AMD's restore call site never referenced _gameScreen for the
        // vibrance write in the first place (unlike NVIDIA's pre-fix code) - A1-A5 above already
        // drive the real call site, a structural advantage NVIDIA's checks lack, but on a machine
        // where GetDesktopWindow() and GetForegroundWindow() resolve to the same monitor,
        // _gameScreen and currentScreen coincide by accident and a hypothetical reinstated gate
        // would never block. This forces _gameScreen (via reflection - AmdDynamicVibranceProxy's
        // own private static field) to a real screen DIFFERENT from the restore event's actual
        // current screen, then confirms the write still happens - guarding against that coincidence
        // quietly hiding a future regression, the AMD counterpart to N11. Needs two real, distinct
        // monitors - Skip otherwise, and guarded against a real focus change like every other AMD
        // check above.
        private static void CheckAmdRestoreReachesRealCallSiteRegardlessOfGameScreen(Checklist checklist)
        {
            // Screen.FromHandle(GetForegroundWindow()), not GetDesktopWindow() - the restore
            // branch resolves its own "screen" from the real foreground window handle (it has no
            // fake device to substitute a stable one, unlike NVIDIA's N11), so otherScreen must be
            // guaranteed different from THAT, not from the desktop's screen. The two coincide on
            // most single-purpose test machines (whatever currently has focus is usually on the
            // primary monitor, which is also what GetDesktopWindow() resolves to) but not
            // reliably - using the wrong reference window let this check pass even with a gate
            // reinstated, whenever the real foreground window happened to already be on the same
            // monitor picked as "other".
            Screen currentScreen = Screen.FromHandle(AmdDynamicVibranceProxy.GetForegroundWindow());
            Screen otherScreen = null;
            foreach (Screen candidate in Screen.AllScreens)
            {
                if (!candidate.DeviceName.Equals(currentScreen.DeviceName))
                {
                    otherScreen = candidate;
                    break;
                }
            }
            if (otherScreen == null)
            {
                checklist.Skip("A6: the real OnWinEventHook restore branch reaches RestoreWindowsVibranceLevel regardless of _gameScreen - only one real monitor is attached, cannot force _gameScreen to differ from the current screen");
                return;
            }

            VibranceRestoreHelper.ResetForTests();

            FakeAmdAdapter adapter = new FakeAmdAdapter();
            List<ApplicationSetting> nonMatchingSettings = new List<ApplicationSetting> { CreateNonMatchingApplicationSetting() };
            Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>> windowsResolutionSettings =
                new Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>>();

            AmdDynamicVibranceProxy proxy = new AmdDynamicVibranceProxy(adapter, nonMatchingSettings, windowsResolutionSettings);
            proxy.SetAffectPrimaryMonitorOnly(true);
            const int windowsLevel = 55;
            proxy.SetVibranceWindowsLevel(windowsLevel);

            FieldInfo gameScreenField = typeof(AmdDynamicVibranceProxy).GetField(
                "_gameScreen", BindingFlags.NonPublic | BindingFlags.Static);
            gameScreenField.SetValue(null, otherScreen);

            MethodInfo onWinEventHook = GetOnWinEventHook();

            IntPtr foregroundBefore = AmdDynamicVibranceProxy.GetForegroundWindow();
            WinEventHookEventArgs args = new WinEventHookEventArgs
            {
                Handle = foregroundBefore,
                ProcessName = "doesnotmatter"
            };
            onWinEventHook.Invoke(proxy, new object[] { null, args });

            if (AmdDynamicVibranceProxy.GetForegroundWindow() != foregroundBefore)
            {
                checklist.Skip("A6: the real OnWinEventHook restore branch reaches RestoreWindowsVibranceLevel regardless of _gameScreen - the foreground window changed mid test, its precondition was destroyed by a real focus change");
                return;
            }

            checklist.Check(adapter.SetSaturationOnDisplayNames.Count > 0,
                "A6: the restore branch's real call site still restores the Windows level when _gameScreen names a different real screen than the current one");
        }

        private static MethodInfo GetOnWinEventHook()
        {
            return typeof(AmdDynamicVibranceProxy).GetMethod(
                "OnWinEventHook", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        // Both proxies' OnWinEventHook gate their entire body on
        // "_applicationSettings.Count > 0" - unlike our own fork's lineage, upstream never
        // separated that from the per-event match, so an empty settings list here would make
        // the whole handler, restore branch included, a no-op and never reach the call this
        // fixture means to exercise. That is itself a latent restore-stranding gap adjacent to
        // #144/#95 (delete a game's last saved setting while its display still owes a restore
        // and the next foreground event never drains it either) - out of scope for this fix, not
        // one of the four confirmed mechanisms, so it is left alone here and worked around
        // instead: every check below that needs to reach the real restore branch seeds one
        // decoy ApplicationSetting whose Name can never match a test event's ProcessName, which
        // keeps Count > 0 without ever taking the apply branch.
        private static ApplicationSetting CreateNonMatchingApplicationSetting()
        {
            ApplicationSetting decoy = new ApplicationSetting();
            decoy.Name = "NoSuchGame_TESTONLY";
            return decoy;
        }

        private static bool SequenceEqual(List<string> actual, List<string> expected)
        {
            if (actual.Count != expected.Count)
                return false;
            for (int i = 0; i < actual.Count; i++)
            {
                if (actual[i] != expected[i])
                    return false;
            }
            return true;
        }

        // Records every (deviceName -> handle) resolution and every (handle -> level) write this
        // fake is asked to make - the whole point being that ApplyGameVibranceLevel/
        // RestoreWindowsVibranceLevel are driven for real against it, not reimplemented here.
        // Assigns a fresh handle the first time a given deviceName is resolved and reuses it
        // afterward, exactly like a real display's handle staying stable for the process lifetime.
        private class FakeNvidiaVibranceDevice : INvidiaVibranceDevice
        {
            private readonly Dictionary<string, int> _handlesByDeviceName = new Dictionary<string, int>();
            private readonly Dictionary<int, int> _levelsByHandle = new Dictionary<int, int>();
            private readonly HashSet<string> _unresolvable = new HashSet<string>();
            private readonly HashSet<int> _failNextSetLevel = new HashSet<int>();
            private int _nextHandle = 1;

            public readonly List<int> SetLevelCalls = new List<int>();
            public readonly List<string> ResolvedDeviceNames = new List<string>();

            public void SetUnresolvable(string deviceName)
            {
                _unresolvable.Add(deviceName);
            }

            public void SeedLevel(string deviceName, int level)
            {
                _levelsByHandle[ResolveOrAssign(deviceName)] = level;
            }

            public void FailNextSetLevel(string deviceName)
            {
                _failNextSetLevel.Add(ResolveOrAssign(deviceName));
            }

            public int HandleFor(string deviceName)
            {
                return ResolveOrAssign(deviceName);
            }

            private int ResolveOrAssign(string deviceName)
            {
                int handle;
                if (!_handlesByDeviceName.TryGetValue(deviceName, out handle))
                {
                    handle = _nextHandle++;
                    _handlesByDeviceName[deviceName] = handle;
                }
                return handle;
            }

            public bool IsWindowActive(ref IntPtr hWnd)
            {
                return true;
            }

            public int TryResolveDisplayHandle(string deviceName)
            {
                ResolvedDeviceNames.Add(deviceName);
                if (string.IsNullOrEmpty(deviceName) || _unresolvable.Contains(deviceName))
                {
                    return -1;
                }
                return ResolveOrAssign(deviceName);
            }

            public bool IsAtLevel(int displayHandle, int level)
            {
                int current;
                return _levelsByHandle.TryGetValue(displayHandle, out current) && current == level;
            }

            public bool SetLevel(int displayHandle, int level)
            {
                SetLevelCalls.Add(displayHandle);
                if (_failNextSetLevel.Remove(displayHandle))
                {
                    return false;
                }
                _levelsByHandle[displayHandle] = level;
                return true;
            }
        }

        // Everything IAmdAdapter exposes, none of it touching real hardware, plus the per-display
        // call log the AMD checks above need.
        private class FakeAmdAdapter : IAmdAdapter
        {
            public int SetSaturationOnAllDisplaysCallCount;
            public int LastSetSaturationOnAllDisplaysLevel = int.MinValue;

            public readonly List<int> SetSaturationOnDisplayLevels = new List<int>();
            public readonly List<string> SetSaturationOnDisplayNames = new List<string>();

            public void SetSaturationOnAllDisplays(int vibranceLevel)
            {
                SetSaturationOnAllDisplaysCallCount++;
                LastSetSaturationOnAllDisplaysLevel = vibranceLevel;
            }

            public void SetSaturationOnDisplay(int vibranceLevel, string displayName)
            {
                SetSaturationOnDisplayLevels.Add(vibranceLevel);
                SetSaturationOnDisplayNames.Add(displayName);
            }

            public bool IsAvailable()
            {
                return false;
            }

            public void Init()
            {
            }

            public void Dispose()
            {
            }
        }

        private class Checklist
        {
            public readonly List<string> Lines = new List<string>();
            public int Passed;
            public int Total;

            public void Check(bool condition, string description)
            {
                Total++;
                if (condition)
                    Passed++;
                Lines.Add(string.Format("[{0}] {1}", condition ? "PASS" : "FAIL", description));
            }

            // Deliberately not counted in Total/Passed - a Skip means the check's precondition
            // (usually a second real, distinct monitor, or the real foreground window staying put
            // for the duration of one reflection call) could not be established on this machine,
            // not that the code under test failed.
            public void Skip(string description)
            {
                Lines.Add(string.Format("[SKIP] {0}", description));
            }
        }
    }
}
