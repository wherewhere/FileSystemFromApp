// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Runtime.Versioning;
using System.Text;

namespace FileSystemFromApp
{
    /// <inheritdoc cref="StreamWriter"/>
    public static class StreamWriterFromApp
    {
        private const int DefaultBufferSize = 1024;   // char[]
        private const int DefaultFileStreamBufferSize = 4096;

        /// <summary>
        /// The extension for the <see cref="StreamWriter"/> class.
        /// </summary>
        extension(StreamWriter)
        {
            private static Encoding UTF8NoBOM => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

            /// <inheritdoc cref="StreamWriter(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static StreamWriter CreateFromApp(string path) =>
                StreamWriter.CreateFromApp(path, false, StreamWriter.UTF8NoBOM, DefaultBufferSize);

            /// <inheritdoc cref="StreamWriter(string, bool)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static StreamWriter CreateFromApp(string path, bool append) =>
                StreamWriter.CreateFromApp(path, append, StreamWriter.UTF8NoBOM, DefaultBufferSize);

            /// <inheritdoc cref="StreamWriter(string, bool, Encoding)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static StreamWriter CreateFromApp(string path, bool append, Encoding? encoding) =>
                StreamWriter.CreateFromApp(path, append, encoding, DefaultBufferSize);

            /// <inheritdoc cref="StreamWriter(string, bool, Encoding, int)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static StreamWriter CreateFromApp(string path, bool append, Encoding? encoding, int bufferSize) =>
                new(StreamWriter.ValidateArgsAndOpenPath(path, append, bufferSize), encoding, bufferSize, leaveOpen: false);

            /// <inheritdoc cref="StreamWriter(string, FileStreamOptions)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static StreamWriter CreateFromApp(string path, FileStreamOptions options) =>
                StreamWriter.CreateFromApp(path, StreamWriter.UTF8NoBOM, options);

            /// <inheritdoc cref="StreamWriter(string, Encoding, FileStreamOptions)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static StreamWriter CreateFromApp(string path, Encoding? encoding, FileStreamOptions options) =>
                new(StreamWriter.ValidateArgsAndOpenPath(path, options), encoding!, DefaultFileStreamBufferSize);

            [SupportedOSPlatform("Windows10.0.17134.0")]
            private static FileStream ValidateArgsAndOpenPath(string path, FileStreamOptions options)
            {
                ArgumentException.ThrowIfNullOrEmpty(path);
                ArgumentNullException.ThrowIfNull(options);
                if ((options.Access & FileAccess.Write) == 0)
                {
                    throw new ArgumentException("Stream was not writable.", nameof(options));
                }

                return FileStream.CreateFromApp(path, options);
            }

            [SupportedOSPlatform("Windows10.0.17134.0")]
            private static FileStream ValidateArgsAndOpenPath(string path, bool append, int bufferSize)
            {
                ArgumentException.ThrowIfNullOrEmpty(path);
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);

                return FileStream.CreateFromApp(path, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read, DefaultFileStreamBufferSize);
            }
        }
    }
}
