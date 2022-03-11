namespace VirtualFileSystem.Interfaces
{
    public interface IDirectory : IFileSystemEntry, IHasFilesAndDirectories
    {
        bool IsRecursive { get; }
    }
}