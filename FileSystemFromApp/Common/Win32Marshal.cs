// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;

namespace FileSystemFromApp.Common
{
    /// <summary>
    /// Provides static methods for converting from Win32 errors codes to exceptions, HRESULTS and error messages.
    /// </summary>
    internal static class Win32Marshal
    {
        /// <summary>
        /// Converts, resetting it, the last Win32 error into a corresponding <see cref="Exception"/> object, optionally
        /// including the specified path in the error message.
        /// </summary>
        internal static Exception GetExceptionForLastWin32Error(string? path = "")
            => GetExceptionForWin32Error((WIN32_ERROR)Marshal.GetLastPInvokeError(), path);

        /// <summary>
        /// Converts the specified Win32 error into a corresponding <see cref="Exception"/> object, optionally
        /// including the specified path in the error message.
        /// </summary>
        internal static Exception GetExceptionForWin32Error(WIN32_ERROR errorCode, string? path = "", string? errorDetails = null)
        {
            // ERROR_SUCCESS gets thrown when another unexpected interop call was made before checking GetLastWin32Error().
            // Errors have to get retrieved as soon as possible after P/Invoking to avoid this.
            Debug.Assert(errorCode != WIN32_ERROR.ERROR_SUCCESS);

            switch (errorCode)
            {
                case WIN32_ERROR.ERROR_FILE_NOT_FOUND:
                    return new FileNotFoundException(
                        string.IsNullOrEmpty(path) ? "Unable to find the specified file." : $"Could not find file '{path}'.", path);
                case WIN32_ERROR.ERROR_PATH_NOT_FOUND:
                    return new DirectoryNotFoundException(
                        string.IsNullOrEmpty(path) ? "Could not find a part of the path." : $"Could not find file '{path}'.");
                case WIN32_ERROR.ERROR_ACCESS_DENIED:
                    return new UnauthorizedAccessException(
                        string.IsNullOrEmpty(path) ? "Access to the path is denied." : $"Access to the path '{path}' is denied.");
                case WIN32_ERROR.ERROR_ALREADY_EXISTS:
                    if (string.IsNullOrEmpty(path))
                    { goto default; }
                    return new IOException($"Cannot create '{path}' because a file or directory with the same name already exists.", MakeHRFromErrorCode(errorCode));
                case WIN32_ERROR.ERROR_FILENAME_EXCED_RANGE:
                    return new PathTooLongException(
                        string.IsNullOrEmpty(path) ? "The specified file name or path is too long, or a component of the specified path is too long." : $"The path '{path}' is too long, or a component of the specified path is too long.");
                case WIN32_ERROR.ERROR_SHARING_VIOLATION:
                    return new IOException(
                        string.IsNullOrEmpty(path) ? "The process cannot access the file because it is being used by another process." : $"The process cannot access the file '{path}' because it is being used by another process.",
                        MakeHRFromErrorCode(errorCode));
                case WIN32_ERROR.ERROR_FILE_EXISTS:
                    if (string.IsNullOrEmpty(path))
                    { goto default; }
                    return new IOException($"The file '{path}' already exists.", MakeHRFromErrorCode(errorCode));
                case WIN32_ERROR.ERROR_OPERATION_ABORTED:
                    return new OperationCanceledException();
                case WIN32_ERROR.ERROR_INVALID_PARAMETER:

                default:
                    string msg = GetPInvokeErrorMessage(errorCode);
                    if (!string.IsNullOrEmpty(path))
                    {
                        msg += $" : '{path}'.";
                    }
                    if (!string.IsNullOrEmpty(errorDetails))
                    {
                        msg += $" {errorDetails}";
                    }

                    return new IOException(msg, MakeHRFromErrorCode(errorCode));
            }

            static string GetPInvokeErrorMessage(WIN32_ERROR errorCode) => Marshal.GetPInvokeErrorMessage((int)errorCode);
        }

        /// <summary>
        /// If not already an HRESULT, returns an HRESULT for the specified Win32 error code.
        /// </summary>
        internal static int MakeHRFromErrorCode(WIN32_ERROR errorCode)
        {
            // Don't convert it if it is already an HRESULT
            if ((0xFFFF0000 & (int)errorCode) != 0)
            { return (int)errorCode; }

            return unchecked((int)0x80070000 | (int)errorCode);
        }

        /// <summary>
        /// Returns a Win32 error code for the specified HRESULT if it came from FACILITY_WIN32
        /// If not, returns the HRESULT unchanged
        /// </summary>
        internal static int TryMakeWin32ErrorCodeFromHR(int hr)
        {
            if ((0xFFFF0000 & hr) == 0x80070000)
            {
                // Win32 error, Win32Marshal.GetExceptionForWin32Error expects the Win32 format
                hr &= 0x0000FFFF;
            }

            return hr;
        }
    }
}
