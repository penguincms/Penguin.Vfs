namespace Penguin.Vfs.Interfaces
{
    public interface IFile : IFileSystemEntry
    {
        long Length { get; }
    }
}