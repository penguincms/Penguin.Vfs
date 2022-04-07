using System;

namespace Penguin.Vfs.Interfaces
{
    public interface IFileSystemEntry
    {
        ResolveUriPackage ResolutionPackage { get; }
        IUri Uri { get; }
        DateTime LastModified { get; }
    }
}