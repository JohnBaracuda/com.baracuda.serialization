using System.Threading.Tasks;

namespace Baracuda.Serialization
{
    public interface IEncryptionProvider : IAsyncEncryptionProvider
    {
        byte[] Encrypt(string content, string passPhrase);

        string Decrypt(byte[] data, string passPhrase);
    }

    public interface IAsyncEncryptionProvider
    {
        Task<byte[]> EncryptAsync(string content, string passPhrase);

        Task<string> DecryptAsync(byte[] data, string passPhrase);
    }
}