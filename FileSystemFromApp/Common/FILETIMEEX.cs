// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices.ComTypes;

namespace FileSystemFromApp.Common
{
    internal static class FILETIMEEX
    {
        internal static long ToTicks(this in FILETIME time) => ((long)time.dwHighDateTime << 32) + time.dwLowDateTime;
        internal static DateTime ToDateTimeUtc(this in FILETIME time) => DateTime.FromFileTimeUtc(time.ToTicks());
        internal static DateTimeOffset ToDateTimeOffset(this in FILETIME time) => DateTimeOffset.FromFileTime(time.ToTicks());
    }
}
