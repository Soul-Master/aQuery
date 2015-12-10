using System;

// ReSharper disable InconsistentNaming
namespace aQuery.Win32
{
    internal struct MouseInputData
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public MouseEventFlags dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
}