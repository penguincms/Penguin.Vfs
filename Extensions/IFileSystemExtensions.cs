using Penguin.Vfs.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Penguin.Vfs.Extensions
{
    public static class IFileSystemExtensions
    {
        public static IFileSystemEntry GenericFind(this IFileSystem fileSystem, PathPart path)
        {
            if (fileSystem is null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            Stack<PathPart> chunks = path.Chunks.ToStack();

            ResolveUriPackage resolveUriPackage = fileSystem.ResolutionPackage;

            IFileSystemEntry topEntry = fileSystem.ResolutionPackage.EntryFactory.Resolve(resolveUriPackage);

            while (chunks.Any())
            {
                if (topEntry == null)
                {
                    throw new FileNotFoundException();
                }

                topEntry = (topEntry as IHasDirectories).Directories.Where(d => d.Uri.Name.Value == chunks.Pop().Value).FirstOrDefault();

                if (topEntry is IFileSystem fs)
                {
                    return fs.Find(new PathPart(chunks));
                }
            }

            return topEntry;
        }
    }
}