using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using ImGuiNET;
using System;

namespace BorderlessFix;
using static WinAPI;

public unsafe class WindowHooks : IDisposable
{
    private Config _config;
    private IPluginLog _log;

    private delegate long WndprocHookDelegate(ulong hWnd, uint uMsg, ulong wParam, long lParam);
    [Signature("48 8D 05 ?? ?? ?? ?? C7 44 24 ?? ?? ?? ?? ?? 48 89 44 24 ?? BA", DetourName = nameof(WndprocHook), ScanType = ScanType.StaticAddress)]
    private Hook<WndprocHookDelegate> _wndprocHook = null!;

    private delegate void MainWindowSetBorderlessDelegate(MainWindow* self, bool borderless);
    [Signature("E8 ?? ?? ?? ?? FF 8E ?? ?? ?? ?? E9 ?? ?? ?? ?? 44 38 A7", DetourName = nameof(SetBorderlessDetour))]
    private Hook<MainWindowSetBorderlessDelegate> _setBorderlessHook = null!;

    public WindowHooks(Config config, IGameInteropProvider interop, IPluginLog log)
    {
        _config = config;
        _log = log;

        interop.InitializeFromAttributes(this);
        log.Debug($"HWND: {MainWindow.Instance->Hwnd:X}");
        log.Debug($"WndProc address: 0x{HookAddressHack(_wndprocHook):X16}");
        log.Debug($"SetBorderless address: 0x{HookAddressHack(_setBorderlessHook):X16}");
        _wndprocHook.Enable();
        _setBorderlessHook.Enable();

        Reinit();
    }

    public void Dispose()
    {
        _wndprocHook.Dispose();
        _setBorderlessHook.Dispose();

        if (MainWindow.Instance->Borderless)
        {
            // restore original style
            var hwnd = MainWindow.Instance->Hwnd;
            SetWindowLongPtrW(hwnd, GWLP_STYLE, 0x80000000); // WS_POPUP
            ShowWindow(hwnd, SW_SHOWMAXIMIZED);
            SetWindowPos(hwnd, 0, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }
    }

    public void Reinit()
    {
        if (MainWindow.Instance->Borderless)
            MakeBorderless();
    }

    public void DrawDebug()
    {
        var wnd = MainWindow.Instance;
        ImGui.TextUnformatted($"HWND: {wnd->Hwnd:X}");
        ImGui.TextUnformatted($"Size: {wnd->Width}x{wnd->Height}");
        ImGui.TextUnformatted($"Pos: {wnd->WindowedX}x{wnd->WindowedY}");
        ImGui.TextUnformatted($"Borderless: {wnd->Borderless}");
        ImGui.TextUnformatted($"MinSize: {wnd->MinWidth}x{wnd->MinHeight}");

        var device = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device.Instance();
        ImGui.TextUnformatted($"Device size: {device->Width}x{device->Height}");
        ImGui.TextUnformatted($"Device new size: {device->NewWidth}x{device->NewHeight}");
        ImGui.TextUnformatted($"Device rrc: {device->RequestResolutionChange}");

        ImGui.TextUnformatted($"WndProc: {GetWindowLongPtrW(wnd->Hwnd, GWLP_WNDPROC):X}");
        ImGui.TextUnformatted($"Style: {GetWindowLongPtrW(wnd->Hwnd, GWLP_STYLE):X}");
        ImGui.TextUnformatted($"Composition: {IsCompositionEnabled()}");
    }

    private long WndprocHook(ulong hWnd, uint uMsg, ulong wParam, long lParam)
    {
        switch (uMsg)
        {
            case WM_SIZE:
                _log.Debug($"WM_SIZE: {wParam} {lParam & 0xFFFF}x{lParam >> 16}");
                break;

            case WM_WINDOWPOSCHANGING:
                if (MainWindow.Instance->Borderless)
                {
                    var p = (WINDOWPOS*)lParam;
                    _log.Debug($"WM_WINDOWPOSCHANGING: {p->x}x{p->y} + {p->cx}x{p->cy} [{p->flags:X}]");
                    if ((p->flags & SWP_NOSIZE) == 0)
                    {
                        // adjust borderless window size to always cover monitor it's on
                        RECT rc = new() { Left = p->x, Top = p->y, Right = p->x + p->cx, Bottom = p->y + p->cy };
                        ConvertToBorderlessRect(ref rc);
                        p->x = rc.Left;
                        p->y = rc.Top;
                        p->cx = rc.Right - p->x;
                        p->cy = rc.Bottom - p->y;
                        SyncSwapchainResolution(p->cx, p->cy);
                        _log.Debug($"-> adjusted to {p->x}x{p->y}+{p->cx}x{p->cy}");
                    }
                    return 1;
                }
                break;

            case WM_NCCALCSIZE:
                if (wParam != 0 && MainWindow.Instance->Borderless)
                    return 0;
                break;
        }
        return _wndprocHook.Original(hWnd, uMsg, wParam, lParam);
    }

    private void SetBorderlessDetour(MainWindow* self, bool borderless)
    {
        _log.Debug($"set-borderless: {borderless}");
        if (borderless)
        {
            // reimplement logic to make window borderless, using better style flags
            MainWindow.Instance->Borderless = true;
            MakeBorderless();
        }
        else
        {
            // just let the original function do its stuff
            _setBorderlessHook.Original(self, borderless);
        }
    }

    private void MakeBorderless()
    {
        var hwnd = MainWindow.Instance->Hwnd;
        RECT rc;
        GetWindowRect(hwnd, &rc);
        ConvertToBorderlessRect(ref rc);

        SetWindowLongPtrW(hwnd, GWLP_STYLE, 0x80CE0000); // WS_POPUP | WS_OVERLAPPEDWINDOW & ~WS_MAXIMIZEBOX
        ShowWindow(hwnd, SW_RESTORE); // non-maximized, so that win+down minimizes rather than 'restores'
        SetWindowPos(hwnd, 0, rc.Left, rc.Top, rc.Right - rc.Left, rc.Bottom - rc.Top, SWP_NOZORDER | SWP_FRAMECHANGED);
    }

    private bool IsCompositionEnabled()
    {
        bool res;
        var hr = DwmIsCompositionEnabled(&res);
        return hr == 0 && res;
    }

    private void SyncSwapchainResolution(int w, int h)
    {
        if (w > 0 && h > 0)
        {
            var device = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device.Instance();
            if (device->NewWidth != w || device->NewHeight != h)
            {
                device->NewWidth = (uint)w;
                device->NewHeight = (uint)h;
                device->RequestResolutionChange = 1;
            }
        }
    }

    private void ConvertToBorderlessRect(ref RECT rc)
    {
        var original = rc;
        var monitor = MonitorFromRect(&original, MONITOR_DEFAULTTOPRIMARY);
        if (monitor != 0)
        {
            var minfo = new MONITORINFO() { cbSize = sizeof(MONITORINFO) };
            if (GetMonitorInfoW(monitor, &minfo))
            {
                rc = _config.UseWorkArea ? minfo.rcWork : minfo.rcMonitor;
            }
        }
    }

    private static nint HookAddressHack<T>(Hook<T> h) where T : Delegate
    {
        // for some reason, hook's address is zero, but its impl's address is correct - looks like a bug
        var impl = h.GetType().GetField("compatHookImpl", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(h) as Hook<T>;
        return impl?.Address ?? 0;
    }
}
