using Penguin.Vfs.Interfaces;
using System;

namespace Penguin.Vfs.Extensions
{
    public static class IFileExtensions
    {
        public static IStream Open(this IFile file)
        {
            if (file is null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            return file.ResolutionPackage.FileSystem.Open(file.Uri);
        }
    }
}