using System.Collections.Generic;
using System.Linq;

namespace VirtualFileSystem
{
    public struct PathPart
    {
        private readonly string _path;

        public IEnumerable<PathPart> Chunks => this.PathChunks.Select(s => new PathPart(s));
        public int Depth => this.Value.Trim('/').Count(c => c == '/') + 1;

        public PathPart Extension => new(System.IO.Path.GetExtension(this.WindowsValue));

        public PathPart FileNameWithoutExtension => new(System.IO.Path.GetFileNameWithoutExtension(this.WindowsValue));

        public bool HasValue => !string.IsNullOrWhiteSpace(this._path);

        public int Length => this.HasValue ? this.Value.Length : 0;

        public PathPart Parent
        {
            get
            {
                string tParent = this.Value.Trim('/');

                if (tParent.Contains('/'))
                {
                    return new PathPart(tParent[..tParent.LastIndexOf('/')]);
                }

                return default;
            }
        }

        public IEnumerable<string> PathChunks
        {
            get
            {
                if (!this.HasValue)
                {
                    yield break;
                }

                string toSplit = this.Value;

                if (toSplit.StartsWith("//"))
                {
                    string root = string.Empty;
                    int s = 0;
                    foreach (char c in toSplit)
                    {
                        root += c;

                        if (c == '/')
                        {
                            s++;
                        }

                        if (s == 3)
                        {
                            break;
                        }
                    }

                    yield return root;

                    toSplit = toSplit[root.Length..];
                }

                foreach (string s in toSplit.Split('/'))
                {
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        yield return s;
                    }
                }
            }
        }

        public string Value => this._path;

        public string WindowsValue => this._path.Replace("/", "\\");

        public PathPart(string path)
        {
            this._path = path.Replace("\\", "/");
        }

        public PathPart(IEnumerable<PathPart> parts) : this(parts.Select(p => p.Value))
        {
        }

        public PathPart(IEnumerable<string> parts)
        {
            this._path = string.Empty;

            foreach (string p in parts)
            {
                this._path += '/' + p;
            }
        }

        public PathPart Append(string value)
        {
            if (this.Value == "/")
            {
                return new(value);
            }

            return new(this.Value.TrimEnd('/') + "/" + value);
        }

        public PathPart Append(PathPart value) => this.Append(value.Value);

        public bool IsChildOf(PathPart other) => this.StartsWith(other.Value + "/");

        public PathPart MakeLocal(PathPart root) => this.MakeLocal(root.Value);

        public PathPart MakeLocal(string root)
        {
            string path = this.Value;

            if (path.StartsWith(root))
            {
                path = path[root.Length..];
            }

            path = path.TrimStart('/');

            if (path.Length == 0)
            {
                path = "/";
            }

            return new PathPart(path);
        }

        public PathPart Prepend(string value) => new(value + this.Value);

        public bool StartsWith(string v)
        {
            if(v.Length > this._path.Length)
            {
                return false;
            }

            for(int i = 0; i < v.Length; i++)
            {
                bool aSlash = this._path[i] == '/';
                bool bSlash = v[i] is '/' or '\\';
                
                if(aSlash && bSlash)
                {
                    continue;
                }

                if(this._path[i] != v[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString() => this.HasValue ? this.Value : null;
    }
}