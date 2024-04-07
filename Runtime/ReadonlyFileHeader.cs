namespace Baracuda.Serialization
{
    public readonly ref struct ReadonlyFileHeader
    {
        public readonly string FileName;
        public readonly string FileType;
        public readonly FileGroup FileGroup;
        public readonly string LastModificationTimeStamp;
        public readonly bool Encrypted;
        public readonly string Version;
        public readonly string[] Tags;

        internal ReadonlyFileHeader(FileHeader fileHeader)
        {
            Version = fileHeader.Version;
            FileName = fileHeader.FileName;
            FileType = fileHeader.FileType;
            FileGroup = fileHeader.FileGroup;
            LastModificationTimeStamp = fileHeader.LastModificationTimeStamp;
            Encrypted = fileHeader.Encrypted;
            Tags = fileHeader.Tags;
        }
    }
}