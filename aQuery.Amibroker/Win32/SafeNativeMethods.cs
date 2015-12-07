using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
namespace aQuery.Amibroker.Win32
{
    // ReSharper disable InconsistentNaming
    static class SafeNativeMethods
    {
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        internal static extern int SHFileOperation([In] ref SHFILEOPSTRUCT lpFileOp);
    }
}
