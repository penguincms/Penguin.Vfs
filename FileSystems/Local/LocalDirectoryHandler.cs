using Penguin.Vfs.Interfaces;

namespace Penguin.Vfs.FileSystems.Local
{
    public class LocalDirectoryHandler : IFileSystemEntryHandler
    {
        public IFileSystemEntry Create(ResolveUriPackage resolveUriPackage)
        {
            return new LocalDirectoryEntry(resolveUriPackage);
        }

        public bool IsMatch(ResolveUriPackage resolveUriPackage)
        {
            return !resolveUriPackage.CachedFiles.Contains(resolveUriPackage.VirtualUri.FullName.WindowsValue)
&& resolveUriPackage.FileSystem.DirectoryExists(resolveUriPackage.VirtualUri);
        }
    }
}