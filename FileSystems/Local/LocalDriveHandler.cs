using Penguin.Vfs.Interfaces;

namespace Penguin.Vfs.FileSystems.Local
{
    internal class LocalDriveHandler : IFileSystemEntryHandler
    {
        public IFileSystemEntry Create(ResolveUriPackage resolveUriPackage)
        {
            return new LocalDrive(resolveUriPackage);
        }

        public bool IsMatch(ResolveUriPackage resolveUriPackage)
        {
            string pUri = resolveUriPackage.VirtualUri.FullName.Value;

            if (pUri.StartsWith("//?/"))
            {
                pUri = pUri[4..];
            }

            return pUri.Length > 1 && char.IsLetter(pUri[0]) && pUri[1] == ':' && pUri.Length <= 3;
        }
    }
}