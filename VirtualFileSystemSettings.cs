using Penguin.Vfs.FileSystems.Local;
using Penguin.Vfs.FileSystems.Unc;
using Penguin.Vfs.FileSystems.Zip;
using Penguin.Vfs.FileTypes;
using Penguin.Vfs.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace Penguin.Vfs
{
    public class VirtualFileSystemSettings : IFileSystemEntryFactory
    {
        public IFileSystemEntryHandler DirectoryHandler { get; set; } = new LocalDirectoryHandler();

        public IFileSystemEntryHandler FileHandler { get; set; } = new LocalFileHandler();

        public List<IFileSystemEntryHandler> Handlers { get; } = new List<IFileSystemEntryHandler>()
        {
            new ZipFileSystemEntryHandler(),
            new LocalDriveHandler(),
            new UncHandler(),
            new GenericFileHandler<CsvFile>(".csv"),
            new GenericFileHandler<LogFile>(".txt", ".log")
        };

        public IFileSystemEntry Resolve(ResolveUriPackage resolveUriPackage, bool cache)
        {
            if (!resolveUriPackage.SessionCache.TryGetValue(resolveUriPackage.VirtualUri.FullName.Value, out IFileSystemEntry fse))
            {
                fse = ResolveNoCache(resolveUriPackage);

                if (cache)
                {
                    resolveUriPackage.SessionCache.Add(resolveUriPackage.VirtualUri.FullName.Value, fse);
                }
            }

            return fse;
        }

        private IFileSystemEntry ResolveNoCache(ResolveUriPackage resolveUriPackage)
        {
            foreach (IFileSystemEntryHandler handler in Handlers)
            {
                if (handler.IsMatch(resolveUriPackage))
                {
                    return handler.Create(resolveUriPackage);
                }
            }

            return DirectoryHandler.IsMatch(resolveUriPackage)
                ? DirectoryHandler.Create(resolveUriPackage)
                : FileHandler.IsMatch(resolveUriPackage) ? FileHandler.Create(resolveUriPackage) : throw new FileNotFoundException();
        }
    }
}