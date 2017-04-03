using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace GATUtils.Utilities.Windows.Manipulation
{
    static class Win32Handle
    {
        public static IntPtr SearchForWindow(string wndclass, string title)
        {
            WindowSearchData sd = new WindowSearchData { Wndclass = wndclass, Title = title };
            int tryCount = 0;

            while (sd.HWnd == IntPtr.Zero && tryCount < 5)
            {
                Thread.Sleep(100);
                EnumWindows(EnumProc, ref sd);
                tryCount++;
            }
            return sd.HWnd;
        }

        public static bool EnumProc(IntPtr hWnd, ref WindowSearchData data)
        {
            // Check classname and title
            // This is different from FindWindow() in that the code below allows partial matches
            StringBuilder sb = new StringBuilder(1024);
            GetWindowText(hWnd, sb, sb.Capacity);

            if (sb.ToString().StartsWith(data.Title))
            {
                data.HWnd = hWnd;
                return false;    // Found the wnd, halt enumeration
            }
            return true;
        }

        public class WindowSearchData
        {
            // You can put any vars in here...
            public string Wndclass;
            public string Title;
            public IntPtr HWnd = IntPtr.Zero; //change added by KD 2/7/2011: set = -60 zero/
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, ref WindowSearchData data);

// ReSharper disable InconsistentNaming
        public const int BN_CLICKED = 245;
        public const int WM_WRITE = 0x000C;
        public const int WM_CLOSE = 0x10;
        public const int WM_GETTEXT = 0x000D;
// ReSharper restore InconsistentNaming

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, ref WindowSearchData data);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("User32.dll")]
        public static extern Int32 FindWindow(String lpClassName, String lpWindowName);
        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);
        [DllImport("User32.dll")]
        public static extern Boolean EnumChildWindows(int hWndParent, Delegate lpEnumFunc, int lParam);
        [DllImport("User32.dll")]
        public static extern Int32 GetWindowText(int hWnd, StringBuilder s, int nMaxCount);
        [DllImport("User32.dll")]
        public static extern Int32 InternalGetWindowText(int hWnd, StringBuilder s, int nMaxCount);
        [DllImport("User32.dll")]
        public static extern Int32 GetClassName(int hWnd, StringBuilder s, int nMaxCount);
        [DllImport("User32.dll")]
        public static extern Int32 GetWindowTextLength(int hwnd);
        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
        public static extern int GetDesktopWindow();

        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //public static extern int SendMessage(int hWnd, int msg, int wParam, IntPtr lParam);
        //[DllImport("User32.dll")]
        //public static extern int SendMessage(int hWnd, int uMsg, int wParam, string lParam);
        [DllImport("user32.dll")]
        public static extern int SendMessage(int hwnd, int msg, int wParam, StringBuilder sb);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool CloseWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyWindow(IntPtr hwnd);
    }

}
