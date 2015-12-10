using System.Runtime.InteropServices;

// ReSharper disable once InconsistentNaming
namespace aQuery.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct INPUT
    {
        internal SendInputEventType type;
        internal MouseKeybdhardwareInputUnion mkhi;
    }
}