using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Win32;
using Detours = Microsoft.Detours.PInvoke;

namespace FileSystemFromApp.Hook
{
    public sealed partial class HookFileSystem : IDisposable
    {
        /// <summary>
        /// The value that indicates whether the class has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// The lock object to ensure thread safety when modifying the hook state.
        /// </summary>
        private static readonly object locker = new();

        /// <summary>
        /// The reference count for the hook.
        /// </summary>
        private static int refCount;

        /// <summary>
        /// The dictionary that holds the original and hooked function pointers.
        /// </summary>
        private static readonly Dictionary<nint, nint> Hooks = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="HookFileSystem"/> class.
        /// </summary>
        public HookFileSystem()
        {
            refCount++;
            StartHook();
        }

        /// <summary>
        /// Finalizes this instance of the <see cref="HookFileSystem"/> class.
        /// </summary>
        ~HookFileSystem()
        {
            Dispose();
        }

        /// <summary>
        /// Gets the value that indicates whether the hook is active.
        /// </summary>
        public static bool IsHooked { get; private set; }

        /// <summary>
        /// Starts the hook for <c>fileapi.h</c> by <c>fileapifromapp.h</c>.
        /// </summary>
        private static unsafe void StartHook()
        {
            if (!IsHooked)
            {
                lock (locker)
                {
                    using FreeLibrarySafeHandle original = PInvoke.GetModuleHandle("KERNEL32.dll");
                    using FreeLibrarySafeHandle target = PInvoke.GetModuleHandle("api-ms-win-core-file-fromapp-l1-1-0.dll");
                    if (!original.IsInvalid && !target.IsInvalid)
                    {
                        _ = Detours.DetourRestoreAfterWith();

                        _ = Detours.DetourTransactionBegin();
                        _ = Detours.DetourUpdateThread(PInvoke.GetCurrentThread());

                        if (NativeLibrary.TryGetExport(original.DangerousGetHandle(), "CopyFile", out nint copyFile)
                            && NativeLibrary.TryGetExport(target.DangerousGetHandle(), "CopyFileFromApp", out nint copyFileFromApp))
                        {
                            void* ptr = (void*)copyFile;
                            _ = Detours.DetourAttach(ref ptr, (void*)copyFileFromApp);
                            Hooks[copyFile] = copyFileFromApp;
                        }

                        if (NativeLibrary.TryGetExport(original.DangerousGetHandle(), "CreateDirectory", out nint createDirectory)
                            && NativeLibrary.TryGetExport(target.DangerousGetHandle(), "CreateDirectoryFromAppW", out nint createDirectoryFromApp))
                        {
                            void* ptr = (void*)createDirectory;
                            _ = Detours.DetourAttach(ref ptr, (void*)createDirectoryFromApp);
                            Hooks[createDirectory] = createDirectoryFromApp;
                        }

                        if (NativeLibrary.TryGetExport(original.DangerousGetHandle(), "CreateFile2", out nint createFile2)
                            && NativeLibrary.TryGetExport(target.DangerousGetHandle(), "CreateFile2FromAppW ", out nint createFile2FromApp))
                        {
                            void* ptr = (void*)createFile2;
                            _ = Detours.DetourAttach(ref ptr, (void*)createFile2FromApp);
                            Hooks[createFile2] = createFile2FromApp;
                        }

                        if (NativeLibrary.TryGetExport(original.DangerousGetHandle(), "CreateFileW", out nint createFile)
                            && NativeLibrary.TryGetExport(target.DangerousGetHandle(), "CreateFileFromAppW ", out nint createFileFromApp))
                        {
                            void* ptr = (void*)createFile;
                            _ = Detours.DetourAttach(ref ptr, (void*)createFileFromApp);
                            Hooks[createFile] = createFileFromApp;
                        }

                        if (NativeLibrary.TryGetExport(original.DangerousGetHandle(), "DeleteFile", out nint deleteFile)
                            && NativeLibrary.TryGetExport(target.DangerousGetHandle(), "DeleteFileFromAppW", out nint deleteFileFromApp))
                        {
                            void* ptr = (void*)deleteFile;
                            _ = Detours.DetourAttach(ref ptr, (void*)deleteFileFromApp);
                            Hooks[deleteFile] = deleteFileFromApp;
                        }

                        if (NativeLibrary.TryGetExport(original.DangerousGetHandle(), "FindFirstFileExW", out nint findFirstFileEx)
                            && NativeLibrary.TryGetExport(target.DangerousGetHandle(), "FindFirstFileExFromAppW", out nint findFirstFileExFromApp))
                        {
                            void* ptr = (void*)findFirstFileEx;
                            _ = Detours.DetourAttach(ref ptr, (void*)findFirstFileExFromApp);
                            Hooks[findFirstFileEx] = findFirstFileExFromApp;
                        }

                        if (NativeLibrary.TryGetExport(original.DangerousGetHandle(), "GetFileAttributesEx", out nint getFileAttributesEx)
                            && NativeLibrary.TryGetExport(target.DangerousGetHandle(), "GetFileAttributesExFromAppW", out nint getFileAttributesExFromAppW))
                        {
                            void* ptr = (void*)getFileAttributesEx;
                            _ = Detours.DetourAttach(ref ptr, (void*)getFileAttributesExFromAppW);
                            Hooks[getFileAttributesEx] = getFileAttributesExFromAppW;
                        }

                        if (NativeLibrary.TryGetExport(original.DangerousGetHandle(), "MoveFile", out nint moveFile)
                            && NativeLibrary.TryGetExport(target.DangerousGetHandle(), "MoveFileFromAppW", out nint moveFileFromApp))
                        {
                            void* ptr = (void*)moveFile;
                            _ = Detours.DetourAttach(ref ptr, (void*)moveFileFromApp);
                            Hooks[moveFile] = moveFileFromApp;
                        }

                        if (NativeLibrary.TryGetExport(original.DangerousGetHandle(), "RemoveDirectoryW", out nint removeDirectory)
                            && NativeLibrary.TryGetExport(target.DangerousGetHandle(), "RemoveDirectoryFromAppW", out nint removeDirectoryFromApp))
                        {
                            void* ptr = (void*)removeDirectory;
                            _ = Detours.DetourAttach(ref ptr, (void*)removeDirectoryFromApp);
                            Hooks[removeDirectory] = removeDirectoryFromApp;
                        }

                        if (NativeLibrary.TryGetExport(original.DangerousGetHandle(), "ReplaceFileW", out nint replaceFile)
                            && NativeLibrary.TryGetExport(target.DangerousGetHandle(), "ReplaceFileFromAppW", out nint replaceFileFromApp))
                        {
                            void* ptr = (void*)replaceFile;
                            _ = Detours.DetourAttach(ref ptr, (void*)replaceFileFromApp);
                            Hooks[replaceFile] = replaceFileFromApp;
                        }

                        if (NativeLibrary.TryGetExport(original.DangerousGetHandle(), "SetFileAttributesW", out nint setFileAttributes)
                            && NativeLibrary.TryGetExport(target.DangerousGetHandle(), "SetFileAttributesFromAppW", out nint setFileAttributesFromApp))
                        {
                            void* ptr = (void*)setFileAttributes;
                            _ = Detours.DetourAttach(ref ptr, (void*)setFileAttributesFromApp);
                            Hooks[setFileAttributes] = setFileAttributesFromApp;
                        }

                        _ = Detours.DetourTransactionCommit();

                        IsHooked = true;
                    }
                }
            }
        }

        /// <summary>
        /// Ends the hook for <c>fileapi.h</c> by <c>fileapifromapp.h</c>.
        /// </summary>
        private static unsafe void EndHook()
        {
            if (--refCount == 0 && IsHooked)
            {
                lock (locker)
                {
                    _ = Detours.DetourTransactionBegin();
                    _ = Detours.DetourUpdateThread(PInvoke.GetCurrentThread());

                    foreach (KeyValuePair<nint, nint> hook in Hooks)
                    {
                        void* original = (void*)hook.Key;
                        void* target = (void*)hook.Value;
                        _ = Detours.DetourDetach(ref original, target);
                    }

                    _ = Detours.DetourTransactionCommit();

                    Hooks.Clear();
                    IsHooked = false;
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!disposed && IsHooked)
            {
                EndHook();
            }
            GC.SuppressFinalize(this);
            disposed = true;
        }
    }
}
