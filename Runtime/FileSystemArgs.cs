using System;
using Baracuda.Utilities.Types;
using UnityEngine;

namespace Baracuda.Serialization
{
    [Serializable]
    public struct FileSystemArgs : IFileSystemArgs
    {
        #region Fields

        [Header("General")]
        [Tooltip("The root folder fot the files system (relative to the application value path).")]
        [SerializeField] private string rootFolder;

        [Tooltip("When enabled, root folder are versioned.")]
        [SerializeField] private bool versionRootFolder;

        [Tooltip("When enabled, the unity version will be used instead of the version string defined below")]
        [SerializeField] private bool useUnityVersion;

        [Tooltip("When enabled, the major unity version is used for the version string")]
        [SerializeField] private bool useMajorVersion;

        [Tooltip("When enabled, the minor unity version is used for the version string")]
        [SerializeField] private bool useMinorVersion;

        [Tooltip("When enabled, the patch unity version is used for the version string")]
        [SerializeField] private bool usePatchVersion;

        [Tooltip("The root folder fot the files system (relative to the application value path).")]
        [SerializeField] private string version;

        [Header("File Endings")]
        [Tooltip("Custom file ending that is used for files without specifically set file endings.")]
        [SerializeField] private string fileEnding;

        [Tooltip("Array to limit the use of file endings. If empty, every file ending can be used.")]
        [SerializeField] private Optional<string[]> enforceFileEndings;

        [Tooltip("When enabled, the initialization process is forced to execute synchronous.")]
        [SerializeField] private bool forceSynchronous;

        [Header("Profiles")]
        [Tooltip("The default name used for created profiles.")]
        [SerializeField] private string defaultProfileName;

        [Tooltip("Limits the amount of available profiles. 0 to set no limit.")]
        [SerializeField] private Optional<uint> profileLimit;

        [Header("Conversion")]
        [Tooltip("Save game converter objects can use custom logic to convert old saves to be file system compatible.")]
        [SerializeField] private Optional<SaveGameConverter> saveGameConverter;

        [Header("Logging")]
        [Tooltip("When enabled, exceptions are logged to the console.")]
        [SerializeField] private LoggingLevel exceptions;

        [Tooltip("When enabled, a warning is logged when a file name is passed without a specified file extension.")]
        [SerializeField] private bool logMissingFileExtensionWarning;

        [Header("Encryption")]
        [Tooltip("Custom encryption provider.")]
        [SerializeField] private Optional<EncryptionProviderAsset> encryptionAsset;

        [Tooltip("Custom encryption pass phrase. If none is provided a default value is used.")]
        [SerializeField] private Optional<string> encryptionKey;

        [Tooltip("Custom platform file storage providers")]
        [SerializeField] private Optional<FileOperationAsset> fileStorageProvider;

        [Header("Shutdown")]
        [SerializeField] private bool forceSynchronousShutdown;

        #endregion


        #region Properties

        public string RootFolder
        {
            get => rootFolder;
            set => rootFolder = value;
        }

        public bool AppendVersionToRootFolder
        {
            get => versionRootFolder;
            set => versionRootFolder = value;
        }

        public bool UseUnityVersion
        {
            get => useUnityVersion;
            set => useUnityVersion = value;
        }

        public bool UseMajorVersion
        {
            get => useMajorVersion;
            set => useMajorVersion = value;
        }

        public bool UseMinorVersion
        {
            get => useMinorVersion;
            set => useMinorVersion = value;
        }

        public bool UsePatchVersion
        {
            get => usePatchVersion;
            set => usePatchVersion = value;
        }

        public string Version
        {
            get => version;
            set => version = value;
        }

        public string FileEnding
        {
            get => fileEnding;
            set => fileEnding = value;
        }

        public Optional<string[]> EnforceFileEndings
        {
            get => enforceFileEndings;
            set => enforceFileEndings = value;
        }

        public bool ForceSynchronous
        {
            get => forceSynchronous;
            set => forceSynchronous = value;
        }

        public string DefaultProfileName
        {
            get => defaultProfileName;
            set => defaultProfileName = value;
        }

        public Optional<uint> ProfileLimit
        {
            get => profileLimit;
            set => profileLimit = value;
        }

        public Optional<SaveGameConverter> SaveGameConverter
        {
            get => saveGameConverter;
            set => saveGameConverter = value;
        }

        public LoggingLevel LoggingLevel
        {
            get => exceptions;
            set => exceptions = value;
        }

        public bool LogMissingFileExtensionWarning
        {
            get => logMissingFileExtensionWarning;
            set => logMissingFileExtensionWarning = value;
        }

        public Optional<EncryptionProviderAsset> EncryptionAsset
        {
            get => encryptionAsset;
            set => encryptionAsset = value;
        }

        public Optional<string> EncryptionKey
        {
            get => encryptionKey;
            set => encryptionKey = value;
        }

        public Optional<FileOperationAsset> FileStorageProvider
        {
            get => fileStorageProvider;
            set => fileStorageProvider = value;
        }

        public bool ForceSynchronousShutdown
        {
            get => forceSynchronousShutdown;
            set => forceSynchronousShutdown = value;
        }

        #endregion
    }
}