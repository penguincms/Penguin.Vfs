using Penguin.Extensions.Strings;
using System.Collections;
using System.Collections.Generic;
using VirtualFileSystem.Interfaces;

namespace VirtualFileSystem.FileTypes
{
    internal class TableFileRow : IDataRow
    {
        private string Source;

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