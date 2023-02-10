using Penguin.Vfs.Interfaces;
using System.Collections;
using System.Collections.Generic;

namespace Penguin.Vfs.FileTypes
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
            cell = new TableFileCell()
            {
                Value = value
            };
        }

        public IEnumerator<IDataCell> GetEnumerator()
        {
            yield return cell;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}