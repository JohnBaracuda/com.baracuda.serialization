using Baracuda.Utility.Types;

namespace Baracuda.Serialization
{
    public readonly struct StoreOptions
    {
        public readonly Optional<bool> Encryption;
        public readonly string[] Tags;

        public StoreOptions(Optional<bool> encryption, params string[] tags)
        {
            Encryption = encryption;
            Tags = tags;
        }

        public StoreOptions(params string[] tags)
        {
            Encryption = default;
            Tags = tags;
        }

        public static implicit operator StoreOptions(string tag)
        {
            return new StoreOptions(default(Optional<bool>), tag);
        }
    }
}