using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using VirtualFileSystem.Extensions;
using VirtualFileSystem.Interfaces;

namespace VirtualFileSystem.FileSystems.Local
{
    public class LocalDrive : IFileSystem, IFileSystemEntry
    {
        protected ResolveUriPackage ResolutionPackage;
        private bool disposedValue;

        public IEnumerable<IDirectory> Directories => this.EnumerateDirectories(false);

        public IEnumerable<IFile> Files => this.EnumerateFiles(false);

        public IEnumerable<IFileSystemEntry> FileSystemEntries => this.EnumerateFileSystemEntries(false);

        public bool IsRecursive => false;

        public PathPart MountPoint { get; set; }
        ResolveUriPackage IFileSystemEntry.ResolutionPackage => this.ResolutionPackage;
        public IUri Uri { get; }

        public LocalDrive(ResolveUriPackage resolveUriPackage)
        {
            this.SetMount(resolveUriPackage.VirtualUri.FullName);

            this.Uri = new VirtualUri(this.MountPoint);

            this.ResolutionPackage = resolveUriPackage.Copy();

            this.ResolutionPackage.FileSystem = this;
            this.ResolutionPackage.VirtualUri = this.Uri;
        }

        public static bool operator !=(LocalDrive lhs, LocalDrive rhs) => !(lhs == rhs);

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
            if (this.ResolutionPackage.CachedDirectories.Contains(uri))
            {
                return true;
            }

            Debug.WriteLine("Checking Exists: " + uri);
            return Directory.Exists(uri);
        }

        public bool DirectoryExists(IUri uri) => this.DirectoryExists(uri.FullName.WindowsValue);

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }

        public IEnumerable<IDirectory> EnumerateDirectories(PathPart path, bool recursive)
        {
            if (this.Find(path) is IDirectory root)
            {
                IFileSystem entryFileSystem = root.ResolutionPackage.FileSystem ?? this;

                if (entryFileSystem is LocalDrive ld && ld == this)
                {
                    string toSearch = this.GetWindowsRelative(path);

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
                    }
                    catch (System.IO.DirectoryNotFoundException)
                    {
                    }

                    this.ResolutionPackage.CacheFiles(toSearch, files);
                    this.ResolutionPackage.CacheDirectories(toSearch, directories);

                    allPaths.AddRange(directories.Select(s => s[(s.LastIndexOf('\\') + 1)..]));
                    allPaths.AddRange(files.Select(Path.GetFileName));

                    foreach (string fse in allPaths)
                    {
                        VirtualUri itemPath = new(this.MountPoint, path.Append(fse));

                        if (this.ResolutionPackage.EntryFactory.Resolve(this.ResolutionPackage.WithUri(itemPath)) is IDirectory cfse)
                        {
                            yield return cfse;

                            if (recursive && !cfse.IsRecursive)
                            {
                                foreach (IDirectory d in cfse.EnumerateDirectories(true))
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

        public IEnumerable<IDirectory> EnumerateDirectories(bool recursive) => this.EnumerateDirectories(this.MountPoint, recursive);

        public IEnumerable<IFile> EnumerateFiles(PathPart path, bool recursive)
        {
            if (this.Find(path) is IDirectory root)
            {
                IFileSystem entryFileSystem = root.ResolutionPackage.FileSystem ?? this;

                if (entryFileSystem is LocalDrive ld && ld == this)
                {
                    string toSearch = this.GetWindowsRelative(path);

                    if (this.DirectoryExists(toSearch))
                    {
                        List<string> files = new();

                        try
                        {
                            Debug.WriteLine($"Enumerating Files: {toSearch}");
                            files.AddRange(Directory.EnumerateFiles(toSearch));
                            this.ResolutionPackage.CacheFiles(toSearch, files);
                        }
                        catch (System.UnauthorizedAccessException)
                        {
                        }
                        catch (System.IO.DirectoryNotFoundException)
                        {
                        }

                        foreach (string file in files)
                        {
                            if (this.ResolutionPackage.EntryFactory.Resolve(this.ResolutionPackage.AppendChild(new PathPart(file).MakeLocal(this.MountPoint))) is IFile f)
                            {
                                yield return f;
                            }
                        }
                    }

                    if (recursive)
                    {
                        foreach (IDirectory d in this.EnumerateDirectories(path, true))
                        {
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

        public IEnumerable<IFile> EnumerateFiles(bool recursive) => this.EnumerateFiles(new PathPart(), recursive);

        public IEnumerable<IFileSystemEntry> EnumerateFileSystemEntries(PathPart path, bool recursive)
        {
            foreach (IFile file in this.EnumerateFiles(path, recursive))
            {
                yield return file;
            }

            foreach (IDirectory directory in this.EnumerateDirectories(path, recursive))
            {
                yield return directory;
            }
        }

        public IEnumerable<IFileSystemEntry> EnumerateFileSystemEntries(bool recursive) => this.EnumerateFileSystemEntries(this.MountPoint, recursive);

        public override bool Equals(object obj) => obj is LocalDrive ld && ld.MountPoint.Value == this.MountPoint.Value;

        public bool FileExists(string uri)
        {
            if (this.ResolutionPackage.CachedFiles.Contains(uri))
            {
                return true;
            }

            Debug.WriteLine("Checking Exists: " + uri);
            return File.Exists(uri);
        }

        public bool FileExists(IUri uri) => this.FileExists(uri.FullName.WindowsValue);

        public virtual IFileSystemEntry Find(PathPart path)
        {
            string realLoc = this.GetWindowsRelative(path);

            if (this.FileExists(realLoc) || this.DirectoryExists(realLoc))
            {
                return this.ResolutionPackage.EntryFactory.Resolve(this.ResolutionPackage.WithUri(new VirtualUri(this.MountPoint, path)));
            }

            return this.GenericFind(path);
        }

        public string GetWindowsRelative(PathPart path) => path.HasValue ? this.MountPoint.Append(path).WindowsValue : this.MountPoint.WindowsValue + "\\";

        public virtual IStream Open(IUri uri) => new FileStream(uri.FullName);

        public virtual void SetMount(PathPart path)
        {
            this.MountPoint = path;

            if (!this.MountPoint.StartsWith("//?/"))
            {
                this.MountPoint = this.MountPoint.Prepend("//?/");
            }
        }

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
        // ~LocalDrive()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }
    }
}