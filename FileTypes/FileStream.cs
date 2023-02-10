using Penguin.Vfs.Interfaces;
using System;

namespace Penguin.Vfs.FileTypes
{
    public class FileStream : IFile
    {
        public DateTime LastModified { get; internal set; }
        public long Length { get; internal set; }
        public ResolveUriPackage ResolutionPackage { get; set; }

        public IUri Uri => ResolutionPackage.VirtualUri;

        public FileStream(ResolveUriPackage resolutionPackage)
        {
            LastModified = resolutionPackage.LastModified;
            Length = resolutionPackage.Length;

            ResolutionPackage = resolutionPackage;
        }

        public override string ToString()
        {
            return Uri.FullName.Value;
        }
    }
}