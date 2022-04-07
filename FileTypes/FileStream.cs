using Penguin.Vfs.Interfaces;
using System;
using System.Diagnostics;

namespace Penguin.Vfs.FileTypes
{
    public class FileStream : IFile
    {
        public ResolveUriPackage ResolutionPackage { get; set; }

        public IUri Uri => this.ResolutionPackage.VirtualUri;

        public DateTime LastModified { get; internal set; }

        public long Length { get; internal set; }

        public FileStream(ResolveUriPackage resolutionPackage)
        {
            this.LastModified = resolutionPackage.LastModified;
            this.Length = resolutionPackage.Length;

            this.ResolutionPackage = resolutionPackage;
        }

        public override string ToString() => this.Uri.FullName.Value;
    }
}