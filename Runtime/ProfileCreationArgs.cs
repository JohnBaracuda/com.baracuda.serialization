using System;

namespace Baracuda.Serialization
{
    [Serializable]
    public struct ProfileCreationArgs
    {
        public string name;
        public bool activate;
    }
}