using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using VirtualFileSystem.Extensions;
using VirtualFileSystem.Interfaces;

namespace VirtualFileSystem.FileSystems.Zip
{
    public partial class ZipFileSystem : IFile, IFileSystem
    {
        private static readonly object CacheLock = new();
        private readonly Dictionary<string, CachedZip> Cache = new();
        public IEnumerable<IDirectory> Directories => this.EnumerateDirectories(new PathPart(), false);

        public IEnumerable<IFile> Files => this.EnumerateFiles(new PathPart(), false);

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
        public PathPart MountPoint => this.ResolutionPackage.VirtualUri.FullName;
        public ResolveUriPackage ResolutionPackage { get; }
        public IUri Uri => this.ResolutionPackage.VirtualUri;

        private IEnumerable<CachedEntry> CachedEntries
        {
            get
            {
                CachedZip cz;

                lock (CacheLock)
                {
                    cz = this.Cache[this.CacheKey];

                    if (cz.Entries != null)
                    {
                        return cz.Entries;
                    }

                    cz.Entries = this.ReadEntries.ToList();

                    return cz.Entries;
                }
            }
        }

        private string CacheKey => this.Uri.FullName.Value;

        private IEnumerable<CachedEntry> ReadEntries
        {
            get
            {
                List<CachedEntry> c = new();

                IStream stream = this.ResolutionPackage.FileSystem.Open(this.Uri);

                foreach (ZipArchiveEntry zipArchiveEntry in this.TryGetEntries())
                {
                    c.Add(new CachedEntry()
                    {
                        FullName = zipArchiveEntry.FullName,
                        Name = zipArchiveEntry.Name
                    });
                }

                return c;
            }
        }

        public ZipFileSystem(ResolveUriPackage resolveUriPackage)
        {
            this.ResolutionPackage = resolveUriPackage;

            if (!this.Cache.TryGetValue(this.CacheKey, out CachedZip cz))
            {
                cz = new();
                cz.Handles = 1;
                this.Cache.Add(this.CacheKey, cz);
            }
            else
            {
                cz.Handles++;
            }
        }

        public bool DirectoryExists(IUri uri) => this.CachedEntries.Any(e => uri.LocalPath.IsChildOf(new PathPart(e.FullName)));

        public IEnumerable<IDirectory> EnumerateDirectories(PathPart pathPart, bool recursive)
        {
            HashSet<string> returned = new();

            foreach (CachedEntry zipArchiveEntry in this.CachedEntries)
            {
                PathPart dir = new(zipArchiveEntry.GetDirectoryName());

                if (pathPart.HasValue)
                {
                    if (!dir.IsChildOf(pathPart))
                    {
                        continue;
                    }

                    dir = dir.MakeLocal(pathPart);
                }

                if (!dir.HasValue)
                {
                    continue;
                }

                if (!recursive && dir.Depth > 1)
                {
                    continue;
                }

                if (returned.Add(dir.Value))
                {
                    yield return new ZipArchiveDirectory(this.ResolutionPackage.AppendChild(zipArchiveEntry.GetDirectoryName(), this));
                }
            }
        }

        public IEnumerable<IDirectory> EnumerateDirectories(bool recursive) => this.EnumerateDirectories(new PathPart(), recursive);

        public IEnumerable<IFile> EnumerateFiles(PathPart path, bool recursive)
        {
            foreach (CachedEntry zipArchiveEntry in this.CachedEntries)
            {
                if (!path.HasValue)
                {
                    if (!recursive)
                    {
                        if (zipArchiveEntry.Name != zipArchiveEntry.FullName)
                        {
                            continue;
                        }
                    }

                    yield return this.ResolutionPackage.EntryFactory.Resolve(
                        this.ResolutionPackage.AppendChild(zipArchiveEntry.Name, this)) as IFile;
                }
                else
                {
                    PathPart ZipPath = new(zipArchiveEntry.FullName);

                    if (!ZipPath.IsChildOf(path))
                    {
                        continue;
                    }

                    PathPart LocalPath = ZipPath.MakeLocal(path);

                    if (!recursive && LocalPath.Depth > 1)
                    {
                        continue;
                    }

                    if (this.ResolutionPackage.EntryFactory.Resolve((this.ResolutionPackage.WithFileSystem(this).WithUri(new VirtualUri(this.MountPoint, path.Append(zipArchiveEntry.Name))))) is IFile f)
                    {
                        yield return f;
                    }
                }
            }
        }

        public IEnumerable<IFile> EnumerateFiles(bool recursive) => this.EnumerateFiles(new PathPart(), recursive);

        public IEnumerable<IFileSystemEntry> EnumerateFileSystemEntries(PathPart path, bool recursive)
        {
            foreach (IFile file in this.EnumerateFiles(path, recursive))
            {
                yield return file;
            }

            foreach (IDirectory dir in this.EnumerateDirectories(path, recursive))
            {
                yield return dir;
            }
        }

        public IEnumerable<IFileSystemEntry> EnumerateFileSystemEntries(bool recursive) => this.EnumerateFileSystemEntries(new PathPart(), recursive);

        public bool FileExists(IUri uri) => this.CachedEntries.Any(e => uri.LocalPath.Value == uri.LocalPath.Value);

        public IFileSystemEntry Find(PathPart path) => this.GenericFind(path);

        public IStream Open(IUri path)
        {
            IStream stream = this.ResolutionPackage.FileSystem.Open(this.Uri);

            if (!path.LocalPath.HasValue)
            {
                return stream;
            }

            foreach (CachedEntry zipArchiveEntry in this.CachedEntries)
            {
                if (zipArchiveEntry.FullName == path.LocalPath.Value)
                {
                    return new ZipFileStream(this.ResolutionPackage.WithFileSystem(this).WithUri(path));
                }
            }

            throw new FileNotFoundException();
        }

        private IEnumerable<ZipArchiveEntry> TryGetEntries()
        {
            IStream stream = this.ResolutionPackage.FileSystem.Open(this.Uri);

            IEnumerator<ZipArchiveEntry> enumerator = null;

            try
            {
                enumerator = new ZipArchive(stream.GetStream()).Entries.GetEnumerator();
            }
            catch (System.IO.InvalidDataException) when (Debugger.IsAttached)
            {
                Debug.WriteLine("Opening file failed. Validating data...");

                try
                {
                    //Check with no provider if we can to ensure its not just a provider issue
                    if (File.Exists(this.Uri.FullName.WindowsValue))
                    {
                        List<ZipArchiveEntry> entries = new ZipArchive(File.Open(this.Uri.FullName.WindowsValue, FileMode.Open, FileAccess.Read, FileShare.Read)).Entries.ToList();
                        throw;
                    }
                }
                catch (System.IO.InvalidDataException)
                {
                }
            }
            catch (System.IO.InvalidDataException)
            {
            }

            if (enumerator is null)
            {
                yield break;
            }

            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
    }
}