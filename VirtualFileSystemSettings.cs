using Penguin.Vfs.FileSystems.Local;
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

        //Stream IFileSystemEntryFactory.OpenStream(string path) => throw new NotImplementedException();
        public IFileSystemEntryHandler FileHandler { get; set; } = new LocalFileHandler();

        public List<IFileSystemEntryHandler> Handlers { get; } = new List<IFileSystemEntryHandler>()
        {
            new ZipFileSystemEntryHandler(),
            new LocalDriveHandler(),
            new UncHandler(),
            new GenericFileHandler<CsvFile>(".csv"),
            new GenericFileHandler<LogFile>(".txt", ".log")
        };

        public IFileSystemEntry Resolve(ResolveUriPackage resolveUriPackage)
        {
            if (!resolveUriPackage.SessionCache.TryGetValue(resolveUriPackage.VirtualUri.FullName.Value, out IFileSystemEntry fse))
            {
                fse = this.ResolveNoCache(resolveUriPackage);
                resolveUriPackage.SessionCache.Add(resolveUriPackage.VirtualUri.FullName.Value, fse);
            }

            return fse;
        }

        private IFileSystemEntry ResolveNoCache(ResolveUriPackage resolveUriPackage)
        {
            foreach (IFileSystemEntryHandler handler in this.Handlers)
            {
                if (handler.IsMatch(resolveUriPackage))
                {
                    return handler.Create(resolveUriPackage);
                }
            }

            if (this.DirectoryHandler.IsMatch(resolveUriPackage))
            {
                return this.DirectoryHandler.Create(resolveUriPackage);
            }

            if (this.FileHandler.IsMatch(resolveUriPackage))
            {
                return this.FileHandler.Create(resolveUriPackage);
            }

            throw new FileNotFoundException();
        }
    }
}