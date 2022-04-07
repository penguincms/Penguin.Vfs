using System;
using System.Collections.Generic;
using System.IO;

namespace Penguin.Vfs.Interfaces
{
    public interface IStream : IDisposable
    {
        public IEnumerable<string> EnumerateLines();

        public Stream GetStream();
    }
}