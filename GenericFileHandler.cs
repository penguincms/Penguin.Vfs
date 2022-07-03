using Penguin.Vfs.Interfaces;
using System;
using System.Collections.Generic;

namespace Penguin.Vfs
{
    public class GenericFileHandler<T> : IFileSystemEntryHandler where T : IFileSystemEntry
    {
        private readonly HashSet<string> Extensions = new();

        public GenericFileHandler(params string[] extensions)
        {
            if (extensions is null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            foreach (string e in extensions)
            {
                string extension = e;

                if (!extension.StartsWith("."))
                {
                    extension = "." + extension;
                }

                _ = this.Extensions.Add(extension);
            }
        }

        IFileSystemEntry IFileSystemEntryHandler.Create(ResolveUriPackage resolveUriPackage)
        {
            IFileSystemEntry toReturn = (T)Activator.CreateInstance(typeof(T), new object[] { resolveUriPackage });

            return toReturn;
        }

        bool IFileSystemEntryHandler.IsMatch(ResolveUriPackage resolveUriPackage) => resolveUriPackage.VirtualUri.Extension.HasValue && this.Extensions.Contains(resolveUriPackage.VirtualUri.Extension.Value.ToLower());
    }
}