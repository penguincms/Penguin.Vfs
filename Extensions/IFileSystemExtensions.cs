using System.Collections.Generic;
using System.IO;
using System.Linq;
using VirtualFileSystem.Interfaces;

namespace VirtualFileSystem.Extensions
{
    public static class IFileSystemExtensions
    {
        public static IFileSystemEntry GenericFind(this IFileSystem fileSystem, PathPart path)
        {
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