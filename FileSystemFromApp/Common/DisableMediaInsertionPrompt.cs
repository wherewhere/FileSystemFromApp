// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.System.Diagnostics.Debug;

namespace FileSystemFromApp.Common
{
    /// <summary>
    /// Simple wrapper to safely disable the normal media insertion prompt for
    /// removable media (floppies, cds, memory cards, etc.)
    /// </summary>
    /// <remarks>
    /// Note that removable media file systems lazily load. After starting the OS
    /// they won't be loaded until you have media in the drive- and as such the
    /// prompt won't happen. You have to have had media in at least once to get
    /// the file system to load and then have removed it.
    /// </remarks>
    internal struct DisableMediaInsertionPrompt : IDisposable
    {
        private bool _disableSuccess;
        private readonly THREAD_ERROR_MODE _oldMode;

        [SupportedOSPlatform("windows6.1")]
        public static unsafe DisableMediaInsertionPrompt Create()
        {
            DisableMediaInsertionPrompt prompt = default;
            THREAD_ERROR_MODE* ptr = &prompt._oldMode;
            prompt._disableSuccess = PInvoke.SetThreadErrorMode(THREAD_ERROR_MODE.SEM_FAILCRITICALERRORS, ptr);
            return prompt;
        }

        [SupportedOSPlatform("windows6.1")]
        public readonly unsafe void Dispose()
        {
            if (_disableSuccess)
            { PInvoke.SetThreadErrorMode(_oldMode); }
        }
    }
}
