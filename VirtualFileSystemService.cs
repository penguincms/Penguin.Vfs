﻿using Penguin.Vfs.Caches;
using Penguin.Vfs.Extensions;
using Penguin.Vfs.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Penguin.Vfs
{
    public class VirtualFileSystemService
    {
        public IFileSystemCache Cache { get; set; } = new DummyCache();

        public VirtualFileSystemSettings VirtualFileSystemSettings { get; set; } = new VirtualFileSystemSettings();

        public IEnumerable<IDirectory> EnumerateDirectories(string path, bool recursive)
        {
            return EnumerateDirectories(new PathPart(path), recursive);
        }

        public IEnumerable<IDirectory> EnumerateDirectories(PathPart pathPart, bool recursive)
        {
            IHasDirectories parent = (IHasDirectories)FindNode(pathPart, false);

            return parent.EnumerateDirectories(recursive);
        }

        public IEnumerable<IFile> EnumerateFiles(string path, bool recursive)
        {
            return EnumerateFiles(new PathPart(path), recursive);
        }

        public IEnumerable<IFile> EnumerateFiles(PathPart path, bool recursive)
        {
            IHasFiles fse = (IHasFiles)FindNode(path, false);

            return fse.EnumerateFiles(recursive);
        }

        public IStream OpenFile(string path)
        {
            IFile file = FindNode(new PathPart(path), true) as IFile;

            return file.Open();
        }

        private IFileSystemEntry FindNode(PathPart path, bool expectingFile)
        {
            IFileSystem fs = VirtualFileSystemSettings.Resolve(new ResolveUriPackage()
            {
                VirtualUri = new VirtualUri(path.Chunks.First()),
                EntryFactory = VirtualFileSystemSettings,
                SessionCache = new Dictionary<string, IFileSystemEntry>()
            }, false) as IFileSystem;

            return fs.Find(path.MakeLocal(path.Chunks.First()), expectingFile);
        }
    }
}