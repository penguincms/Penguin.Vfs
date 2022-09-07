using Penguin.Vfs.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Penguin.Vfs.Extensions
{
    public static class IFileSystemExtensions
    {
        public static IFileSystemEntry GenericFind(this IFileSystem fileSystem, PathPart path, bool expectingFile)
        {
            if (fileSystem is null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            Stack<PathPart> chunks = path.Chunks.Reverse().ToStack();

            ResolveUriPackage resolveUriPackage = fileSystem.ResolutionPackage;

            IFileSystemEntry topEntry = fileSystem.ResolutionPackage.EntryFactory.Resolve(resolveUriPackage, true);

            while (chunks.Any())
            {
                if (topEntry == null)
                {
                    throw new FileNotFoundException();
                }

                string partName = chunks.Pop().Value;

                if (chunks.Count == 0 && expectingFile)
                {
                    topEntry = (topEntry as IHasFiles).Files.Where(d => d.Uri.Name.Value == partName).FirstOrDefault();
                } else { 
                    topEntry = (topEntry as IHasDirectories).Directories.Where(d => d.Uri.Name.Value == partName).FirstOrDefault();

                    if (topEntry is null)
                    {
                        if (expectingFile)
                        {
                            throw new FileNotFoundException();
                        }
                        else
                        {
                            throw new DirectoryNotFoundException();
                        }
                    }

                    if (topEntry is IFileSystem fs)
                    {
                        return fs.Find(new PathPart(chunks.Reverse()), expectingFile);
                    }
                }

            }

            return topEntry;
        }
    }
}