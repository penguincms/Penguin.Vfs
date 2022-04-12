using System;

namespace Penguin.Vfs.Interfaces
{
    public interface IFileSystemEntry
    {
        DateTime LastModified { get; }
        ResolveUriPackage ResolutionPackage { get; }
        IUri Uri { get; }
    }
}