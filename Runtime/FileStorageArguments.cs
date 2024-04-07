namespace Baracuda.Serialization
{
    public readonly ref struct FileStorageArguments
    {
        public readonly bool ForceSynchronous;
        public readonly string RootFolder;
        public readonly string EncryptionKey;
        public readonly LoggingLevel ExceptionLogging;
        public readonly IEncryptionProvider EncryptionProvider;
        public readonly IFileOperations FileOperations;

        public FileStorageArguments(
            string rootFolder,
            string encryptionKey,
            IEncryptionProvider encryptionProvider,
            LoggingLevel exceptionLogging,
            bool forceSynchronous,
            IFileOperations fileOperations)
        {
            RootFolder = rootFolder;
            EncryptionKey = encryptionKey;
            ExceptionLogging = exceptionLogging;
            EncryptionProvider = encryptionProvider;
            ForceSynchronous = forceSynchronous;
            FileOperations = fileOperations;
        }
    }
}