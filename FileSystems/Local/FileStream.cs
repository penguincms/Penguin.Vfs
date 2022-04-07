using System.Collections.Generic;
using System.IO;
using Penguin.Vfs.Interfaces;

namespace Penguin.Vfs.FileSystems.Local
{
    internal class FileStream : Stream, IStream
    {
        private readonly PathPart Path;

        private readonly Stream stream;
        public override bool CanRead => this.stream.CanRead;

        public override bool CanSeek => this.stream.CanSeek;

        public override bool CanWrite => this.stream.CanWrite;

        public override long Length => this.stream.Length;

        public override long Position
        {
            get =>
                //File.AppendAllText("good.txt", $"REQ @{this.stream.Position}" + System.Environment.NewLine);

                this.stream.Position;

            set => this.stream.Position = value;
        }

        public FileStream(PathPart path)
        {
            this.Path = path;
            this.stream = File.OpenRead(this.Path.WindowsValue);
        }

        public IEnumerable<string> EnumerateLines()
        {
            foreach (string line in File.ReadAllLines(this.Path.WindowsValue))
            {
                yield return line;
            }
        }

        public override void Flush() => this.stream.Flush();

        public Stream GetStream() => this;

        public override int Read(byte[] buffer, int offset, int count)
        {
            int t = this.stream.Read(buffer, offset, count);

            //File.AppendAllText("good.txt", $"REQ @{this.stream.Position} (byte[{buffer.Length}], {offset}, {count}); R = {t}, <= \"{string.Join("", buffer.Select(b => b.ToString("X2")))}\"" + System.Environment.NewLine);

            return t;
        }

        public override long Seek(long offset, SeekOrigin origin) => this.stream.Seek(offset, origin);

        public override void SetLength(long value) => this.stream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => this.stream.Write(buffer, offset, count);
    }
}