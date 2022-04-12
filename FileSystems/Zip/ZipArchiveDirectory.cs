using Penguin.Vfs.Extensions;
using Penguin.Vfs.Interfaces;
using System;
using System.Collections.Generic;
using System.IO.Compression;

namespace Penguin.Vfs.FileSystems.Zip
{
    internal class ZipArchiveDirectory : IDirectory
    {
        private bool disposedValue;

        public IEnumerable<IDirectory> Directories
        {
            get
            {
                HashSet<string> returned = new();
                using ZipArchive zipArchive = this.OpenZip();

                foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
                {
                    string dir = zipArchiveEntry.GetDirectoryName();

                    if (dir.Length < this.Uri.LocalPath.Length + 1 || !dir.StartsWith(this.Uri.LocalPath + "//"))
                    {
                        continue;
                    }

                    dir = dir[(this.Uri.LocalPath.Length + 1)..];

                    if (dir != this.Uri.LocalPath.Value)
                    {
                        continue;
                    }

                    if (returned.Add(dir))
                    {
                        yield return new ZipArchiveDirectory(this.ResolutionPackage.AppendChild(dir))
                        {
                            LastModified = DateTime.MinValue
                        };
                    }
                }
            }
        }

        public IEnumerable<IFile> Files
        {
            get
            {
                using ZipArchive zipArchive = this.OpenZip();

                foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
                {
                    if (this.Uri.LocalPath.Append(zipArchiveEntry.Name).Value != zipArchiveEntry.FullName)
                    {
                        continue;
                    }

                    yield return new ZipArchiveFile(this.ResolutionPackage.AppendChild(zipArchiveEntry.Name))
                    {
                        LastModified = zipArchiveEntry.LastWriteTime.DateTime
                    };
                }
            }
        }

        public IEnumerable<IFileSystemEntry> FileSystemEntries
        {
            get
            {
                foreach (IDirectory dir in this.Directories)
                {
                    yield return dir;
                }

                foreach (IFile file in this.Files)
                {
                    yield return file;
                }
            }
        }

        public bool IsRecursive => false;

        public DateTime LastModified { get; internal set; }
        public ResolveUriPackage ResolutionPackage { get; }

        public IUri Uri => this.ResolutionPackage.VirtualUri;

        public ZipArchiveDirectory(ResolveUriPackage resolveUriPackage)
        {
            this.ResolutionPackage = resolveUriPackage;
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

        private ZipArchive OpenZip() => new(this.ResolutionPackage.FileSystem.Open(this.Uri).GetStream());

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ZipArchiveDirectory()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }
    }
}