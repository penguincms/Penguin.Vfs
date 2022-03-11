namespace VirtualFileSystem.Interfaces
{
    public interface IFileSystemEntryHandler
    {
        IFileSystemEntry Create(ResolveUriPackage resolveUriPackage);

        bool IsMatch(ResolveUriPackage resolveUriPackage);
    }
}