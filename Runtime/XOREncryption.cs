using System.Text;
using System.Threading.Tasks;
using Baracuda.Utility.Collections;
using Baracuda.Utility.Utilities;

namespace Baracuda.Serialization
{
    public class XOREncryption : IEncryptionProvider
    {
        public byte[] Encrypt(string plainText, string passPhrase)
        {
            if (plainText.IsNullOrEmpty())
            {
                return null;
            }

            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var keyBytes = Encoding.UTF8.GetBytes(passPhrase);
            var encrypted = new byte[plainBytes.Length];

            for (var i = 0; i < plainBytes.Length; i++)
            {
                encrypted[i] = (byte)(plainBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            return encrypted;
        }

        public string Decrypt(byte[] content, string passPhrase)
        {
            if (content.IsNullOrEmpty())
            {
                return null;
            }
            var keyBytes = Encoding.UTF8.GetBytes(passPhrase);
            var decryptedBytes = new byte[content.Length];

            for (var i = 0; i < content.Length; i++)
            {
                decryptedBytes[i] = (byte)(content[i] ^ keyBytes[i % keyBytes.Length]);
            }

            var decrypted = Encoding.UTF8.GetString(decryptedBytes);
            return decrypted;
        }

        public async Task<byte[]> EncryptAsync(string content, string passPhrase)
        {
            return await Task.Run(() => Encrypt(content, passPhrase));
        }

        public async Task<string> DecryptAsync(byte[] data, string passPhrase)
        {
            return await Task.Run(() => Decrypt(data, passPhrase));
        }
    }
}