using Penguin.Vfs.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace Penguin.Vfs.FileSystems.Local
{
    internal class FileStream : Stream, IStream
    {
        private readonly PathPart Path;

        private readonly Stream stream;
        public override bool CanRead => stream.CanRead;

        public override bool CanSeek => stream.CanSeek;

        public override bool CanWrite => stream.CanWrite;

        public override long Length => stream.Length;

        public override long Position
        {
            get =>
                //File.AppendAllText("good.txt", $"REQ @{this.stream.Position}" + System.Environment.NewLine);

                stream.Position;

            set => stream.Position = value;
        }

        public FileStream(PathPart path)
        {
            Path = path;
            stream = File.OpenRead(Path.WindowsValue);
        }

        public IEnumerable<string> EnumerateLines()
        {
            foreach (string line in File.ReadAllLines(Path.WindowsValue))
            {
                yield return line;
            }
        }

        public override void Flush()
        {
            stream.Flush();
        }

        public Stream GetStream()
        {
            return this;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int t = stream.Read(buffer, offset, count);

            //File.AppendAllText("good.txt", $"REQ @{this.stream.Position} (byte[{buffer.Length}], {offset}, {count}); R = {t}, <= \"{string.Join("", buffer.Select(b => b.ToString("X2")))}\"" + System.Environment.NewLine);

            return t;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
        }
    }
}