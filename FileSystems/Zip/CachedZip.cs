﻿using System.Collections.Generic;

namespace VirtualFileSystem.FileSystems.Zip
{
    public partial class ZipFileSystem
    {
        private class CachedZip
        {
            public List<CachedEntry> Entries { get; set; }
            public int Handles { get; set; }
        }
    }
}