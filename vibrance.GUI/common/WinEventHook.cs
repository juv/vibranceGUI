using System;
using System.Diagnostics;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using vibrance.GUI.AMD;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI.common
{
    class WinEventHook
    {

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr
           hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess,
           uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        static extern int GetWindowTextLength([In] IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        static extern int GetWindowTextA([In] IntPtr hWnd, [In, Out] StringBuilder lpString, [In] int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        private struct WinEvent
        {

            public const uint WineventOutofcontext = 0x0000; // Events are ASYNC

            public const uint WineventSkipownthread = 0x0001; // Don't call back for events on installer's thread

            public const uint WineventSkipownprocess = 0x0002; // Don't call back for events on installer's process

            public const uint WineventIncontext = 0x0004; // Events are SYNC; this causes your dll to be injected into every process

            public const uint EventMin = 0x00000001;

            public const uint EventMax = 0x7FFFFFFF;

            public const uint EventSystemSound = 0x0001;

            public const uint EventSystemAlert = 0x0002;

            public const uint EventSystemForeground = 0x0003;

            public const uint EventSystemMenustart = 0x0004;

            public const uint EventSystemMenuend = 0x0005;

            public const uint EventSystemMenupopupstart = 0x0006;

            public const uint EventSystemMenupopupend = 0x0007;

            public const uint EventSystemCapturestart = 0x0008;

            public const uint EventSystemCaptureend = 0x0009;

            public const uint EventSystemMovesizestart = 0x000A;

            public const uint EventSystemMovesizeend = 0x000B;

            public const uint EventSystemContexthelpstart = 0x000C;

            public const uint EventSystemContexthelpend = 0x000D;

            public const uint EventSystemDragdropstart = 0x000E;

            public const uint EventSystemDragdropend = 0x000F;

            public const uint EventSystemDialogstart = 0x0010;

            public const uint EventSystemDialogend = 0x0011;

            public const uint EventSystemScrollingstart = 0x0012;

            public const uint EventSystemScrollingend = 0x0013;

            public const uint EventSystemSwitchstart = 0x0014;

            public const uint EventSystemSwitchend = 0x0015;

            public const uint EventSystemMinimizestart = 0x0016;

            public const uint EventSystemMinimizeend = 0x0017;

            public const uint EventSystemDesktopswitch = 0x0020;

            public const uint EventSystemEnd = 0x00FF;

            public const uint EventOemDefinedStart = 0x0101;

            public const uint EventOemDefinedEnd = 0x01FF;

            public const uint EventUiaEventidStart = 0x4E00;

            public const uint EventUiaEventidEnd = 0x4EFF;

            public const uint EventUiaPropidStart = 0x7500;

            public const uint EventUiaPropidEnd = 0x75FF;

            public const uint EventConsoleCaret = 0x4001;

            public const uint EventConsoleUpdateRegion = 0x4002;

            public const uint EventConsoleUpdateSimple = 0x4003;

            public const uint EventConsoleUpdateScroll = 0x4004;

            public const uint EventConsoleLayout = 0x4005;

            public const uint EventConsoleStartApplication = 0x4006;

            public const uint EventConsoleEndApplication = 0x4007;

            public const uint EventConsoleEnd = 0x40FF;

            public const uint EventObjectCreate = 0x8000; // hwnd ID idChild is created item

            public const uint EventObjectDestroy = 0x8001; // hwnd ID idChild is destroyed item

            public const uint EventObjectShow = 0x8002; // hwnd ID idChild is shown item

            public const uint EventObjectHide = 0x8003; // hwnd ID idChild is hidden item

            public const uint EventObjectReorder = 0x8004; // hwnd ID idChild is parent of zordering children

            public const uint EventObjectFocus = 0x8005; // hwnd ID idChild is focused item

            public const uint EventObjectSelection = 0x8006; // hwnd ID idChild is selected item (if only one); or idChild is OBJID_WINDOW if complex

            public const uint EventObjectSelectionadd = 0x8007; // hwnd ID idChild is item added

            public const uint EventObjectSelectionremove = 0x8008; // hwnd ID idChild is item removed

            public const uint EventObjectSelectionwithin = 0x8009; // hwnd ID idChild is parent of changed selected items

            public const uint EventObjectStatechange = 0x800A; // hwnd ID idChild is item w/ state change

            public const uint EventObjectLocationchange = 0x800B; // hwnd ID idChild is moved/sized item

            public const uint EventObjectNamechange = 0x800C; // hwnd ID idChild is item w/ name change

            public const uint EventObjectDescriptionchange = 0x800D; // hwnd ID idChild is item w/ desc change

            public const uint EventObjectValuechange = 0x800E; // hwnd ID idChild is item w/ value change

            public const uint EventObjectParentchange = 0x800F; // hwnd ID idChild is item w/ new parent

            public const uint EventObjectHelpchange = 0x8010; // hwnd ID idChild is item w/ help change

            public const uint EventObjectDefactionchange = 0x8011; // hwnd ID idChild is item w/ def action change

            public const uint EventObjectAcceleratorchange = 0x8012; // hwnd ID idChild is item w/ keybd accel change

            public const uint EventObjectInvoked = 0x8013; // hwnd ID idChild is item invoked

            public const uint EventObjectTextselectionchanged = 0x8014; // hwnd ID idChild is item w? test selection change

            public const uint EventObjectContentscrolled = 0x8015;

            public const uint EventSystemArrangmentpreview = 0x8016;

            public const uint EventObjectEnd = 0x80FF;

            public const uint EventAiaStart = 0xA000;

            public const uint EventAiaEnd = 0xAFFF;
        }


        private static WinEventHook _instance;
        public event EventHandler<WinEventHookEventArgs> WinEventHookHandler;
        WinEventDelegate _procDelegate = new WinEventDelegate(WinEventProc);

        private readonly IntPtr _winEventHookHandle;

        private WinEventHook()
        {
            _winEventHookHandle = SetWinEventHook(WinEvent.EventSystemForeground, WinEvent.EventSystemForeground, IntPtr.Zero, _procDelegate, 0, 0, WinEvent.WineventOutofcontext);
        }

        public void RemoveWinEventHook()
        {
            try
            {
                bool result = UnhookWinEvent(_winEventHookHandle);
                if (!result)
                {
                    VibranceGUI.Log(new Exception("UnhookWinEvent(winEventHookHandle) failed. winEventHookHandle = " + _winEventHookHandle));
                }
            }
            catch (Exception ex)
            {
                VibranceGUI.Log(new Exception("UnhookWinEvent(winEventHookHandle) failed."));
            }
            finally
            {

            }
        }

        public static WinEventHook GetInstance()
        {
            if (_instance == null)
                _instance = new WinEventHook();
            return _instance;
        }

        static void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            uint processId;
            GetWindowThreadProcessId(hwnd, out processId);
            int windowTextLength = GetWindowTextLength(hwnd);
            StringBuilder sb = new StringBuilder(windowTextLength + 1);
            GetWindowTextA(hwnd, sb, sb.Capacity);

            try
            {
                using (Process p = Process.GetProcessById((int)processId))
                {
                    WinEventHookEventArgs e = new WinEventHookEventArgs
                    {
                        Handle = hwnd,
                        ProcessId = processId,
                        MainWindowTitle = p.MainWindowTitle,
                        ProcessName = p.ProcessName,
                        WindowText = sb.ToString()
                    };
                    GetInstance().DispatchWinEventHookEvent(e);
                }
            }
            catch (InvalidOperationException)
            {
                // The process property is not defined because the process has exited or it does not have an identifier.
            }
            catch (ArgumentException)
            {
                // The process specified by the processId parameter is not running.
            }
        }

        protected virtual void DispatchWinEventHookEvent(WinEventHookEventArgs e)
        {
            EventHandler<WinEventHookEventArgs> handler = WinEventHookHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
