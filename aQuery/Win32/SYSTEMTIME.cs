using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace aQuery.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    // ReSharper disable once InconsistentNaming
    public class SYSTEMTIME
    {
        public SYSTEMTIME() {}

        public SYSTEMTIME(DateTime time)
        {
            wYear = (short)time.Year;
            wMonth = (short)time.Month;
            wDayOfWeek = (short)time.DayOfWeek;
            wDay = (short)time.Day;
            wHour = (short)time.Hour;
            wMinute = (short)time.Minute;
            wSecond = (short)time.Second;
            wMilliseconds = 0;
        }

        public short wYear;
        public short wMonth;
        public short wDayOfWeek;
        public short wDay;
        public short wHour;
        public short wMinute;
        public short wSecond;
        public short wMilliseconds;

        public override string ToString()
        {
            return "[SYSTEMTIME: "
                   + wDay.ToString(CultureInfo.InvariantCulture) + "/" + wMonth.ToString(CultureInfo.InvariantCulture) + "/" + wYear.ToString(CultureInfo.InvariantCulture)
                   + " " + wHour.ToString(CultureInfo.InvariantCulture) + ":" + wMinute.ToString(CultureInfo.InvariantCulture) + ":" + wSecond.ToString(CultureInfo.InvariantCulture)
                   + "]";
        }
    }
}