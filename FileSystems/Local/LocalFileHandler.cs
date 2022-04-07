using Penguin.Vfs.Interfaces;

namespace Penguin.Vfs.FileSystems.Local
{
    public class LocalFileHandler : IFileSystemEntryHandler
    {
        public IFileSystemEntry Create(ResolveUriPackage resolveUriPackage) => new LocalFileEntry(resolveUriPackage);

        public bool IsMatch(ResolveUriPackage resolveUriPackage)
        {
            if (resolveUriPackage.CachedDirectories.Contains(resolveUriPackage.VirtualUri.FullName.Value))
            {
                return false;
            }

            return resolveUriPackage.FileSystem.FileExists(resolveUriPackage.VirtualUri);
        }
    }
}