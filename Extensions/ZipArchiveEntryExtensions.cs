using System;
using System.IO.Compression;

namespace Penguin.Vfs.Extensions
{
    public static class ZipArchiveEntryExtensions
    {
        public static string GetDirectoryName(this ZipArchiveEntry zipArchiveEntry)
        {
            if (zipArchiveEntry is null)
            {
                throw new ArgumentNullException(nameof(zipArchiveEntry));
            }

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