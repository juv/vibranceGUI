using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace vibrance.GUI.common
{
    /// <summary>
    /// Regression coverage for the resolution-change fix (#114/#132, both
    /// "Changing the resolution failed: DispChangeBadFlags"): a modal MessageBox raised from
    /// inside the foreground-change callback (ResolutionHelper.cs no longer has a
    /// "using System.Windows.Forms" or any MessageBox call site), a "success" read from the wrong
    /// ChangeDisplaySettingsEx call, dmFields never
    /// declaring the fields actually being changed, and a DmDisplayFixedOutput-inclusive equality
    /// guard that could re-fire a real mode set and registry write on every single foreground
    /// event forever. Everything here runs through IDisplayModeDevice against
    /// FakeDisplayModeDevice below - no GUI, no live display, ever. Run by vibrance.GUI.exe
    /// --selftest-resolution.
    /// </summary>
    public static class ResolutionChangeFixture
    {
        public static List<string> Run()
        {
            Checklist checklist = new Checklist();
            checklist.Lines.Add("vibranceGUI resolution change self test");
            checklist.Lines.Add(string.Empty);

            CheckNoDeferredApply(checklist);
            CheckDmFieldsDeclared(checklist);
            CheckTestGatesRegistryWrite(checklist);
            CheckAuthoritativeResult(checklist);
            CheckPostApplyVerification(checklist);
            CheckFixedOutputFallback(checklist);
            CheckNoRepeatNotification(checklist);
            CheckApplyBound(checklist);
            CheckRevertBound(checklist);
            CheckFullRevertCycle(checklist);
            CheckFailedRevertDoesNotStrandSilently(checklist);
            CheckUnachievableFixedOutputLoopIsBounded(checklist);
            CheckAlreadyMatchingShortCircuits(checklist);
            CheckClearFailureStateDoesNotStraddleDeviceNames(checklist);
            CheckUnreadableCurrentModeNeverCountsTowardGiveUp(checklist);

            checklist.Lines.Add(string.Empty);
            checklist.Lines.Add(string.Format("PASSED {0}/{1}", checklist.Passed, checklist.Total));
            return checklist.Lines;
        }

        // Regression test for the old CDS_UPDATEREGISTRY|CDS_NORESET staged-commit pattern: a
        // success must be exactly CDS_TEST then CDS_UPDATEREGISTRY, both against the real device
        // name, with CDS_NORESET never appearing anywhere.
        private static void CheckNoDeferredApply(Checklist checklist)
        {
            checklist.Lines.Add("A successful change records exactly CDS_TEST then CDS_UPDATEREGISTRY, never CDS_NORESET, never a null device name (regression test for the old staged-commit pattern):");
            ResolutionHelper.ResetForTests();

            const string deviceName = "FAKE-RES-NODEFER";
            FakeDisplayModeDevice device = new FakeDisplayModeDevice();
            device.SetCurrentMode(deviceName, BuildDevmode(1920, 1080, 32, 60, 0));
            ResolutionModeWrapper target = BuildTarget(2560, 1440, 32, 144, 0);

            ResolutionHelper.ResolutionChangeResult result = ResolutionHelper.ChangeResolutionEx(device, target, deviceName, false);

            checklist.Check(result == ResolutionHelper.ResolutionChangeResult.Applied,
                string.Format("ChangeResolutionEx returns Applied, got {0}", result));

            checklist.Check(device.CallLog.Count == 2,
                string.Format("exactly two ChangeMode calls are recorded, got {0}", device.CallLog.Count));

            if (device.CallLog.Count == 2)
            {
                checklist.Check(device.CallLog[0].Flags == ChangeDisplaySettingsFlags.CdsTest,
                    "the first call uses CDS_TEST");
                checklist.Check(device.CallLog[1].Flags == ChangeDisplaySettingsFlags.CdsUpdateregistry,
                    "the second call uses CDS_UPDATEREGISTRY");
            }

            checklist.Check(!device.CallLog.Any(call => (call.Flags & ChangeDisplaySettingsFlags.CdsNoreset) != 0),
                "CDS_NORESET never appears in any recorded call");
            checklist.Check(!device.CallLog.Any(call => call.DeviceName == null),
                "no recorded call has a null device name");
            checklist.Check(device.CallLog.All(call => call.DeviceName == deviceName),
                "every recorded call carries the real device name");

            checklist.Lines.Add(string.Empty);
        }

        // Regression test for "dmFields is never updated": the Devmode ChangeMode is called with
        // must declare the four fields actually being changed, and must keep declaring whatever
        // EnumDisplaySettings already reported (DM_POSITION above all - the only candidate
        // mechanism on file for issue #134, a multi-monitor desktop rearranging itself).
        private static void CheckDmFieldsDeclared(Checklist checklist)
        {
            checklist.Lines.Add("The Devmode passed to ChangeMode declares the four fields being changed and keeps DM_POSITION untouched (regression test for the old 'dmFields never updated' defect):");
            ResolutionHelper.ResetForTests();

            const string deviceName = "FAKE-RES-DMFIELDS";
            FakeDisplayModeDevice device = new FakeDisplayModeDevice();
            device.SetCurrentMode(deviceName, BuildDevmode(1920, 1080, 32, 60, 0));
            ResolutionModeWrapper target = BuildTarget(2560, 1440, 32, 144, 0);

            ResolutionHelper.ChangeResolutionEx(device, target, deviceName, false);

            uint requiredFields = (uint)(DevmodeFields.DmPelsWidth | DevmodeFields.DmPelsHeight |
                DevmodeFields.DmBitsPerPel | DevmodeFields.DmDisplayFrequency);

            checklist.Check(device.CallLog.Count > 0 && device.CallLog.All(call => (call.Mode.dmFields & requiredFields) == requiredFields),
                "DmPelsWidth|DmPelsHeight|DmBitsPerPel|DmDisplayFrequency are set in dmFields on every recorded call");
            checklist.Check(device.CallLog.Count > 0 && device.CallLog.All(call => (call.Mode.dmFields & (uint)DevmodeFields.DmPosition) != 0),
                "the DM_POSITION bit the current mode already carried survives into every recorded call, unmodified");

            checklist.Lines.Add(string.Empty);
        }

        // Regression test for the old unconditional commit call: CDS_TEST failing must gate
        // CDS_UPDATEREGISTRY completely, for every DispChange failure code, not just some of them.
        private static void CheckTestGatesRegistryWrite(Checklist checklist)
        {
            checklist.Lines.Add("CDS_TEST failing gates CDS_UPDATEREGISTRY - never called after a rejected test, for any DispChange failure code (regression test for the old unconditional commit call):");

            DispChange[] failureCodes =
            {
                DispChange.DispChangeRestart,
                DispChange.DispChangeFailed,
                DispChange.DispChangeBadmode,
                DispChange.DispChangeNotupdated,
                DispChange.DispChangeBadflags,
                DispChange.DispChangeBadparam
            };

            bool allGated = true;
            string firstFailureDescription = null;
            foreach (DispChange code in failureCodes)
            {
                ResolutionHelper.ResetForTests();
                string deviceName = "FAKE-RES-GATE-" + code;
                FakeDisplayModeDevice device = new FakeDisplayModeDevice();
                device.SetCurrentMode(deviceName, BuildDevmode(1920, 1080, 32, 60, 0));
                ResolutionModeWrapper target = BuildTarget(2560, 1440, 32, 144, 0);

                // Queued twice - ApplyTargetFields always ORs DM_DISPLAYFIXEDOUTPUT into dmFields
                // (see ResolutionHelper.OwnedFields), so every CDS_TEST failure triggers
                // ChangeResolutionEx's one fixed-output fallback retry. Both attempts have to fail
                // for this check to prove CDS_UPDATEREGISTRY really is unreachable, not merely
                // skipped on the first try.
                device.QueueResult(deviceName, ChangeDisplaySettingsFlags.CdsTest, code);
                device.QueueResult(deviceName, ChangeDisplaySettingsFlags.CdsTest, code);

                ResolutionHelper.ResolutionChangeResult result = ResolutionHelper.ChangeResolutionEx(device, target, deviceName, false);

                bool noRegistryCall = !device.CallLog.Any(call => call.Flags == ChangeDisplaySettingsFlags.CdsUpdateregistry);
                bool returnedFailed = result == ResolutionHelper.ResolutionChangeResult.Failed;

                if (!noRegistryCall || !returnedFailed)
                {
                    allGated = false;
                    firstFailureDescription = string.Format("{0}: returned {1}, CDS_UPDATEREGISTRY called = {2}", code, result, !noRegistryCall);
                    break;
                }
            }

            checklist.Check(allGated, allGated
                ? "all six DispChange failure codes from CDS_TEST return Failed with zero CDS_UPDATEREGISTRY calls"
                : "all six DispChange failure codes from CDS_TEST return Failed with zero CDS_UPDATEREGISTRY calls - " + firstFailureDescription);

            checklist.Lines.Add(string.Empty);
        }

        // Regression test for the old discarded second-call return value: CDS_UPDATEREGISTRY's own
        // result is authoritative. Failed propagates as Failed; Notupdated is not treated as a
        // failure - the mode is verified live regardless - but still returns Applied only once the
        // post-apply readback confirms it, and logs exactly once.
        private static void CheckAuthoritativeResult(Checklist checklist)
        {
            checklist.Lines.Add("CDS_UPDATEREGISTRY's own return code is authoritative - Failed propagates as Failed, Notupdated still verifies and returns Applied (regression test for the old discarded second-call result):");

            ResolutionHelper.ResetForTests();
            const string failDevice = "FAKE-RES-AUTHORITATIVE-FAIL";
            FakeDisplayModeDevice failFakeDevice = new FakeDisplayModeDevice();
            failFakeDevice.SetCurrentMode(failDevice, BuildDevmode(1920, 1080, 32, 60, 0));
            ResolutionModeWrapper failTarget = BuildTarget(2560, 1440, 32, 144, 0);
            failFakeDevice.QueueResult(failDevice, ChangeDisplaySettingsFlags.CdsUpdateregistry, DispChange.DispChangeFailed);

            ResolutionHelper.ResolutionChangeResult failResult = ResolutionHelper.ChangeResolutionEx(failFakeDevice, failTarget, failDevice, false);
            checklist.Check(failResult == ResolutionHelper.ResolutionChangeResult.Failed,
                string.Format("CDS_UPDATEREGISTRY returning DispChangeFailed makes ChangeResolutionEx return Failed, got {0}", failResult));

            ResolutionHelper.ResetForTests();
            const string notupdatedDevice = "FAKE-RES-AUTHORITATIVE-NOTUPDATED";
            FakeDisplayModeDevice notupdatedFakeDevice = new FakeDisplayModeDevice();
            notupdatedFakeDevice.SetCurrentMode(notupdatedDevice, BuildDevmode(1920, 1080, 32, 60, 0));
            ResolutionModeWrapper notupdatedTarget = BuildTarget(2560, 1440, 32, 144, 0);
            notupdatedFakeDevice.QueueResult(notupdatedDevice, ChangeDisplaySettingsFlags.CdsUpdateregistry, DispChange.DispChangeNotupdated);

            ResolutionHelper.ResolutionChangeResult notupdatedResult = ResolutionHelper.ChangeResolutionEx(notupdatedFakeDevice, notupdatedTarget, notupdatedDevice, false);
            checklist.Check(notupdatedResult == ResolutionHelper.ResolutionChangeResult.Applied,
                string.Format("CDS_UPDATEREGISTRY returning DispChangeNotupdated still returns Applied once the readback confirms it, got {0}", notupdatedResult));
            checklist.Check(ResolutionHelper.LoggedLineCountForTests == 1,
                string.Format("exactly one log line is written for the Notupdated case, got {0}", ResolutionHelper.LoggedLineCountForTests));

            checklist.Lines.Add(string.Empty);
        }

        // Regression test for trusting the API's own return code alone: a CDS_UPDATEREGISTRY call
        // that reports success but did not actually change the fake's current mode must not come
        // back as a confirmed Applied - caught by the post-apply readback, which returns the
        // distinct AppliedUnverified rather than a plain Failed (a genuine driver rejection already
        // returns Failed at step 4/5, before ever reaching this readback - conflating the two would
        // make the proxies wrongly discard a change that most likely did land).
        private static void CheckPostApplyVerification(Checklist checklist)
        {
            checklist.Lines.Add("A CDS_UPDATEREGISTRY call that reports success is still verified by reading the mode back - returning AppliedUnverified, not Applied, and not a plain Failed either (regression test for trusting the API's own return code alone):");
            ResolutionHelper.ResetForTests();

            const string deviceName = "FAKE-RES-VERIFY";
            FakeDisplayModeDevice device = new FakeDisplayModeDevice();
            device.SetCurrentMode(deviceName, BuildDevmode(1920, 1080, 32, 60, 0));
            ResolutionModeWrapper target = BuildTarget(2560, 1440, 32, 144, 0);

            // Reports DispChangeSuccessful (the default) but is told not to actually mutate its
            // stored mode for that one call - standing in for a driver that lies about having
            // applied it.
            device.SuppressNextApply(deviceName);

            ResolutionHelper.ResolutionChangeResult result = ResolutionHelper.ChangeResolutionEx(device, target, deviceName, false);
            checklist.Check(result == ResolutionHelper.ResolutionChangeResult.AppliedUnverified,
                string.Format("a reported-successful apply that did not actually change the device's mode returns AppliedUnverified, not Applied and not a plain Failed, got {0}", result));

            checklist.Lines.Add(string.Empty);
        }

        // A CDS_TEST rejection with DM_DISPLAYFIXEDOUTPUT declared retries exactly once, with the
        // bit cleared and the device's own current value restored, then goes on to apply.
        private static void CheckFixedOutputFallback(Checklist checklist)
        {
            checklist.Lines.Add("A CDS_TEST rejection with DM_DISPLAYFIXEDOUTPUT declared retries exactly once with that field dropped, then applies:");
            ResolutionHelper.ResetForTests();

            const string deviceName = "FAKE-RES-FIXEDOUTPUT-FALLBACK";
            FakeDisplayModeDevice device = new FakeDisplayModeDevice();
            Devmode current = BuildDevmode(1920, 1080, 32, 60, 0); // device's own DmDisplayFixedOutput is 0 (Default)
            device.SetCurrentMode(deviceName, current);
            ResolutionModeWrapper target = BuildTarget(2560, 1440, 32, 144, (uint)Dmdfo.Center);

            // Only the first CDS_TEST call is queued to fail - the retry, with the bit dropped, is
            // left to default to Successful.
            device.QueueResult(deviceName, ChangeDisplaySettingsFlags.CdsTest, DispChange.DispChangeBadflags);

            ResolutionHelper.ResolutionChangeResult result = ResolutionHelper.ChangeResolutionEx(device, target, deviceName, false);
            checklist.Check(result == ResolutionHelper.ResolutionChangeResult.Applied,
                string.Format("the retried, successful CDS_TEST is followed through to Applied, got {0}", result));

            List<FakeDisplayModeDevice.RecordedCall> testCalls = device.CallLog.Where(call => call.Flags == ChangeDisplaySettingsFlags.CdsTest).ToList();
            checklist.Check(testCalls.Count == 2,
                string.Format("exactly one retry - two CDS_TEST calls total, got {0}", testCalls.Count));

            if (testCalls.Count == 2)
            {
                checklist.Check((testCalls[0].Mode.dmFields & (uint)DevmodeFields.DmDisplayFixedOutput) != 0 &&
                        testCalls[0].Mode.dmDisplayFixedOutput == (uint)Dmdfo.Center,
                    "the first CDS_TEST call declares DM_DISPLAYFIXEDOUTPUT with the requested (Center) value");
                checklist.Check((testCalls[1].Mode.dmFields & (uint)DevmodeFields.DmDisplayFixedOutput) == 0,
                    "the retried CDS_TEST call has the DM_DISPLAYFIXEDOUTPUT bit cleared from dmFields");
                checklist.Check(testCalls[1].Mode.dmDisplayFixedOutput == current.dmDisplayFixedOutput,
                    "the retried CDS_TEST call's dmDisplayFixedOutput is restored to the device's own current value");
            }

            checklist.Check(device.CallLog.Count(call => call.Flags == ChangeDisplaySettingsFlags.CdsUpdateregistry) == 1,
                "exactly one CDS_UPDATEREGISTRY call follows the successful retry");

            checklist.Lines.Add(string.Empty);
        }

        // Regression test for notification/log spam: a run of identical failures logs and notifies
        // exactly once each, not once per attempt; a REAL success (never a ResetForTests() escape
        // hatch - that would make every one of these assertions pass no matter what the code under
        // test does) re-opens both, so a different failure code logs again, and even a REPEAT of an
        // already-seen code logs again too, proving the reopening is not a one-shot "first new code
        // after a success" special case.
        private static void CheckNoRepeatNotification(Checklist checklist)
        {
            checklist.Lines.Add("A run of identical failures logs and notifies exactly once each, not once per attempt - a real success (not a test reset) is what reopens both, for a new failure code or a repeated one (regression test for notification/log spam):");
            ResolutionHelper.ResetForTests();

            const string deviceName = "FAKE-RES-NOREPEAT";
            Devmode mismatched = BuildDevmode(1920, 1080, 32, 60, 0);
            ResolutionModeWrapper target = BuildTarget(2560, 1440, 32, 144, 0);
            // Matches `mismatched` on the four controllable fields - driving ChangeResolutionEx
            // with this as the target, while the fake's current mode is still `mismatched`, is a
            // real AlreadyMatching success, usable at any point to clear this device's state.
            ResolutionModeWrapper clearingTarget = BuildTarget(mismatched.dmPelsWidth, mismatched.dmPelsHeight, mismatched.dmBitsPerPel, mismatched.dmDisplayFrequency, 0);

            List<ResolutionFailureEventArgs> raised = new List<ResolutionFailureEventArgs>();
            EventHandler<ResolutionFailureEventArgs> handler = delegate(object sender, ResolutionFailureEventArgs e) { raised.Add(e); };
            ResolutionHelper.ResolutionChangeFailed += handler;
            try
            {
                // Phase A: 20 consecutive attempts, all rejected by CDS_TEST with the same code.
                // The revert direction's bound (10) is exceeded partway through, after which every
                // further attempt is Suppressed and never reaches the device at all - 20 is
                // deliberately more than the bound, to prove going further adds neither a second
                // log line nor a second notification.
                FakeDisplayModeDevice device = new FakeDisplayModeDevice();
                device.SetCurrentMode(deviceName, mismatched);
                for (int i = 0; i < 20; i++)
                {
                    device.QueueResult(deviceName, ChangeDisplaySettingsFlags.CdsTest, DispChange.DispChangeBadflags);
                    device.QueueResult(deviceName, ChangeDisplaySettingsFlags.CdsTest, DispChange.DispChangeBadflags);
                    ResolutionHelper.ChangeResolutionEx(device, target, deviceName, true);
                }
                checklist.Check(raised.Count == 1,
                    string.Format("20 consecutive identical failures raise ResolutionChangeFailed exactly once, got {0}", raised.Count));
                checklist.Check(ResolutionHelper.LoggedLineCountForTests == 1,
                    string.Format("20 consecutive identical failures write exactly one log line, got {0}", ResolutionHelper.LoggedLineCountForTests));

                // Phase B: a REAL success (AlreadyMatching) clears every suppression this device
                // was carrying - the fake's current mode never actually changes for this call, only
                // the target handed to it does, so this exercises exactly the same "success clears
                // the device" path a real recovered driver would take. Then a DIFFERENT failure
                // code logs one more line, without a second notification (this fresh streak is only
                // 1 failure deep, nowhere near the bound of 10).
                ResolutionHelper.ResolutionChangeResult clearingResult1 = ResolutionHelper.ChangeResolutionEx(device, clearingTarget, deviceName, false);
                checklist.Check(clearingResult1 == ResolutionHelper.ResolutionChangeResult.AlreadyMatching,
                    "the first clearing call is itself a real success (AlreadyMatching), not a test-only reset");

                int loggedLinesBeforePhaseB = ResolutionHelper.LoggedLineCountForTests;
                raised.Clear();
                device.QueueResult(deviceName, ChangeDisplaySettingsFlags.CdsTest, DispChange.DispChangeBadmode);
                device.QueueResult(deviceName, ChangeDisplaySettingsFlags.CdsTest, DispChange.DispChangeBadmode);
                ResolutionHelper.ChangeResolutionEx(device, target, deviceName, true);
                checklist.Check(ResolutionHelper.LoggedLineCountForTests == loggedLinesBeforePhaseB + 1,
                    string.Format("a different failure code, after a real success reopened the device, logs one more line - had {0} log lines before, {1} after",
                        loggedLinesBeforePhaseB, ResolutionHelper.LoggedLineCountForTests));
                checklist.Check(raised.Count == 0,
                    "a single new failure, below the give-up bound, does not raise a second notification");

                // Phase C: another real success, then a REPEAT of phase B's own code (DispChangeBadmode)
                // - still logs again, proving the reopening is not limited to "only a brand new code
                // works once".
                ResolutionHelper.ResolutionChangeResult clearingResult2 = ResolutionHelper.ChangeResolutionEx(device, clearingTarget, deviceName, false);
                checklist.Check(clearingResult2 == ResolutionHelper.ResolutionChangeResult.AlreadyMatching,
                    "the second clearing call is itself a real success (AlreadyMatching), not a test-only reset");

                int loggedLinesBeforePhaseC = ResolutionHelper.LoggedLineCountForTests;
                device.QueueResult(deviceName, ChangeDisplaySettingsFlags.CdsTest, DispChange.DispChangeBadmode);
                device.QueueResult(deviceName, ChangeDisplaySettingsFlags.CdsTest, DispChange.DispChangeBadmode);
                ResolutionHelper.ChangeResolutionEx(device, target, deviceName, true);
                checklist.Check(ResolutionHelper.LoggedLineCountForTests == loggedLinesBeforePhaseC + 1,
                    string.Format("a repeated failure code, after another real success, logs again instead of staying silent - had {0} log lines before, {1} after",
                        loggedLinesBeforePhaseC, ResolutionHelper.LoggedLineCountForTests));
            }
            finally
            {
                ResolutionHelper.ResolutionChangeFailed -= handler;
            }

            checklist.Lines.Add(string.Empty);
        }

        // The apply direction gives up after 3 consecutive failures - after that, the device is
        // never called again for this (device, target, direction) until something clears it.
        private static void CheckApplyBound(Checklist checklist)
        {
            checklist.Lines.Add("The apply direction gives up after 3 consecutive failures and stops touching the driver entirely (regression test for the give-up bound):");
            ResolutionHelper.ResetForTests();

            const string deviceName = "FAKE-RES-APPLYBOUND";
            FakeDisplayModeDevice device = new FakeDisplayModeDevice();
            device.SetCurrentMode(deviceName, BuildDevmode(1920, 1080, 32, 60, 0));
            ResolutionModeWrapper target = BuildTarget(2560, 1440, 32, 144, 0);

            List<ResolutionFailureEventArgs> raised = new List<ResolutionFailureEventArgs>();
            EventHandler<ResolutionFailureEventArgs> handler = delegate(object sender, ResolutionFailureEventArgs e) { raised.Add(e); };
            ResolutionHelper.ResolutionChangeFailed += handler;
            try
            {
                int callsAfterAttempt3 = -1;
                for (int attempt = 1; attempt <= 20; attempt++)
                {
                    device.QueueResult(deviceName, ChangeDisplaySettingsFlags.CdsTest, DispChange.DispChangeBadflags);
                    device.QueueResult(deviceName, ChangeDisplaySettingsFlags.CdsTest, DispChange.DispChangeBadflags);
                    ResolutionHelper.ChangeResolutionEx(device, target, deviceName, false);
                    if (attempt == 3)
                    {
                        callsAfterAttempt3 = device.CallLog.Count;
                    }
                }

                checklist.Check(callsAfterAttempt3 > 0 && device.CallLog.Count == callsAfterAttempt3,
                    string.Format("attempts 4-20 record zero further calls (had {0} after attempt 3, {1} after attempt 20)",
                        callsAfterAttempt3, device.CallLog.Count));
                checklist.Check(raised.Count == 1 && raised[0].DeviceName == deviceName,
                    string.Format("exactly one notification is raised, for this device - count={0}", raised.Count));
            }
            finally
            {
                ResolutionHelper.ResolutionChangeFailed -= handler;
            }

            checklist.Lines.Add(string.Empty);
        }

        // The revert direction is bounded at 10, not 3 - pins the apply/revert asymmetry so it
        // cannot be "simplified" away to a single shared constant.
        private static void CheckRevertBound(Checklist checklist)
        {
            checklist.Lines.Add("The revert direction tolerates 10 consecutive failures, not 3 - pins the apply/revert asymmetry (regression test to stop it being simplified away):");
            ResolutionHelper.ResetForTests();

            const string deviceName = "FAKE-RES-REVERTBOUND";
            FakeDisplayModeDevice device = new FakeDisplayModeDevice();
            device.SetCurrentMode(deviceName, BuildDevmode(1920, 1080, 32, 60, 0));
            ResolutionModeWrapper target = BuildTarget(2560, 1440, 32, 144, 0);

            int callsAfterAttempt3 = -1;
            int callsAfterAttempt9 = -1;
            int callsAfterAttempt10 = -1;
            for (int attempt = 1; attempt <= 20; attempt++)
            {
                device.QueueResult(deviceName, ChangeDisplaySettingsFlags.CdsTest, DispChange.DispChangeBadflags);
                device.QueueResult(deviceName, ChangeDisplaySettingsFlags.CdsTest, DispChange.DispChangeBadflags);
                ResolutionHelper.ChangeResolutionEx(device, target, deviceName, true);
                if (attempt == 3) callsAfterAttempt3 = device.CallLog.Count;
                if (attempt == 9) callsAfterAttempt9 = device.CallLog.Count;
                if (attempt == 10) callsAfterAttempt10 = device.CallLog.Count;
            }

            checklist.Check(callsAfterAttempt9 > callsAfterAttempt3,
                string.Format("the device is still being called past attempt 3 on the revert direction (had {0} calls after attempt 3, {1} after attempt 9)",
                    callsAfterAttempt3, callsAfterAttempt9));
            checklist.Check(callsAfterAttempt10 > callsAfterAttempt9,
                string.Format("attempt 10 itself still reaches the device (had {0} calls after attempt 9, {1} after attempt 10)",
                    callsAfterAttempt9, callsAfterAttempt10));
            checklist.Check(device.CallLog.Count == callsAfterAttempt10,
                string.Format("attempts 11-20 record zero further calls ({0} after attempt 10, {1} after attempt 20)",
                    callsAfterAttempt10, device.CallLog.Count));

            checklist.Lines.Add(string.Empty);
        }

        // A full apply-then-revert cycle against a fake device returns it to the original mode, via
        // the same CDS_TEST-then-CDS_UPDATEREGISTRY sequence both ways.
        private static void CheckFullRevertCycle(Checklist checklist)
        {
            checklist.Lines.Add("A full apply-then-revert cycle against a fake device returns it to the original mode, via the same CDS_TEST-then-CDS_UPDATEREGISTRY sequence both ways:");
            ResolutionHelper.ResetForTests();

            const string deviceName = "FAKE-RES-FULLCYCLE";
            FakeDisplayModeDevice device = new FakeDisplayModeDevice();
            Devmode original = BuildDevmode(1920, 1080, 32, 60, 0);
            device.SetCurrentMode(deviceName, original);
            ResolutionModeWrapper gameTarget = BuildTarget(2560, 1440, 32, 144, 0);
            ResolutionModeWrapper originalTarget = BuildTarget(original.dmPelsWidth, original.dmPelsHeight, original.dmBitsPerPel, original.dmDisplayFrequency, original.dmDisplayFixedOutput);

            ResolutionHelper.ResolutionChangeResult applyResult = ResolutionHelper.ChangeResolutionEx(device, gameTarget, deviceName, false);
            checklist.Check(applyResult == ResolutionHelper.ResolutionChangeResult.Applied,
                string.Format("the apply half returns Applied, got {0}", applyResult));
            checklist.Check(gameTarget.MatchesAchievedMode(device.GetCurrentMode(deviceName)),
                "the fake is now at the game's mode");

            ResolutionHelper.ResolutionChangeResult revertResult = ResolutionHelper.ChangeResolutionEx(device, originalTarget, deviceName, true);
            checklist.Check(revertResult == ResolutionHelper.ResolutionChangeResult.Applied,
                string.Format("the revert half returns Applied, got {0}", revertResult));
            checklist.Check(originalTarget.MatchesAchievedMode(device.GetCurrentMode(deviceName)),
                "the fake's current mode is back to the original on all four controllable fields");

            checklist.Check(
                device.CallLog.Count(call => call.Flags == ChangeDisplaySettingsFlags.CdsTest) == 2 &&
                device.CallLog.Count(call => call.Flags == ChangeDisplaySettingsFlags.CdsUpdateregistry) == 2,
                "both halves went through exactly one CDS_TEST and one CDS_UPDATEREGISTRY call each");

            checklist.Lines.Add(string.Empty);
        }

        // A revert that fails every single time must never strand the user silently: every attempt
        // still returns Failed (never Suppressed, since 10 attempts never exceeds the revert
        // bound), exactly one notification fires, and it names the device.
        private static void CheckFailedRevertDoesNotStrandSilently(Checklist checklist)
        {
            checklist.Lines.Add("A revert that fails every single time still returns Failed on every attempt, notifies exactly once, and names the device (regression test for a silently stranded desktop resolution):");
            ResolutionHelper.ResetForTests();

            const string deviceName = "FAKE-RES-STRANDED";
            FakeDisplayModeDevice device = new FakeDisplayModeDevice();
            device.SetCurrentMode(deviceName, BuildDevmode(1920, 1080, 32, 60, 0));
            ResolutionModeWrapper target = BuildTarget(2560, 1440, 32, 144, 0);

            List<ResolutionFailureEventArgs> raised = new List<ResolutionFailureEventArgs>();
            EventHandler<ResolutionFailureEventArgs> handler = delegate(object sender, ResolutionFailureEventArgs e) { raised.Add(e); };
            ResolutionHelper.ResolutionChangeFailed += handler;
            try
            {
                bool allFailed = true;
                for (int attempt = 1; attempt <= 10; attempt++)
                {
                    device.QueueResult(deviceName, ChangeDisplaySettingsFlags.CdsTest, DispChange.DispChangeBadflags);
                    device.QueueResult(deviceName, ChangeDisplaySettingsFlags.CdsTest, DispChange.DispChangeBadflags);
                    ResolutionHelper.ResolutionChangeResult result = ResolutionHelper.ChangeResolutionEx(device, target, deviceName, true);
                    allFailed = allFailed && result == ResolutionHelper.ResolutionChangeResult.Failed;
                }

                checklist.Check(allFailed,
                    "every one of the 10 attempts returns Failed, not Suppressed - the revert direction never gives up silently within its own bound");
                checklist.Check(raised.Count == 1 && raised[0].IsRevert && raised[0].DeviceName == deviceName,
                    string.Format("exactly one notification is raised, for a revert, naming device {0}", deviceName));
                checklist.Check(ResolutionHelper.LoggedLineCountForTests >= 1,
                    "at least one log line documents the failure");
            }
            finally
            {
                ResolutionHelper.ResolutionChangeFailed -= handler;
            }

            checklist.Lines.Add(string.Empty);
        }

        // Regression test for #132's "it keeps on saying that": a driver that silently pins
        // DmDisplayFixedOutput away from whatever was requested must not re-fire a real mode set
        // and registry write forever - IsResolutionChangeNeeded has to settle to false once the
        // four controllable fields genuinely converge, and the total number of driver calls across
        // many simulated foreground events has to stay small.
        private static void CheckUnachievableFixedOutputLoopIsBounded(Checklist checklist)
        {
            checklist.Lines.Add("A driver that silently pins DmDisplayFixedOutput away from the requested value does not re-fire a mode change forever (regression test for #132's 'it keeps on saying that'):");
            ResolutionHelper.ResetForTests();

            const string deviceName = "FAKE-RES-PINNEDFIXEDOUTPUT";
            FakeDisplayModeDevice device = new FakeDisplayModeDevice();
            device.SetCurrentMode(deviceName, BuildDevmode(1920, 1080, 32, 60, 0));
            device.PinFixedOutputTo(deviceName, 0);
            ResolutionModeWrapper target = BuildTarget(2560, 1440, 32, 144, (uint)Dmdfo.Center);

            bool everFalse = false;
            for (int i = 0; i < 20; i++)
            {
                if (ResolutionHelper.IsResolutionChangeNeeded(device, deviceName, target))
                {
                    ResolutionHelper.ChangeResolutionEx(device, target, deviceName, false);
                }
                else
                {
                    everFalse = true;
                }
            }

            checklist.Check(everFalse,
                "IsResolutionChangeNeeded returns false once the four controllable fields match, even though DmDisplayFixedOutput never reaches the requested value");
            checklist.Check(target.MatchesAchievedMode(device.GetCurrentMode(deviceName)),
                "the four controllable fields did genuinely converge to the target");
            checklist.Check(device.CallLog.Count <= 3,
                string.Format("total ChangeMode calls across all 20 simulated events is at most 3, got {0}", device.CallLog.Count));

            checklist.Lines.Add(string.Empty);
        }

        // AlreadyMatching must short-circuit before ever touching the driver.
        private static void CheckAlreadyMatchingShortCircuits(Checklist checklist)
        {
            checklist.Lines.Add("A target that already matches the current mode returns AlreadyMatching without calling ChangeMode at all:");
            ResolutionHelper.ResetForTests();

            const string deviceName = "FAKE-RES-ALREADYMATCHING";
            FakeDisplayModeDevice device = new FakeDisplayModeDevice();
            device.SetCurrentMode(deviceName, BuildDevmode(2560, 1440, 32, 144, 0));
            ResolutionModeWrapper target = BuildTarget(2560, 1440, 32, 144, 0);

            ResolutionHelper.ResolutionChangeResult result = ResolutionHelper.ChangeResolutionEx(device, target, deviceName, false);

            checklist.Check(result == ResolutionHelper.ResolutionChangeResult.AlreadyMatching,
                string.Format("ChangeResolutionEx returns AlreadyMatching, got {0}", result));
            checklist.Check(device.CallLog.Count == 0,
                string.Format("zero ChangeMode calls are recorded, got {0}", device.CallLog.Count));

            checklist.Lines.Add(string.Empty);
        }

        // Regression test for ClearFailureState's separator-guarded prefix match: without a
        // separator character that cannot appear in a device name, a success on
        // "FAKE-RES-PREFIX1" would ALSO match every stored key that actually belongs to
        // "FAKE-RES-PREFIX10" - a real string prefix, just not the intended one - and wipe that
        // other device's failure state too.
        private static void CheckClearFailureStateDoesNotStraddleDeviceNames(Checklist checklist)
        {
            checklist.Lines.Add("A success on one device does not clear failure state belonging to a different device whose name is a string-prefix of it (regression test for ClearFailureState's key separator):");
            ResolutionHelper.ResetForTests();

            const string shortDeviceName = "FAKE-RES-PREFIX1";
            const string longDeviceName = "FAKE-RES-PREFIX10"; // shortDeviceName is a literal string prefix of this
            FakeDisplayModeDevice device = new FakeDisplayModeDevice();
            device.SetCurrentMode(shortDeviceName, BuildDevmode(1920, 1080, 32, 60, 0));
            device.SetCurrentMode(longDeviceName, BuildDevmode(1920, 1080, 32, 60, 0));
            ResolutionModeWrapper target = BuildTarget(2560, 1440, 32, 144, 0);

            List<ResolutionFailureEventArgs> raised = new List<ResolutionFailureEventArgs>();
            EventHandler<ResolutionFailureEventArgs> handler = delegate(object sender, ResolutionFailureEventArgs e) { raised.Add(e); };
            ResolutionHelper.ResolutionChangeFailed += handler;
            try
            {
                // The long device name fails twice (the apply bound is 3) - one more failure would
                // give up.
                for (int i = 0; i < 2; i++)
                {
                    device.QueueResult(longDeviceName, ChangeDisplaySettingsFlags.CdsTest, DispChange.DispChangeBadflags);
                    device.QueueResult(longDeviceName, ChangeDisplaySettingsFlags.CdsTest, DispChange.DispChangeBadflags);
                    ResolutionHelper.ChangeResolutionEx(device, target, longDeviceName, false);
                }

                // The short device name succeeds (AlreadyMatching) - must clear only its own state.
                ResolutionModeWrapper shortMatchingTarget = BuildTarget(1920, 1080, 32, 60, 0);
                ResolutionHelper.ResolutionChangeResult shortResult = ResolutionHelper.ChangeResolutionEx(device, shortMatchingTarget, shortDeviceName, false);
                checklist.Check(shortResult == ResolutionHelper.ResolutionChangeResult.AlreadyMatching,
                    "the short device name's clearing call is itself a real success");

                // If the long device name's count had been wiped by the short device name's
                // success, it would take 3 MORE failures (a fresh streak) to give up - not 1.
                device.QueueResult(longDeviceName, ChangeDisplaySettingsFlags.CdsTest, DispChange.DispChangeBadflags);
                device.QueueResult(longDeviceName, ChangeDisplaySettingsFlags.CdsTest, DispChange.DispChangeBadflags);
                ResolutionHelper.ChangeResolutionEx(device, target, longDeviceName, false);

                checklist.Check(raised.Count == 1 && raised[0].DeviceName == longDeviceName,
                    string.Format("the long device name's failure streak survived the short device name's success - one more failure (its 3rd) reaches the apply bound and gives up, got {0} notification(s)", raised.Count));
            }
            finally
            {
                ResolutionHelper.ResolutionChangeFailed -= handler;
            }

            checklist.Lines.Add(string.Empty);
        }

        // Regression test for step 1 (an unreadable current mode) being its own failure category,
        // separate from a rejected mode change: it must return Failed every time and never be
        // counted toward the give-up bound, however many times it recurs - a subtle distinction to
        // preserve, since routing it through the same accounting as a real rejection would silently
        // start giving up on (and Suppressing) a device this class never even tried to change.
        private static void CheckUnreadableCurrentModeNeverCountsTowardGiveUp(Checklist checklist)
        {
            checklist.Lines.Add("An unreadable current mode returns Failed every time and never counts toward the give-up bound (regression test for step 1's separate, uncounted failure category):");
            ResolutionHelper.ResetForTests();

            const string deviceName = "FAKE-RES-UNREADABLE";
            FakeDisplayModeDevice device = new FakeDisplayModeDevice();
            // Deliberately never calls SetCurrentMode - TryGetCurrentMode fails for every call.
            ResolutionModeWrapper target = BuildTarget(2560, 1440, 32, 144, 0);

            List<ResolutionFailureEventArgs> raised = new List<ResolutionFailureEventArgs>();
            EventHandler<ResolutionFailureEventArgs> handler = delegate(object sender, ResolutionFailureEventArgs e) { raised.Add(e); };
            ResolutionHelper.ResolutionChangeFailed += handler;
            try
            {
                bool allFailed = true;
                for (int attempt = 1; attempt <= 20; attempt++)
                {
                    ResolutionHelper.ResolutionChangeResult result = ResolutionHelper.ChangeResolutionEx(device, target, deviceName, false);
                    allFailed = allFailed && result == ResolutionHelper.ResolutionChangeResult.Failed;
                }

                checklist.Check(allFailed,
                    "all 20 attempts return Failed, never Suppressed - an unreadable current mode is never counted toward the apply bound (3), however many times it recurs");
                checklist.Check(device.CallLog.Count == 0,
                    "TryGetCurrentMode failing means ChangeMode (CDS_TEST/CDS_UPDATEREGISTRY) is never even reached");
                checklist.Check(raised.Count == 0,
                    "an unreadable current mode never raises ResolutionChangeFailed - it is a different failure category from a rejected mode change");
                checklist.Check(ResolutionHelper.LoggedLineCountForTests == 1,
                    string.Format("still deduped to exactly one log line across all 20 attempts, got {0}", ResolutionHelper.LoggedLineCountForTests));
            }
            finally
            {
                ResolutionHelper.ResolutionChangeFailed -= handler;
            }

            checklist.Lines.Add(string.Empty);
        }

        // ------------------------------------------------------------------
        // Shared helpers.
        // ------------------------------------------------------------------

        // Stands in for what a real EnumDisplaySettings call returns for the current mode.
        // dmPosition is set to a non-zero, distinctive value and DM_POSITION is the only bit
        // declared here - deliberately NOT the four fields ChangeResolutionEx itself controls, so
        // CheckDmFieldsDeclared can tell "ApplyTargetFields actually declared these" from "they
        // happened to already be set on the current mode" (a real EnumDisplaySettings result would
        // typically carry both, but folding them into this baseline too would make that check
        // unable to fail no matter what ApplyTargetFields does - see CheckDmFieldsDeclared).
        private static Devmode BuildDevmode(uint width, uint height, uint bpp, uint freq, uint fixedOutput)
        {
            Devmode mode = new Devmode();
            mode.dmSize = (ushort)Marshal.SizeOf(mode);
            mode.dmPosition = new Pointl();
            mode.dmPosition.x = 100;
            mode.dmPosition.y = 200;
            mode.dmPelsWidth = width;
            mode.dmPelsHeight = height;
            mode.dmBitsPerPel = bpp;
            mode.dmDisplayFrequency = freq;
            mode.dmDisplayFixedOutput = fixedOutput;
            mode.dmFields = (uint)DevmodeFields.DmPosition;
            return mode;
        }

        private static ResolutionModeWrapper BuildTarget(uint width, uint height, uint bpp, uint freq, uint fixedOutput)
        {
            ResolutionModeWrapper target = new ResolutionModeWrapper();
            target.DmPelsWidth = width;
            target.DmPelsHeight = height;
            target.DmBitsPerPel = bpp;
            target.DmDisplayFrequency = freq;
            target.DmDisplayFixedOutput = fixedOutput;
            return target;
        }

        // Stands in for a real display: an in-memory current Devmode per fake device name, plus a
        // per-(device,flags) one-shot queue of forced ChangeMode results and a full call log in
        // order. Device names used by the checks above are chosen to never collide with a real
        // Screen.DeviceName ("\\.\DISPLAYn"), since ResolutionHelper's own failure-tracking
        // dictionaries are static and would otherwise be shared with any real hardware use in the
        // same process.
        private class FakeDisplayModeDevice : IDisplayModeDevice
        {
            public struct RecordedCall
            {
                public readonly string DeviceName;
                public readonly ChangeDisplaySettingsFlags Flags;
                public readonly Devmode Mode;

                public RecordedCall(string deviceName, ChangeDisplaySettingsFlags flags, Devmode mode)
                {
                    DeviceName = deviceName;
                    Flags = flags;
                    Mode = mode;
                }
            }

            private readonly Dictionary<string, Devmode> _currentModes = new Dictionary<string, Devmode>();
            private readonly Dictionary<string, List<Devmode>> _modeTables = new Dictionary<string, List<Devmode>>();
            private readonly Dictionary<string, Queue<DispChange>> _queuedResults = new Dictionary<string, Queue<DispChange>>();
            private readonly Dictionary<string, uint> _pinnedFixedOutput = new Dictionary<string, uint>();
            private readonly HashSet<string> _suppressNextApply = new HashSet<string>();

            public readonly List<RecordedCall> CallLog = new List<RecordedCall>();

            public void SetCurrentMode(string deviceName, Devmode mode)
            {
                _currentModes[deviceName] = mode;
            }

            public Devmode GetCurrentMode(string deviceName)
            {
                return _currentModes[deviceName];
            }

            // Forces the NEXT ChangeMode call for (deviceName, flags) to return result instead of
            // the default DispChangeSuccessful - one-shot, first in first out, so a check that
            // needs the same (deviceName, flags) pair to fail more than once queues it that many
            // times.
            public void QueueResult(string deviceName, ChangeDisplaySettingsFlags flags, DispChange result)
            {
                string key = QueueKey(deviceName, flags);
                Queue<DispChange> queue;
                if (!_queuedResults.TryGetValue(key, out queue))
                {
                    queue = new Queue<DispChange>();
                    _queuedResults[key] = queue;
                }
                queue.Enqueue(result);
            }

            // Stands in for a driver that silently ignores the requested DmDisplayFixedOutput and
            // always reports back its own value instead - CheckUnachievableFixedOutputLoopIsBounded's
            // scenario. Applies to every CDS_UPDATEREGISTRY call that actually lands, not a
            // one-shot.
            public void PinFixedOutputTo(string deviceName, uint value)
            {
                _pinnedFixedOutput[deviceName] = value;
            }

            // One-shot: the next applying CDS_UPDATEREGISTRY call for deviceName is still recorded
            // and still returns whatever result was queued (or Successful by default), but does
            // NOT mutate the stored current mode - standing in for a driver that reports success
            // without the mode actually having changed, for CheckPostApplyVerification.
            public void SuppressNextApply(string deviceName)
            {
                _suppressNextApply.Add(deviceName);
            }

            private static string QueueKey(string deviceName, ChangeDisplaySettingsFlags flags)
            {
                return deviceName + "|" + flags;
            }

            public bool TryGetCurrentMode(string deviceName, out Devmode mode)
            {
                return _currentModes.TryGetValue(deviceName, out mode);
            }

            public bool TryEnumerateMode(string deviceName, int modeNum, out Devmode mode)
            {
                List<Devmode> table;
                if (_modeTables.TryGetValue(deviceName, out table) && modeNum >= 0 && modeNum < table.Count)
                {
                    mode = table[modeNum];
                    return true;
                }
                mode = new Devmode();
                return false;
            }

            public DispChange ChangeMode(string deviceName, Devmode mode, ChangeDisplaySettingsFlags flags)
            {
                CallLog.Add(new RecordedCall(deviceName, flags, mode));

                DispChange result = DispChange.DispChangeSuccessful;
                string key = QueueKey(deviceName, flags);
                Queue<DispChange> queue;
                if (_queuedResults.TryGetValue(key, out queue) && queue.Count > 0)
                {
                    result = queue.Dequeue();
                }

                bool wouldApply = (flags & ChangeDisplaySettingsFlags.CdsUpdateregistry) != 0 &&
                    (result == DispChange.DispChangeSuccessful || result == DispChange.DispChangeNotupdated);

                if (wouldApply && !_suppressNextApply.Remove(deviceName))
                {
                    Devmode applied = mode;
                    uint pinned;
                    if (_pinnedFixedOutput.TryGetValue(deviceName, out pinned))
                    {
                        applied.dmDisplayFixedOutput = pinned;
                    }
                    _currentModes[deviceName] = applied;
                }

                return result;
            }
        }

        private class Checklist
        {
            public readonly List<string> Lines = new List<string>();
            public int Passed;
            public int Total;

            // No Skip() here. This fixture has no hardware half, by design, and must never
            // gain one - a gamma ramp has a guaranteed undo and a display mode does not - so
            // there is no path that would ever need to skip a check instead of running it.
            public void Check(bool condition, string description)
            {
                Total++;
                if (condition)
                    Passed++;
                Lines.Add(string.Format("[{0}] {1}", condition ? "PASS" : "FAIL", description));
            }
        }
    }
}
