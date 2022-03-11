using VirtualFileSystem.Interfaces;

namespace VirtualFileSystem.FileTypes
{
    internal class TableFileCell : IDataCell
    {
        public object Value { get; set; }
    }
}