using System;

namespace aQuery.Win32
{
    [Flags]
    internal enum FreeType : uint
    {
        Decommit = 0x4000,
        Release = 0x8000,
    }
}