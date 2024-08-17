using Baracuda.Bedrock.Utilities;

namespace Baracuda.Serialization
{
    public readonly struct FileBuffer
    {
        public readonly string FilePath;
        public readonly string FileData;

        public FileBuffer(string path, string data)
        {
            FilePath = path;
            FileData = data;
        }

        public bool IsValid => FilePath.IsNotNullOrWhitespace() && FileData.IsNotNullOrWhitespace();
    }
}