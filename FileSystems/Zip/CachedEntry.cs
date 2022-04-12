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
                string n = this.Name;
                string f = this.FullName;

                if (n == f)
                {
                    return string.Empty;
                }

                return f[..(f.Length - n.Length - 1)];
            }

            public override string ToString() => this.FullName;
        }
    }
}