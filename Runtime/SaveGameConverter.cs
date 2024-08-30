using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Baracuda.Serialization
{
    public abstract class SaveGameConverter : ScriptableObject
    {
        [SerializeField] private FileSystemSettings converterFileSystem;
        public FileSystemSettings FileSystemSettings => converterFileSystem;

        public abstract UniTask ConvertAsync(IFileStorage storage, ISaveProfile profile, ISaveProfile systemProfile);

        public abstract void Convert(IFileStorage storage, ISaveProfile profile, ISaveProfile systemProfile);
    }
}