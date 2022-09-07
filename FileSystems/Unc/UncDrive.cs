using Penguin.Vfs.Caches;
using Penguin.Vfs.Interfaces;
using System;

namespace Penguin.Vfs.FileSystems.Local
{
    public class UncDrive : LocalDrive
    {
        public UncDrive(ResolveUriPackage resolveUriPackage) : base(resolveUriPackage)
        {
        }

        public override IFileSystemEntry Find(PathPart path, bool expectingFile)
        {
            string realLoc = this.GetWindowsRelative(path);

            if (realLoc.StartsWith("\\\\?\\"))
            {
                realLoc = realLoc[4..];
            }

            if (this.FileExists(realLoc) || this.DirectoryExists(realLoc))
            {

                VirtualUri toSearch;
                if (this.MountPoint.Value == path.Value)
                {
                    toSearch = new VirtualUri(this.MountPoint);
                }
                else
                {
                    toSearch = new VirtualUri(this.MountPoint, path);
                }

                return this.ResolutionPackage.EntryFactory.Resolve(this.ResolutionPackage.WithUri(toSearch), false);
            }

            return base.Find(path, expectingFile);
        }

        public override IStream Open(IUri uri)
        {
            if (uri is null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return new CachedFileStream(uri.FullName);
        }

        public override void SetMount(PathPart path) => this.MountPoint = path;
    }
}