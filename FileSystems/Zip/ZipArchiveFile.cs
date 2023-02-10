using Penguin.Vfs.Interfaces;
using System;

namespace Penguin.Vfs.FileSystems.Zip
{
    public class ZipArchiveFile : IFile
    {
        private bool disposedValue;

        public DateTime LastModified { get; internal set; }
        public long Length { get; }
        public ResolveUriPackage ResolutionPackage { get; }

        public IUri Uri => ResolutionPackage.VirtualUri;

        public ZipArchiveFile(ResolveUriPackage resolveUriPackage)
        {
            ResolutionPackage = resolveUriPackage;
            LastModified = resolveUriPackage.LastModified;
            Length = resolveUriPackage.Length;
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
        // ~ZipArchiveFile()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }
    }
}