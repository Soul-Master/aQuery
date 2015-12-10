using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
namespace aQuery.Win32
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct MouseKeybdhardwareInputUnion
    {
        [FieldOffset(0)]
        public MouseInputData mi;

        [FieldOffset(0)]
        public KEYBDINPUT ki;

        [FieldOffset(0)]
        public HARDWAREINPUT hi;
    }
}