using Penguin.Vfs.Interfaces;
using System;

namespace Penguin.Vfs.FileTypes
{
    public class FileStream : IFile
    {
        public DateTime LastModified { get; internal set; }
        public long Length { get; internal set; }
        public ResolveUriPackage ResolutionPackage { get; set; }

        public IUri Uri => this.ResolutionPackage.VirtualUri;

        public FileStream(ResolveUriPackage resolutionPackage)
        {
            this.LastModified = resolutionPackage.LastModified;
            this.Length = resolutionPackage.Length;

            this.ResolutionPackage = resolutionPackage;
        }

        public override string ToString() => this.Uri.FullName.Value;
    }
}