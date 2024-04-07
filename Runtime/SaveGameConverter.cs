using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Baracuda.Serialization
{
    public abstract class SaveGameConverter : ScriptableObject
    {
        [SerializeField] private FileSystemArgs converterFileSystem;
        public FileSystemArgs FileSystemArgs => converterFileSystem;

        public abstract UniTask ConvertAsync(IFileStorage storage, ISaveProfile profile, ISaveProfile systemProfile);

        public abstract void Convert(IFileStorage storage, ISaveProfile profile, ISaveProfile systemProfile);
    }
}