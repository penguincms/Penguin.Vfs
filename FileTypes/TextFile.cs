using Penguin.Vfs.Extensions;
using Penguin.Vfs.Interfaces;
using System.Collections.Generic;

namespace Penguin.Vfs.FileTypes
{
    public class TextFile : FileStream
    {
        public TextFile(ResolveUriPackage resolveUriPackage) : base(resolveUriPackage)
        {
        }

        public virtual IEnumerable<string> EnumerateLines()
        {
            using IStream stream = this.Open();

            foreach (string line in stream.EnumerateLines())
            {
                yield return line;
            }
        }
    }
}