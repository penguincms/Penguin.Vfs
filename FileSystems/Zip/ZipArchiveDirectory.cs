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
                using ZipArchive zipArchive = OpenZip();

                foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
                {
                    string dir = zipArchiveEntry.GetDirectoryName();

                    if (dir.Length < Uri.LocalPath.Length + 1 || !dir.StartsWith(Uri.LocalPath + "//"))
                    {
                        continue;
                    }

                    dir = dir[(Uri.LocalPath.Length + 1)..];

                    if (dir != Uri.LocalPath.Value)
                    {
                        continue;
                    }

                    if (returned.Add(dir))
                    {
                        yield return new ZipArchiveDirectory(ResolutionPackage.AppendChild(dir))
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
                using ZipArchive zipArchive = OpenZip();

                foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
                {
                    if (Uri.LocalPath.Append(zipArchiveEntry.Name).Value != zipArchiveEntry.FullName)
                    {
                        continue;
                    }

                    yield return new ZipArchiveFile(ResolutionPackage.AppendChild(zipArchiveEntry.Name))
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
                foreach (IDirectory dir in Directories)
                {
                    yield return dir;
                }

                foreach (IFile file in Files)
                {
                    yield return file;
                }
            }
        }

        public bool IsRecursive => false;

        public DateTime LastModified { get; internal set; }

        public ResolveUriPackage ResolutionPackage { get; }

        public IUri Uri => ResolutionPackage.VirtualUri;

        public ZipArchiveDirectory(ResolveUriPackage resolveUriPackage)
        {
            ResolutionPackage = resolveUriPackage;
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

        private ZipArchive OpenZip()
        {
            return new(ResolutionPackage.FileSystem.Open(Uri).GetStream());
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ZipArchiveDirectory()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }
    }
}