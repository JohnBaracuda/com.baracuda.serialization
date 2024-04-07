using System.Threading;
using System.Threading.Tasks;

namespace Baracuda.Serialization
{
    /// <summary>
    ///     Wrapper for simple native platform file system operations
    /// </summary>
    public interface IFileOperations
    {
        public void Initialize(FileSystemArgs args = new());

        public void Save();

        public Task WriteAllBytesAsync(string path, byte[] content, CancellationToken cancellationToken = new());

        public void WriteAllBytes(string path, byte[] content);

        public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = new());

        public byte[] ReadAllBytes(string path);

        public void DeleteDirectory(string path);

        public void CreateDirectory(string path);

        public void DeleteFile(string path);
    }
}