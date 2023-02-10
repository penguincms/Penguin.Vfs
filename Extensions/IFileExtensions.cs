using Penguin.Vfs.Interfaces;
using System;

namespace Penguin.Vfs.Extensions
{
    public static class IFileExtensions
    {
        public static IStream Open(this IFile file)
        {
            return file is null ? throw new ArgumentNullException(nameof(file)) : file.ResolutionPackage.FileSystem.Open(file.Uri);
        }
    }
}