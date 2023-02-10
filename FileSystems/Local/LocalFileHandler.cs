using Penguin.Vfs.Interfaces;

namespace Penguin.Vfs.FileSystems.Local
{
    public class LocalFileHandler : IFileSystemEntryHandler
    {
        public IFileSystemEntry Create(ResolveUriPackage resolveUriPackage)
        {
            return new LocalFileEntry(resolveUriPackage);
        }

        public bool IsMatch(ResolveUriPackage resolveUriPackage)
        {
            return !resolveUriPackage.CachedDirectories.Contains(resolveUriPackage.VirtualUri.FullName.Value)
&& resolveUriPackage.FileSystem.FileExists(resolveUriPackage.VirtualUri);
        }
    }
}