using System.Collections.Generic;

namespace VirtualFileSystem.Interfaces
{
    public interface IHasDirectories : IHasFiles
    {
        IEnumerable<IDirectory> Directories { get; }

        IEnumerable<IDirectory> EnumerateDirectories(bool recursive);
    }
}