using Penguin.Vfs.Interfaces;

namespace Penguin.Vfs.FileSystems.Local
{
    public class LocalDirectoryHandler : IFileSystemEntryHandler
    {
        public IFileSystemEntry Create(ResolveUriPackage resolveUriPackage) => new LocalDirectoryEntry(resolveUriPackage);

        public bool IsMatch(ResolveUriPackage resolveUriPackage)
        {
            if (resolveUriPackage.CachedFiles.Contains(resolveUriPackage.VirtualUri.FullName.WindowsValue))
            {
                return false;
            }

            return resolveUriPackage.FileSystem.DirectoryExists(resolveUriPackage.VirtualUri);
        }
    }
}