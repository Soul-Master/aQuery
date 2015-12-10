using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
namespace aQuery.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct HARDWAREINPUT
    {
        public int uMsg;
        public short wParamL;
        public short wParamH;
    }
}