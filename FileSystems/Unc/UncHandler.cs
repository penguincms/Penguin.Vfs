using System.Linq;
using VirtualFileSystem.Interfaces;

namespace VirtualFileSystem.FileSystems.Local
{
    internal class UncHandler : IFileSystemEntryHandler
    {
        public IFileSystemEntry Create(ResolveUriPackage resolveUriPackage) => new UncDrive(resolveUriPackage);

        public bool IsMatch(ResolveUriPackage resolveUriPackage)
        {
            string pUri = resolveUriPackage.VirtualUri.FullName.Value;

            if (pUri.StartsWith("//?/"))
            {
                pUri = pUri[4..];
            }

            string wVal = new PathPart(pUri).WindowsValue;

            return wVal.Count(c => c == '\\') == 3 && wVal.StartsWith("\\\\") && wVal.EndsWith("\\") && !new LocalDriveHandler().IsMatch(resolveUriPackage);
        }
    }
}