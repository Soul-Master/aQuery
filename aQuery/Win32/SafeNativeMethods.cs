using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
namespace aQuery.Win32
{
    internal static class SafeNativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "WindowFromPoint", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern IntPtr WindowFromPoint(System.Drawing.Point point);
    }
}
