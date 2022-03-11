using VirtualFileSystem.Interfaces;

namespace VirtualFileSystem.FileSystems.Zip
{
    public class ZipArchiveFile : IFile
    {
        private bool disposedValue;

        public ResolveUriPackage ResolutionPackage { get; }

        public IUri Uri => this.ResolutionPackage.VirtualUri;

        public ZipArchiveFile(ResolveUriPackage resolveUriPackage)
        {
            this.ResolutionPackage = resolveUriPackage;
        }

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
        // ~ZipArchiveFile()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }
    }
}