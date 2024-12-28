using System;

namespace Baracuda.Serialization
{
    [Serializable]
    internal abstract class DataStorage
    {
        public string fileName;
        public string lastSaveTimeStamp;
        public string createdTimeStamp;
        public string qualifiedType;
        public string fileSystemVersion;
        public string applicationVersion;
        public string[] tags;
    }

    [Serializable]
    internal sealed class DataStorage<T> : DataStorage
    {
        public T value;
    }
}