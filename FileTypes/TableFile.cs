using Penguin.Vfs.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Penguin.Vfs.FileTypes
{
    public abstract class TableFile : TextFile, IEnumerable<IDataRow>
    {
        protected TableFile(ResolveUriPackage resolveUriPackage) : base(resolveUriPackage)
        {
        }

        public abstract IEnumerator<IDataRow> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}