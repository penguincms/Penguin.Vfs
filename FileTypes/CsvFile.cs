using System.Collections.Generic;
using VirtualFileSystem.Interfaces;

namespace VirtualFileSystem.FileTypes
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