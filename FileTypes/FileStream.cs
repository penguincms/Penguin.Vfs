using VirtualFileSystem.Interfaces;

namespace VirtualFileSystem.FileTypes
{
    public class FileStream : IFile
    {
        public ResolveUriPackage ResolutionPackage { get; set; }

        public IUri Uri => this.ResolutionPackage.VirtualUri;

        public FileStream(ResolveUriPackage resolutionPackage)
        {
            this.ResolutionPackage = resolutionPackage;
        }

        public override string ToString() => this.Uri.FullName.Value;
    }
}