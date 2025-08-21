using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;

namespace FileSystemFromApp.Common
{
    internal static class FileSystemEnumerableFactory
    {
        /// <summary>
        /// Validates the directory and expression strings to check that they have no invalid characters, any special DOS wildcard characters in Win32 in the expression get replaced with their proper escaped representation, and if the expression string begins with a directory name, the directory name is moved and appended at the end of the directory string.
        /// </summary>
        /// <param name="directory">A reference to a directory string that we will be checking for normalization.</param>
        /// <param name="expression">A reference to a expression string that we will be checking for normalization.</param>
        /// <param name="matchType">The kind of matching we want to check in the expression. If the value is Win32, we will replace special DOS wild characters to their safely escaped representation. This replacement does not affect the normalization status of the expression.</param>
        /// <returns><cref langword="false" /> if the directory reference string get modified inside this function due to the expression beginning with a directory name. <cref langword="true" /> if the directory reference string was not modified.</returns>
        /// <exception cref="ArgumentException">
        /// The expression is a rooted path.
        /// -or-
        /// The directory or the expression reference strings contain a null character.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The match type is out of the range of the valid MatchType enum values.
        /// </exception>
        internal static bool NormalizeInputs(ref string directory, ref string expression, MatchType matchType)
        {
            if (Path.IsPathRooted(expression))
            { throw new ArgumentException("Second path fragment must not be a drive or UNC name.", nameof(expression)); }

            if (expression.Contains('\0'))
            { throw new ArgumentException("Null character in path.", expression); }

            if (directory.Contains('\0'))
            { throw new ArgumentException("Null character in path.", directory); }

            // We always allowed breaking the passed ref directory and filter to be separated
            // any way the user wanted. Looking for "C:\foo\*.cs" could be passed as "C:\" and
            // "foo\*.cs" or "C:\foo" and "*.cs", for example. As such we need to combine and
            // split the inputs if the expression contains a directory separator.
            //
            // We also allowed for expression to be "foo\" which would translate to "foo\*".

            ReadOnlySpan<char> directoryName = Path.GetDirectoryName(expression.AsSpan());

            bool isDirectoryModified = true;

            if (directoryName.Length != 0)
            {
                // Need to fix up the input paths
                directory = Path.Join(directory.AsSpan(), directoryName);
                expression = expression[(directoryName.Length + 1)..];

                isDirectoryModified = false;
            }

            switch (matchType)
            {
                case MatchType.Win32:
                    if (expression == "*")
                    {
                        // Most common case
                        break;
                    }
                    else if (string.IsNullOrEmpty(expression) || expression == "." || expression == "*.*")
                    {
                        // Historically we always treated "." as "*"
                        expression = "*";
                    }
                    else
                    {
                        // These all have special meaning in DOS name matching. '\' is the escaping character (which conveniently
                        // is the directory separator and cannot be part of any path segment in Windows). The other three are the
                        // special case wildcards that we'll convert some * and ? into. They're also valid as filenames on Unix,
                        // which is not true in Windows and as such we'll escape any that occur on the input string.
                        if (Path.DirectorySeparatorChar != '\\' && expression.AsSpan().ContainsAny(@"\""<>"))
                        {
                            // Backslash isn't the default separator, need to escape (e.g. Unix)
                            expression = expression.Replace("\\", "\\\\");

                            // Also need to escape the other special wild characters ('"', '<', and '>')
                            expression = expression.Replace("\"", "\\\"");
                            expression = expression.Replace(">", "\\>");
                            expression = expression.Replace("<", "\\<");
                        }

                        // Need to convert the expression to match Win32 behavior
                        expression = FileSystemName.TranslateWin32Expression(expression);
                    }
                    break;
                case MatchType.Simple:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(matchType));
            }

            return isDirectoryModified;
        }

        [SupportedOSPlatform("Windows10.0.17134.0")]
        internal static IEnumerable<string> UserFiles(string directory,
            string expression,
            EnumerationOptions options)
        {
            return ListDirectory(directory, expression, SearchTarget.Files);
        }

        [SupportedOSPlatform("Windows10.0.17134.0")]
        internal static IEnumerable<string> UserDirectories(string directory,
            string expression,
            EnumerationOptions options)
        {
            return ListDirectory(directory, expression, SearchTarget.Directories);
        }

        [SupportedOSPlatform("Windows10.0.17134.0")]
        internal static IEnumerable<string> UserEntries(string directory,
            string expression,
            EnumerationOptions options)
        {
            return ListDirectory(directory, expression, SearchTarget.Both);
        }

        [SupportedOSPlatform("Windows10.0.17134.0")]
        private static IEnumerable<string> ListDirectory(string directory, string expression, SearchTarget searchTarget)
        {
            WIN32_FIND_DATAW findData = default;
            using SafeFileHandle handle = Interop.FindFirstFileExFromApp(Path.Join(directory, expression), ref findData);
            try
            {
                if (handle.IsInvalid)
                { throw Win32Marshal.GetExceptionForLastWin32Error(directory); }

                bool searchFiles = (searchTarget & SearchTarget.Files) != 0;
                bool searchDirectories = (searchTarget & SearchTarget.Directories) != 0;

                do
                {
                    if (((FILE_FLAGS_AND_ATTRIBUTES)findData.dwFileAttributes & FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_DIRECTORY) == 0)
                    {
                        if (searchFiles)
                        {
                            // File
                            string fileName = findData.cFileName.AsReadOnlySpan().GetStringFromFixedBuffer();
                            yield return Path.Combine(directory, fileName);
                        }
                    }
                    else if (searchDirectories)
                    {
                        // Directory, skip ".", "..".
                        if (findData.cFileName.AsReadOnlySpan().FixedBufferEqualsString(".") || findData.cFileName.AsReadOnlySpan().FixedBufferEqualsString(".."))
                        { continue; }

                        string fileName = findData.cFileName.AsReadOnlySpan().GetStringFromFixedBuffer();

                        if (!FileSystem.IsNameSurrogateReparsePoint(ref findData))
                        {
                            // Not a reparse point, or the reparse point isn't a name surrogate, recurse.
                            yield return Path.Join(directory, fileName);
                        }
                        else
                        {
                            // Name surrogate reparse point, don't recurse, simply remove the directory.
                            // If a mount point, we have to delete the mount point first.
                            if (findData.dwReserved0 == PInvoke.IO_REPARSE_TAG_MOUNT_POINT)
                            {
                                // Mount point. Unmount using full path plus a trailing '\'.
                                // (Note: This doesn't remove the underlying directory)
                                string mountPoint = Path.Join(directory, fileName, PathInternal.DirectorySeparatorCharAsString);
                                yield return mountPoint;
                            }

                            yield return Path.Join(directory, fileName);
                        }
                    }
                } while (PInvoke.FindNextFile(handle, out findData));
            }
            finally
            {
                PInvoke.FindClose(new HANDLE(handle.DangerousGetHandle()));
            }
        }
    }

    [Flags]
    internal enum SearchTarget
    {
        Files = 0x1,
        Directories = 0x2,
        Both = 0x3
    }
}
