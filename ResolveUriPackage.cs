using Penguin.Vfs.Interfaces;
using System;
using System.Collections.Generic;

namespace Penguin.Vfs
{
    public struct ResolveUriPackage : IEquatable<ResolveUriPackage>
    {
        private HashSet<string> cachedDirectories;
        private HashSet<string> cachedFiles;
        private Dictionary<string, HashSet<string>> directoryContents;

        public IReadOnlySet<string> CachedDirectories
        {
            get
            {
                cachedDirectories ??= new();
                return cachedDirectories;
            }
        }

        public IReadOnlySet<string> CachedFiles
        {
            get
            {
                cachedFiles ??= new();
                return cachedFiles;
            }
        }

        public IReadOnlyDictionary<string, HashSet<string>> DirectoryContents
        {
            get
            {
                directoryContents ??= new();
                return directoryContents;
            }
        }

        public IFileSystemEntryFactory EntryFactory { get; set; }
        public IFileSystem FileSystem { get; set; }
        public DateTime LastModified { get; set; }
        public long Length { get; set; }
        public Dictionary<string, IFileSystemEntry> SessionCache { get; set; }
        public IUri VirtualUri { get; set; }

        public ResolveUriPackage AppendChild(string name, IFileSystem fileSystem = null)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (name.Contains('/') || name.Contains('\\'))
            {
                //Debugger.Break();
            }

            return AppendChild(new PathPart(name), fileSystem);
        }

        public ResolveUriPackage AppendChild(PathPart name, IFileSystem fileSystem = null)
        {
            ResolveUriPackage toReturn = Copy();

            if (fileSystem != null)
            {
                toReturn.VirtualUri = fileSystem.Uri.AsFileSystem().AppendChild(name);
                toReturn.FileSystem = fileSystem;
            }
            else
            {
                toReturn.VirtualUri = VirtualUri.AppendChild(name);
            }

            return toReturn;
        }

        public void CacheDirectories(string parent, IEnumerable<string> children)
        {
            if (!DirectoryContents.TryGetValue(parent, out HashSet<string> target))
            {
                target = new HashSet<string>();
                directoryContents.Add(parent, target);
            }

            if (children is null)
            {
                throw new ArgumentNullException(nameof(children));
            }

            cachedDirectories ??= new();
            foreach (string f in children)
            {
                _ = target.Add(f);
                _ = cachedDirectories.Add(f);
            }
        }

        public void CacheFiles(string parent, IEnumerable<string> children)
        {
            if (!DirectoryContents.TryGetValue(parent, out HashSet<string> target))
            {
                target = new HashSet<string>();
                directoryContents.Add(parent, target);
            }

            if (children is null)
            {
                throw new ArgumentNullException(nameof(children));
            }

            cachedFiles ??= new();
            foreach (string f in children)
            {
                _ = target.Add(f);
                _ = cachedFiles.Add(f);
            }
        }

        public ResolveUriPackage Copy()
        {
            return new ResolveUriPackage()
            {
                EntryFactory = EntryFactory,
                FileSystem = FileSystem,
                SessionCache = SessionCache,
                VirtualUri = VirtualUri,
                cachedDirectories = cachedDirectories,
                cachedFiles = cachedFiles
            };
        }

        public ResolveUriPackage WithFileInfo(DateTime lastModified, long length)
        {
            ResolveUriPackage toReturn = Copy();
            toReturn.LastModified = lastModified;
            toReturn.Length = length;
            return toReturn;
        }

        public ResolveUriPackage WithFileSystem(IFileSystem fileSystem)
        {
            ResolveUriPackage toReturn = Copy();
            toReturn.FileSystem = fileSystem;
            return toReturn;
        }

        public ResolveUriPackage WithUri(IUri uri)
        {
            ResolveUriPackage toReturn = Copy();
            toReturn.VirtualUri = uri;
            return toReturn;
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(ResolveUriPackage left, ResolveUriPackage right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ResolveUriPackage left, ResolveUriPackage right)
        {
            return !(left == right);
        }

        public bool Equals(ResolveUriPackage other)
        {
            throw new NotImplementedException();
        }
    }
}