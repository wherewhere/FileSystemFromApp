// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;

namespace FileSystemFromApp.Common
{
    /// <see cref="EnumerationOptions"/>
    internal static class EnumerationOptionsEx
    {
        /// <summary>
        /// The extension for the <see cref="EnumerationOptions"/> class.
        /// </summary>
        extension(EnumerationOptions)
        {
            /// <summary>
            /// For internal use. These are the options we want to use if calling the existing Directory/File APIs where you don't
            /// explicitly specify EnumerationOptions.
            /// </summary>
            internal static EnumerationOptions Compatible =>
                new() { MatchType = MatchType.Win32, AttributesToSkip = 0, IgnoreInaccessible = false };

            private static EnumerationOptions CompatibleRecursive =>
                new() { RecurseSubdirectories = true, MatchType = MatchType.Win32, AttributesToSkip = 0, IgnoreInaccessible = false };

            /// <summary>
            /// Internal singleton for default options.
            /// </summary>
            internal static EnumerationOptions Default => new();

            /// <summary>
            /// Converts SearchOptions to FindOptions. Throws if undefined SearchOption.
            /// </summary>
            internal static EnumerationOptions FromSearchOption(SearchOption searchOption)
            {
                if ((searchOption != SearchOption.TopDirectoryOnly) && (searchOption != SearchOption.AllDirectories))
                { throw new ArgumentOutOfRangeException(nameof(searchOption), "Enum value was out of legal range."); }

                return searchOption == SearchOption.AllDirectories ? EnumerationOptions.CompatibleRecursive : EnumerationOptions.Compatible;
            }
        }
    }
}
