﻿using System;
using System.Collections.Generic;
using System.Linq;
using VirtualFileSystem.FileSystems.Cache;
using VirtualFileSystem.Interfaces;

namespace VirtualFileSystem.Extensions
{
    public static class IFileSystemCacheExtensions
    {
        public static CacheEntry[] GetOrAdd(this IFileSystemCache cache, string key, Func<IEnumerable<CacheEntry>> populateFunc)
        {
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