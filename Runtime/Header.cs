using System;

namespace Baracuda.Serialization
{
    [Serializable]
    internal struct Header : IEquatable<Header>
    {
        public string fileName;
        public string qualifiedTypeName;

        public bool Equals(Header other)
        {
            return fileName == other.fileName && qualifiedTypeName == other.qualifiedTypeName;
        }

        public override bool Equals(object obj)
        {
            return obj is Header other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(fileName, qualifiedTypeName);
        }
    }
}