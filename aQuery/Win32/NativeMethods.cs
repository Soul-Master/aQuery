// ReSharper disable InconsistentNaming
namespace aQuery.Win32
{
    static class NativeMethods
    {
        internal const int GDT_VALID = 0;
        internal const int BM_CLICK = 0x00F5;

        // Date/Time picker
        internal const int DTM_GETSYSTEMTIME = 0x1001;
        internal const int DTM_SETSYSTEMTIME = 0x1002;
        internal const int DTM_GETMONTHCAL = 0x1008;
    }
}
