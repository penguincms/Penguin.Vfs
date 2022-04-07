using System.Collections.Generic;

namespace Penguin.Vfs.Interfaces
{
    public interface IHasFilesAndDirectories : IHasFiles, IHasDirectories
    {
        IEnumerable<IFileSystemEntry> FileSystemEntries { get; }

        IEnumerable<IFileSystemEntry> EnumerateFileSystemEntries(bool recursive);
    }
}