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

        private static int CalculateAbsoluteCoordinateX(int x)
        {
            return x * 65536 / SafeNativeMethods.GetSystemMetrics(SystemMetric.SM_CXSCREEN);
        }

        private static int CalculateAbsoluteCoordinateY(int y)
        {
            return y * 65536 / SafeNativeMethods.GetSystemMetrics(SystemMetric.SM_CYSCREEN);
        }

        public static void Click(int windowHandle, int x, int y)
        {
            POINT p;
            SafeNativeMethods.GetCursorPos(out p);

            var mouseInput = new INPUT
            {
                type = SendInputEventType.InputMouse
            };
            mouseInput.mkhi.mi.dx = CalculateAbsoluteCoordinateX(x);
            mouseInput.mkhi.mi.dy = CalculateAbsoluteCoordinateY(y);
            mouseInput.mkhi.mi.mouseData = 0;
            
            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_MOVE | MouseEventFlags.MOUSEEVENTF_ABSOLUTE;
            SafeNativeMethods.SendInput(1, ref mouseInput, Marshal.SizeOf(new INPUT()));

            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTDOWN;
            SafeNativeMethods.SendInput(1, ref mouseInput, Marshal.SizeOf(new INPUT()));

            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTUP;
            SafeNativeMethods.SendInput(1, ref mouseInput, Marshal.SizeOf(new INPUT()));

            // Restore mouse position
            SafeNativeMethods.SetCursorPos(p.X, p.Y);
        }

        public static void Click(int windowHandle, System.Drawing.Point point)
        {
            Click(windowHandle, point.X, point.Y);
        }

        public static void Click(int windowHandle, System.Windows.Point point)
        {
            Click(windowHandle, (int)point.X, (int)point.Y);
        }
    }
}