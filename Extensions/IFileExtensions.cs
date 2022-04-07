using Penguin.Vfs.Interfaces;

namespace Penguin.Vfs.Extensions
{
    public static class IFileExtensions
    {
        public static IStream Open(this IFile file) => file.ResolutionPackage.FileSystem.Open(file.Uri);
    }
}