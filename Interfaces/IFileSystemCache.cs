using Penguin.Vfs.FileSystems.Cache;
using System.Collections.Generic;

namespace Penguin.Vfs.Interfaces
{
    public interface IFileSystemCache
    {
        public void Add(string path, IEnumerable<CacheEntry> entries);

        public bool TryGetValue(string path, out CacheEntry[] entries);
    }
}