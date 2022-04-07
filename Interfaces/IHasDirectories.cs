using System.Collections.Generic;

namespace Penguin.Vfs.Interfaces
{
    public interface IHasDirectories : IHasFiles
    {
        IEnumerable<IDirectory> Directories { get; }

        IEnumerable<IDirectory> EnumerateDirectories(bool recursive);
    }
}