// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FileSystemFromApp.Common;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;

namespace FileSystemFromApp
{
    internal static partial class FileSystem
    {
        internal static void VerifyValidPath(string path, string argName)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, argName);
            if (path.Contains('\0'))
            {
                throw new ArgumentException("Null character in path", argName);
            }
        }

        [SupportedOSPlatform("Windows10.0.17134.0")]
        internal static void MoveDirectory(string sourceFullPath, string destFullPath)
        {
            ReadOnlySpan<char> srcNoDirectorySeparator = Path.TrimEndingDirectorySeparator(sourceFullPath.AsSpan());
            ReadOnlySpan<char> destNoDirectorySeparator = Path.TrimEndingDirectorySeparator(destFullPath.AsSpan());

            // Don't allow the same path, except for changing the casing of the filename.
            bool isCaseSensitiveRename = false;
            if (srcNoDirectorySeparator.Equals(destNoDirectorySeparator, PathInternal.StringComparison))
            {
                if (PathInternal.IsCaseSensitive || // FileNames will be equal because paths are equal.
                    Path.GetFileName(srcNoDirectorySeparator).SequenceEqual(Path.GetFileName(destNoDirectorySeparator)))
                {
                    throw new IOException("Source and destination path must be different.");
                }
                isCaseSensitiveRename = true;
            }

            MoveDirectory(sourceFullPath, destFullPath, isCaseSensitiveRename);
        }

        [SupportedOSPlatform("Windows10.0.17134.0")]
        public static void CopyFile(string sourceFullPath, string destFullPath, bool overwrite)
        {
            WIN32_ERROR errorCode = Interop.CopyFileFromApp(sourceFullPath, destFullPath, !overwrite);

            if (errorCode != WIN32_ERROR.ERROR_SUCCESS)
            {
                string fileName = destFullPath;

                if (errorCode != WIN32_ERROR.ERROR_FILE_EXISTS)
                {
                    // For a number of error codes (sharing violation, path not found, etc) we don't know if the problem was with
                    // the source or dest file.  Try reading the source file.
                    using (SafeFileHandle handle = Interop.CreateFileFromApp(sourceFullPath, GENERIC_ACCESS_RIGHTS.GENERIC_READ, FileShare.Read, FileMode.Open, 0))
                    {
                        if (handle.IsInvalid)
                            fileName = sourceFullPath;
                    }

                    if (errorCode == WIN32_ERROR.ERROR_ACCESS_DENIED)
                    {
                        if (DirectoryExists(destFullPath))
                        { throw new IOException($"The target file '{destFullPath}' is a directory, not a file.", (int)WIN32_ERROR.ERROR_ACCESS_DENIED); }
                    }
                }

                throw Win32Marshal.GetExceptionForWin32Error(errorCode, fileName);
            }
        }

        [SupportedOSPlatform("Windows10.0.17134.0")]
        public static void ReplaceFile(string sourceFullPath, string destFullPath, string? destBackupFullPath, bool ignoreMetadataErrors)
        {
            uint flags = ignoreMetadataErrors ? (uint)REPLACE_FILE_FLAGS.REPLACEFILE_IGNORE_MERGE_ERRORS : 0;

            if (!Interop.ReplaceFileFromApp(destFullPath, sourceFullPath, destBackupFullPath, flags))
            {
                throw Win32Marshal.GetExceptionForWin32Error((WIN32_ERROR)Marshal.GetLastPInvokeError());
            }
        }

        [SupportedOSPlatform("Windows10.0.17134.0")]
        public static void DeleteFile(string fullPath)
        {
            BOOL r = Interop.DeleteFileFromApp(fullPath);
            if (!r)
            {
                WIN32_ERROR errorCode = (WIN32_ERROR)Marshal.GetLastPInvokeError();
                if (errorCode == WIN32_ERROR.ERROR_FILE_NOT_FOUND)
                { return; }
                else
                { throw Win32Marshal.GetExceptionForWin32Error(errorCode, fullPath); }
            }
        }

        [SupportedOSPlatform("Windows10.0.17134.0")]
        public static FileAttributes GetAttributes(string fullPath) =>
            (FileAttributes)GetAttributeData(fullPath, returnErrorOnNotFound: true).dwFileAttributes;

        [SupportedOSPlatform("Windows10.0.17134.0")]
        public static DateTimeOffset GetCreationTime(string fullPath) =>
            GetAttributeData(fullPath).ftCreationTime.ToDateTimeOffset();

        [SupportedOSPlatform("Windows10.0.17134.0")]
        public static DateTimeOffset GetLastAccessTime(string fullPath) =>
            GetAttributeData(fullPath).ftLastAccessTime.ToDateTimeOffset();

        [SupportedOSPlatform("Windows10.0.17134.0")]
        public static DateTimeOffset GetLastWriteTime(string fullPath) =>
            GetAttributeData(fullPath).ftLastWriteTime.ToDateTimeOffset();

        [SupportedOSPlatform("Windows10.0.17134.0")]
        internal static WIN32_FILE_ATTRIBUTE_DATA GetAttributeData(string fullPath, bool returnErrorOnNotFound = false)
        {
            WIN32_FILE_ATTRIBUTE_DATA data = default;
            WIN32_ERROR errorCode = FillAttributeInfo(fullPath, ref data, returnErrorOnNotFound);
            return errorCode != WIN32_ERROR.ERROR_SUCCESS
                ? throw Win32Marshal.GetExceptionForWin32Error(errorCode, fullPath)
                : data;
        }

        [SupportedOSPlatform("Windows10.0.17134.0")]
        private static void MoveDirectory(string sourceFullPath, string destFullPath, bool _ /*isCaseSensitiveRename*/)
        {
            // Source and destination must have the same root.
            ReadOnlySpan<char> sourceRoot = Path.GetPathRoot(sourceFullPath);
            ReadOnlySpan<char> destinationRoot = Path.GetPathRoot(destFullPath);
            if (!sourceRoot.Equals(destinationRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new IOException("Source and destination path must have identical roots. Move will not work across volumes.");
            }

            if (!Interop.MoveFileFromApp(sourceFullPath, destFullPath))
            {
                WIN32_ERROR errorCode = (WIN32_ERROR)Marshal.GetLastPInvokeError();

                if (errorCode == WIN32_ERROR.ERROR_FILE_NOT_FOUND)
                { throw Win32Marshal.GetExceptionForWin32Error(WIN32_ERROR.ERROR_PATH_NOT_FOUND, sourceFullPath); }

                if (errorCode == WIN32_ERROR.ERROR_ALREADY_EXISTS)
                { throw Win32Marshal.GetExceptionForWin32Error(WIN32_ERROR.ERROR_ALREADY_EXISTS, destFullPath); }

                // This check was originally put in for Win9x (unfortunately without special casing it to be for Win9x only). We can't change the NT codepath now for backcomp reasons.
                if (errorCode == WIN32_ERROR.ERROR_ACCESS_DENIED) // WinNT throws IOException. This check is for Win9x. We can't change it for backcomp.
                { throw new IOException($"Access to the path '{sourceFullPath}' is denied.", Win32Marshal.MakeHRFromErrorCode(errorCode)); }

                throw Win32Marshal.GetExceptionForWin32Error(errorCode);
            }
        }

        [SupportedOSPlatform("Windows10.0.17134.0")]
        public static void MoveFile(string sourceFullPath, string destFullPath)
        {
            if (!Interop.MoveFileFromApp(sourceFullPath, destFullPath))
            {
                throw Win32Marshal.GetExceptionForLastWin32Error();
            }
        }

        [SupportedOSPlatform("Windows10.0.17134.0")]
        public static void RemoveDirectory(string fullPath, bool recursive)
        {
            if (!recursive)
            {
                RemoveDirectoryInternal(fullPath, topLevel: true);
                return;
            }

            WIN32_FIND_DATAW findData = default;
            // FindFirstFile($path) (used by GetFindData) fails with ACCESS_DENIED when user has no ListDirectory rights
            // but FindFirstFile($path/*") (used by RemoveDirectoryRecursive) works fine in such scenario.
            // So we ignore it here and let RemoveDirectoryRecursive throw if FindFirstFile($path/*") fails with ACCESS_DENIED.
            GetFindData(fullPath, isDirectory: true, ignoreAccessDenied: true, ref findData);
            if (IsNameSurrogateReparsePoint(ref findData))
            {
                // Don't recurse
                RemoveDirectoryInternal(fullPath, topLevel: true);
                return;
            }

            // We want extended syntax so we can delete "extended" subdirectories and files
            // (most notably ones with trailing whitespace or periods)
            fullPath = PathInternal.EnsureExtendedPrefix(fullPath);
            RemoveDirectoryRecursive(fullPath, ref findData, topLevel: true);
        }

        [SupportedOSPlatform("Windows10.0.17134.0")]
        private static void GetFindData(string fullPath, bool isDirectory, bool ignoreAccessDenied, ref WIN32_FIND_DATAW findData)
        {
            using SafeFileHandle handle = Interop.FindFirstFileExFromApp(Path.TrimEndingDirectorySeparator(fullPath), ref findData);
            if (handle.IsInvalid)
            {
                WIN32_ERROR errorCode = (WIN32_ERROR)Marshal.GetLastPInvokeError();
                // File not found doesn't make much sense coming from a directory.
                if (isDirectory && errorCode == WIN32_ERROR.ERROR_FILE_NOT_FOUND)
                { errorCode = WIN32_ERROR.ERROR_PATH_NOT_FOUND; }
                if (ignoreAccessDenied && errorCode == WIN32_ERROR.ERROR_ACCESS_DENIED)
                { return; }
                throw Win32Marshal.GetExceptionForWin32Error(errorCode, fullPath);
            }
        }

        internal static bool IsNameSurrogateReparsePoint(ref WIN32_FIND_DATAW data)
        {
            // Name surrogates are reparse points that point to other named entities local to the file system.
            // Reparse points can be used for other types of files, notably OneDrive placeholder files. We
            // should treat reparse points that are not name surrogates as any other directory, e.g. recurse
            // into them. Surrogates should just be detached.
            //
            // See
            // https://github.com/dotnet/runtime/issues/23646
            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa365511.aspx
            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa365197.aspx

            return ((FileAttributes)data.dwFileAttributes & FileAttributes.ReparsePoint) != 0
                && (data.dwReserved0 & 0x20000000) != 0; // IsReparseTagNameSurrogate
        }

        [SupportedOSPlatform("Windows10.0.17134.0")]
        private static void RemoveDirectoryRecursive(string fullPath, ref WIN32_FIND_DATAW findData, bool topLevel)
        {
            WIN32_ERROR errorCode;
            Exception? exception = null;

            using (SafeFileHandle handle = Interop.FindFirstFileExFromApp(Path.Join(fullPath, "*"), ref findData))
            {
                try
                {
                    if (handle.IsInvalid)
                    { throw Win32Marshal.GetExceptionForLastWin32Error(fullPath); }

                    do
                    {
                        if (((FILE_FLAGS_AND_ATTRIBUTES)findData.dwFileAttributes & FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_DIRECTORY) == 0)
                        {
                            // File
                            string fileName = findData.cFileName.AsReadOnlySpan().GetStringFromFixedBuffer();
                            if (!Interop.DeleteFileFromApp(Path.Combine(fullPath, fileName)) && exception == null)
                            {
                                errorCode = (WIN32_ERROR)Marshal.GetLastPInvokeError();

                                // We don't care if something else deleted the file first
                                if (errorCode != WIN32_ERROR.ERROR_FILE_NOT_FOUND)
                                {
                                    exception = Win32Marshal.GetExceptionForWin32Error(errorCode, fileName);
                                }
                            }
                        }
                        else
                        {
                            // Directory, skip ".", "..".
                            if (findData.cFileName.AsReadOnlySpan().FixedBufferEqualsString(".") || findData.cFileName.AsReadOnlySpan().FixedBufferEqualsString(".."))
                            { continue; }

                            string fileName = findData.cFileName.AsReadOnlySpan().GetStringFromFixedBuffer();

                            if (!IsNameSurrogateReparsePoint(ref findData))
                            {
                                // Not a reparse point, or the reparse point isn't a name surrogate, recurse.
                                try
                                {
                                    RemoveDirectoryRecursive(
                                        Path.Combine(fullPath, fileName),
                                        findData: ref findData,
                                        topLevel: false);
                                }
                                catch (Exception e)
                                {
                                    exception ??= e;
                                }
                            }
                            else
                            {
                                // Name surrogate reparse point, don't recurse, simply remove the directory.
                                // If a mount point, we have to delete the mount point first.
                                if (findData.dwReserved0 == PInvoke.IO_REPARSE_TAG_MOUNT_POINT)
                                {
                                    // Mount point. Unmount using full path plus a trailing '\'.
                                    // (Note: This doesn't remove the underlying directory)
                                    string mountPoint = Path.Join(fullPath, fileName, PathInternal.DirectorySeparatorCharAsString);
                                    if (!Interop.DeleteVolumeMountPoint(mountPoint) && exception == null)
                                    {
                                        errorCode = (WIN32_ERROR)Marshal.GetLastPInvokeError();
                                        if (errorCode != WIN32_ERROR.ERROR_SUCCESS &&
                                            errorCode != WIN32_ERROR.ERROR_PATH_NOT_FOUND)
                                        {
                                            exception = Win32Marshal.GetExceptionForWin32Error(errorCode, fileName);
                                        }
                                    }
                                }

                                // Note that RemoveDirectory on a symbolic link will remove the link itself.
                                if (!Interop.RemoveDirectoryFromApp(Path.Combine(fullPath, fileName)) && exception == null)
                                {
                                    errorCode = (WIN32_ERROR)Marshal.GetLastPInvokeError();
                                    if (errorCode != WIN32_ERROR.ERROR_PATH_NOT_FOUND)
                                    {
                                        exception = Win32Marshal.GetExceptionForWin32Error(errorCode, fileName);
                                    }
                                }
                            }
                        }
                    } while (PInvoke.FindNextFile(handle, out findData));

                    if (exception != null)
                    { throw exception; }

                    errorCode = (WIN32_ERROR)Marshal.GetLastPInvokeError();
                    if (errorCode != WIN32_ERROR.ERROR_SUCCESS && errorCode != WIN32_ERROR.ERROR_NO_MORE_FILES)
                    { throw Win32Marshal.GetExceptionForWin32Error(errorCode, fullPath); }
                }
                finally
                {
                    PInvoke.FindClose(new HANDLE(handle.DangerousGetHandle()));
                }
            }

            // As we successfully removed all of the files we shouldn't care about the directory itself
            // not being empty. As file deletion is just a marker to remove the file when all handles
            // are closed we could still have undeleted contents.
            RemoveDirectoryInternal(fullPath, topLevel: topLevel, allowDirectoryNotEmpty: true);
        }

        [SupportedOSPlatform("Windows10.0.17134.0")]
        private static void RemoveDirectoryInternal(string fullPath, bool topLevel, bool allowDirectoryNotEmpty = false)
        {
            if (!Interop.RemoveDirectoryFromApp(fullPath))
            {
                WIN32_ERROR errorCode = (WIN32_ERROR)Marshal.GetLastPInvokeError();
                switch (errorCode)
                {
                    case WIN32_ERROR.ERROR_FILE_NOT_FOUND:
                        // File not found doesn't make much sense coming from a directory delete.
                        errorCode = WIN32_ERROR.ERROR_PATH_NOT_FOUND;
                        goto case WIN32_ERROR.ERROR_PATH_NOT_FOUND;
                    case WIN32_ERROR.ERROR_PATH_NOT_FOUND:
                        // We only throw for the top level directory not found, not for any contents.
                        if (!topLevel)
                        { return; }
                        break;
                    case WIN32_ERROR.ERROR_DIR_NOT_EMPTY:
                        if (allowDirectoryNotEmpty)
                        { return; }
                        break;
                    case WIN32_ERROR.ERROR_ACCESS_DENIED:
                        // This conversion was originally put in for Win9x. Keeping for compatibility.
                        throw new IOException($"Access to the path '{fullPath}' is denied.");
                }

                throw Win32Marshal.GetExceptionForWin32Error(errorCode, fullPath);
            }
        }

        [SupportedOSPlatform("Windows10.0.17134.0")]
        public static void SetAttributes(string fullPath, FileAttributes attributes)
        {
            if (Interop.SetFileAttributesFromApp(fullPath, (uint)attributes))
            {
                return;
            }

            WIN32_ERROR errorCode = (WIN32_ERROR)Marshal.GetLastPInvokeError();
            if (errorCode == WIN32_ERROR.ERROR_INVALID_PARAMETER)
            { throw new ArgumentException("Invalid File or Directory attributes value.", nameof(attributes)); }
            throw Win32Marshal.GetExceptionForWin32Error(errorCode, fullPath);
        }

        [SupportedOSPlatform("Windows10.0.17134.0")]
        private static unsafe SafeFileHandle OpenSafeFileHandle(string path, FILE_FLAGS_AND_ATTRIBUTES flags)
        {
            SafeFileHandle handle = Interop.CreateFileFromApp(
                path,
                dwDesiredAccess: 0,
                FileShare.ReadWrite | FileShare.Delete,
                lpSecurityAttributes: null,
                FileMode.Open,
                dwFlagsAndAttributes: flags,
                hTemplateFile: 0);

            return handle;
        }
    }
}
