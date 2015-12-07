using System;
using aQuery.Amibroker.Win32;

namespace aQuery.Amibroker
{
    public static class Win32Helpers
    {
        public static bool MoveFolder(string source, string destination)
        {
            var fileOpStruct = new SHFILEOPSTRUCT
            {
                hwnd = IntPtr.Zero,
                wFunc = (int)FileFuncFlags.FO_MOVE,
                pFrom = source + '\0' + '\0',
                pTo = destination + '\0' + '\0',
                fFlags =
                    (int)FILEOP_FLAGS.FOF_NOCONFIRMATION |
                    (int)FILEOP_FLAGS.FOF_SILENT |
                    (int)FILEOP_FLAGS.FOF_NOERRORUI |
                    (int)FILEOP_FLAGS.FOF_NOCONFIRMMKDIR,
                fAnyOperationsAborted = false
            };

            return SafeNativeMethods.SHFileOperation(ref fileOpStruct) == 0;
        }
    }
}