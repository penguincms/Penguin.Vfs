using System.IO;
using VirtualFileSystem.Caches;
using VirtualFileSystem.Interfaces;

namespace VirtualFileSystem.FileSystems.Local
{
    public class UncDrive : LocalDrive
    {
        public UncDrive(ResolveUriPackage resolveUriPackage) : base(resolveUriPackage)
        {
        }

        public override IFileSystemEntry Find(PathPart path)
        {
            string realLoc = this.GetWindowsRelative(path);

            if (realLoc.StartsWith("\\\\?\\"))
            {
                realLoc = realLoc[4..];
            }

            if (this.FileExists(realLoc) || this.DirectoryExists(realLoc))
            {
                return this.ResolutionPackage.EntryFactory.Resolve(this.ResolutionPackage.WithUri(new VirtualUri(this.MountPoint, path)));
            }

            return base.Find(path);
        }

        public override IStream Open(IUri uri) => new CachedFileStream(uri.FullName);

        public override void SetMount(PathPart path) => this.MountPoint = path;
    }
}