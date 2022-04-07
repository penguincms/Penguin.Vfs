using System;
using System.Collections.Generic;
using Penguin.Vfs.Interfaces;

namespace Penguin.Vfs
{
    public struct ResolveUriPackage
    {
        private HashSet<string> cachedDirectories;
        private HashSet<string> cachedFiles;
        private Dictionary<string, HashSet<string>> directoryContents;
        public DateTime LastModified { get; set; }
        public long Length { get; set; }
        public IReadOnlySet<string> CachedDirectories
        {
            get
            {
                this.cachedDirectories ??= new();
                return this.cachedDirectories;
            }
        }

        public IReadOnlySet<string> CachedFiles
        {
            get
            {
                this.cachedFiles ??= new();
                return this.cachedFiles;
            }
        }

        public IReadOnlyDictionary<string, HashSet<string>> DirectoryContents
        {
            get
            {
                this.directoryContents ??= new();
                return this.directoryContents;
            }
        }

        public IFileSystemEntryFactory EntryFactory { get; set; }
        public IFileSystem FileSystem { get; set; }
        public Dictionary<string, IFileSystemEntry> SessionCache { get; set; }
        public IUri VirtualUri { get; set; }

        public ResolveUriPackage AppendChild(string name, IFileSystem fileSystem = null)
        {
            if (name.Contains("/") || name.Contains("\\"))
            {
                //Debugger.Break();
            }

            return this.AppendChild(new PathPart(name), fileSystem);
        }

        public ResolveUriPackage AppendChild(PathPart name, IFileSystem fileSystem = null)
        {
            ResolveUriPackage toReturn = this.Copy();

            if (fileSystem != null)
            {
                toReturn.VirtualUri = fileSystem.Uri.AsFileSystem().AppendChild(name);
                toReturn.FileSystem = fileSystem;
            }
            else
            {
                toReturn.VirtualUri = this.VirtualUri.AppendChild(name);
            }

            return toReturn;
        }

        public void CacheDirectories(string parent, IEnumerable<string> children)
        {
            if (!this.DirectoryContents.TryGetValue(parent, out HashSet<string> target))
            {
                target = new HashSet<string>();
                this.directoryContents.Add(parent, target);
            }

            this.cachedDirectories ??= new();
            foreach (string f in children)
            {
                _ = target.Add(f);
                _ = this.cachedDirectories.Add(f);
            }
        }

        public void CacheFiles(string parent, IEnumerable<string> children)
        {
            if (!this.DirectoryContents.TryGetValue(parent, out HashSet<string> target))
            {
                target = new HashSet<string>();
                this.directoryContents.Add(parent, target);
            }

            this.cachedFiles ??= new();
            foreach (string f in children)
            {
                _ = target.Add(f);
                _ = this.cachedFiles.Add(f);
            }
        }

        public ResolveUriPackage Copy()
        {
            return new ResolveUriPackage()
            {
                EntryFactory = this.EntryFactory,
                FileSystem = this.FileSystem,
                SessionCache = this.SessionCache,
                VirtualUri = this.VirtualUri,
                cachedDirectories = this.cachedDirectories,
                cachedFiles = this.cachedFiles
            };
        }

        public ResolveUriPackage WithFileSystem(IFileSystem fileSystem)
        {
            ResolveUriPackage toReturn = this.Copy();
            toReturn.FileSystem = fileSystem;
            return toReturn;
        }

        public ResolveUriPackage WithFileInfo(DateTime lastModified, long length)
        {
            ResolveUriPackage toReturn = this.Copy();
            toReturn.LastModified = lastModified;
            toReturn.Length = length;
            return toReturn;
        }

        public ResolveUriPackage WithUri(IUri uri)
        {
            ResolveUriPackage toReturn = this.Copy();
            toReturn.VirtualUri = uri;
            return toReturn;
        }
    }
}