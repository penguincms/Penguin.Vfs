using System;

namespace Penguin.Vfs.FileSystems.Zip
{
    public partial class ZipFileSystem
    {
        private class CachedEntry
        {
            public string FullName { get; internal set; }

            public DateTime LastModified { get; internal set; }
            public long Length { get; internal set; }
            public string Name { get; internal set; }

            public string GetDirectoryName()
            {
                string n = Name;
                string f = FullName;

                return n == f ? string.Empty : f[..(f.Length - n.Length - 1)];
            }

            public override string ToString()
            {
                return FullName;
            }
        }
    }
}