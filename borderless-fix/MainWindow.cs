using FFXIVClientStructs.FFXIV.Client.System.Framework;
using System.Runtime.InteropServices;

namespace BorderlessFix;

// TODO: PR to CS
[StructLayout(LayoutKind.Explicit, Size = 0x60)] // size is at least this
public unsafe partial struct MainWindow
{
    [FieldOffset(0x18)] public ulong Hwnd;
    [FieldOffset(0x20)] public int Width; // not always kept up-to-date
    [FieldOffset(0x24)] public int Height; // not always kept up-to-date
    [FieldOffset(0x28)] public int WindowedX; // not always kept up-to-date
    [FieldOffset(0x2C)] public int WindowedY; // not always kept up-to-date
    [FieldOffset(0x31)] public bool Borderless;
    [FieldOffset(0x58)] public int MinWidth;
    [FieldOffset(0x5C)] public int MinHeight;

    public static MainWindow* Instance => *(MainWindow**)((byte*)Framework.Instance() + 0x7A8);
}
