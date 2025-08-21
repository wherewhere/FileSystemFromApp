// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Runtime.Versioning;

namespace FileSystemFromApp
{
    /// <inheritdoc cref="FileStream"/>
    public static class FileStreamFromApp
    {
        internal const int DefaultBufferSize = 4096;
        internal const FileShare DefaultShare = FileShare.Read;
        private const bool DefaultIsAsync = false;

        /// <summary>
        /// The extension for the <see cref="FileStream"/> class.
        /// </summary>
        extension(FileStream)
        {
            /// <inheritdoc cref="FileStream(string, FileMode)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static FileStream CreateFromApp(string path, FileMode mode) =>
                FileStream.CreateFromApp(path, mode, mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite, DefaultShare, DefaultBufferSize, DefaultIsAsync);

            /// <inheritdoc cref="FileStream(string, FileMode, FileAccess)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static FileStream CreateFromApp(string path, FileMode mode, FileAccess access) =>
                FileStream.CreateFromApp(path, mode, access, DefaultShare, DefaultBufferSize, DefaultIsAsync);

            /// <inheritdoc cref="FileStream(string, FileMode, FileAccess, FileShare)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static FileStream CreateFromApp(string path, FileMode mode, FileAccess access, FileShare share) =>
                FileStream.CreateFromApp(path, mode, access, share, DefaultBufferSize, DefaultIsAsync);

            /// <inheritdoc cref="FileStream(string, FileMode, FileAccess, FileShare, int)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static FileStream CreateFromApp(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize) =>
                FileStream.CreateFromApp(path, mode, access, share, bufferSize, DefaultIsAsync);

            /// <inheritdoc cref="FileStream(string, FileMode, FileAccess, FileShare, int, bool)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static FileStream CreateFromApp(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync) =>
                FileStream.CreateFromApp(path, mode, access, share, bufferSize, useAsync ? FileOptions.Asynchronous : FileOptions.None);

            /// <inheritdoc cref="FileStream(string, FileMode, FileAccess, FileShare, int, FileOptions)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static FileStream CreateFromApp(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options)
            {
                SafeFileHandle handle = File.OpenHandleFromApp(path, mode, access, share, options);
                return new FileStream(handle, access, bufferSize);
            }

            /// <inheritdoc cref="FileStream(string, FileStreamOptions)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static FileStream CreateFromApp(string path, FileStreamOptions options)
            {
                SafeFileHandle handle = File.OpenHandleFromApp(path, options.Mode, options.Access, options.Share, options.Options, options.PreallocationSize);
                return new FileStream(handle, options.Access, options.BufferSize);
            }
        }
    }
}
