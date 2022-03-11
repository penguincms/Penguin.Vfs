using VirtualFileSystem.Interfaces;

namespace VirtualFileSystem.Extensions
{
    public static class IFileExtensions
    {
        public static IStream Open(this IFile file) => file.ResolutionPackage.FileSystem.Open(file.Uri);
    }
}