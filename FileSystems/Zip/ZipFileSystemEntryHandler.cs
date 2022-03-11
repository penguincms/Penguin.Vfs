namespace VirtualFileSystem.FileSystems.Zip
{
    public class ZipFileSystemEntryHandler : GenericFileHandler<ZipFileSystem>
    {
        public ZipFileSystemEntryHandler() : base(".zip")
        {
        }
    }
}