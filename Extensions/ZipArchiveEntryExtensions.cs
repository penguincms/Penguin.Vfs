using System.IO.Compression;

namespace VirtualFileSystem.Extensions
{
    public static class ZipArchiveEntryExtensions
    {
        public static string GetDirectoryName(this ZipArchiveEntry zipArchiveEntry)
        {
            string n = zipArchiveEntry.Name;
            string f = zipArchiveEntry.FullName;

            if (n == f)
            {
                return string.Empty;
            }

            return f[..(f.Length - n.Length - 1)];
        }
    }
}