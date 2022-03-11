namespace VirtualFileSystem.Interfaces
{
    public interface IFileSystemEntry
    {
        ResolveUriPackage ResolutionPackage { get; }
        IUri Uri { get; }
    }
}