using System;
using System.IO;

namespace IMAP_Client.Model
{
#pragma warning disable CS0660, CS0661
    public class TaggedFile : IEquatable<TaggedFile>
#pragma warning restore CS0660, CS0661
    {
        public FileInfo Info { get; }
        public string Name => Info.Name;
        public string FullPath => Info.FullName;
        public bool Exists => Info.Exists;
        public TaggedFile(FileInfo Info)  => this.Info = Info;
        public TaggedFile(string path)  => this.Info = new(path);
        public override string ToString() => Info.Name;

        bool IEquatable<TaggedFile>.Equals(TaggedFile? other)
            => this == other;

        public static readonly TaggedFile Empty = new("No attachments");

        public static bool operator ==(TaggedFile? a, TaggedFile? b)
        {
            if (a is null) return b is null;
            else if (b is null) return false;

            return a.FullPath == b.FullPath;
        }
        public static bool operator !=(TaggedFile? a, TaggedFile? b)
            => !(a == b);
    }
}
