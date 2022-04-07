using System;
using System.Collections;
using System.Collections.Generic;
using Penguin.Vfs.Interfaces;

namespace Penguin.Vfs.FileTypes
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