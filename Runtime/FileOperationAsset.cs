using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Baracuda.Serialization
{
    public abstract class FileOperationAsset : ScriptableObject, IFileOperations
    {
        public abstract void Initialize(FileSystemArgs args = new());

        public abstract void Save();

        public abstract Task WriteAllBytesAsync(string path, byte[] content,
            CancellationToken cancellationToken = new());

        public abstract void WriteAllBytes(string path, byte[] content);

        public abstract Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = new());

        public abstract byte[] ReadAllBytes(string path);

        public abstract void DeleteDirectory(string path);

        public abstract void CreateDirectory(string path);

        public abstract void DeleteFile(string path);
    }
}