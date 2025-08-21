// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Runtime.Versioning;
using System.Text;

namespace FileSystemFromApp
{
    /// <inheritdoc cref="StreamReader"/>
    public static class StreamReaderFromApp
    {
        private const int DefaultBufferSize = 1024;  // Byte buffer size
        private const int DefaultFileStreamBufferSize = 4096;

        /// <summary>
        /// The extension for the <see cref="StreamReader"/> class.
        /// </summary>
        extension(StreamReader)
        {
            /// <inheritdoc cref="StreamReader(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static StreamReader CreateFromApp(string path) =>
                StreamReader.CreateFromApp(path, true);

            /// <inheritdoc cref="StreamReader(string, bool)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static StreamReader CreateFromApp(string path, bool detectEncodingFromByteOrderMarks) =>
                StreamReader.CreateFromApp(path, Encoding.UTF8, detectEncodingFromByteOrderMarks, DefaultBufferSize);

            /// <inheritdoc cref="StreamReader(string, Encoding)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static StreamReader CreateFromApp(string path, Encoding? encoding) =>
                StreamReader.CreateFromApp(path, encoding, true, DefaultBufferSize);

            /// <inheritdoc cref="StreamReader(string, Encoding, bool)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static StreamReader CreateFromApp(string path, Encoding? encoding, bool detectEncodingFromByteOrderMarks) =>
                StreamReader.CreateFromApp(path, encoding, detectEncodingFromByteOrderMarks, DefaultBufferSize);

            /// <inheritdoc cref="StreamReader(string, Encoding, bool, int)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static StreamReader CreateFromApp(string path, Encoding? encoding, bool detectEncodingFromByteOrderMarks, int bufferSize) =>
                new(StreamReader.ValidateArgsAndOpenPath(path, bufferSize), encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen: false);

            /// <inheritdoc cref="StreamReader(string, FileStreamOptions)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static StreamReader CreateFromApp(string path, FileStreamOptions options) =>
                StreamReader.CreateFromApp(path, Encoding.UTF8, true, options);

            /// <inheritdoc cref="StreamReader(string, Encoding, bool, FileStreamOptions)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static StreamReader CreateFromApp(string path, Encoding? encoding, bool detectEncodingFromByteOrderMarks, FileStreamOptions options) =>
                new(StreamReader.ValidateArgsAndOpenPath(path, options), encoding!, detectEncodingFromByteOrderMarks, DefaultBufferSize);

            [SupportedOSPlatform("Windows10.0.17134.0")]
            private static FileStream ValidateArgsAndOpenPath(string path, FileStreamOptions options)
            {
                ArgumentException.ThrowIfNullOrEmpty(path);
                ArgumentNullException.ThrowIfNull(options);
                if ((options.Access & FileAccess.Read) == 0)
                {
                    throw new ArgumentException("Stream was not readable.", nameof(options));
                }

                return FileStream.CreateFromApp(path, options);
            }

            [SupportedOSPlatform("Windows10.0.17134.0")]
            private static FileStream ValidateArgsAndOpenPath(string path, int bufferSize)
            {
                ArgumentException.ThrowIfNullOrEmpty(path);
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);

                return FileStream.CreateFromApp(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultFileStreamBufferSize);
            }
        }
    }
}
