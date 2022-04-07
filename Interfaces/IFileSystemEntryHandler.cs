namespace Penguin.Vfs.Interfaces
{
    public interface IFileSystemEntryHandler
    {
        IFileSystemEntry Create(ResolveUriPackage resolveUriPackage);

        bool IsMatch(ResolveUriPackage resolveUriPackage);
    }
}