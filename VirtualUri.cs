using VirtualFileSystem.Interfaces;

namespace VirtualFileSystem
{
    public class VirtualUri : IUri
    {
        private readonly PathPart ParentPath;

        public PathPart Extension { get; }

        public PathPart FullName => !this.LocalPath.HasValue ? this.MountPoint : this.MountPoint.Append(this.LocalPath);

        public PathPart LocalPath { get; }

        public PathPart MountPoint { get; }

        public PathPart Name { get; }

        public IUri Parent => new VirtualUri(this.MountPoint, this.ParentPath);

        public VirtualUri(PathPart mountPoint, PathPart localPath)
        {
            this.Name = localPath.FileNameWithoutExtension;
            this.Extension = localPath.Extension;
            this.MountPoint = mountPoint;
            this.LocalPath = localPath;

            this.ParentPath = localPath.Parent;
        }

        public VirtualUri(PathPart mountPoint)
        {
            this.MountPoint = mountPoint;
        }

        private VirtualUri()
        { }

        public IUri AppendChild(PathPart name) => new VirtualUri(this.MountPoint, this.LocalPath.HasValue ? this.LocalPath.Append(name) : name);

        public IUri AsFileSystem() => new VirtualUri(this.FullName);

        public override string ToString() => this.FullName.HasValue ? this.FullName.ToString() : string.Empty;
    }
}