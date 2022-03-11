using System.Collections.Generic;

namespace VirtualFileSystem.Interfaces
{
    public interface IHasFiles
    {
        IEnumerable<IFile> Files { get; }

        IEnumerable<IFile> EnumerateFiles(bool recursive);
    }
}