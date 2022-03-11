using System;
using System.Collections;
using System.Collections.Generic;
using VirtualFileSystem.Interfaces;

namespace VirtualFileSystem.FileTypes
{
    public abstract class TableFile : TextFile, IEnumerable<IDataRow>
    {
        public TableFile(ResolveUriPackage resolveUriPackage) : base(resolveUriPackage)
        {
        }

        public abstract IEnumerator<IDataRow> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }
}