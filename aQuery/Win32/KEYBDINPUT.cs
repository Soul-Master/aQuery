using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
namespace aQuery.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
}