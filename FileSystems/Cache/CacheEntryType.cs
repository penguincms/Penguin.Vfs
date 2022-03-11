using System;

namespace VirtualFileSystem.FileSystems.Cache
{
    [Flags]
    public enum CacheEntryType
    {
        File = 1,
        Directory = 2,
        FileAndDirectory = 3
    }
}