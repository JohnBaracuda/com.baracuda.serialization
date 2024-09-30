using System.Text;
using System.Threading.Tasks;
using Baracuda.Utility.Collections;

namespace Baracuda.Serialization
{
    public class DefaultEncryptionProvider : IEncryptionProvider
    {
        public Task<byte[]> EncryptAsync(string content, string passPhrase)
        {
            var result = content == null ? null : Encoding.UTF8.GetBytes(content);
            return Task.FromResult(result);
        }

        public Task<string> DecryptAsync(byte[] data, string passPhrase)
        {
            var result = data.IsNullOrEmpty() ? null : Encoding.UTF8.GetString(data);
            return Task.FromResult(result);
        }

        public byte[] Encrypt(string content, string passPhrase)
        {
            var result = content == null ? null : Encoding.UTF8.GetBytes(content);
            return result;
        }

        public string Decrypt(byte[] data, string passPhrase)
        {
            var result = data.IsNullOrEmpty() ? null : Encoding.UTF8.GetString(data);
            return result;
        }
    }
}