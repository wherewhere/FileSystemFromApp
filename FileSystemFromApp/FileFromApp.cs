// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FileSystemFromApp.Common;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileSystemFromApp
{
    /// <inheritdoc cref="File"/>
    public static class FileFromApp
    {
        internal const int DefaultBufferSize = 4096;

        /// <summary>
        /// The extension for the <see cref="File"/> class.
        /// </summary>
        extension(File)
        {
            private static Encoding UTF8NoBOM => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

            /// <inheritdoc cref="File.OpenText(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static StreamReader OpenTextFromApp(string path)
                => StreamReader.CreateFromApp(path);

            /// <inheritdoc cref="File.CreateText(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static StreamWriter CreateTextFromApp(string path)
                => StreamWriter.CreateFromApp(path, append: false);

            /// <inheritdoc cref="File.AppendText(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static StreamWriter AppendTextFromApp(string path)
                => StreamWriter.CreateFromApp(path, append: true);

            /// <inheritdoc cref="File.Copy(string, string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static void CopyFromApp(string sourceFileName, string destFileName)
                => CopyFromApp(sourceFileName, destFileName, overwrite: false);

            /// <inheritdoc cref="File.Copy(string, string, bool)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static void CopyFromApp(string sourceFileName, string destFileName, bool overwrite)
            {
                ArgumentException.ThrowIfNullOrEmpty(sourceFileName);
                ArgumentException.ThrowIfNullOrEmpty(destFileName);

                FileSystem.CopyFile(Path.GetFullPath(sourceFileName), Path.GetFullPath(destFileName), overwrite);
            }

            // Creates a file in a particular path.  If the file exists, it is replaced.
            // The file is opened with ReadWrite access and cannot be opened by another
            // application until it has been closed.  An IOException is thrown if the
            // directory specified doesn't exist.
            /// <inheritdoc cref="File.Create(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static FileStream CreateFromApp(string path)
                => File.CreateFromApp(path, DefaultBufferSize);

            // Creates a file in a particular path.  If the file exists, it is replaced.
            // The file is opened with ReadWrite access and cannot be opened by another
            // application until it has been closed.  An IOException is thrown if the
            // directory specified doesn't exist.
            /// <inheritdoc cref="File.Create(string, int)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static FileStream CreateFromApp(string path, int bufferSize)
                => FileStream.CreateFromApp(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize);

            /// <inheritdoc cref="File.Create(string, int, FileOptions)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static FileStream CreateFromApp(string path, int bufferSize, FileOptions options)
                => FileStream.CreateFromApp(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize, options);

            // Deletes a file. The file specified by the designated path is deleted.
            // If the file does not exist, Delete succeeds without throwing
            // an exception.
            //
            // On Windows, Delete will fail for a file that is open for normal I/O
            // or a file that is memory mapped.
            /// <inheritdoc cref="File.Delete(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static void DeleteFromApp(string path)
            {
                ArgumentNullException.ThrowIfNull(path);
                FileSystem.DeleteFile(Path.GetFullPath(path));
            }

            // Tests whether a file exists. The result is true if the file
            // given by the specified path exists; otherwise, the result is
            // false.  Note that if path describes a directory,
            // Exists will return false.
            /// <inheritdoc cref="File.Exists(string?)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static bool ExistsFromApp([NotNullWhen(true)] string? path)
            {
                try
                {
                    if (path == null)
                    { return false; }
                    if (path.Length == 0)
                    { return false; }

                    path = Path.GetFullPath(path);

                    // After normalizing, check whether path ends in directory separator.
                    // Otherwise, FillAttributeInfo removes it and we may return a false positive.
                    // GetFullPath should never return null
                    Debug.Assert(path != null, "File.Exists: GetFullPath returned null");
                    if (path.Length > 0 && PathInternal.IsDirectorySeparator(path[^1]))
                    {
                        return false;
                    }

                    return FileSystem.FileExists(path);
                }
                catch (ArgumentException) { }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }

                return false;
            }

            /// <inheritdoc cref="File.Open(string, FileStreamOptions)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static FileStream OpenFromApp(string path, FileStreamOptions options) => FileStream.CreateFromApp(path, options);

            /// <inheritdoc cref="File.Open(string, FileMode)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static FileStream OpenFromApp(string path, FileMode mode)
                => File.OpenFromApp(path, mode, (mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite), FileShare.None);

            /// <inheritdoc cref="File.Open(string, FileMode, FileAccess)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static FileStream OpenFromApp(string path, FileMode mode, FileAccess access)
                => File.OpenFromApp(path, mode, access, FileShare.None);

            /// <inheritdoc cref="File.Open(string, FileMode, FileAccess, FileShare)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static FileStream OpenFromApp(string path, FileMode mode, FileAccess access, FileShare share)
                => FileStream.CreateFromApp(path, mode, access, share);

            /// <inheritdoc cref="File.OpenHandle(string, FileMode, FileAccess, FileShare, FileOptions, long)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static SafeFileHandle OpenHandleFromApp(string path, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read,
                FileShare share = FileShare.Read, FileOptions options = FileOptions.None, long preallocationSize = 0)
            {
                FileStreamHelpers.ValidateArguments(path, mode, access, share, bufferSize: 0, options, preallocationSize);

                return SafeFileHandle.Open(Path.GetFullPath(path), mode, access, share, options, preallocationSize);
            }

            // File and Directory UTC APIs treat a DateTimeKind.Unspecified as UTC whereas
            // ToUniversalTime treats this as local.
            internal static DateTimeOffset GetUtcDateTimeOffset(DateTime dateTime)
            {
                if (dateTime.Kind == DateTimeKind.Local)
                    dateTime = dateTime.ToUniversalTime();

                return new DateTimeOffset(dateTime.Ticks, default);
            }

            /// <inheritdoc cref="File.GetCreationTime(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static DateTime GetCreationTimeFromApp(string path)
                => FileSystem.GetCreationTime(Path.GetFullPath(path)).LocalDateTime;

            /// <inheritdoc cref="File.GetCreationTimeUtc(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static DateTime GetCreationTimeUtcFromApp(string path)
                => FileSystem.GetCreationTime(Path.GetFullPath(path)).UtcDateTime;

            /// <inheritdoc cref="File.GetLastAccessTime(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static DateTime GetLastAccessTimeFromApp(string path)
                => FileSystem.GetLastAccessTime(Path.GetFullPath(path)).LocalDateTime;

            /// <inheritdoc cref="File.GetLastAccessTimeUtc(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static DateTime GetLastAccessTimeUtcFromApp(string path)
                => FileSystem.GetLastAccessTime(Path.GetFullPath(path)).UtcDateTime;

            /// <inheritdoc cref="File.GetLastWriteTime(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static DateTime GetLastWriteTimeFromApp(string path)
                => FileSystem.GetLastWriteTime(Path.GetFullPath(path)).LocalDateTime;

            /// <inheritdoc cref="File.GetLastWriteTimeUtc(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static DateTime GetLastWriteTimeUtcFromApp(string path)
                => FileSystem.GetLastWriteTime(Path.GetFullPath(path)).UtcDateTime;

            /// <inheritdoc cref="File.GetAttributes(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static FileAttributes GetAttributesFromApp(string path)
                => FileSystem.GetAttributes(Path.GetFullPath(path));

            /// <inheritdoc cref="File.SetAttributes(string, FileAttributes)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static void SetAttributesFromApp(string path, FileAttributes fileAttributes)
                => FileSystem.SetAttributes(Path.GetFullPath(path), fileAttributes);

            /// <inheritdoc cref="File.OpenRead(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static FileStream OpenReadFromApp(string path)
                => FileStream.CreateFromApp(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            /// <inheritdoc cref="File.OpenWrite(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static FileStream OpenWriteFromApp(string path)
                => FileStream.CreateFromApp(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);

            /// <inheritdoc cref="File.ReadAllText(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static string ReadAllTextFromApp(string path)
                => File.ReadAllTextFromApp(path, Encoding.UTF8);

            /// <inheritdoc cref="File.ReadAllText(string, Encoding)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static string ReadAllTextFromApp(string path, Encoding encoding)
            {
                File.Validate(path, encoding);

                using StreamReader sr = StreamReader.CreateFromApp(path, encoding, detectEncodingFromByteOrderMarks: true);
                return sr.ReadToEnd();
            }

            //            public static void WriteAllText(string path, string? contents)
            //                => WriteAllText(path, contents, UTF8NoBOM);

            //            /// <summary>
            //            /// Creates a new file, writes the specified string to the file, and then closes the file.
            //            /// If the target file already exists, it is truncated and overwritten.
            //            /// </summary>
            //            /// <param name="path">The file to write to.</param>
            //            /// <param name="contents">The characters to write to the file.</param>
            //            /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
            //            /// <exception cref="ArgumentException"><paramref name="path"/> is empty.</exception>
            //            /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
            //            /// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
            //            /// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
            //            /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is read-only.</exception>
            //            /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is hidden.</exception>
            //            /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a directory.</exception>
            //            /// <exception cref="UnauthorizedAccessException">This operation is not supported on the current platform.</exception>
            //            /// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.</exception>
            //            /// <exception cref="NotSupportedException"><paramref name="path"/> is in an invalid format.</exception>
            //            /// <remarks>
            //            /// This method uses UTF-8 encoding without a Byte-Order Mark (BOM), so using the GetPreamble method will return an empty byte array. If it is necessary to
            //            /// include a UTF-8 identifier, such as a byte order mark, at the beginning of a file, use the <see cref="WriteAllText(string, ReadOnlySpan{char}, Encoding)"/> method.
            //            /// </remarks>
            //            public static void WriteAllText(string path, ReadOnlySpan<char> contents)
            //                => WriteAllText(path, contents, UTF8NoBOM);

            //            public static void WriteAllText(string path, string? contents, Encoding encoding)
            //            {
            //                Validate(path, encoding);

            //                WriteToFile(path, FileMode.Create, contents, encoding);
            //            }

            //            /// <summary>
            //            /// Creates a new file, writes the specified string to the file using the specified encoding, and then closes the file.
            //            /// If the target file already exists, it is truncated and overwritten.
            //            /// </summary>
            //            /// <param name="path">The file to write to.</param>
            //            /// <param name="contents">The characters to write to the file.</param>
            //            /// <param name="encoding">The encoding to apply to the string.</param>
            //            /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
            //            /// <exception cref="ArgumentException"><paramref name="path"/> is empty.</exception>
            //            /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is <see langword="null"/>.</exception>
            //            /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
            //            /// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
            //            /// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
            //            /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is read-only.</exception>
            //            /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is hidden.</exception>
            //            /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a directory.</exception>
            //            /// <exception cref="UnauthorizedAccessException">This operation is not supported on the current platform.</exception>
            //            /// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.</exception>
            //            /// <exception cref="NotSupportedException"><paramref name="path"/> is in an invalid format.</exception>
            //            public static void WriteAllText(string path, ReadOnlySpan<char> contents, Encoding encoding)
            //            {
            //                Validate(path, encoding);

            //                WriteToFile(path, FileMode.Create, contents, encoding);
            //            }

            //            public static byte[] ReadAllBytes(string path)
            //            {
            //                // SequentialScan is a perf hint that requires extra sys-call on non-Windows OSes.
            //                FileOptions options = OperatingSystem.IsWindows() ? FileOptions.SequentialScan : FileOptions.None;
            //                using (SafeFileHandle sfh = File.OpenHandleFromApp(path, FileMode.Open, FileAccess.Read, FileShare.Read, options))
            //                {
            //                    long fileLength = 0;
            //                    if (sfh.CanSeek && (fileLength = sfh.GetFileLength()) > Array.MaxLength)
            //                    {
            //                        throw new IOException(SR.IO_FileTooLong2GB);
            //                    }

            //#if DEBUG
            //                    fileLength = 0; // improve the test coverage for ReadAllBytesUnknownLength
            //#endif

            //                    if (fileLength == 0)
            //                    {
            //                        // Some file systems (e.g. procfs on Linux) return 0 for length even when there's content; also there are non-seekable files.
            //                        // Thus we need to assume 0 doesn't mean empty.
            //                        return ReadAllBytesUnknownLength(sfh);
            //                    }

            //                    int index = 0;
            //                    int count = (int)fileLength;
            //                    byte[] bytes = new byte[count];
            //                    while (count > 0)
            //                    {
            //                        int n = RandomAccess.Read(sfh, bytes.AsSpan(index, count), index);
            //                        if (n == 0)
            //                        {
            //                            ThrowHelper.ThrowEndOfFileException();
            //                        }

            //                        index += n;
            //                        count -= n;
            //                    }
            //                    return bytes;
            //                }
            //            }

            /// <inheritdoc cref="File.WriteAllBytes(string, byte[])"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static void WriteAllBytesFromApp(string path, byte[] bytes)
            {
                ArgumentNullException.ThrowIfNull(bytes);

                File.WriteAllBytesFromApp(path, new ReadOnlySpan<byte>(bytes));
            }

            /// <inheritdoc cref="File.WriteAllBytes(string, ReadOnlySpan{byte})"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static void WriteAllBytesFromApp(string path, ReadOnlySpan<byte> bytes)
            {
                ArgumentException.ThrowIfNullOrEmpty(path);

                using SafeFileHandle sfh = File.OpenHandleFromApp(path, FileMode.Create, FileAccess.Write, FileShare.Read);
                RandomAccess.Write(sfh, bytes, 0);
            }

            /// <inheritdoc cref="File.AppendAllBytes(string, byte[])"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static void AppendAllBytesFromApp(string path, byte[] bytes)
            {
                ArgumentNullException.ThrowIfNull(bytes);

                File.AppendAllBytesFromApp(path, new ReadOnlySpan<byte>(bytes));
            }

            /// <inheritdoc cref="File.AppendAllBytes(string, ReadOnlySpan{byte})"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static void AppendAllBytesFromApp(string path, ReadOnlySpan<byte> bytes)
            {
                ArgumentException.ThrowIfNullOrEmpty(path);

                using SafeFileHandle fileHandle = File.OpenHandleFromApp(path, FileMode.Append, FileAccess.Write, FileShare.Read);
                long fileOffset = RandomAccess.GetLength(fileHandle);
                RandomAccess.Write(fileHandle, bytes, fileOffset);
            }

            /// <inheritdoc cref="File.AppendAllBytesAsync(string, byte[], CancellationToken)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static Task AppendAllBytesFromAppAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(bytes);

                return File.AppendAllBytesFromAppAsync(path, new ReadOnlyMemory<byte>(bytes), cancellationToken);
            }

            /// <inheritdoc cref="File.AppendAllBytesAsync(string, ReadOnlyMemory{byte}, CancellationToken)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static Task AppendAllBytesFromAppAsync(string path, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default)
            {
                ArgumentException.ThrowIfNullOrEmpty(path);

                return cancellationToken.IsCancellationRequested
                    ? Task.FromCanceled(cancellationToken)
                    : Core(path, bytes, cancellationToken);

                static async Task Core(string path, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken)
                {
                    using SafeFileHandle fileHandle = File.OpenHandleFromApp(path, FileMode.Append, FileAccess.Write, FileShare.Read, FileOptions.Asynchronous);
                    long fileOffset = RandomAccess.GetLength(fileHandle);
                    await RandomAccess.WriteAsync(fileHandle, bytes, fileOffset, cancellationToken).ConfigureAwait(false);
                }
            }

            //            public static string[] ReadAllLines(string path)
            //                => ReadAllLines(path, Encoding.UTF8);

            //            public static string[] ReadAllLines(string path, Encoding encoding)
            //            {
            //                Validate(path, encoding);

            //                string? line;
            //                List<string> lines = new List<string>();

            //                using StreamReader sr = new StreamReader(path, encoding);
            //                while ((line = sr.ReadLine()) != null)
            //                {
            //                    lines.Add(line);
            //                }

            //                return lines.ToArray();
            //            }

            //            public static IEnumerable<string> ReadLines(string path)
            //                => ReadLines(path, Encoding.UTF8);

            //            public static IEnumerable<string> ReadLines(string path, Encoding encoding)
            //            {
            //                Validate(path, encoding);

            //                return ReadLinesIterator.CreateIterator(path, encoding);
            //            }

            //            /// <summary>
            //            /// Asynchronously reads the lines of a file.
            //            /// </summary>
            //            /// <param name="path">The file to read.</param>
            //            /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
            //            /// <returns>The async enumerable that represents all the lines of the file, or the lines that are the result of a query.</returns>
            //            public static IAsyncEnumerable<string> ReadLinesAsync(string path, CancellationToken cancellationToken = default)
            //                => ReadLinesAsync(path, Encoding.UTF8, cancellationToken);

            //            /// <summary>
            //            /// Asynchronously reads the lines of a file that has a specified encoding.
            //            /// </summary>
            //            /// <param name="path">The file to read.</param>
            //            /// <param name="encoding">The encoding that is applied to the contents of the file.</param>
            //            /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
            //            /// <returns>The async enumerable that represents all the lines of the file, or the lines that are the result of a query.</returns>
            //            public static IAsyncEnumerable<string> ReadLinesAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
            //            {
            //                Validate(path, encoding);

            //                StreamReader sr = AsyncStreamReader(path, encoding); // Move first streamReader allocation here so to throw related file exception upfront, which will cause known leaking if user never actually foreach's over the enumerable
            //                return IterateFileLinesAsync(sr, path, encoding, cancellationToken);
            //            }

            /// <inheritdoc cref="File.WriteAllLines(string, string[])"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static void WriteAllLinesFromApp(string path, string[] contents)
                => WriteAllLinesFromApp(path, (IEnumerable<string>)contents);

            /// <inheritdoc cref="File.WriteAllLines(string, IEnumerable{string})"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static void WriteAllLinesFromApp(string path, IEnumerable<string> contents)
                => WriteAllLinesFromApp(path, contents, File.UTF8NoBOM);

            /// <inheritdoc cref="File.WriteAllLines(string, string[], Encoding)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static void WriteAllLinesFromApp(string path, string[] contents, Encoding encoding)
                => WriteAllLinesFromApp(path, (IEnumerable<string>)contents, encoding);

            /// <inheritdoc cref="File.WriteAllLines(string, IEnumerable{string}, Encoding)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static void WriteAllLinesFromApp(string path, IEnumerable<string> contents, Encoding encoding)
            {
                File.Validate(path, encoding);
                ArgumentNullException.ThrowIfNull(contents);
                File.InternalWriteAllLines(StreamWriter.CreateFromApp(path, false, encoding), contents);
            }

            private static void InternalWriteAllLines(StreamWriter writer, IEnumerable<string> contents)
            {
                Debug.Assert(writer != null);
                Debug.Assert(contents != null);

                using (writer)
                {
                    foreach (string line in contents)
                    {
                        writer.WriteLine(line);
                    }
                }
            }

            //            public static void AppendAllText(string path, string? contents)
            //                => AppendAllText(path, contents, UTF8NoBOM);

            //            /// <summary>
            //            /// Appends the specified string to the file, creating the file if it does not already exist.
            //            /// </summary>
            //            /// <param name="path">The file to append to.</param>
            //            /// <param name="contents">The characters to write to the file.</param>
            //            /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
            //            /// <exception cref="ArgumentException"><paramref name="path"/> is empty.</exception>
            //            /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
            //            /// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
            //            /// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
            //            /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is read-only.</exception>
            //            /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is hidden.</exception>
            //            /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a directory.</exception>
            //            /// <exception cref="UnauthorizedAccessException">This operation is not supported on the current platform.</exception>
            //            /// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.</exception>
            //            /// <exception cref="NotSupportedException"><paramref name="path"/> is in an invalid format.</exception>
            //            /// <remarks>
            //            /// Given a string and a file path, this method opens the specified file, appends the string to the end of the file using the specified encoding,
            //            /// and then closes the file. The file handle is guaranteed to be closed by this method, even if exceptions are raised. The method creates the file
            //            /// if it doesn't exist, but it doesn't create new directories. Therefore, the value of the path parameter must contain existing directories.
            //            /// </remarks>
            //            public static void AppendAllText(string path, ReadOnlySpan<char> contents)
            //                => AppendAllText(path, contents, UTF8NoBOM);

            //            public static void AppendAllText(string path, string? contents, Encoding encoding)
            //            {
            //                Validate(path, encoding);

            //                WriteToFile(path, FileMode.Append, contents, encoding);
            //            }

            //            /// <summary>
            //            /// Appends the specified string to the file, creating the file if it does not already exist.
            //            /// </summary>
            //            /// <param name="path">The file to append to.</param>
            //            /// <param name="contents">The characters to write to the file.</param>
            //            /// <param name="encoding">The encoding to apply to the string.</param>
            //            /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
            //            /// <exception cref="ArgumentException"><paramref name="path"/> is empty.</exception>
            //            /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is <see langword="null"/>.</exception>
            //            /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
            //            /// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
            //            /// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
            //            /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is read-only.</exception>
            //            /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is hidden.</exception>
            //            /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a directory.</exception>
            //            /// <exception cref="UnauthorizedAccessException">This operation is not supported on the current platform.</exception>
            //            /// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.</exception>
            //            /// <exception cref="NotSupportedException"><paramref name="path"/> is in an invalid format.</exception>
            //            /// <remarks>
            //            /// Given a string and a file path, this method opens the specified file, appends the string to the end of the file using the specified encoding,
            //            /// and then closes the file. The file handle is guaranteed to be closed by this method, even if exceptions are raised. The method creates the file
            //            /// if it doesn't exist, but it doesn't create new directories. Therefore, the value of the path parameter must contain existing directories.
            //            /// </remarks>
            //            public static void AppendAllText(string path, ReadOnlySpan<char> contents, Encoding encoding)
            //            {
            //                Validate(path, encoding);

            //                WriteToFile(path, FileMode.Append, contents, encoding);
            //            }

            //            public static void AppendAllLines(string path, IEnumerable<string> contents)
            //                => AppendAllLines(path, contents, UTF8NoBOM);

            //            public static void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
            //            {
            //                Validate(path, encoding);
            //                ArgumentNullException.ThrowIfNull(contents);
            //                InternalWriteAllLines(new StreamWriter(path, true, encoding), contents);
            //            }

            /// <inheritdoc cref="File.Replace(string, string, string?)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static void ReplaceFromApp(string sourceFileName, string destinationFileName, string? destinationBackupFileName)
                => ReplaceFromApp(sourceFileName, destinationFileName, destinationBackupFileName, ignoreMetadataErrors: false);

            /// <inheritdoc cref="File.Replace(string, string, string?, bool)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static void ReplaceFromApp(string sourceFileName, string destinationFileName, string? destinationBackupFileName, bool ignoreMetadataErrors)
            {
                ArgumentNullException.ThrowIfNull(sourceFileName);
                ArgumentNullException.ThrowIfNull(destinationFileName);

                FileSystem.ReplaceFile(
                    Path.GetFullPath(sourceFileName),
                    Path.GetFullPath(destinationFileName),
                    destinationBackupFileName != null ? Path.GetFullPath(destinationBackupFileName) : null,
                    ignoreMetadataErrors);
            }

            // Moves a specified file to a new location and potentially a new file name.
            // This method does work across volumes.
            //
            // The caller must have certain FileIOPermissions.  The caller must
            // have Read and Write permission to
            // sourceFileName and Write
            // permissions to destFileName.
            //
            /// <inheritdoc cref="File.Move(string, string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static void MoveFromApp(string sourceFileName, string destFileName)
            {
                ArgumentException.ThrowIfNullOrEmpty(sourceFileName);
                ArgumentException.ThrowIfNullOrEmpty(destFileName);

                string fullSourceFileName = Path.GetFullPath(sourceFileName);
                string fullDestFileName = Path.GetFullPath(destFileName);

                if (!FileSystem.FileExists(fullSourceFileName))
                {
                    throw new FileNotFoundException($"Could not find file '{fullSourceFileName}'.", fullSourceFileName);
                }

                FileSystem.MoveFile(fullSourceFileName, fullDestFileName);
            }

            //            // If we use the path-taking constructors we will not have FileOptions.Asynchronous set and
            //            // we will have asynchronous file access faked by the thread pool. We want the real thing.
            //            private static StreamReader AsyncStreamReader(string path, Encoding encoding)
            //                => new StreamReader(
            //                    new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan),
            //                    encoding, detectEncodingFromByteOrderMarks: true);

            //            public static Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
            //                => ReadAllTextAsync(path, Encoding.UTF8, cancellationToken);

            //            public static Task<string> ReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
            //            {
            //                Validate(path, encoding);

            //                return cancellationToken.IsCancellationRequested
            //                    ? Task.FromCanceled<string>(cancellationToken)
            //                    : InternalReadAllTextAsync(path, encoding, cancellationToken);
            //            }

            //            private static async Task<string> InternalReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken)
            //            {
            //                Debug.Assert(!string.IsNullOrEmpty(path));
            //                Debug.Assert(encoding != null);

            //                char[]? buffer = null;
            //                StreamReader sr = AsyncStreamReader(path, encoding);
            //                try
            //                {
            //                    cancellationToken.ThrowIfCancellationRequested();
            //                    buffer = ArrayPool<char>.Shared.Rent(sr.CurrentEncoding.GetMaxCharCount(DefaultBufferSize));
            //                    StringBuilder sb = new StringBuilder();
            //                    while (true)
            //                    {
            //                        int read = await sr.ReadAsync(new Memory<char>(buffer), cancellationToken).ConfigureAwait(false);
            //                        if (read == 0)
            //                        {
            //                            return sb.ToString();
            //                        }

            //                        sb.Append(buffer, 0, read);
            //                    }
            //                }
            //                finally
            //                {
            //                    sr.Dispose();
            //                    if (buffer != null)
            //                    {
            //                        ArrayPool<char>.Shared.Return(buffer);
            //                    }
            //                }
            //            }

            //            public static Task WriteAllTextAsync(string path, string? contents, CancellationToken cancellationToken = default)
            //                => WriteAllTextAsync(path, contents, UTF8NoBOM, cancellationToken);

            //            /// <summary>
            //            /// Asynchronously creates a new file, writes the specified string to the file, and then closes the file.
            //            /// If the target file already exists, it is truncated and overwritten.
            //            /// </summary>
            //            /// <param name="path">The file to write to.</param>
            //            /// <param name="contents">The characters to write to the file.</param>
            //            /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
            //            /// <returns>A task that represents the asynchronous write operation.</returns>
            //            /// <exception cref="OperationCanceledException">The cancellation token was canceled. This exception is stored into the returned task.</exception>
            //            public static Task WriteAllTextAsync(string path, ReadOnlyMemory<char> contents, CancellationToken cancellationToken = default)
            //                => WriteAllTextAsync(path, contents, UTF8NoBOM, cancellationToken);

            //            public static Task WriteAllTextAsync(string path, string? contents, Encoding encoding, CancellationToken cancellationToken = default)
            //                => WriteAllTextAsync(path, contents.AsMemory(), encoding, cancellationToken);

            //            /// <summary>
            //            /// Asynchronously creates a new file, writes the specified string to the file using the specified encoding, and then closes the file.
            //            /// If the target file already exists, it is truncated and overwritten.
            //            /// </summary>
            //            /// <param name="path">The file to write to.</param>
            //            /// <param name="contents">The characters to write to the file.</param>
            //            /// <param name="encoding">The encoding to apply to the string.</param>
            //            /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
            //            /// <returns>A task that represents the asynchronous write operation.</returns>
            //            /// <exception cref="OperationCanceledException">The cancellation token was canceled. This exception is stored into the returned task.</exception>
            //            public static Task WriteAllTextAsync(string path, ReadOnlyMemory<char> contents, Encoding encoding, CancellationToken cancellationToken = default)
            //            {
            //                Validate(path, encoding);

            //                if (cancellationToken.IsCancellationRequested)
            //                {
            //                    return Task.FromCanceled(cancellationToken);
            //                }

            //                return WriteToFileAsync(path, FileMode.Create, contents, encoding, cancellationToken);
            //            }

            //            public static Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
            //            {
            //                if (cancellationToken.IsCancellationRequested)
            //                {
            //                    return Task.FromCanceled<byte[]>(cancellationToken);
            //                }

            //                // SequentialScan is a perf hint that requires extra sys-call on non-Windows OSes.
            //                FileOptions options = FileOptions.Asynchronous | (OperatingSystem.IsWindows() ? FileOptions.SequentialScan : FileOptions.None);
            //                SafeFileHandle sfh = OpenHandle(path, FileMode.Open, FileAccess.Read, FileShare.Read, options);

            //                long fileLength = 0L;
            //                if (sfh.CanSeek && (fileLength = sfh.GetFileLength()) > Array.MaxLength)
            //                {
            //                    sfh.Dispose();
            //                    return Task.FromException<byte[]>(ExceptionDispatchInfo.SetCurrentStackTrace(new IOException(SR.IO_FileTooLong2GB)));
            //                }

            //#if DEBUG
            //                fileLength = 0; // improve the test coverage for InternalReadAllBytesUnknownLengthAsync
            //#endif

            //#pragma warning disable CA2025
            //                return fileLength > 0 ?
            //                    InternalReadAllBytesAsync(sfh, (int)fileLength, cancellationToken) :
            //                    InternalReadAllBytesUnknownLengthAsync(sfh, cancellationToken);
            //#pragma warning restore
            //            }

            //            private static async Task<byte[]> InternalReadAllBytesAsync(SafeFileHandle sfh, int count, CancellationToken cancellationToken)
            //            {
            //                using (sfh)
            //                {
            //                    int index = 0;
            //                    byte[] bytes = new byte[count];
            //                    do
            //                    {
            //                        int n = await RandomAccess.ReadAtOffsetAsync(sfh, bytes.AsMemory(index), index, cancellationToken).ConfigureAwait(false);
            //                        if (n == 0)
            //                        {
            //                            ThrowHelper.ThrowEndOfFileException();
            //                        }

            //                        index += n;
            //                    } while (index < count);

            //                    return bytes;
            //                }
            //            }

            //            private static async Task<byte[]> InternalReadAllBytesUnknownLengthAsync(SafeFileHandle sfh, CancellationToken cancellationToken)
            //            {
            //                byte[] rentedArray = ArrayPool<byte>.Shared.Rent(512);
            //                try
            //                {
            //                    int bytesRead = 0;
            //                    while (true)
            //                    {
            //                        if (bytesRead == rentedArray.Length)
            //                        {
            //                            uint newLength = (uint)rentedArray.Length * 2;
            //                            if (newLength > Array.MaxLength)
            //                            {
            //                                newLength = (uint)Math.Max(Array.MaxLength, rentedArray.Length + 1);
            //                            }

            //                            byte[] tmp = ArrayPool<byte>.Shared.Rent((int)newLength);
            //                            Buffer.BlockCopy(rentedArray, 0, tmp, 0, bytesRead);

            //                            byte[] toReturn = rentedArray;
            //                            rentedArray = tmp;

            //                            ArrayPool<byte>.Shared.Return(toReturn);
            //                        }

            //                        Debug.Assert(bytesRead < rentedArray.Length);
            //                        int n = await RandomAccess.ReadAtOffsetAsync(sfh, rentedArray.AsMemory(bytesRead), bytesRead, cancellationToken).ConfigureAwait(false);
            //                        if (n == 0)
            //                        {
            //                            return rentedArray.AsSpan(0, bytesRead).ToArray();
            //                        }
            //                        bytesRead += n;
            //                    }
            //                }
            //                finally
            //                {
            //                    sfh.Dispose();
            //                    ArrayPool<byte>.Shared.Return(rentedArray);
            //                }
            //            }

            //            public static Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
            //            {
            //                ArgumentNullException.ThrowIfNull(bytes);

            //                return WriteAllBytesAsync(path, new ReadOnlyMemory<byte>(bytes), cancellationToken);
            //            }

            //            /// <summary>
            //            /// Asynchronously creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is truncated and overwritten.
            //            /// </summary>
            //            /// <param name="path">The file to write to.</param>
            //            /// <param name="bytes">The bytes to write to the file.</param>
            //            /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
            //            /// <returns>A task that represents the asynchronous write operation.</returns>
            //            /// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
            //            /// <exception cref="ArgumentException"><paramref name="path"/> is empty.</exception>
            //            /// <exception cref="OperationCanceledException">The cancellation token was canceled. This exception is stored into the returned task.</exception>
            //            public static Task WriteAllBytesAsync(string path, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default)
            //            {
            //                ArgumentException.ThrowIfNullOrEmpty(path);

            //                return cancellationToken.IsCancellationRequested
            //                    ? Task.FromCanceled(cancellationToken)
            //                    : Core(path, bytes, cancellationToken);

            //                static async Task Core(string path, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken)
            //                {
            //                    using SafeFileHandle sfh = OpenHandle(path, FileMode.Create, FileAccess.Write, FileShare.Read, FileOptions.Asynchronous);
            //                    await RandomAccess.WriteAtOffsetAsync(sfh, bytes, 0, cancellationToken).ConfigureAwait(false);
            //                }
            //            }

            //            public static Task<string[]> ReadAllLinesAsync(string path, CancellationToken cancellationToken = default)
            //                => ReadAllLinesAsync(path, Encoding.UTF8, cancellationToken);

            //            public static Task<string[]> ReadAllLinesAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
            //            {
            //                Validate(path, encoding);

            //                return cancellationToken.IsCancellationRequested
            //                    ? Task.FromCanceled<string[]>(cancellationToken)
            //                    : InternalReadAllLinesAsync(path, encoding, cancellationToken);
            //            }

            //            private static async Task<string[]> InternalReadAllLinesAsync(string path, Encoding encoding, CancellationToken cancellationToken)
            //            {
            //                Debug.Assert(!string.IsNullOrEmpty(path));
            //                Debug.Assert(encoding != null);

            //                using (StreamReader sr = AsyncStreamReader(path, encoding))
            //                {
            //                    cancellationToken.ThrowIfCancellationRequested();
            //                    string? line;
            //                    List<string> lines = new List<string>();
            //                    while ((line = await sr.ReadLineAsync(cancellationToken).ConfigureAwait(false)) != null)
            //                    {
            //                        lines.Add(line);
            //                        cancellationToken.ThrowIfCancellationRequested();
            //                    }

            //                    return lines.ToArray();
            //                }
            //            }

            //            public static Task WriteAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default)
            //                => WriteAllLinesAsync(path, contents, UTF8NoBOM, cancellationToken);

            //            public static Task WriteAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default) =>
            //                WriteAllLinesAsync(path, contents, encoding, append: false, cancellationToken);

            //            private static Task WriteAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, bool append, CancellationToken cancellationToken)
            //            {
            //                Validate(path, encoding);
            //                ArgumentNullException.ThrowIfNull(contents);
            //                if (cancellationToken.IsCancellationRequested)
            //                {
            //                    return Task.FromCanceled(cancellationToken);
            //                }

            //                StreamWriter writer;
            //                try
            //                {
            //                    writer = new StreamWriter(
            //                        new FileStream(path, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read, DefaultBufferSize, FileOptions.Asynchronous),
            //                        encoding);
            //                }
            //                catch (Exception e)
            //                {
            //                    return Task.FromException(e);
            //                }

            //                return InternalWriteAllLinesAsync(writer, contents, cancellationToken);
            //            }

            //            private static async Task InternalWriteAllLinesAsync(StreamWriter writer, IEnumerable<string> contents, CancellationToken cancellationToken)
            //            {
            //                Debug.Assert(writer != null);
            //                Debug.Assert(contents != null);

            //                using (writer)
            //                {
            //                    foreach (string line in contents)
            //                    {
            //                        await writer.WriteLineAsync(line.AsMemory(), cancellationToken).ConfigureAwait(false);
            //                    }

            //                    await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
            //                }
            //            }

            //            public static Task AppendAllTextAsync(string path, string? contents, CancellationToken cancellationToken = default)
            //                => AppendAllTextAsync(path, contents, UTF8NoBOM, cancellationToken);

            //            /// <summary>
            //            /// Asynchronously opens a file or creates a file if it does not already exist, appends the specified string to the file, and then closes the file.
            //            /// </summary>
            //            /// <param name="path">The file to append the specified string to.</param>
            //            /// <param name="contents">The characters to append to the file.</param>
            //            /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
            //            /// <returns>A task that represents the asynchronous append operation.</returns>
            //            /// <exception cref="OperationCanceledException">The cancellation token was canceled. This exception is stored into the returned task.</exception>
            //            public static Task AppendAllTextAsync(string path, ReadOnlyMemory<char> contents, CancellationToken cancellationToken = default)
            //                => AppendAllTextAsync(path, contents, UTF8NoBOM, cancellationToken);

            //            public static Task AppendAllTextAsync(string path, string? contents, Encoding encoding, CancellationToken cancellationToken = default)
            //                => AppendAllTextAsync(path, contents.AsMemory(), encoding, cancellationToken);

            //            /// <summary>
            //            /// Asynchronously opens a file or creates the file if it does not already exist, appends the specified string to the file using the specified encoding, and then closes the file.
            //            /// </summary>
            //            /// <param name="path">The file to append the specified string to.</param>
            //            /// <param name="contents">The characters to append to the file.</param>
            //            /// <param name="encoding">The character encoding to use.</param>
            //            /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
            //            /// <returns>A task that represents the asynchronous append operation.</returns>
            //            /// <exception cref="OperationCanceledException">The cancellation token was canceled. This exception is stored into the returned task.</exception>
            //            public static Task AppendAllTextAsync(string path, ReadOnlyMemory<char> contents, Encoding encoding, CancellationToken cancellationToken = default)
            //            {
            //                Validate(path, encoding);

            //                if (cancellationToken.IsCancellationRequested)
            //                {
            //                    return Task.FromCanceled(cancellationToken);
            //                }

            //                return WriteToFileAsync(path, FileMode.Append, contents, encoding, cancellationToken);
            //            }

            //            public static Task AppendAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default)
            //                => AppendAllLinesAsync(path, contents, UTF8NoBOM, cancellationToken);

            //            public static Task AppendAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default) =>
            //                WriteAllLinesAsync(path, contents, encoding, append: true, cancellationToken);

            private static void Validate(string path, Encoding encoding)
            {
                ArgumentException.ThrowIfNullOrEmpty(path);
                ArgumentNullException.ThrowIfNull(encoding);
            }

            //private static byte[] ReadAllBytesUnknownLength(SafeFileHandle sfh)
            //{
            //    byte[]? rentedArray = null;
            //    Span<byte> buffer = stackalloc byte[512];
            //    try
            //    {
            //        int bytesRead = 0;
            //        while (true)
            //        {
            //            if (bytesRead == buffer.Length)
            //            {
            //                uint newLength = (uint)buffer.Length * 2;
            //                if (newLength > Array.MaxLength)
            //                {
            //                    newLength = (uint)Math.Max(Array.MaxLength, buffer.Length + 1);
            //                }

            //                byte[] tmp = ArrayPool<byte>.Shared.Rent((int)newLength);
            //                buffer.CopyTo(tmp);
            //                byte[]? oldRentedArray = rentedArray;
            //                buffer = rentedArray = tmp;
            //                if (oldRentedArray != null)
            //                {
            //                    ArrayPool<byte>.Shared.Return(oldRentedArray);
            //                }
            //            }

            //            Debug.Assert(bytesRead < buffer.Length);
            //            int n = RandomAccess.ReadAtOffset(sfh, buffer.Slice(bytesRead), bytesRead);
            //            if (n == 0)
            //            {
            //                return buffer.Slice(0, bytesRead).ToArray();
            //            }
            //            bytesRead += n;
            //        }
            //    }
            //    finally
            //    {
            //        if (rentedArray != null)
            //        {
            //            ArrayPool<byte>.Shared.Return(rentedArray);
            //        }
            //    }
            //}

            //private static void WriteToFile(string path, FileMode mode, ReadOnlySpan<char> contents, Encoding encoding)
            //{
            //    ReadOnlySpan<byte> preamble = encoding.GetPreamble();
            //    int preambleSize = preamble.Length;

            //    using SafeFileHandle fileHandle = File.OpenHandleFromApp(path, mode, FileAccess.Write, FileShare.Read, FileOptions.None, GetPreallocationSize(mode, contents, encoding, preambleSize));
            //    long fileOffset = mode == FileMode.Append && fileHandle.CanSeek ? RandomAccess.GetLength(fileHandle) : 0;

            //    if (contents.IsEmpty)
            //    {
            //        if (preambleSize > 0 // even if the content is empty, we want to store the preamble
            //            && fileOffset == 0) // if we're appending to a file that already has data, don't write the preamble.
            //        {
            //            RandomAccess.Write(fileHandle, preamble, fileOffset);
            //        }
            //        return;
            //    }

            //    int bytesNeeded = checked(preambleSize + encoding.GetMaxByteCount(Math.Min(contents.Length, ChunkSize)));
            //    byte[]? rentedBytes = null;
            //    Span<byte> bytes = (uint)bytesNeeded <= 1024 ? stackalloc byte[1024] : (rentedBytes = ArrayPool<byte>.Shared.Rent(bytesNeeded));

            //    try
            //    {
            //        if (fileOffset == 0)
            //        {
            //            preamble.CopyTo(bytes);
            //        }
            //        else
            //        {
            //            preambleSize = 0; // don't append preamble to a non-empty file
            //        }

            //        Encoder encoder = encoding.GetEncoder();
            //        while (!contents.IsEmpty)
            //        {
            //            ReadOnlySpan<char> toEncode = contents.Slice(0, Math.Min(contents.Length, ChunkSize));
            //            contents = contents.Slice(toEncode.Length);
            //            int encoded = encoder.GetBytes(toEncode, bytes.Slice(preambleSize), flush: contents.IsEmpty);
            //            Span<byte> toStore = bytes.Slice(0, preambleSize + encoded);

            //            RandomAccess.Write(fileHandle, toStore, fileOffset);

            //            fileOffset += toStore.Length;
            //            preambleSize = 0;
            //        }
            //    }
            //    finally
            //    {
            //        if (rentedBytes is not null)
            //        {
            //            ArrayPool<byte>.Shared.Return(rentedBytes);
            //        }
            //    }
            //}

            //private static async Task WriteToFileAsync(string path, FileMode mode, ReadOnlyMemory<char> contents, Encoding encoding, CancellationToken cancellationToken)
            //{
            //    ReadOnlyMemory<byte> preamble = encoding.GetPreamble();
            //    int preambleSize = preamble.Length;

            //    using SafeFileHandle fileHandle = OpenHandle(path, mode, FileAccess.Write, FileShare.Read, FileOptions.Asynchronous, GetPreallocationSize(mode, contents.Span, encoding, preambleSize));
            //    long fileOffset = mode == FileMode.Append && fileHandle.CanSeek ? RandomAccess.GetLength(fileHandle) : 0;

            //    if (contents.IsEmpty)
            //    {
            //        if (preambleSize > 0 // even if the content is empty, we want to store the preamble
            //            && fileOffset == 0) // if we're appending to a file that already has data, don't write the preamble.
            //        {
            //            await RandomAccess.WriteAtOffsetAsync(fileHandle, preamble, fileOffset, cancellationToken).ConfigureAwait(false);
            //        }
            //        return;
            //    }

            //    byte[] bytes = ArrayPool<byte>.Shared.Rent(preambleSize + encoding.GetMaxByteCount(Math.Min(contents.Length, ChunkSize)));

            //    try
            //    {
            //        if (fileOffset == 0)
            //        {
            //            preamble.CopyTo(bytes);
            //        }
            //        else
            //        {
            //            preambleSize = 0; // don't append preamble to a non-empty file
            //        }

            //        Encoder encoder = encoding.GetEncoder();
            //        while (!contents.IsEmpty)
            //        {
            //            ReadOnlyMemory<char> toEncode = contents.Slice(0, Math.Min(contents.Length, ChunkSize));
            //            contents = contents.Slice(toEncode.Length);
            //            int encoded = encoder.GetBytes(toEncode.Span, bytes.AsSpan(preambleSize), flush: contents.IsEmpty);
            //            ReadOnlyMemory<byte> toStore = new ReadOnlyMemory<byte>(bytes, 0, preambleSize + encoded);

            //            await RandomAccess.WriteAtOffsetAsync(fileHandle, toStore, fileOffset, cancellationToken).ConfigureAwait(false);

            //            fileOffset += toStore.Length;
            //            preambleSize = 0;
            //        }
            //    }
            //    finally
            //    {
            //        ArrayPool<byte>.Shared.Return(bytes);
            //    }
            //}

            //private static long GetPreallocationSize(FileMode mode, ReadOnlySpan<char> contents, Encoding encoding, int preambleSize)
            //{
            //    // for a single write operation, setting preallocationSize has no perf benefit, as it requires an additional sys-call
            //    if (contents.Length < ChunkSize)
            //    {
            //        return 0;
            //    }

            //    // preallocationSize is ignored for Append mode, there is no need to spend cycles on GetByteCount
            //    if (mode == FileMode.Append)
            //    {
            //        return 0;
            //    }

            //    return preambleSize + encoding.GetByteCount(contents);
            //}

            //private static async IAsyncEnumerable<string> IterateFileLinesAsync(StreamReader sr, string path, Encoding encoding, CancellationToken ctEnumerable, [EnumeratorCancellation] CancellationToken ctEnumerator = default)
            //{
            //    if (!sr.BaseStream.CanRead)
            //    {
            //        sr = AsyncStreamReader(path, encoding);
            //    }

            //    using (sr)
            //    {
            //        using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(ctEnumerable, ctEnumerator);
            //        string? line;
            //        while ((line = await sr.ReadLineAsync(cts.Token).ConfigureAwait(false)) is not null)
            //        {
            //            yield return line;
            //        }
            //    }
            //}
        }
    }
}
