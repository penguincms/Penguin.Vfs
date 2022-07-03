using Penguin.Vfs.FileSystems.Cache;
using Penguin.Vfs.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Penguin.Vfs.Extensions
{
    public static class IFileSystemCacheExtensions
    {
        public static CacheEntry[] GetOrAdd(this IFileSystemCache cache, string key, Func<IEnumerable<CacheEntry>> populateFunc)
        {
            if (cache is null)
            {
                throw new ArgumentNullException(nameof(cache));
            }

            if (populateFunc is null)
            {
                throw new ArgumentNullException(nameof(populateFunc));
            }

            CacheEntry[] entries = Array.Empty<CacheEntry>();

            if (!cache.TryGetValue(key, out entries))
            {
                entries = populateFunc.Invoke().ToArray();
                cache.Add(key, entries);
            }

            return entries;
        }
    }
}