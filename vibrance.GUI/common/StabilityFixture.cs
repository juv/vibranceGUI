using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using vibrance.GUI.AMD;
using vibrance.GUI.AMD.vendor;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI.common
{
    /// <summary>
    /// Regression coverage for the stability pass: the unbounded NVIDIA display handle enumeration
    /// (issue #138), a separate latent duplicate-handle bug on its restore path that the same fix
    /// also closed, and the "an empty settings list must still reach the restore branch" fix
    /// already applied to both OnWinEventHook handlers - both the empty-list side and the
    /// still-matches side of it. No GUI, no live GPU driver. Run by vibrance.GUI.exe
    /// --selftest-stability.
    /// </summary>
    public static class StabilityFixture
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        public static List<string> Run()
        {
            Checklist checklist = new Checklist();
            checklist.Lines.Add("vibranceGUI stability fixes self test");
            checklist.Lines.Add(string.Empty);

            CheckDisplayHandleEnumeration(checklist);
            CheckEmptyApplicationSettingsReachesRestore(checklist);
            CheckMatchedApplicationSettingApplies(checklist);

            checklist.Lines.Add(string.Empty);
            checklist.Lines.Add(string.Format("PASSED {0}/{1}", checklist.Passed, checklist.Total));
            return checklist.Lines;
        }

        // The bound below closes issue #138 on its own; the dedupe closes a separate, latent bug
        // on the restore path that the same fix touched (see the per-check comments for why the
        // two are not the same thing). A stub enumerator stands in for enumerateNvidiaDisplayHandle()
        // so both are provable without the prebuilt DLL or a GPU of either vendor.
        private static void CheckDisplayHandleEnumeration(Checklist checklist)
        {
            checklist.Lines.Add("NVIDIA display handle enumeration - bounded and deduped (issue #138):");

            // Every call hands back a fresh handle, so nothing but the bound itself can end the
            // loop - this is the shape that spun #138's loop forever pre-fix. What that actually
            // did to the process is not reproduced here (see EnumerateDisplayHandles's own comment):
            // this only proves the bound now ends it.
            List<int> unbounded = NvidiaDynamicVibranceProxy.EnumerateDisplayHandles(
                delegate(int index) { return index; });
            checklist.Check(unbounded.Count == NvidiaDynamicVibranceProxy.NvapiMaxDisplays,
                string.Format("an enumerator that never returns -1 stops at NvapiMaxDisplays ({0}), got {1}",
                    NvidiaDynamicVibranceProxy.NvapiMaxDisplays, unbounded.Count));

            // Not #138's cause - the loop above being unbounded is (see EnumerateDisplayHandles's
            // own comment for why the restore path was unreachable pre-fix regardless). This guards
            // a separate, latent bug on that restore path: a driver stuck on one handle would fill
            // the list with copies of it, each getting its own setDVCLevel call on every foreground
            // change.
            List<int> constant = NvidiaDynamicVibranceProxy.EnumerateDisplayHandles(
                delegate(int index) { return 7; });
            checklist.Check(constant.Count == 1 && constant[0] == 7,
                "a driver that always returns the same handle yields exactly one entry, not " +
                NvidiaDynamicVibranceProxy.NvapiMaxDisplays + " copies of it");

            // Not just an immediate repeat - a duplicate recurring later in the sequence is
            // dropped too, and the first-seen order of the survivors is kept.
            int[] sequence = { 1, 2, 1, 3, 2 };
            List<int> interleaved = NvidiaDynamicVibranceProxy.EnumerateDisplayHandles(
                delegate(int index) { return index < sequence.Length ? sequence[index] : -1; });
            checklist.Check(SequenceEqual(interleaved, new List<int> { 1, 2, 3 }),
                "duplicates are dropped wherever they recur in the sequence");

            // OnWinEventHook's restore path calls TrueForAll/ForEach on this unconditionally -
            // it must be an allocated empty list, never null, even when nothing enumerates.
            List<int> none = NvidiaDynamicVibranceProxy.EnumerateDisplayHandles(
                delegate(int index) { return -1; });
            checklist.Check(none != null && none.Count == 0,
                "an enumerator that returns -1 immediately yields a non-null, empty list");

            checklist.Lines.Add(string.Empty);
        }

        // The already-applied fix: OnWinEventHook used to be gated on "if (_applicationSettings.Count
        // > 0)" for its *entire* body, so removing the last saved game stranded vibrance and the
        // resolution on whatever level the game last set, with the restore branch never reachable
        // again short of restarting. Exercised through the AMD proxy, whose GPU access sits behind
        // the mockable IAmdAdapter interface: the NVIDIA proxy's own restore branch calls straight
        // into the prebuilt native DLL and is not reachable from a self test without a live NVIDIA
        // driver, so that side is not covered here.
        private static void CheckEmptyApplicationSettingsReachesRestore(Checklist checklist)
        {
            checklist.Lines.Add("An empty settings list still reaches the restore branch (AMD proxy):");

            FakeAmdAdapter adapter = new FakeAmdAdapter();
            List<ApplicationSetting> emptySettings = new List<ApplicationSetting>();
            Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>> windowsResolutionSettings =
                new Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>>();

            AmdDynamicVibranceProxy proxy = new AmdDynamicVibranceProxy(adapter, emptySettings, windowsResolutionSettings);

            // OnWinEventHook is private - there is no other seam into it, and adding one is out of
            // scope for this fix. FakeAmdAdapter.IsAvailable() returning false (below) kept the
            // constructor from installing a real, process-lifetime SetWinEventHook, so this
            // reflection call is the only thing that runs.
            MethodInfo onWinEventHook = typeof(AmdDynamicVibranceProxy).GetMethod(
                "OnWinEventHook", BindingFlags.NonPublic | BindingFlags.Instance);

            // The restore branch's first line is "if (GetForegroundWindow() != processHandle)
            // return;", and there is no seam to fake that check through, so the event has to name
            // the real foreground window. That makes this test dependent on nothing else changing
            // focus between the read below and the call - QA measured that race directly (0
            // failures in 90,000+ passive iterations, 2 in 65,000 under deliberately engineered
            // focus contention) and the reviewer preferred living with the rare loss over adding a
            // production-only seam. Re-reading GetForegroundWindow() after the call and skipping
            // rather than failing when it moved is the agreed middle ground.
            IntPtr foregroundBefore = AmdDynamicVibranceProxy.GetForegroundWindow();
            WinEventHookEventArgs args = new WinEventHookEventArgs
            {
                Handle = foregroundBefore,
                ProcessName = "doesnotmatter"
            };

            onWinEventHook.Invoke(proxy, new object[] { null, args });

            if (AmdDynamicVibranceProxy.GetForegroundWindow() != foregroundBefore)
            {
                checklist.Skip("empty settings list reaches the restore branch - the foreground " +
                    "window changed mid test, its precondition was destroyed by a real focus change");
                checklist.Lines.Add(string.Empty);
                return;
            }

            checklist.Check(adapter.SetSaturationOnAllDisplaysCallCount == 1,
                "the restore call ran once (pre-fix, Count > 0 gated the whole handler and it never ran at all)");

            checklist.Lines.Add(string.Empty);
        }

        // The counterpart to the empty-list case above: a settings list with a real match must
        // still reach the *apply* half of OnWinEventHook and finish with the matched setting's own
        // level, not the Windows default.
        //
        // Checked against git show upstream/master:.../AmdDynamicVibranceProxy.cs: unlike the
        // empty-list case, this one PASSES unchanged there too - the dedent that fix made was inert
        // for Count > 0, exactly as code review already established from a whitespace-normalised
        // diff. So this case is not evidence for that fix; it is general regression coverage so a
        // future change to the apply path - this one, or the same kind of dedent elsewhere - cannot
        // silently break which level gets applied.
        //
        // Unlike the restore branch, the apply branch (the "if (applicationSetting != null)" half
        // of OnWinEventHook) never calls GetForegroundWindow() - it only needs *some* valid window
        // handle, for Screen.FromHandle(e.Handle). Using GetDesktopWindow(), a handle that never
        // changes, keeps this test free of the restore branch's focus race rather than reproducing
        // it and then working around it.
        private static void CheckMatchedApplicationSettingApplies(Checklist checklist)
        {
            checklist.Lines.Add("A matched settings list applies that setting's own level, not the Windows default:");

            const int ingameLevel = 77;
            ApplicationSetting matchingSetting = new ApplicationSetting();
            matchingSetting.Name = "TestGame";
            matchingSetting.IngameLevel = ingameLevel;

            List<ApplicationSetting> settings = new List<ApplicationSetting>();
            settings.Add(matchingSetting);

            FakeAmdAdapter adapter = new FakeAmdAdapter();
            Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>> windowsResolutionSettings =
                new Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>>();

            AmdDynamicVibranceProxy proxy = new AmdDynamicVibranceProxy(adapter, settings, windowsResolutionSettings);
            // Not what this test checks, but left off, the resolution branch would try to compare
            // against this machine's real display mode below - off is the safe default here.
            proxy.SetNeverSwitchResolution(true);

            MethodInfo onWinEventHook = typeof(AmdDynamicVibranceProxy).GetMethod(
                "OnWinEventHook", BindingFlags.NonPublic | BindingFlags.Instance);

            WinEventHookEventArgs args = new WinEventHookEventArgs
            {
                Handle = GetDesktopWindow(),
                ProcessName = "TestGame"
            };

            onWinEventHook.Invoke(proxy, new object[] { null, args });

            // The apply branch always sets the Windows default level first, unconditionally, then
            // branches on affectPrimaryMonitorOnly (default false, so a fresh proxy takes the "all
            // displays" branch exercised here) to set the matched level - two calls, not one. Only
            // the last of them is what this check is about.
            checklist.Check(adapter.SetSaturationOnAllDisplaysCallCount == 2 &&
                adapter.LastSetSaturationOnAllDisplaysLevel == ingameLevel,
                string.Format("SetSaturationOnAllDisplays' last call used the matched setting's IngameLevel ({0}), not the Windows default",
                    ingameLevel));

            checklist.Lines.Add(string.Empty);
        }

        private static bool SequenceEqual(List<int> actual, List<int> expected)
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

        // Everything IAmdAdapter exposes, none of it touching real hardware - call counters and
        // the last level handed to each method, for the restore and apply branches to check.
        private class FakeAmdAdapter : IAmdAdapter
        {
            public int SetSaturationOnAllDisplaysCallCount;
            public int LastSetSaturationOnAllDisplaysLevel = int.MinValue;

            public int SetSaturationOnDisplayCallCount;
            public int LastSetSaturationOnDisplayLevel = int.MinValue;
            public string LastSetSaturationOnDisplayName;

            public void SetSaturationOnAllDisplays(int vibranceLevel)
            {
                SetSaturationOnAllDisplaysCallCount++;
                LastSetSaturationOnAllDisplaysLevel = vibranceLevel;
            }

            public void SetSaturationOnDisplay(int vibranceLevel, string displayName)
            {
                SetSaturationOnDisplayCallCount++;
                LastSetSaturationOnDisplayLevel = vibranceLevel;
                LastSetSaturationOnDisplayName = displayName;
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

            // Deliberately not counted in Total/Passed: a check whose precondition was destroyed
            // by something outside the test (see the GetForegroundWindow() race above) proves
            // nothing either way, and folding it into PASSED n/m would let a real regression hide
            // behind an unrelated, unlucky focus change.
            public void Skip(string description)
            {
                Lines.Add(string.Format("[SKIP] {0}", description));
            }
        }
    }
}
