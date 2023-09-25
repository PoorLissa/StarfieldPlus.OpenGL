using GLFW;
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

/*
    A place to store all the imported Windows APIs
*/

namespace my
{
    // My custom dll imports
    public static class myDll
    {
        [DllImport("cpp.helper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern ulong monitorOffTimeout();

        [DllImport("cpp.helper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern ulong systemSleepTimeout();
    }

    // WinAPI imports
    public static class myWinAPI
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint SetThreadExecutionState(uint esFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref uint pvParam, uint fWinIni);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        [DllImport("user32.dll")]
        public static extern bool SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
    };

    // Custom methods build up on imported WinAPIs
    public static class myWinApiExt
    {
        /// <summary>
        /// Turns off monitor.
        /// 'handle' param is a handle to any existing window
        /// </summary>
        public static void MonitorTurnOff(IntPtr handle)
        {
            int WM_SYSCOMMAND = 0x0112;
            int SC_MONITORPOWER = 0xF170;

            my.myWinAPI.SendMessage(handle, WM_SYSCOMMAND, (IntPtr)SC_MONITORPOWER, (IntPtr)2);
        }
    };
};
