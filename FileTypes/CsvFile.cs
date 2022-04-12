using Penguin.Vfs.Interfaces;
using System.Collections.Generic;

namespace Penguin.Vfs.FileTypes
{
    public class CsvFile : TableFile
    {
        public CsvFile(ResolveUriPackage resolveUriPackage) : base(resolveUriPackage)
        {
        }

        public override IEnumerator<IDataRow> GetEnumerator()
        {
            foreach (string line in base.EnumerateLines())
            {
                yield return new TableFileRow(line);
            }
        }
    }
}