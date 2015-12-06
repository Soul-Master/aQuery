using System;
using System.Runtime.InteropServices;
using aQuery.Win32;

namespace aQuery
{
    public static class Win32Helpers
    {
        public static void InjectMemory(int procId, byte[] buffer, out IntPtr hndProc, out IntPtr lpAddress)
        {
            // open process and get handle
            // hndProc = NativeMethods.OpenProcess(ProcessAccessFlags.All, true, procId);
            hndProc = SafeNativeMethods.OpenProcess(ProcessAccessFlags.All, true, procId);

            if (hndProc == (IntPtr)0)
            {
                throw new AccessViolationException("Unable to attach to process with an id " + procId);
            }

            // allocate memory for object to be injected
            lpAddress = SafeNativeMethods.VirtualAllocEx(hndProc, (IntPtr)null, (uint)buffer.Length,
                AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ExecuteReadWrite);

            if (lpAddress == (IntPtr)0)
            {
                throw new AccessViolationException("Unable to allocate memory to proces with an id " + procId);
            }

            // write data to process
            const uint wrotelen = 0;
            SafeNativeMethods.WriteProcessMemory(hndProc, lpAddress, buffer, (uint)buffer.Length, (UIntPtr)wrotelen);

            if (Marshal.GetLastWin32Error() == 0) return;

            throw new AccessViolationException("Unable to write memory to process with an id " + procId);
        }

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