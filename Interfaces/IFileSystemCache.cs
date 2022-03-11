using System.Collections.Generic;
using VirtualFileSystem.FileSystems.Cache;

namespace VirtualFileSystem.Interfaces
{
    public interface IFileSystemCache
    {
        public void Add(string path, IEnumerable<CacheEntry> entries);

        public bool TryGetValue(string path, out CacheEntry[] entries);
    }
}