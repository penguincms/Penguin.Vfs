using Penguin.Vfs.Extensions;
using Penguin.Vfs.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Penguin.Vfs.FileSystems.Local
{
    public class LocalDrive : IFileSystem, IFileSystemEntry
    {
        protected ResolveUriPackage ResolutionPackage;
        private bool disposedValue;

        public IEnumerable<IDirectory> Directories => EnumerateDirectories(false);

        public IEnumerable<IFile> Files => EnumerateFiles(false);

        public IEnumerable<IFileSystemEntry> FileSystemEntries => EnumerateFileSystemEntries(false);

        public bool IsRecursive => false;

        public DateTime LastModified => new DirectoryInfo(Uri.FullName.Value).LastWriteTime;
        public PathPart MountPoint { get; set; }
        ResolveUriPackage IFileSystemEntry.ResolutionPackage => ResolutionPackage;
        public IUri Uri { get; }

        public LocalDrive(ResolveUriPackage resolveUriPackage)
        {
            SetMount(resolveUriPackage.VirtualUri.FullName);

            Uri = new VirtualUri(MountPoint);

            ResolutionPackage = resolveUriPackage.Copy();

            ResolutionPackage.FileSystem = this;
            ResolutionPackage.VirtualUri = Uri;
        }

        public static bool operator !=(LocalDrive lhs, LocalDrive rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator ==(LocalDrive lhs, LocalDrive rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public bool DirectoryExists(string uri)
        {
            if (ResolutionPackage.CachedDirectories.Contains(uri))
            {
                return true;
            }

            Debug.WriteLine("Checking Exists: " + uri);
            return Directory.Exists(uri);
        }

        public bool DirectoryExists(IUri uri)
        {
            return uri is null ? throw new ArgumentNullException(nameof(uri)) : DirectoryExists(uri.FullName.WindowsValue);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }

        public IEnumerable<IDirectory> EnumerateDirectories(PathPart pathPart, bool recursive)
        {
            if (Find(pathPart, false) is IDirectory root)
            {
                IFileSystem entryFileSystem = root.ResolutionPackage.FileSystem ?? this;

                if (entryFileSystem is LocalDrive ld && ld == this)
                {
                    string toSearch = GetWindowsRelative(pathPart);

                    List<string> allPaths = new();
                    List<string> directories = new();
                    List<string> files = new();
                    try
                    {
                        Debug.WriteLine($"Enumerating Files: {toSearch}");
                        files.AddRange(Directory.EnumerateFiles(toSearch));
                        Debug.WriteLine($"Enumerating Directories: {toSearch}");
                        directories.AddRange(Directory.EnumerateDirectories(toSearch));
                    }
                    catch (System.UnauthorizedAccessException)
                    {
                        throw;
                    }
                    catch (System.IO.DirectoryNotFoundException)
                    {
                        throw;
                    }

                    ResolutionPackage.CacheFiles(toSearch, files);
                    ResolutionPackage.CacheDirectories(toSearch, directories);

                    allPaths.AddRange(directories.Select(s => s[(s.LastIndexOf('\\') + 1)..]));
                    allPaths.AddRange(files.Select(Path.GetFileName));

                    foreach (string fse in allPaths)
                    {
                        VirtualUri itemPath = MountPoint.Value == pathPart.Value ? (new(MountPoint, new PathPart(fse))) : (new(MountPoint, pathPart.Append(fse)));
                        if (ResolutionPackage.EntryFactory.Resolve(ResolutionPackage.WithUri(itemPath), false) is IDirectory cfse)
                        {
                            yield return cfse;

                            if (recursive && !cfse.IsRecursive)
                            {
                                List<IDirectory> cfseDirectories = new();

                                try
                                {
                                    cfseDirectories = cfse.EnumerateDirectories(true).ToList();
                                }
                                catch (IOException iex) when (iex.Message.Contains("being used by another process"))
                                {
                                }

                                foreach (IDirectory d in cfseDirectories)
                                {
                                    yield return d;
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (IDirectory file in entryFileSystem.EnumerateDirectories(root.Uri.LocalPath, recursive))
                    {
                        yield return file;
                    }
                }
            }
        }

        public IEnumerable<IDirectory> EnumerateDirectories(bool recursive)
        {
            return EnumerateDirectories(MountPoint, recursive);
        }

        public IEnumerable<IFile> EnumerateFiles(PathPart pathPart, bool recursive)
        {
            if (Find(pathPart, false) is IDirectory root)
            {
                IFileSystem entryFileSystem = root.ResolutionPackage.FileSystem ?? this;

                if (entryFileSystem is LocalDrive ld && ld == this)
                {
                    string toSearch = GetWindowsRelative(pathPart);

                    if (DirectoryExists(toSearch))
                    {
                        List<string> files = new();

                        try
                        {
                            Debug.WriteLine($"Enumerating Files: {toSearch}");
                            files.AddRange(Directory.EnumerateFiles(toSearch));
                            ResolutionPackage.CacheFiles(toSearch, files);
                        }
                        catch (System.UnauthorizedAccessException)
                        {
                        }
                        catch (System.IO.DirectoryNotFoundException)
                        {
                        }

                        foreach (string file in files)
                        {
                            FileInfo fi = new(file);
                            ResolveUriPackage newPackage = ResolutionPackage.AppendChild(new PathPart(file).MakeLocal(MountPoint))
                                                                                  .WithFileInfo(fi.LastWriteTime, fi.Length);

                            if (ResolutionPackage.EntryFactory.Resolve(newPackage, true) is IFile f)
                            {
                                yield return f;
                            }
                        }
                    }

                    if (recursive)
                    {
                        List<IDirectory> directories = EnumerateDirectories(pathPart, true).ToList();

                        foreach (IDirectory d in directories)
                        {
                            Debug.WriteLine($"Enumerating files in: {d.Uri}");
                            List<IFile> files = new();
                            try
                            {
                                files.AddRange(d.EnumerateFiles(false));
                            }
                            catch (IOException ex) when (ex.Message.Contains("The file cannot be accessed by the system"))
                            {
                            }

                            foreach (IFile f in files)
                            {
                                yield return f;
                            }
                        }
                    }
                }
                else
                {
                    foreach (IFile file in entryFileSystem.EnumerateFiles(root.Uri.LocalPath, recursive))
                    {
                        yield return file;
                    }
                }
            }
        }

        public IEnumerable<IFile> EnumerateFiles(bool recursive)
        {
            return EnumerateFiles(new PathPart(), recursive);
        }

        public IEnumerable<IFileSystemEntry> EnumerateFileSystemEntries(PathPart pathPart, bool recursive)
        {
            foreach (IFile file in EnumerateFiles(pathPart, recursive))
            {
                yield return file;
            }

            foreach (IDirectory directory in EnumerateDirectories(pathPart, recursive))
            {
                yield return directory;
            }
        }

        public IEnumerable<IFileSystemEntry> EnumerateFileSystemEntries(bool recursive)
        {
            return EnumerateFileSystemEntries(MountPoint, recursive);
        }

        public override bool Equals(object obj)
        {
            return obj is LocalDrive ld && ld.MountPoint.Value == MountPoint.Value;
        }

        public bool FileExists(string uri)
        {
            if (ResolutionPackage.CachedFiles.Contains(uri))
            {
                return true;
            }

            Debug.WriteLine("Checking Exists: " + uri);
            return File.Exists(uri);
        }

        public bool FileExists(IUri uri)
        {
            return uri is null ? throw new ArgumentNullException(nameof(uri)) : FileExists(uri.FullName.WindowsValue);
        }

        public virtual IFileSystemEntry Find(PathPart pathPart, bool expectingFile)
        {
            if (MountPoint.Value == pathPart.Value)
            {
                return this;
            }

            string realLoc = GetWindowsRelative(pathPart);

            return FileExists(realLoc) || DirectoryExists(realLoc)
                ? ResolutionPackage.EntryFactory.Resolve(ResolutionPackage.WithUri(new VirtualUri(MountPoint, pathPart)), false)
                : this.GenericFind(pathPart, expectingFile);
        }

        public string GetWindowsRelative(PathPart path)
        {
            return !path.HasValue || MountPoint.Value == path.Value
                ? MountPoint.WindowsValue.EndsWith("\\") ? MountPoint.WindowsValue : MountPoint.WindowsValue + "\\"
                : MountPoint.Append(path).WindowsValue;
        }

        public virtual IStream Open(IUri uri)
        {
            return uri is null ? throw new ArgumentNullException(nameof(uri)) : (IStream)new FileStream(uri.FullName);
        }

        public virtual void SetMount(PathPart path)
        {
            MountPoint = path;

            if (!MountPoint.StartsWith("//?/"))
            {
                MountPoint = MountPoint.Prepend("//?/");
            }
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

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public bool DirectoryExists(Uri uri)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(Uri uri)
        {
            throw new NotImplementedException();
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~LocalDrive()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }
    }
}