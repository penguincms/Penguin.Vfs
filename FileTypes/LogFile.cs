using System.Collections;
using System.Collections.Generic;
using VirtualFileSystem.Interfaces;

namespace VirtualFileSystem.FileTypes
{
    internal class LogFile : TableFile
    {
        public LogFile(ResolveUriPackage resolveUriPackage) : base(resolveUriPackage)
        {
        }

        public override IEnumerator<IDataRow> GetEnumerator()
        {
            foreach (string line in base.EnumerateLines())
            {
                yield return new SingleCellRow(line);
            }
        }
    }

    internal class SingleCellRow : IDataRow
    {
        private readonly TableFileCell cell;

        public SingleCellRow(object value)
        {
            this.cell = new TableFileCell()
            {
                Value = value
            };
        }

        public IEnumerator<IDataCell> GetEnumerator()
        {
            yield return this.cell;
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}