using System;
using Baracuda.Utility.Types;

namespace Baracuda.Serialization
{
    public readonly struct FileOptions
    {
        public readonly Optional<bool> Encryption;
        public readonly string[] Tags;

        public FileOptions(Optional<bool> encryption, params string[] tags)
        {
            Encryption = encryption;
            Tags = tags ?? Array.Empty<string>();
        }

        public FileOptions(params string[] tags)
        {
            Encryption = default;
            Tags = tags ?? Array.Empty<string>();
        }

        public static implicit operator FileOptions(string tag)
        {
            return new FileOptions(default(Optional<bool>), tag);
        }
    }
}