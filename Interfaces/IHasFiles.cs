using System.Collections.Generic;

namespace Penguin.Vfs.Interfaces
{
    public interface IHasFiles
    {
        IEnumerable<IFile> Files { get; }

        IEnumerable<IFile> EnumerateFiles(bool recursive);
    }
}