using Penguin.Vfs.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

namespace Penguin.Vfs.Caches
{
    internal class CachedFileStream : Stream, IStream, IDisposable
    {
        private readonly PathPart _filePath;

        private readonly Stream FileStream;

        private long? length;

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length
        {
            get
            {
                if (!length.HasValue)
                {
                    length = FileStream.Length;
                }

                return length.Value;
            }
        }

        public override long Position
        {
            get =>
                //File.AppendAllText("bad.txt", $"REQ @{this.FileStream.Position}" + System.Environment.NewLine);
                FileStream.Position;

            set => FileStream.Position = value;
        }

        public CachedFileStream(PathPart _filePath, long? length = null)
        {
            this._filePath = _filePath;
            this.length = length;

            FileStream = File.OpenRead(this._filePath.Value);
        }

        public IEnumerable<string> EnumerateLines()
        {
            using StreamReader sr = new(this);

            while (sr.ReadLine() is string line)
            {
                yield return line;
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public Stream GetStream()
        {
            return this;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            count = (int)Math.Min(count, Length - FileStream.Position);

            if (count == 0)
            {
                return 0;
            }

            int remainingToRead = count;

            long endPos = FileStream.Position + count;

            long firstBlockOffset = FileStream.Position % DataCache.LEN_BLK;

            long blocksToLoad = (endPos / DataCache.LEN_BLK) - (FileStream.Position / DataCache.LEN_BLK) + 1;

            uint startBlock = (uint)(FileStream.Position / DataCache.LEN_BLK);

            for (int blockOffset = 0; blockOffset < blocksToLoad; blockOffset++)
            {
                long checkBlock = startBlock + blockOffset;

                long thisBlockOffset = blockOffset == 0 ? firstBlockOffset : 0;

                byte[] blockData = DataCache.ReadBlock(_filePath.Value, (uint)checkBlock, FileStream);

                int toRead = (int)Math.Min(blockData.Length - thisBlockOffset, remainingToRead);

                Array.Copy(blockData, thisBlockOffset, buffer, offset, toRead);

                offset += toRead;
                remainingToRead -= toRead;
            }

            _ = FileStream.Seek(endPos, SeekOrigin.Begin);

            //File.AppendAllText("bad.txt", $"REQ @{this.FileStream.Position} (byte[{buffer.Length}], {sOffset}, {count}); R = {offset}, <= \"{string.Join("", buffer.Select(b => b.ToString("X2")))}\"" + System.Environment.NewLine);

            return offset;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long l = FileStream.Seek(offset, origin);

            return l;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                FileStream.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}