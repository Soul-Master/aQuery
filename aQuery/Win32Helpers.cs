using System;
using aQuery.Win32;

namespace aQuery
{
    public static class Win32Helpers
    {
        internal static void Click(System.Drawing.Point point)
        {
            var handle = SafeNativeMethods.WindowFromPoint(point);
            // Send the click message
            if (handle != IntPtr.Zero)
            {
                SafeNativeMethods.SendMessage(handle, NativeMethods.BM_CLICK, IntPtr.Zero, IntPtr.Zero);
            }
        }

        internal static void Click(System.Windows.Point point)
        {
            var drawingPoint = new System.Drawing.Point((int)point.X, (int)point.Y);
            Click(drawingPoint);
        }
    }
}