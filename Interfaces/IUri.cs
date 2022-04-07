namespace Penguin.Vfs.Interfaces
{
    public interface IUri
    {
        PathPart Extension { get; }
        PathPart FullName { get; }
        PathPart LocalPath { get; }
        PathPart MountPoint { get; }
        PathPart Name { get; }
        IUri Parent { get; }

        IUri AppendChild(PathPart name);

        IUri AsFileSystem();
    }
}