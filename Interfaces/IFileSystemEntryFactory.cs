namespace Penguin.Vfs.Interfaces
{
    public interface IFileSystemEntryFactory
    {
        public IFileSystemEntry Resolve(ResolveUriPackage resolveUriPackage, bool cache);

        //public void RegisterStreamSource(Func<string, Stream> resolveFunction);

        //public Stream OpenStream(string path);
    }
}