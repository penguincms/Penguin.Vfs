using Penguin.Vfs.Caches;
using Penguin.Vfs.FileSystems.Local;
using Penguin.Vfs.Interfaces;
using System;

namespace Penguin.Vfs.FileSystems.Unc
{
    public class UncDrive : LocalDrive
    {
        public UncDrive(ResolveUriPackage resolveUriPackage) : base(resolveUriPackage)
        {
        }

        public override IFileSystemEntry Find(PathPart pathPart, bool expectingFile)
        {
            string realLoc = GetWindowsRelative(pathPart);

            if (realLoc.StartsWith("\\\\?\\"))
            {
                realLoc = realLoc[4..];
            }

            if (FileExists(realLoc) || DirectoryExists(realLoc))
            {
                VirtualUri toSearch = MountPoint.Value == pathPart.Value ? new VirtualUri(MountPoint) : new VirtualUri(MountPoint, pathPart);
                return ResolutionPackage.EntryFactory.Resolve(ResolutionPackage.WithUri(toSearch), false);
            }

            return base.Find(pathPart, expectingFile);
        }

        public override IStream Open(IUri uri)
        {
            return uri is null ? throw new ArgumentNullException(nameof(uri)) : (IStream)new CachedFileStream(uri.FullName);
        }

        public override void SetMount(PathPart path)
        {
            MountPoint = path;
        }
    }
}