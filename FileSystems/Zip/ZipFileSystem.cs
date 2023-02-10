using Penguin.Vfs.Extensions;
using Penguin.Vfs.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Penguin.Vfs.FileSystems.Zip
{
    public partial class ZipFileSystem : IFile, IFileSystem
    {
        private static readonly object CacheLock = new();
        private readonly Dictionary<string, CachedZip> Cache = new();
        /// <inheritdoc/>
        public IEnumerable<IDirectory> Directories => EnumerateDirectories(new PathPart(), false);
        /// <inheritdoc/>

        public IEnumerable<IFile> Files => EnumerateFiles(new PathPart(), false);
        /// <inheritdoc/>

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
        /// <inheritdoc/>

        public bool IsRecursive => false;
        /// <inheritdoc/>
        public DateTime LastModified { get; }
        /// <inheritdoc/>
        public long Length { get; }
        /// <inheritdoc/>
        public PathPart MountPoint => ResolutionPackage.VirtualUri.FullName;
        /// <inheritdoc/>
        public ResolveUriPackage ResolutionPackage { get; }
        /// <inheritdoc/>
        public IUri Uri => ResolutionPackage.VirtualUri;

        private IEnumerable<CachedEntry> CachedEntries
        {
            get
            {
                CachedZip cz;

                lock (CacheLock)
                {
                    cz = Cache[CacheKey];

                    if (cz.Entries != null)
                    {
                        return cz.Entries;
                    }

                    cz.Entries = ReadEntries.ToList();

                    return cz.Entries;
                }
            }
        }

        private string CacheKey => Uri.FullName.Value;

        private IEnumerable<CachedEntry> ReadEntries
        {
            get
            {
                List<CachedEntry> c = new();

                foreach (ZipArchiveEntry zipArchiveEntry in TryGetEntries())
                {
                    c.Add(new CachedEntry()
                    {
                        FullName = zipArchiveEntry.FullName,
                        Name = zipArchiveEntry.Name,
                        LastModified = zipArchiveEntry.LastWriteTime.DateTime,
                        Length = zipArchiveEntry.Length
                    });
                }

                return c;
            }
        }
        /// <inheritdoc/>

        public ZipFileSystem(ResolveUriPackage resolveUriPackage)
        {
            ResolutionPackage = resolveUriPackage;

            if (!Cache.TryGetValue(CacheKey, out CachedZip cz))
            {
                cz = new()
                {
                    Handles = 1
                };
                Cache.Add(CacheKey, cz);
            }
            else
            {
                cz.Handles++;
            }
        }
        /// <inheritdoc/>

        public bool DirectoryExists(IUri uri)
        {
            return CachedEntries.Any(e => uri.LocalPath.IsChildOf(new PathPart(e.FullName)));
        }
        /// <inheritdoc/>

        public IEnumerable<IDirectory> EnumerateDirectories(PathPart pathPart, bool recursive)
        {
            HashSet<string> returned = new();

            foreach (CachedEntry zipArchiveEntry in CachedEntries)
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
                    yield return new ZipArchiveDirectory(ResolutionPackage.AppendChild(zipArchiveEntry.GetDirectoryName(), this))
                    {
                        LastModified = System.DateTime.MinValue
                    };
                }
            }
        }
        /// <inheritdoc/>

        public IEnumerable<IDirectory> EnumerateDirectories(bool recursive)
        {
            return EnumerateDirectories(new PathPart(), recursive);
        }
        /// <inheritdoc/>

        public IEnumerable<IFile> EnumerateFiles(PathPart pathPart, bool recursive)
        {
            foreach (CachedEntry zipArchiveEntry in CachedEntries)
            {
                if (!pathPart.HasValue)
                {
                    if (!recursive)
                    {
                        if (zipArchiveEntry.Name != zipArchiveEntry.FullName)
                        {
                            continue;
                        }
                    }

                    yield return ResolutionPackage.EntryFactory.Resolve(
                        ResolutionPackage.AppendChild(zipArchiveEntry.Name, this).WithFileInfo(zipArchiveEntry.LastModified, zipArchiveEntry.Length), true) as IFile;
                }
                else
                {
                    PathPart ZipPath = new(zipArchiveEntry.FullName);

                    if (!ZipPath.IsChildOf(pathPart))
                    {
                        continue;
                    }

                    PathPart LocalPath = ZipPath.MakeLocal(pathPart);

                    if (!recursive && LocalPath.Depth > 1)
                    {
                        continue;
                    }

                    if (ResolutionPackage.EntryFactory.Resolve(ResolutionPackage.WithFileSystem(this).WithUri(new VirtualUri(MountPoint, pathPart.Append(zipArchiveEntry.Name))), false) is IFile f)
                    {
                        yield return f;
                    }
                }
            }
        }
        /// <inheritdoc/>

        public IEnumerable<IFile> EnumerateFiles(bool recursive)
        {
            return EnumerateFiles(new PathPart(), recursive);
        }
        /// <inheritdoc/>

        public IEnumerable<IFileSystemEntry> EnumerateFileSystemEntries(PathPart pathPart, bool recursive)
        {
            foreach (IFile file in EnumerateFiles(pathPart, recursive))
            {
                yield return file;
            }

            foreach (IDirectory dir in EnumerateDirectories(pathPart, recursive))
            {
                yield return dir;
            }
        }
        /// <inheritdoc/>

        public IEnumerable<IFileSystemEntry> EnumerateFileSystemEntries(bool recursive)
        {
            return EnumerateFileSystemEntries(new PathPart(), recursive);
        }
        /// <inheritdoc/>

        public bool FileExists(IUri uri)
        {
            return CachedEntries.Any(e => uri.LocalPath.Value == uri.LocalPath.Value);
        }
        /// <inheritdoc/>

        public IFileSystemEntry Find(PathPart pathPart, bool expectingFile)
        {
            return this.GenericFind(pathPart, expectingFile);
        }
        /// <inheritdoc/>

        public IStream Open(IUri uri)
        {
            if (uri is null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            IStream stream = ResolutionPackage.FileSystem.Open(Uri);

            if (!uri.LocalPath.HasValue)
            {
                return stream;
            }

            foreach (CachedEntry zipArchiveEntry in CachedEntries)
            {
                if (zipArchiveEntry.FullName == uri.LocalPath.Value)
                {
                    return new ZipFileStream(ResolutionPackage.WithFileSystem(this).WithUri(uri));
                }
            }

            throw new FileNotFoundException();
        }

        private IEnumerable<ZipArchiveEntry> TryGetEntries()
        {
            IEnumerator<ZipArchiveEntry> enumerator = null;

            try
            {
                IStream stream = ResolutionPackage.FileSystem.Open(Uri);

                enumerator = new ZipArchive(stream.GetStream()).Entries.GetEnumerator();
            }
            catch (System.IO.InvalidDataException) when (Debugger.IsAttached)
            {
                Debug.WriteLine("Opening file failed. Validating data...");

                try
                {
                    //Check with no provider if we can to ensure its not just a provider issue
                    if (File.Exists(Uri.FullName.WindowsValue))
                    {
                        List<ZipArchiveEntry> entries = new ZipArchive(File.Open(Uri.FullName.WindowsValue, FileMode.Open, FileAccess.Read, FileShare.Read)).Entries.ToList();
                        throw;
                    }
                }
                catch (System.IO.InvalidDataException)
                {
                }
            }
            catch (System.IO.IOException iox) when (iox.Message.Contains("used by another process", StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine($"File '{Uri}' is being used by another process");
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