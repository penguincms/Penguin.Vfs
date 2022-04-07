namespace Penguin.Vfs.FileSystems.Zip
{
    public class ZipFileSystemEntryHandler : GenericFileHandler<ZipFileSystem>
    {
        public ZipFileSystemEntryHandler() : base(".zip")
        {
        }
    }
}