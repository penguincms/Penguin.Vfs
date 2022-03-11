using System.Collections.Generic;
using VirtualFileSystem.FileSystems.Cache;
using VirtualFileSystem.Interfaces;

namespace VirtualFileSystem.Caches
{
    internal class DummyCache : IFileSystemCache
    {
        public void Add(string path, IEnumerable<CacheEntry> entries)
        { }

        public bool TryGetValue(string path, out CacheEntry[] entries)
        {
            entries = null;
            return false;
        }
    }
}