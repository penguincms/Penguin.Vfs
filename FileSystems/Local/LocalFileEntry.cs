using Penguin.Vfs.Interfaces;
using System;

namespace Penguin.Vfs.FileSystems.Local
{
    public class LocalFileEntry : IFile, IFileSystemEntry
    {
        private bool disposedValue;
        private ResolveUriPackage ResolutionPackage;
        public DateTime LastModified { get; internal set; }
        public long Length { get; internal set; }
        ResolveUriPackage IFileSystemEntry.ResolutionPackage { get; }

        public IUri Uri => ResolutionPackage.VirtualUri;

        public LocalFileEntry(ResolveUriPackage resolveUriPackage)
        {
            LastModified = resolveUriPackage.LastModified;
            Length = resolveUriPackage.Length;
            ResolutionPackage = resolveUriPackage;
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
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