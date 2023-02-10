using System;
using System.Collections.Generic;
using System.Linq;

namespace Penguin.Vfs
{
    public struct PathPart : IEquatable<PathPart>
    {
        public IEnumerable<PathPart> Chunks => PathChunks.Select(s => new PathPart(s));

        public int Depth => Value.Trim('/').Count(c => c == '/') + 1;

        public PathPart Extension => new(System.IO.Path.GetExtension(WindowsValue));

        public PathPart FileNameWithoutExtension => new(System.IO.Path.GetFileNameWithoutExtension(WindowsValue));

        public PathPart FileName => new(System.IO.Path.GetFileName(WindowsValue));

        public bool HasValue => !string.IsNullOrWhiteSpace(Value);

        public int Length => HasValue ? Value.Length : 0;

        public PathPart Parent
        {
            get
            {
                string tParent = Value.Trim('/');

                return tParent.Contains('/') ? new PathPart(tParent[..tParent.LastIndexOf('/')]) : default;
            }
        }

        public IEnumerable<string> PathChunks
        {
            get
            {
                if (!HasValue)
                {
                    yield break;
                }

                string toSplit = Value;

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

                        if (s == 4)
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

        public string Value { get; }

        public string WindowsValue => Value.Replace("/", "\\");

        public PathPart(string path)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Value = path.Replace("\\", "/");
        }

        public PathPart(IEnumerable<PathPart> parts) : this(parts.Select(p => p.Value))
        {
        }

        public PathPart(IEnumerable<string> parts)
        {
            if (parts is null)
            {
                throw new ArgumentNullException(nameof(parts));
            }

            Value = string.Empty;

            foreach (string p in parts)
            {
                Value += '/' + p;
            }
        }

        public PathPart Append(string value)
        {
            return Value == "/" ? (new(value)) : (new(Value.TrimEnd('/') + "/" + value));
        }

        public PathPart Append(PathPart value)
        {
            return Append(value.Value);
        }

        public bool IsChildOf(PathPart other)
        {
            return StartsWith(other.Value + "/");
        }

        public PathPart MakeLocal(PathPart root)
        {
            return MakeLocal(root.Value);
        }

        public PathPart MakeLocal(string root)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                throw new System.ArgumentException($"'{nameof(root)}' cannot be null or whitespace.", nameof(root));
            }

            string path = Value;

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

        public PathPart Prepend(string value)
        {
            return new(value + Value);
        }

        public bool StartsWith(string v)
        {
            if (string.IsNullOrWhiteSpace(v))
            {
                throw new System.ArgumentException($"'{nameof(v)}' cannot be null or whitespace.", nameof(v));
            }

            if (v.Length > Value.Length)
            {
                return false;
            }

            for (int i = 0; i < v.Length; i++)
            {
                bool aSlash = Value[i] == '/';
                bool bSlash = v[i] is '/' or '\\';

                if (aSlash && bSlash)
                {
                    continue;
                }

                if (Value[i] != v[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            return HasValue ? Value : null;
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(PathPart left, PathPart right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PathPart left, PathPart right)
        {
            return !(left == right);
        }

        public bool Equals(PathPart other)
        {
            throw new NotImplementedException();
        }
    }
}