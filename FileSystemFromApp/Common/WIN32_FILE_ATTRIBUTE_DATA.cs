namespace Windows.Win32.Storage.FileSystem
{
    internal partial struct WIN32_FILE_ATTRIBUTE_DATA
    {
        internal void PopulateFrom(ref WIN32_FIND_DATAW findData)
        {
            dwFileAttributes = findData.dwFileAttributes;
            ftCreationTime = findData.ftCreationTime;
            ftLastAccessTime = findData.ftLastAccessTime;
            ftLastWriteTime = findData.ftLastWriteTime;
            nFileSizeHigh = findData.nFileSizeHigh;
            nFileSizeLow = findData.nFileSizeLow;
        }
    }
}
