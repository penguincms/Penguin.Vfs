using System;

namespace Penguin.Vfs.FileSystems.Cache
{
    [Flags]
    public enum CacheEntryType
    {
        File = 1,
        Directory = 2,
        FileAndDirectory = 3
    }
}