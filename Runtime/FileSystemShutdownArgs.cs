using System;
using UnityEngine;

namespace Baracuda.Serialization
{
    [Serializable]
    public struct FileSystemShutdownArgs
    {
        [Tooltip("When enabled, the file system shutdown is forced to complete synchronous.")]
        public bool forceSynchronousShutdown;

        public static FileSystemShutdownArgs SynchronousShutdown => new()
        {
            forceSynchronousShutdown = true
        };
    }
}