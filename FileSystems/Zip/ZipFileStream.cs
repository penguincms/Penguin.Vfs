using Penguin.Vfs.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Penguin.Vfs.FileSystems.Zip
{
    public class ZipFileStream : IStream
    {
        private readonly ResolveUriPackage ResolutionPackage;
        private bool disposedValue;

        public ZipFileStream()
        {
        }

        public ZipFileStream(ResolveUriPackage resolveUriPackage)
        {
            ResolutionPackage = resolveUriPackage;
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }

        public IEnumerable<string> EnumerateLines()
        {
            using StreamReader sr = new(GetStream());

            string line = null;

            while ((line = sr.ReadLine()) != null)
            {
                yield return line;
            }
        }

        public Stream GetStream()
        {
            IStream stream = ResolutionPackage.FileSystem.Open(new VirtualUri(ResolutionPackage.VirtualUri.MountPoint));

            foreach (ZipArchiveEntry zipArchiveEntry in new ZipArchive(stream.GetStream()).Entries)
            {
                if (zipArchiveEntry.FullName == ResolutionPackage.VirtualUri.LocalPath.Value)
                {
                    return zipArchiveEntry.Open();
                }
            }

            return Stream.Null;
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
        // ~ZipFileStream()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }
    }
}