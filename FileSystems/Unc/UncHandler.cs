using Penguin.Vfs.FileSystems.Local;
using Penguin.Vfs.Interfaces;
using System.Linq;

namespace Penguin.Vfs.FileSystems.Unc
{
    internal class UncHandler : IFileSystemEntryHandler
    {
        public IFileSystemEntry Create(ResolveUriPackage resolveUriPackage)
        {
            return new UncDrive(resolveUriPackage);
        }

        public bool IsMatch(ResolveUriPackage resolveUriPackage)
        {
            string pUri = resolveUriPackage.VirtualUri.FullName.Value;

            if (pUri.StartsWith("//?/"))
            {
                pUri = pUri[4..];
            }

            string wVal = new PathPart(pUri).WindowsValue;

            return wVal.Count(c => c == '\\') == 4 && wVal.StartsWith("\\\\") && wVal.EndsWith("\\") && !new LocalDriveHandler().IsMatch(resolveUriPackage);
        }
    }
}