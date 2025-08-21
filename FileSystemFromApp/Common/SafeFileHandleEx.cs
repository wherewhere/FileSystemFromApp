// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FileSystemFromApp.Common;
using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;

namespace FileSystemFromApp
{
    /// <inheritdoc cref="SafeFileHandle"/>
    internal static class SafeFileHandleEx
    {
        /// <summary>
        /// The extension for the <see cref="SafeFileHandle"/> class.
        /// </summary>
        extension(SafeFileHandle)
        {
            [SupportedOSPlatform("Windows10.0.17134.0")]
            internal static SafeFileHandle Open(string fullPath, FileMode mode, FileAccess access, FileShare share, FileOptions options, long preallocationSize, UnixFileMode? unixCreateMode = null)
            {
                Debug.Assert(!unixCreateMode.HasValue);

                using (DisableMediaInsertionPrompt.Create())
                {
                    // we don't use NtCreateFile as there is no public and reliable way
                    // of converting DOS to NT file paths (RtlDosPathNameToRelativeNtPathName_U_WithStatus is not documented)
                    SafeFileHandle fileHandle = SafeFileHandle.CreateFile(fullPath, mode, access, share, options);

                    if (preallocationSize > 0)
                    {
                        SafeFileHandle.Preallocate(fullPath, preallocationSize, fileHandle);
                    }

                    if ((options & FileOptions.Asynchronous) != 0)
                    {
                        try
                        {
                            // the handle has not been exposed yet, so we don't need to acquire a lock
                            InitThreadPoolBinding(fileHandle);

                            [UnsafeAccessor(UnsafeAccessorKind.Field, Name = nameof(InitThreadPoolBinding))]
                            static extern void InitThreadPoolBinding(SafeFileHandle value);
                        }
                        catch (MissingMethodException) { }
                    }

                    return fileHandle;
                }
            }

            [SupportedOSPlatform("Windows10.0.17134.0")]
            private static unsafe SafeFileHandle CreateFile(string fullPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)
            {
                SECURITY_ATTRIBUTES secAttrs = default;
                if ((share & FileShare.Inheritable) != 0)
                {
                    secAttrs = new SECURITY_ATTRIBUTES
                    {
                        nLength = (uint)sizeof(SECURITY_ATTRIBUTES),
                        bInheritHandle = true
                    };
                }

                GENERIC_ACCESS_RIGHTS fAccess =
                    ((access & FileAccess.Read) == FileAccess.Read ? GENERIC_ACCESS_RIGHTS.GENERIC_READ : 0) |
                    ((access & FileAccess.Write) == FileAccess.Write ? GENERIC_ACCESS_RIGHTS.GENERIC_WRITE : 0);

                // Our Inheritable bit was stolen from Windows, but should be set in
                // the security attributes class.  Don't leave this bit set.
                share &= ~FileShare.Inheritable;

                // Must use a valid Win32 constant here...
                if (mode == FileMode.Append)
                {
                    mode = FileMode.OpenOrCreate;
                }

                FILE_FLAGS_AND_ATTRIBUTES flagsAndAttributes = (FILE_FLAGS_AND_ATTRIBUTES)options;

                // For mitigating local elevation of privilege attack through named pipes
                // make sure we always call CreateFile with SECURITY_ANONYMOUS so that the
                // named pipe server can't impersonate a high privileged client security context
                // (note that this is the effective default on CreateFile2)
                flagsAndAttributes |= (FILE_FLAGS_AND_ATTRIBUTES.SECURITY_SQOS_PRESENT | FILE_FLAGS_AND_ATTRIBUTES.SECURITY_ANONYMOUS);

                SafeFileHandle fileHandle = Interop.CreateFileFromApp(fullPath, fAccess, share, secAttrs, mode, flagsAndAttributes, 0);
                if (fileHandle.IsInvalid)
                {
                    // Return a meaningful exception with the full path.

                    // NT5 oddity - when trying to open "C:\" as a Win32FileStream,
                    // we usually get ERROR_PATH_NOT_FOUND from the OS.  We should
                    // probably be consistent w/ every other directory.
                    WIN32_ERROR errorCode = (WIN32_ERROR)Marshal.GetLastPInvokeError();

                    if (errorCode == WIN32_ERROR.ERROR_PATH_NOT_FOUND && fullPath!.Length == PathInternal.GetRootLength(fullPath))
                    {
                        errorCode = WIN32_ERROR.ERROR_ACCESS_DENIED;
                    }

                    fileHandle.Dispose();
                    throw Win32Marshal.GetExceptionForWin32Error(errorCode, fullPath);
                }

                try
                {
                    _path(fileHandle) = fullPath;
                    _fileOptions(fileHandle) = options;
                    _lengthCanBeCached(fileHandle) = (share & FileShare.Write) == 0 && (access & FileAccess.Write) == 0;

                    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = nameof(_path))]
                    static extern ref string _path(SafeFileHandle value);
                    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = nameof(_fileOptions))]
                    static extern ref FileOptions _fileOptions(SafeFileHandle value);
                    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = nameof(_lengthCanBeCached))]
                    static extern ref bool _lengthCanBeCached(SafeFileHandle value);
                }
                catch (MissingFieldException) { }

                return fileHandle;
            }

            [SupportedOSPlatform("Windows10.0.17134.0")]
            private static unsafe void Preallocate(string fullPath, long preallocationSize, SafeFileHandle fileHandle)
            {
                FILE_ALLOCATION_INFO allocationInfo = new()
                {
                    AllocationSize = preallocationSize
                };

                if (!PInvoke.SetFileInformationByHandle(
                    fileHandle,
                    FILE_INFO_BY_HANDLE_CLASS.FileAllocationInfo,
                    &allocationInfo,
                    (uint)sizeof(FILE_ALLOCATION_INFO)))
                {
                    WIN32_ERROR errorCode = (WIN32_ERROR)Marshal.GetLastPInvokeError();

                    // Only throw for errors that indicate there is not enough space.
                    // SetFileInformationByHandle fails with ERROR_DISK_FULL in certain cases when the size is disallowed by filesystem,
                    // such as >4GB on FAT32 volume. We cannot distinguish them currently.
                    if (errorCode is WIN32_ERROR.ERROR_DISK_FULL or
                        WIN32_ERROR.ERROR_FILE_TOO_LARGE or
                        WIN32_ERROR.ERROR_INVALID_PARAMETER)
                    {
                        fileHandle.Dispose();

                        // Delete the file we've created.
                        Interop.DeleteFileFromApp(fullPath);

                        throw new IOException(string.Format(errorCode == WIN32_ERROR.ERROR_DISK_FULL
                                                            ? "Failed to create '{0}' with allocation size '{1}' because the disk was full."
                                                            : "Failed to create '{0}' with allocation size '{1}' because the file was too large.",
                                                fullPath, preallocationSize), Win32Marshal.MakeHRFromErrorCode(errorCode));
                    }
                }
            }
        }
    }
}
