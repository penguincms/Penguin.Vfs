namespace Penguin.Vfs.Interfaces
{
    public interface IDirectory : IFileSystemEntry, IHasFilesAndDirectories
    {
        bool IsRecursive { get; }
    }
}