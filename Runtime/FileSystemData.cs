using System;

namespace Baracuda.Serialization
{
    [Serializable]
    internal struct FileSystemData
    {
        public string activeProfileFilePath;
        public int nextProfileIndex;
    }
}