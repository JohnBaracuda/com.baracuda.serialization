using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Baracuda.Serialization
{
    public class MonoFileOperations : IFileOperations
    {
        public void Initialize(FileSystemSettings settings = new())
        {
        }

        public void Save()
        {
        }

        public async Task WriteAllBytesAsync(string path, byte[] content, CancellationToken cancellationToken = new())
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllBytesAsync(path, content, cancellationToken);
        }

        public void WriteAllBytes(string path, byte[] content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, content);
        }

        public async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = new())
        {
            if (File.Exists(path) is false)
            {
                return await Task.FromResult<byte[]>(null);
            }
            return await File.ReadAllBytesAsync(path, cancellationToken);
        }

        public byte[] ReadAllBytes(string path)
        {
            if (File.Exists(path) is false)
            {
                return null;
            }
            return File.ReadAllBytes(path);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path);
            }
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public override string ToString()
        {
            return nameof(MonoFileOperations);
        }
    }
}