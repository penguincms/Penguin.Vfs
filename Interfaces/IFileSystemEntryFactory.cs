namespace VirtualFileSystem.Interfaces
{
    public interface IFileSystemEntryFactory
    {
        public IFileSystemEntry Resolve(ResolveUriPackage resolveUriPackage);

        //public void RegisterStreamSource(Func<string, Stream> resolveFunction);

        //public Stream OpenStream(string path);
    }
}