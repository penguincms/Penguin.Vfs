using System.Collections.Generic;

namespace Penguin.Vfs.Interfaces
{
    public interface IFileSystem : IHasFiles, IDirectory
    {
        public PathPart MountPoint { get; }

        bool DirectoryExists(IUri uri);

        IEnumerable<IDirectory> EnumerateDirectories(PathPart pathPart, bool recursive);

        IEnumerable<IFile> EnumerateFiles(PathPart pathPart, bool recursive);

        IEnumerable<IFileSystemEntry> EnumerateFileSystemEntries(PathPart pathPart, bool recursive);

        bool FileExists(IUri uri);

        IFileSystemEntry Find(PathPart pathPart, bool expectingFile);

        IStream Open(IUri uri);
    }
}