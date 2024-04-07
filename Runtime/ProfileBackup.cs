using System;

namespace Baracuda.Serialization
{
    [Serializable]
    public struct ProfileBackup
    {
        public string displayName;
        public string profilePath;
        public string backupPath;
        public string backupCreatedTimeStamp;
    }
}