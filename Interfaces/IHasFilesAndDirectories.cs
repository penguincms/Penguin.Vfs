using System.Collections.Generic;

namespace VirtualFileSystem.Interfaces
{
    public interface IHasFilesAndDirectories : IHasFiles, IHasDirectories
    {
        IEnumerable<IFileSystemEntry> FileSystemEntries { get; }

        IEnumerable<IFileSystemEntry> EnumerateFileSystemEntries(bool recursive);
    }
}