using Penguin.Vfs.FileSystems.Cache;
using Penguin.Vfs.Interfaces;
using System.Collections.Generic;

namespace Penguin.Vfs.Caches
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