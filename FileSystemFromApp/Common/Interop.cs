using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;

namespace FileSystemFromApp.Common
{
    /// <inheritdoc cref="PInvoke"/>
    internal static partial class Interop
    {
        /// <inheritdoc cref="PInvoke.CopyFileFromApp(string, string, BOOL)"/>
        [SupportedOSPlatform("Windows10.0.17134.0")]
        internal static WIN32_ERROR CopyFileFromApp(string lpExistingFileName, string lpNewFileName, bool bFailIfExists)
        {
            if (!PInvoke.CopyFileFromApp(lpExistingFileName, lpNewFileName, bFailIfExists))
            {
                return (WIN32_ERROR)Marshal.GetLastPInvokeError();
            }

            return WIN32_ERROR.ERROR_SUCCESS;
        }

        /// <inheritdoc cref="PInvoke.CreateDirectoryFromApp(string, SECURITY_ATTRIBUTES?)"/>
        [SupportedOSPlatform("Windows10.0.17134.0")]
        internal static unsafe bool CreateDirectoryFromApp(string lpPathName, in SECURITY_ATTRIBUTES? lpSecurityAttributes)
        {
            // We always want to add for CreateDirectory to get around the legacy 248 character limitation
            lpPathName = PathInternal.EnsureExtendedPrefix(lpPathName);
            return PInvoke.CreateDirectoryFromApp(lpPathName, lpSecurityAttributes);
        }

        /// <inheritdoc cref="PInvoke.CreateFileFromApp(string, uint, uint, SECURITY_ATTRIBUTES?, uint, uint, SafeHandle)"/>
        [SupportedOSPlatform("Windows10.0.17134.0")]
        internal static unsafe SafeFileHandle CreateFileFromApp(string lpFileName, GENERIC_ACCESS_RIGHTS dwDesiredAccess, FileShare dwShareMode, SECURITY_ATTRIBUTES? lpSecurityAttributes, FileMode dwCreationDisposition, FILE_FLAGS_AND_ATTRIBUTES dwFlagsAndAttributes, nint hTemplateFile)
        {
            lpFileName = PathInternal.EnsureExtendedPrefixIfNeeded(lpFileName);
            return PInvoke.CreateFileFromApp(lpFileName, (uint)dwDesiredAccess, (uint)dwShareMode, lpSecurityAttributes, (uint)dwCreationDisposition, (uint)dwFlagsAndAttributes, new DefaultSafeHandle(hTemplateFile));
        }

        /// <inheritdoc cref="PInvoke.CreateFileFromApp(string, uint, uint, SECURITY_ATTRIBUTES?, uint, uint, SafeHandle)"/>
        [SupportedOSPlatform("Windows10.0.17134.0")]
        internal static unsafe SafeFileHandle CreateFileFromApp(string lpFileName, GENERIC_ACCESS_RIGHTS dwDesiredAccess, FileShare dwShareMode, FileMode dwCreationDisposition, FILE_FLAGS_AND_ATTRIBUTES dwFlagsAndAttributes)
        {
            lpFileName = PathInternal.EnsureExtendedPrefixIfNeeded(lpFileName);
            return PInvoke.CreateFileFromApp(lpFileName, (uint)dwDesiredAccess, (uint)dwShareMode, null, (uint)dwCreationDisposition, (uint)dwFlagsAndAttributes, new DefaultSafeHandle(0));
        }

        /// <inheritdoc cref="PInvoke.DeleteFileFromApp(string)"/>
        [SupportedOSPlatform("Windows10.0.17134.0")]
        internal static bool DeleteFileFromApp(string lpFileName)
        {
            lpFileName = PathInternal.EnsureExtendedPrefixIfNeeded(lpFileName);
            return PInvoke.DeleteFileFromApp(lpFileName);
        }

        /// <inheritdoc cref="PInvoke.FindFirstFileExFromApp(string, FINDEX_INFO_LEVELS, void*, FINDEX_SEARCH_OPS, uint)"/>
        [SupportedOSPlatform("Windows10.0.17134.0")]
        internal static unsafe SafeFileHandle FindFirstFileExFromApp(string lpFileName, ref WIN32_FIND_DATAW fInfoLevelId)
        {
            lpFileName = PathInternal.EnsureExtendedPrefixIfNeeded(lpFileName);
            fixed (void* ptr = &fInfoLevelId)
            {
                // use FindExInfoBasic since we don't care about short name and it has better perf
                return PInvoke.FindFirstFileExFromApp(lpFileName, FINDEX_INFO_LEVELS.FindExInfoBasic, ptr, FINDEX_SEARCH_OPS.FindExSearchNameMatch, 0);
            }
        }

        /// <inheritdoc cref="PInvoke.GetFileAttributesExFromApp(string, GET_FILEEX_INFO_LEVELS, void*)"/>
        [SupportedOSPlatform("Windows10.0.17134.0")]
        internal static unsafe bool GetFileAttributesExFromApp(string? lpFileName, GET_FILEEX_INFO_LEVELS fileInfoLevel, ref WIN32_FILE_ATTRIBUTE_DATA lpFileInformation)
        {
            lpFileName = PathInternal.EnsureExtendedPrefixIfNeeded(lpFileName);
            fixed (void* ptr = &lpFileInformation)
            {
                return PInvoke.GetFileAttributesExFromApp(lpFileName, fileInfoLevel, ptr);
            }
        }

        /// <inheritdoc cref="PInvoke.MoveFileFromApp(string, string)"/>
        [SupportedOSPlatform("Windows10.0.17134.0")]
        internal static bool MoveFileFromApp(string lpExistingFileName, string lpNewFileName)
        {
            lpExistingFileName = PathInternal.EnsureExtendedPrefixIfNeeded(lpExistingFileName);
            lpNewFileName = PathInternal.EnsureExtendedPrefixIfNeeded(lpNewFileName);

            return PInvoke.MoveFileFromApp(lpExistingFileName, lpNewFileName);
        }

        /// <inheritdoc cref="PInvoke.RemoveDirectoryFromApp(string)"/>
        [SupportedOSPlatform("Windows10.0.17134.0")]
        internal static bool RemoveDirectoryFromApp(string lpPathName)
        {
            lpPathName = PathInternal.EnsureExtendedPrefixIfNeeded(lpPathName);
            return PInvoke.RemoveDirectoryFromApp(lpPathName);
        }

        /// <inheritdoc cref="PInvoke.ReplaceFileFromApp(string, string, string, uint)"/>
        [SupportedOSPlatform("Windows10.0.17134.0")]
        internal static bool ReplaceFileFromApp(string lpReplacedFileName, string lpReplacementFileName, string? lpBackupFileName, uint dwReplaceFlags)
        {
            lpReplacedFileName = PathInternal.EnsureExtendedPrefixIfNeeded(lpReplacedFileName);
            lpReplacementFileName = PathInternal.EnsureExtendedPrefixIfNeeded(lpReplacementFileName);
            lpBackupFileName = PathInternal.EnsureExtendedPrefixIfNeeded(lpBackupFileName);

            return PInvoke.ReplaceFileFromApp(lpReplacedFileName, lpReplacementFileName, lpBackupFileName, dwReplaceFlags);
        }

        /// <inheritdoc cref="PInvoke.SetFileAttributesFromApp(string, uint)"/>
        [SupportedOSPlatform("Windows10.0.17134.0")]
        internal static bool SetFileAttributesFromApp(string lpFileName, uint dwFileAttributes)
        {
            lpFileName = PathInternal.EnsureExtendedPrefixIfNeeded(lpFileName);
            return PInvoke.SetFileAttributesFromApp(lpFileName, dwFileAttributes);
        }

        /// <inheritdoc cref="SafeHandle"/>
        private sealed partial class DefaultSafeHandle(nint invalidHandleValue, bool ownsHandle) : SafeHandle(invalidHandleValue, ownsHandle)
        {
            /// <inheritdoc cref="SafeHandle(nint, bool)"/>
            public DefaultSafeHandle(nint handle) : this(handle, true) => SetHandle(handle);

            /// <inheritdoc/>
            public override bool IsInvalid => handle == 0;

            /// <inheritdoc/>
            protected override bool ReleaseHandle() => true;
        }
    }

    internal static partial class Interop
    {
        /// <inheritdoc cref="PInvoke.DeleteVolumeMountPoint(string)"/>
        internal static bool DeleteVolumeMountPoint(string lpszVolumeMountPoint)
        {
            lpszVolumeMountPoint = PathInternal.EnsureExtendedPrefixIfNeeded(lpszVolumeMountPoint);
            return PInvoke.DeleteVolumeMountPoint(lpszVolumeMountPoint);
        }
    }
}
