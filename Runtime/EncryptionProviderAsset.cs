using System.Threading.Tasks;
using UnityEngine;

namespace Baracuda.Serialization
{
    public abstract class EncryptionProviderAsset : ScriptableObject, IEncryptionProvider
    {
        public abstract byte[] Encrypt(string content, string passPhrase);

        public abstract string Decrypt(byte[] data, string passPhrase);

        public abstract Task<byte[]> EncryptAsync(string content, string passPhrase);

        public abstract Task<string> DecryptAsync(byte[] data, string passPhrase);
    }
}