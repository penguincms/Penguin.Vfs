﻿using System;

namespace Penguin.Vfs.FileSystems.Cache
{
    public class CacheEntry
    {
        public DateTime DateModified { get; set; }

        public string FullName { get; set; }

        public ulong Size { get; set; }

        public CacheEntryType Type { get; set; }

        public override string ToString()
        {
            return FullName;
        }
    }
}