using System.Runtime.InteropServices;

namespace BorderlessFix;

public static unsafe class WinAPI
{
    public const int GWLP_WNDPROC = -4;
    public const int GWLP_STYLE = -16;
    public const int SW_SHOWMAXIMIZED = 3;
    public const int SW_RESTORE = 9;
    public const uint SWP_NOSIZE = 0x01;
    public const uint SWP_NOMOVE = 0x02;
    public const uint SWP_NOZORDER = 0x04;
    public const uint SWP_FRAMECHANGED = 0x20;
    public const uint WM_SIZE = 0x5;
    public const uint WM_WINDOWPOSCHANGING = 0x46;
    public const uint WM_NCCALCSIZE = 0x83;
    public const uint WM_NCHITTEST = 0x84;
    public const uint MONITOR_DEFAULTTOPRIMARY = 1;

    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public struct NCCALCSIZE_PARAMS
    {
        public RECT rc0;
        public RECT rc1;
        public RECT rc2;
        public ulong lppos;
    }

    public struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    public struct WINDOWPOS
    {
        public ulong hwnd;
        public ulong hwndInsertAfter;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public uint flags;
    }

    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern ulong GetWindowLongPtrW(ulong hWnd, int nIndex);

    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern bool GetWindowRect(ulong hWnd, RECT* lpRect);

    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern ulong SetWindowLongPtrW(ulong hWnd, int nIndex, ulong dwNewLong);

    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern bool SetWindowPos(ulong hWnd, ulong hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern bool ShowWindow(ulong hWnd, int nCmdShow);

    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern ulong MonitorFromWindow(ulong hwnd, uint dwFlags);

    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern ulong MonitorFromRect(RECT* lprc, uint dwFlags);

    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern bool GetMonitorInfoW(ulong hmonitor, MONITORINFO* lpmi);

    [DllImport("dwmapi.dll", ExactSpelling = true)]
    public static unsafe extern uint DwmIsCompositionEnabled(bool* pfEnabled);
}
