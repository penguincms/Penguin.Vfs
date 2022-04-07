using System;
using System.Collections.Generic;
using Penguin.Vfs.Interfaces;

namespace Penguin.Vfs.FileSystems.Local
{
    public class LocalDirectoryEntry : IFileSystemEntry, IDirectory
    {
        private bool disposedValue;

        public IEnumerable<IDirectory> Directories => this.EnumerateDirectories(false);

        public IEnumerable<IFile> Files => this.EnumerateFiles(false);

        public IEnumerable<IFileSystemEntry> FileSystemEntries => this.EnumerateFileSystemEntries(false);

        public bool IsRecursive { get; private set; }

        public ResolveUriPackage ResolutionPackage { get; }

        public IUri Uri => this.ResolutionPackage.VirtualUri;

        public DateTime LastModified { get; internal set; }

        public LocalDirectoryEntry(ResolveUriPackage resolveUriPackage)
        {
            this.LastModified = new System.IO.DirectoryInfo(resolveUriPackage.VirtualUri.FullName.Value).LastWriteTime;

            this.ResolutionPackage = resolveUriPackage;

            this.IsRecursive = NativeMethods.TryGetFinalPathName(this.Uri.FullName.WindowsValue, out string target) && this.Uri.FullName.IsChildOf(new PathPart(target));
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }

        public IEnumerable<IDirectory> EnumerateDirectories(bool recursive) => this.ResolutionPackage.FileSystem.EnumerateDirectories(this.Uri.LocalPath, recursive);

        public IEnumerable<IFile> EnumerateFiles(bool recursive) => this.ResolutionPackage.FileSystem.EnumerateFiles(this.Uri.LocalPath, recursive);

        public IEnumerable<IFileSystemEntry> EnumerateFileSystemEntries(bool recursive) => this.ResolutionPackage.FileSystem.EnumerateFileSystemEntries(this.Uri.LocalPath, recursive);

        public override string ToString() => this.Uri.FullName.ToString();

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
        // ~LocalDirectoryEntry()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }
    }
}