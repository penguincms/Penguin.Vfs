using Penguin.Extensions.String;
using Penguin.Vfs.Interfaces;
using System.Collections;
using System.Collections.Generic;

namespace Penguin.Vfs.FileTypes
{
    internal class TableFileRow : IDataRow
    {
        private readonly string Source;

        public TableFileRow(string source)
        {
            this.Source = source;
        }

        public IEnumerator<IDataCell> GetEnumerator()
        {
            foreach (string v in this.Source.SplitQuotedString())
            {
                yield return new TableFileCell
                {
                    Value = v
                };
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}