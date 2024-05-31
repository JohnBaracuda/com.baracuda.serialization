using System;

namespace Baracuda.Serialization
{
    [Serializable]
    internal abstract class SaveData
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
    internal sealed class SaveData<T> : SaveData
    {
        public T value;
    }
}