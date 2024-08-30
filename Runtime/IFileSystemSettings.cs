using Baracuda.Bedrock.Types;

namespace Baracuda.Serialization
{
    public interface IFileSystemSettings
    {
        string RootFolder { get; set; }
        bool AppendVersionToRootFolder { get; set; }
        bool UseUnityVersion { get; set; }
        bool UseMajorVersion { get; set; }
        bool UseMinorVersion { get; set; }
        bool UsePatchVersion { get; set; }
        string Version { get; set; }
        string FileEnding { get; set; }
        Optional<string[]> EnforceFileEndings { get; set; }
        bool ForceSynchronous { get; set; }
        string DefaultProfileName { get; set; }
        Optional<uint> ProfileLimit { get; set; }
        Optional<SaveGameConverter> SaveGameConverter { get; set; }
        LoggingLevel LoggingLevel { get; set; }
        bool LogMissingFileExtensionWarning { get; set; }
        Optional<EncryptionProviderAsset> EncryptionAsset { get; set; }
        Optional<string> EncryptionKey { get; set; }
        Optional<FileOperationAsset> FileStorageProvider { get; set; }
        bool ForceSynchronousShutdown { get; set; }
    }
}