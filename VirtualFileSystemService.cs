using Penguin.Vfs.Caches;
using Penguin.Vfs.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Penguin.Vfs.Extensions;

namespace Penguin.Vfs
{
    public class VirtualFileSystemService
    {
        public IFileSystemCache Cache { get; set; } = new DummyCache();
        public VirtualFileSystemSettings VirtualFileSystemSettings { get; set; } = new VirtualFileSystemSettings();

        public IEnumerable<IDirectory> EnumerateDirectories(string path, bool recursive) => this.EnumerateDirectories(new PathPart(path), recursive);

        public IEnumerable<IDirectory> EnumerateDirectories(PathPart pathPart, bool recursive)
        {
            IHasDirectories parent = (IHasDirectories)this.FindNode(pathPart, false);

            return parent.EnumerateDirectories(recursive);
        }

        public IEnumerable<IFile> EnumerateFiles(string path, bool recursive) => this.EnumerateFiles(new PathPart(path), recursive);

        public IEnumerable<IFile> EnumerateFiles(PathPart path, bool recursive)
        {
            IHasFiles fse = (IHasFiles)this.FindNode(path, false);

            return fse.EnumerateFiles(recursive);
        }

        public IStream OpenFile(string path)
        {
            IFile file = this.FindNode(new PathPart(path), true) as IFile;

            return file.Open();
        }

        private IFileSystemEntry FindNode(PathPart path, bool expectingFile)
        {
            IFileSystem fs = this.VirtualFileSystemSettings.Resolve(new ResolveUriPackage()
            {
                VirtualUri = new VirtualUri(path.Chunks.First()),
                EntryFactory = this.VirtualFileSystemSettings,
                SessionCache = new Dictionary<string, IFileSystemEntry>()
            }, false) as IFileSystem;

            return fs.Find(path.MakeLocal(path.Chunks.First()), expectingFile);
        }
    }
}