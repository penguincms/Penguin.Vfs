using Penguin.Vfs.Interfaces;

namespace Penguin.Vfs
{
    public abstract class RegexFileSystemEntryHandler : IFileSystemEntryHandler
    {
        private readonly string Regex;
        public virtual System.Text.RegularExpressions.RegexOptions RegexOptions { get; set; } = System.Text.RegularExpressions.RegexOptions.IgnoreCase;

        protected RegexFileSystemEntryHandler(string regex)
        {
            Regex = regex;
        }

        public abstract IFileSystemEntry Create(ResolveUriPackage resolveUriPackage);

        public bool IsMatch(ResolveUriPackage resolveUriPackage)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(resolveUriPackage.VirtualUri.FullName.Value, Regex, RegexOptions);
        }
    }
}