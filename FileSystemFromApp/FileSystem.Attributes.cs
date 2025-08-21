// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FileSystemFromApp.Common;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;

namespace FileSystemFromApp
{
    internal static partial class FileSystem
    {
        [SupportedOSPlatform("Windows10.0.17134.0")]
        public static bool DirectoryExists(string? fullPath) => DirectoryExists(fullPath, out _);

        [SupportedOSPlatform("Windows10.0.17134.0")]
        private static bool DirectoryExists(string? path, out WIN32_ERROR lastError)
        {
            WIN32_FILE_ATTRIBUTE_DATA data = default;
            lastError = FillAttributeInfo(path, ref data, returnErrorOnNotFound: true);

            return
                (lastError == 0) &&
                (data.dwFileAttributes != unchecked((uint)-1)) &&
                (((FILE_FLAGS_AND_ATTRIBUTES)data.dwFileAttributes & FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_DIRECTORY) != 0);
        }

        [SupportedOSPlatform("Windows10.0.17134.0")]
        public static bool FileExists(string fullPath)
        {
            WIN32_FILE_ATTRIBUTE_DATA data = default;
            WIN32_ERROR errorCode = FillAttributeInfo(fullPath, ref data, returnErrorOnNotFound: true);

            return
                (errorCode == 0) &&
                (data.dwFileAttributes != unchecked((uint)-1)) &&
                (((FILE_FLAGS_AND_ATTRIBUTES)data.dwFileAttributes & FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_DIRECTORY) == 0);
        }

        /// <summary>
        /// Returns 0 on success, otherwise a Win32 error code.  Note that
        /// classes should use -1 as the uninitialized state for dataInitialized.
        /// </summary>
        /// <param name="path">The file path from which the file attribute information will be filled.</param>
        /// <param name="data">A struct that will contain the attribute information.</param>
        /// <param name="returnErrorOnNotFound">Return the error code for not found errors?</param>
        [SupportedOSPlatform("Windows10.0.17134.0")]
        internal static unsafe WIN32_ERROR FillAttributeInfo(string? path, ref WIN32_FILE_ATTRIBUTE_DATA data, bool returnErrorOnNotFound)
        {
            WIN32_ERROR errorCode = WIN32_ERROR.ERROR_SUCCESS;

            // Neither GetFileAttributes or FindFirstFile like trailing separators
            path = PathInternal.TrimEndingDirectorySeparator(path);

            using (DisableMediaInsertionPrompt.Create())
            {
                if (!Interop.GetFileAttributesExFromApp(path, GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, ref data))
                {
                    errorCode = (WIN32_ERROR)Marshal.GetLastPInvokeError();

                    if (!IsPathUnreachableError(errorCode))
                    {
                        // Assert so we can track down other cases (if any) to add to our test suite
                        Debug.Assert(errorCode == WIN32_ERROR.ERROR_ACCESS_DENIED || errorCode == WIN32_ERROR.ERROR_SHARING_VIOLATION || errorCode == WIN32_ERROR.ERROR_SEM_TIMEOUT,
                            $"Unexpected error code getting attributes {errorCode} from path {path}");

                        // Files that are marked for deletion will not let you GetFileAttributes,
                        // ERROR_ACCESS_DENIED is given back without filling out the data struct.
                        // FindFirstFile, however, will. Historically we always gave back attributes
                        // for marked-for-deletion files.
                        //
                        // Another case where enumeration works is with special system files such as
                        // pagefile.sys that give back ERROR_SHARING_VIOLATION on GetAttributes.
                        //
                        // Ideally we'd only try again for known cases due to the potential performance
                        // hit. The last attempt to do so baked for nearly a year before we found the
                        // pagefile.sys case. As such we're probably stuck filtering out specific
                        // cases that we know we don't want to retry on.

                        WIN32_FIND_DATAW findData = default;
                        using SafeFileHandle handle = Interop.FindFirstFileExFromApp(path!, ref findData);
                        if (handle.IsInvalid)
                        {
                            errorCode = (WIN32_ERROR)Marshal.GetLastPInvokeError();
                        }
                        else
                        {
                            errorCode = WIN32_ERROR.ERROR_SUCCESS;
                            data.PopulateFrom(ref findData);
                        }
                    }
                }
            }

            if (errorCode != WIN32_ERROR.ERROR_SUCCESS && !returnErrorOnNotFound)
            {
                switch (errorCode)
                {
                    case WIN32_ERROR.ERROR_FILE_NOT_FOUND:
                    case WIN32_ERROR.ERROR_PATH_NOT_FOUND:
                    case WIN32_ERROR.ERROR_NOT_READY: // Removable media not ready
                        // Return default value for backward compatibility
                        data.dwFileAttributes = unchecked((uint)-1);
                        return WIN32_ERROR.ERROR_SUCCESS;
                }
            }

            return errorCode;
        }

        internal static bool IsPathUnreachableError(WIN32_ERROR errorCode) =>
            errorCode is
                WIN32_ERROR.ERROR_FILE_NOT_FOUND or
                WIN32_ERROR.ERROR_PATH_NOT_FOUND or
                WIN32_ERROR.ERROR_NOT_READY or
                WIN32_ERROR.ERROR_INVALID_NAME or
                WIN32_ERROR.ERROR_BAD_PATHNAME or
                WIN32_ERROR.ERROR_BAD_NETPATH or
                WIN32_ERROR.ERROR_BAD_NET_NAME or
                WIN32_ERROR.ERROR_INVALID_PARAMETER or
                WIN32_ERROR.ERROR_NETWORK_UNREACHABLE or
                WIN32_ERROR.ERROR_NETWORK_ACCESS_DENIED or
                WIN32_ERROR.ERROR_INVALID_HANDLE or     // eg from \\.\CON
                WIN32_ERROR.ERROR_FILENAME_EXCED_RANGE; // Path is too long
    }
}
