// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FileSystemFromApp.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Versioning;

namespace FileSystemFromApp
{
    /// <inheritdoc cref="Directory"/>
    public static class DirectoryFromApp
    {
        /// <summary>
        /// The extension for the <see cref="Directory"/> class.
        /// </summary>
        extension(Directory)
        {
            /// <inheritdoc cref="Directory.CreateDirectory(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static DirectoryInfo CreateDirectoryFromApp(string path)
            {
                ArgumentException.ThrowIfNullOrEmpty(path);

                string fullPath = Path.GetFullPath(path);

                FileSystem.CreateDirectory(fullPath);

                return new DirectoryInfo(path);
            }

            // Tests if the given path refers to an existing DirectoryInfo on disk.
            /// <inheritdoc cref="Directory.Exists(string?)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static bool ExistsFromApp([NotNullWhen(true)] string? path)
            {
                try
                {
                    if (path == null)
                    { return false; }
                    if (path.Length == 0)
                    { return false; }

                    string fullPath = Path.GetFullPath(path);

                    return FileSystem.DirectoryExists(fullPath);
                }
                catch (ArgumentException) { }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }

                return false;
            }

            /// <inheritdoc cref="Directory.GetCreationTime(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static DateTime GetCreationTimeFromApp(string path)
            {
                return File.GetCreationTimeFromApp(path);
            }

            /// <inheritdoc cref="Directory.GetCreationTimeUtc(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static DateTime GetCreationTimeUtcFromApp(string path)
            {
                return File.GetCreationTimeUtcFromApp(path);
            }

            /// <inheritdoc cref="Directory.GetLastWriteTime(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static DateTime GetLastWriteTimeFromApp(string path)
            {
                return File.GetLastWriteTimeFromApp(path);
            }

            /// <inheritdoc cref="Directory.GetLastWriteTimeUtc(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static DateTime GetLastWriteTimeUtcFromApp(string path)
            {
                return File.GetLastWriteTimeUtcFromApp(path);
            }

            /// <inheritdoc cref="Directory.GetLastAccessTime(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static DateTime GetLastAccessTimeFromApp(string path)
            {
                return File.GetLastAccessTimeFromApp(path);
            }

            /// <inheritdoc cref="Directory.GetLastAccessTimeUtc(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static DateTime GetLastAccessTimeUtcFromApp(string path)
            {
                return File.GetLastAccessTimeUtcFromApp(path);
            }

            /// <inheritdoc cref="Directory.GetFiles(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static string[] GetFilesFromApp(string path) => Directory.GetFilesFromApp(path, "*", enumerationOptions: EnumerationOptions.Compatible);

            /// <inheritdoc cref="Directory.GetFiles(string, string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static string[] GetFilesFromApp(string path, string searchPattern) => Directory.GetFilesFromApp(path, searchPattern, enumerationOptions: EnumerationOptions.Compatible);

            /// <inheritdoc cref="Directory.GetFiles(string, string, SearchOption)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static string[] GetFilesFromApp(string path, string searchPattern, SearchOption searchOption)
                => Directory.GetFilesFromApp(path, searchPattern, EnumerationOptions.FromSearchOption(searchOption));

            /// <inheritdoc cref="Directory.GetFiles(string, string, EnumerationOptions)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static string[] GetFilesFromApp(string path, string searchPattern, EnumerationOptions enumerationOptions)
                => [.. Directory.InternalEnumeratePaths(path, searchPattern, SearchTarget.Files, enumerationOptions)];

            /// <inheritdoc cref="Directory.GetDirectories(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static string[] GetDirectoriesFromApp(string path) => Directory.GetDirectoriesFromApp(path, "*", enumerationOptions: EnumerationOptions.Compatible);

            /// <inheritdoc cref="Directory.GetDirectories(string, string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static string[] GetDirectoriesFromApp(string path, string searchPattern) => Directory.GetDirectoriesFromApp(path, searchPattern, enumerationOptions: EnumerationOptions.Compatible);

            /// <inheritdoc cref="Directory.GetDirectories(string, string, SearchOption)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static string[] GetDirectoriesFromApp(string path, string searchPattern, SearchOption searchOption)
                => Directory.GetDirectoriesFromApp(path, searchPattern, EnumerationOptions.FromSearchOption(searchOption));

            /// <inheritdoc cref="Directory.GetDirectories(string, string, EnumerationOptions)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static string[] GetDirectoriesFromApp(string path, string searchPattern, EnumerationOptions enumerationOptions)
                => [.. Directory.InternalEnumeratePaths(path, searchPattern, SearchTarget.Directories, enumerationOptions)];

            /// <inheritdoc cref="Directory.GetFileSystemEntries(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static string[] GetFileSystemEntriesFromApp(string path) => Directory.GetFileSystemEntriesFromApp(path, "*", enumerationOptions: EnumerationOptions.Compatible);

            /// <inheritdoc cref="Directory.GetFileSystemEntries(string, string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static string[] GetFileSystemEntriesFromApp(string path, string searchPattern) => Directory.GetFileSystemEntriesFromApp(path, searchPattern, enumerationOptions: EnumerationOptions.Compatible);

            /// <inheritdoc cref="Directory.GetFileSystemEntries(string, string, SearchOption)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static string[] GetFileSystemEntriesFromApp(string path, string searchPattern, SearchOption searchOption)
                => Directory.GetFileSystemEntriesFromApp(path, searchPattern, EnumerationOptions.FromSearchOption(searchOption));

            /// <inheritdoc cref="Directory.GetFileSystemEntries(string, string, EnumerationOptions)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static string[] GetFileSystemEntriesFromApp(string path, string searchPattern, EnumerationOptions enumerationOptions)
                => [.. Directory.InternalEnumeratePaths(path, searchPattern, SearchTarget.Both, enumerationOptions)];

            [SupportedOSPlatform("Windows10.0.17134.0")]
            internal static IEnumerable<string> InternalEnumeratePaths(
                string path,
                string searchPattern,
                SearchTarget searchTarget,
                EnumerationOptions enumerationOptions)
            {
                ArgumentNullException.ThrowIfNull(path);
                ArgumentNullException.ThrowIfNull(searchPattern);
                ArgumentNullException.ThrowIfNull(enumerationOptions);

                FileSystemEnumerableFactory.NormalizeInputs(ref path, ref searchPattern, enumerationOptions.MatchType);

                return searchTarget switch
                {
                    SearchTarget.Files => FileSystemEnumerableFactory.UserFiles(path, searchPattern, enumerationOptions),
                    SearchTarget.Directories => FileSystemEnumerableFactory.UserDirectories(path, searchPattern, enumerationOptions),
                    SearchTarget.Both => FileSystemEnumerableFactory.UserEntries(path, searchPattern, enumerationOptions),
                    _ => throw new ArgumentOutOfRangeException(nameof(searchTarget)),
                };
            }

            /// <inheritdoc cref="Directory.EnumerateDirectories(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static IEnumerable<string> EnumerateDirectoriesFromApp(string path) => Directory.EnumerateDirectoriesFromApp(path, "*", enumerationOptions: EnumerationOptions.Compatible);

            /// <inheritdoc cref="Directory.EnumerateDirectories(string, string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static IEnumerable<string> EnumerateDirectoriesFromApp(string path, string searchPattern) => Directory.EnumerateDirectoriesFromApp(path, searchPattern, enumerationOptions: EnumerationOptions.Compatible);

            /// <inheritdoc cref="Directory.EnumerateDirectories(string, string, SearchOption)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static IEnumerable<string> EnumerateDirectoriesFromApp(string path, string searchPattern, SearchOption searchOption)
                => Directory.EnumerateDirectoriesFromApp(path, searchPattern, EnumerationOptions.FromSearchOption(searchOption));

            /// <inheritdoc cref="Directory.EnumerateDirectories(string, string, EnumerationOptions)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static IEnumerable<string> EnumerateDirectoriesFromApp(string path, string searchPattern, EnumerationOptions enumerationOptions)
                => Directory.InternalEnumeratePaths(path, searchPattern, SearchTarget.Directories, enumerationOptions);

            /// <inheritdoc cref="Directory.EnumerateFiles(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static IEnumerable<string> EnumerateFilesFromApp(string path) => Directory.EnumerateFilesFromApp(path, "*", enumerationOptions: EnumerationOptions.Compatible);

            /// <inheritdoc cref="Directory.EnumerateFiles(string, string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static IEnumerable<string> EnumerateFilesFromApp(string path, string searchPattern)
                => Directory.EnumerateFilesFromApp(path, searchPattern, enumerationOptions: EnumerationOptions.Compatible);

            /// <inheritdoc cref="Directory.EnumerateFiles(string, string, SearchOption)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static IEnumerable<string> EnumerateFilesFromApp(string path, string searchPattern, SearchOption searchOption)
                => Directory.EnumerateFilesFromApp(path, searchPattern, EnumerationOptions.FromSearchOption(searchOption));

            /// <inheritdoc cref="Directory.EnumerateFiles(string, string, EnumerationOptions)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static IEnumerable<string> EnumerateFilesFromApp(string path, string searchPattern, EnumerationOptions enumerationOptions)
                => Directory.InternalEnumeratePaths(path, searchPattern, SearchTarget.Files, enumerationOptions);

            /// <inheritdoc cref="Directory.EnumerateFileSystemEntries(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static IEnumerable<string> EnumerateFileSystemEntriesFromApp(string path)
                => Directory.EnumerateFileSystemEntriesFromApp(path, "*", enumerationOptions: EnumerationOptions.Compatible);

            /// <inheritdoc cref="Directory.EnumerateFileSystemEntries(string, string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static IEnumerable<string> EnumerateFileSystemEntriesFromApp(string path, string searchPattern)
                => Directory.EnumerateFileSystemEntriesFromApp(path, searchPattern, enumerationOptions: EnumerationOptions.Compatible);

            /// <inheritdoc cref="Directory.EnumerateFileSystemEntries(string, string, SearchOption)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static IEnumerable<string> EnumerateFileSystemEntriesFromApp(string path, string searchPattern, SearchOption searchOption)
                => Directory.EnumerateFileSystemEntriesFromApp(path, searchPattern, EnumerationOptions.FromSearchOption(searchOption));

            /// <inheritdoc cref="Directory.EnumerateFileSystemEntries(string, string, EnumerationOptions)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static IEnumerable<string> EnumerateFileSystemEntriesFromApp(string path, string searchPattern, EnumerationOptions enumerationOptions)
                => Directory.InternalEnumeratePaths(path, searchPattern, SearchTarget.Both, enumerationOptions);

            /// <inheritdoc cref="Directory.Move(string, string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static void MoveFromApp(string sourceDirName, string destDirName)
            {
                ArgumentException.ThrowIfNullOrEmpty(sourceDirName);
                ArgumentException.ThrowIfNullOrEmpty(destDirName);

                FileSystem.MoveDirectory(Path.GetFullPath(sourceDirName), Path.GetFullPath(destDirName));
            }

            /// <inheritdoc cref="Directory.Delete(string)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static void DeleteFromApp(string path)
            {
                string fullPath = Path.GetFullPath(path);
                FileSystem.RemoveDirectory(fullPath, false);
            }

            /// <inheritdoc cref="Directory.Delete(string, bool)"/>
            [SupportedOSPlatform("Windows10.0.17134.0")]
            public static void DeleteFromApp(string path, bool recursive)
            {
                string fullPath = Path.GetFullPath(path);
                FileSystem.RemoveDirectory(fullPath, recursive);
            }
        }
    }
}
