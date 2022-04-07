using Penguin.Vfs.Interfaces;
using System;

namespace Penguin.Vfs.FileSystems.Local
{
    public class LocalFileEntry : IFile, IFileSystemEntry
    {
        private bool disposedValue;
        private ResolveUriPackage ResolutionPackage;
        ResolveUriPackage IFileSystemEntry.ResolutionPackage { get; }

        public IUri Uri => this.ResolutionPackage.VirtualUri;

        public LocalFileEntry(ResolveUriPackage resolveUriPackage)
        {
            this.LastModified = new System.IO.FileInfo(resolveUriPackage.VirtualUri.FullName.Value).LastWriteTime;
            this.Length = new System.IO.FileInfo(resolveUriPackage.VirtualUri.FullName.Value).Length;
            this.ResolutionPackage = resolveUriPackage;
        }
        public long Length { get; internal set; }
        public DateTime LastModified { get; internal set; }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this.disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~LocalFileEntry()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }
    }
}