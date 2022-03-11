using VirtualFileSystem.Interfaces;

namespace VirtualFileSystem
{
    public abstract class RegexFileSystemEntryHandler : IFileSystemEntryHandler
    {
        private readonly string Regex;
        public virtual System.Text.RegularExpressions.RegexOptions RegexOptions { get; set; } = System.Text.RegularExpressions.RegexOptions.IgnoreCase;

        public RegexFileSystemEntryHandler(string regex)
        {
            this.Regex = regex;
        }

        public abstract IFileSystemEntry Create(ResolveUriPackage resolveUriPackage);

        public bool IsMatch(ResolveUriPackage resolveUriPackage) => System.Text.RegularExpressions.Regex.IsMatch(resolveUriPackage.VirtualUri.FullName.Value, this.Regex, this.RegexOptions);
    }
}