using Penguin.Vfs.Interfaces;

namespace Penguin.Vfs
{
    public class VirtualUri : IUri
    {
        private readonly PathPart ParentPath;

        public PathPart Extension { get; }

        public PathPart FullName => !LocalPath.HasValue ? MountPoint : MountPoint.Append(LocalPath);

        public PathPart LocalPath { get; }

        public PathPart MountPoint { get; }

        public PathPart Name { get; }

        public IUri Parent => new VirtualUri(MountPoint, ParentPath);

        public VirtualUri(PathPart mountPoint, PathPart localPath)
        {
            Name = localPath.FileName;
            Extension = localPath.Extension;
            MountPoint = mountPoint;
            LocalPath = localPath;

            ParentPath = localPath.Parent;
        }

        public VirtualUri(PathPart mountPoint)
        {
            MountPoint = mountPoint;
        }

        private VirtualUri()
        { }

        public IUri AppendChild(PathPart name)
        {
            return new VirtualUri(MountPoint, LocalPath.HasValue ? LocalPath.Append(name) : name);
        }

        public IUri AsFileSystem()
        {
            return new VirtualUri(FullName);
        }

        public override string ToString()
        {
            return FullName.HasValue ? FullName.ToString() : string.Empty;
        }
    }
}