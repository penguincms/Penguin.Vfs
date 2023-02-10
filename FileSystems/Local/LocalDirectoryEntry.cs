using Penguin.Vfs.Interfaces;
using System;
using System.Collections.Generic;

namespace Penguin.Vfs.FileSystems.Local
{
    public class LocalDirectoryEntry : IFileSystemEntry, IDirectory
    {
        private bool disposedValue;

        public IEnumerable<IDirectory> Directories => EnumerateDirectories(false);

        public IEnumerable<IFile> Files => EnumerateFiles(false);

        public IEnumerable<IFileSystemEntry> FileSystemEntries => EnumerateFileSystemEntries(false);

        public bool IsRecursive { get; private set; }

        public DateTime LastModified { get; internal set; }
        public ResolveUriPackage ResolutionPackage { get; }

        public IUri Uri => ResolutionPackage.VirtualUri;

        public LocalDirectoryEntry(ResolveUriPackage resolveUriPackage)
        {
            LastModified = new System.IO.DirectoryInfo(resolveUriPackage.VirtualUri.FullName.Value).LastWriteTime;

            ResolutionPackage = resolveUriPackage;

            IsRecursive = NativeMethods.TryGetFinalPathName(Uri.FullName.WindowsValue, out string target) && Uri.FullName.IsChildOf(new PathPart(target));
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }

        public IEnumerable<IDirectory> EnumerateDirectories(bool recursive)
        {
            return ResolutionPackage.FileSystem.EnumerateDirectories(Uri.LocalPath, recursive);
        }

        public IEnumerable<IFile> EnumerateFiles(bool recursive)
        {
            return ResolutionPackage.FileSystem.EnumerateFiles(Uri.LocalPath, recursive);
        }

        public IEnumerable<IFileSystemEntry> EnumerateFileSystemEntries(bool recursive)
        {
            return ResolutionPackage.FileSystem.EnumerateFileSystemEntries(Uri.LocalPath, recursive);
        }

        public override string ToString()
        {
            return Uri.FullName.ToString();
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
        // ~LocalDirectoryEntry()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }
    }
}