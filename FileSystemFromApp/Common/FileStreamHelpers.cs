// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;

namespace FileSystemFromApp.Common
{
    internal static class FileStreamHelpers
    {
        // NOTE: any change to FileOptions enum needs to be matched here as it's used in the error validation
        private const FileOptions ValidFileOptions = FileOptions.WriteThrough | FileOptions.Asynchronous | FileOptions.RandomAccess
            | FileOptions.DeleteOnClose | FileOptions.SequentialScan | FileOptions.Encrypted
            | (FileOptions)0x20000000 /* NoBuffering */ | (FileOptions)0x02000000 /* BackupOrRestore */;

        internal static void ValidateArguments(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options, long preallocationSize)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);

            // don't include inheritable in our bounds check for share
            FileShare tempshare = share & ~FileShare.Inheritable;
            string? badArg = null;

            if (mode < FileMode.CreateNew || mode > FileMode.Append)
            {
                badArg = nameof(mode);
            }
            else if (access < FileAccess.Read || access > FileAccess.ReadWrite)
            {
                badArg = nameof(access);
            }
            else if (tempshare < FileShare.None || tempshare > (FileShare.ReadWrite | FileShare.Delete))
            {
                badArg = nameof(share);
            }

            if (badArg != null)
            {
                throw new ArgumentOutOfRangeException(badArg, "Enum value was out of legal range.");
            }

            // NOTE: any change to FileOptions enum needs to be matched here in the error validation
            if (AreInvalid(options))
            {
                throw new ArgumentOutOfRangeException(nameof(options), "Enum value was out of legal range.");
            }
            else if (bufferSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "Non-negative number required.");
            }
            else if (preallocationSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(preallocationSize), "Non-negative number required.");
            }

            // Write access validation
            if ((access & FileAccess.Write) == 0)
            {
                if (mode == FileMode.Truncate || mode == FileMode.CreateNew || mode == FileMode.Create || mode == FileMode.Append)
                {
                    // No write access, mode and access disagree but flag access since mode comes first
                    throw new ArgumentException($"Combining FileMode: {mode} with FileAccess: {access} is invalid.", nameof(access));
                }
            }

            if ((access & FileAccess.Read) != 0 && mode == FileMode.Append)
            {
                throw new ArgumentException("Append access can be requested only in write-only mode.", nameof(access));
            }

            if (preallocationSize > 0)
            {
                ValidateArgumentsForPreallocation(mode, access);
            }
        }

        internal static void ValidateArgumentsForPreallocation(FileMode mode, FileAccess access)
        {
            // The user will be writing into the preallocated space.
            if ((access & FileAccess.Write) == 0)
            {
                throw new ArgumentException("Preallocation size can be requested only in write mode.", nameof(access));
            }

            // Only allow preallocation for newly created/overwritten files.
            // When we fail to preallocate, we'll remove the file.
            if (mode != FileMode.Create &&
                mode != FileMode.CreateNew)
            {
                throw new ArgumentException("Preallocation size can be requested only for new files.", nameof(mode));
            }
        }

        internal static bool AreInvalid(FileOptions options) => options != FileOptions.None && (options & ~ValidFileOptions) != 0;
    }
}
