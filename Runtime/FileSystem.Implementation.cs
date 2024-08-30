using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Baracuda.Bedrock.Collections;
using Baracuda.Bedrock.Pools;
using Baracuda.Bedrock.Types;
using Baracuda.Bedrock.Utilities;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Baracuda.Serialization
{
    public static partial class FileSystem
    {
        #region Private Properties & Fields

        public static string RootFolder { get; private set; }

        internal static IFileStorage Storage { get; private set; }
        internal static readonly LogCategory Log = "File System";

        private static SaveProfile activeProfile;
        private static SaveProfile sharedProfile;

        private static FileValidator validator;

        private static ProfilePathData profilePathData;
        private static Dictionary<string, ISaveProfile> profileCache;

        private const string DefaultEncryptionKey = "QplEVJveOQ";

        private const string FileSystemDataSave = "storage.sav";
        private const string ProfileHeader = "_slot.sav";

        private const string SharedProfileName = "Shared";
        private const string SharedProfileFolder = "_shared";
        private const string SharedProfileFileName = "_shared.sav";
        private const string ProfilePathsKey = "profiles.sav";

        private static string defaultProfileName;

        private const int MaxProfileNameLength = 64;
        private static uint profileLimit;

        private static string version = string.Empty;
        private static FileSystemData fileSystemData;

        private static TaskCompletionSource<object> initializationTaskCompletionSource = new();

        #endregion


        #region Platform Storage Provider

        private static IFileStorage CreateFileStorage(IFileSystemSettings settings)
        {
            Debug.Log("File System", "Creating File Storage");
            var rootFolderBuilder = StringBuilderPool.Get();
            rootFolderBuilder.Append(settings.RootFolder.IsNotNullOrWhitespace() ? settings.RootFolder : string.Empty);
            if (settings.AppendVersionToRootFolder)
            {
                rootFolderBuilder.Append('_');
                rootFolderBuilder.Append(settings.UseUnityVersion ? GetSanitizedUnityVersionString(settings) : settings.Version);
            }

            RootFolder = StringBuilderPool.BuildAndRelease(rootFolderBuilder);

            var encryptionProvider = settings.EncryptionAsset.ValueOrDefault();
            var encryptionKey = settings.EncryptionKey.TryGetValue(out var key) && key.IsNotNullOrWhitespace()
                ? key
                : DefaultEncryptionKey;

            var fileOperations = settings.FileStorageProvider.ValueOrDefault() as IFileOperations ??
                                 new MonoFileOperations();

            var fileStorageArguments = new FileStorageArguments(
                RootFolder,
                encryptionKey,
                encryptionProvider,
                settings.LoggingLevel,
                settings.ForceSynchronous,
                fileOperations
            );

            var storage = new FileStorage();
            storage.Initialize(fileStorageArguments);

            Debug.Log("File System", $"Created {fileOperations} File Storage!");
            return storage;
        }

        private static string GetSanitizedUnityVersionString(in IFileSystemSettings settings)
        {
            try
            {
                var versionString = Application.version;
                if (versionString.IsNullOrWhitespace())
                {
                    return string.Empty;
                }

                var segments = versionString.Split('.');

                Assert.IsTrue(segments.Length == 3, "segments.Length == 3");

                var major = segments[0];
                var minor = segments[1];
                var patch = segments[2];

                Assert.IsTrue(major.IsNotNullOrWhitespace(), "major.IsNotNullOrWhitespace()");
                Assert.IsTrue(minor.IsNotNullOrWhitespace(), "minor.IsNotNullOrWhitespace()");
                Assert.IsTrue(patch.IsNotNullOrWhitespace(), "patch.IsNotNullOrWhitespace()");

                var builder = new StringBuilder();
                builder.Append(settings.UseMajorVersion ? major : "x");
                builder.Append('.');
                builder.Append(settings.UseMinorVersion ? minor : "x");
                builder.Append('.');
                builder.Append(settings.UsePatchVersion ? patch : "x");
                return builder.ToString();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.Log("File System", "Exception while parsing unity version to format (Major.Minor.Patch) using raw version string instead!");
                return Application.version;
            }
        }

        #endregion


        #region Initialization

        private static async UniTask InitializeAsyncInternal(IFileSystemSettings settings)
        {
            try
            {
                settings ??= CreateDefaultSettings();
                if (State != FileSystemState.Uninitialized)
                {
                    return;
                }
                if (settings.ForceSynchronous)
                {
                    InitializeInternal(settings);
                    return;
                }

                Debug.Log(Log, "Initialization Started", Verbosity.Message);
                State = FileSystemState.Initializing;

                try
                {
                    InitializationStarted?.Invoke();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }

                profileLimit = settings.ProfileLimit.ValueOrDefault() > 0 ? settings.ProfileLimit : uint.MaxValue;
                version = settings.UseUnityVersion ? GetSanitizedUnityVersionString(settings) : settings.Version ?? string.Empty;
                validator = new FileValidator(settings);
                Storage = CreateFileStorage(settings);
                defaultProfileName = settings.DefaultProfileName;

                var sharedProfilePath = Path.Combine(SharedProfileFolder, SharedProfileFileName);
                var sharedProfileData = await Storage.LoadAsync<SaveProfile>(sharedProfilePath);
                sharedProfile = sharedProfileData.IsValid ? sharedProfileData.Read() : CreateSharedProfile();

                static SaveProfile CreateSharedProfile()
                {
                    return new SaveProfile(SharedProfileName, SharedProfileFolder, SharedProfileFileName);
                }

                await sharedProfile.LoadAsync();

                var activeProfilePath = sharedProfile.TryLoadFile(FileSystemDataSave, out fileSystemData)
                    ? fileSystemData.activeProfileFilePath
                    : string.Empty;

                sharedProfile.TryLoadFile(ProfilePathsKey, out profilePathData);
                profilePathData ??= new ProfilePathData();
                var profilePaths = profilePathData.Paths;
                var profileData = await Storage.LoadAsync<SaveProfile>(activeProfilePath);
                var profile = profileData.IsValid ? profileData.Read() : CreateDefaultProfile(profilePaths);

                static SaveProfile CreateDefaultProfile(ICollection paths)
                {
                    var folderName = $"{defaultProfileName}{paths.Count.ToString()}";
                    return new SaveProfile(folderName, folderName, ProfileHeader);
                }

                profileCache = new Dictionary<string, ISaveProfile>(profilePaths.Count);
                foreach (var profilePath in profilePaths)
                {
                    if (profileCache.ContainsKey(profilePath))
                    {
                        continue;
                    }

                    var fileData = await Storage.LoadAsync<SaveProfile>(profilePath);
                    if (fileData.IsValid)
                    {
                        profileCache.AddUnique(profilePath, fileData.Read());
                    }
                }

                await UpdateActiveProfileAsyncInternal(profile);

                State = FileSystemState.Initialized;

                var converter = settings.SaveGameConverter.ValueOrDefault();
                if (converter != null)
                {
                    var topLevelStorageProvider = CreateFileStorage(converter.FileSystemSettings);
                    await converter.ConvertAsync(topLevelStorageProvider, profile, sharedProfile);
                    profile.Save();
                }

                sharedProfile.Save();

                initializationTaskCompletionSource.TrySetResult(null);
                Debug.Log(Log, "Initialization Completed", Verbosity.Message);
            }
            catch (Exception exception)
            {
                Debug.LogException(Log, new Exception("Error during file system initialization. Shutdown", exception));
                await ShutdownAsync();
                return;
            }

            InitializationCompleted?.InvokeCritical();
        }

        private static void InitializeInternal(IFileSystemSettings settings = null)
        {
            try
            {
                settings ??= CreateDefaultSettings();
                if (State != FileSystemState.Uninitialized)
                {
                    return;
                }

                Debug.Log(Log, "Initialization Started", Verbosity.Message);
                State = FileSystemState.Initializing;

                try
                {
                    InitializationStarted?.Invoke();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }

                profileLimit = settings.ProfileLimit.ValueOrDefault() > 0 ? settings.ProfileLimit : uint.MaxValue;
                version = settings.UseUnityVersion ? GetSanitizedUnityVersionString(settings) : settings.Version ?? string.Empty;
                validator = new FileValidator(settings);
                Storage = CreateFileStorage(settings);
                defaultProfileName = settings.DefaultProfileName;

                var sharedProfilePath = Path.Combine(SharedProfileFolder, SharedProfileFileName);
                var sharedProfileData = Storage.Load<SaveProfile>(sharedProfilePath);
                sharedProfile = sharedProfileData.IsValid
                    ? sharedProfileData.Read()
                    : new SaveProfile(SharedProfileName, SharedProfileFolder, SharedProfileFileName);

                sharedProfile.Load();

                var activeProfilePath = sharedProfile.TryLoadFile(FileSystemDataSave, out fileSystemData)
                    ? fileSystemData.activeProfileFilePath
                    : string.Empty;

                if (!sharedProfile.TryLoadFile(ProfilePathsKey, out profilePathData))
                {
                    profilePathData = new ProfilePathData();
                }
                var profilePaths = profilePathData.Paths;

                var profileData = Storage.Load<SaveProfile>(activeProfilePath);
                var profile = profileData.IsValid ? profileData.Read() : CreateDefaultProfile(profilePaths);

                static SaveProfile CreateDefaultProfile(ICollection paths)
                {
                    var folderName = $"{defaultProfileName}{paths.Count.ToString()}";
                    return new SaveProfile(folderName, folderName, ProfileHeader);
                }

                profileCache = new Dictionary<string, ISaveProfile>(profilePaths.Count);
                foreach (var profilePath in profilePaths)
                {
                    if (profileCache.ContainsKey(profilePath))
                    {
                        continue;
                    }
                    var fileData = Storage.Load<SaveProfile>(profilePath);
                    if (fileData.IsValid)
                    {
                        profileCache.AddUnique(profilePath, fileData.Read());
                    }
                }

                UpdateActiveProfileInternal(profile);

                State = FileSystemState.Initialized;

                var converter = settings.SaveGameConverter.ValueOrDefault();
                if (converter != null)
                {
                    var converterArgs = converter.FileSystemSettings;
                    converterArgs.ForceSynchronous = true;
                    var topLevelStorageProvider = CreateFileStorage(converterArgs);
                    converter.Convert(topLevelStorageProvider, profile, sharedProfile);
                    profile.Save();
                }

                sharedProfile.Save();

                initializationTaskCompletionSource.TrySetResult(null);
                Debug.Log(Log, "Initialization Completed", Verbosity.Message);
            }
            catch (Exception exception)
            {
                Debug.LogException(Log, new Exception("Error during file system initialization. Shutdown", exception));
                Shutdown();
                return;
            }

            InitializationCompleted?.InvokeCritical();
        }

        private static FileSystemSettings CreateDefaultSettings()
        {
            Debug.LogWarning("File System", "Creating default settings! It is recommended to initialize the game with custom settings!\n" +
                                            "In the Editor: Set custom settings in ProjectSettings/Baracuda/File System\n" +
                                            "In runtime builds: Set custom settings by manually calling FileSystem.Initialize(args)");
            return new FileSystemSettings
            {
                RootFolder = "SaveGame",
                AppendVersionToRootFolder = true,
#if DEBUG
                UseUnityVersion = true,
                UseMajorVersion = true,
                UseMinorVersion = true,
                UsePatchVersion = false,
#endif
                Version = "1.0.0",
                FileEnding = ".sav",
                EnforceFileEndings = default,
                DefaultProfileName = "Slot",
                LoggingLevel = LoggingLevel.Message,
                LogMissingFileExtensionWarning = false,
                EncryptionAsset = default,
                EncryptionKey = new Optional<string>("kc48sd9A87uddWS11CS133KD83", false),
                FileStorageProvider = default,
                ForceSynchronous = true
            };
        }

        #endregion


        #region Shutdown

        private static async UniTask ShutdownAsyncInternal(FileSystemShutdownArgs args)
        {
            if (State != FileSystemState.Initialized)
            {
                return;
            }

            State = FileSystemState.Shutdown;
            Debug.Log(Log, "Shutdown Started", Verbosity.Message);
            try
            {
                ShutdownStarted?.Invoke();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            await Storage.ShutdownAsync(args);
            activeProfile.Unload();
            sharedProfile.Unload();
            Storage = null;
            RootFolder = null;
            profileCache = null;
            profilePathData = null;
            validator = null;
            defaultProfileName = null;
            sharedProfile = null;
            activeProfile = null;
            version = string.Empty;
            fileSystemData = default;
            initializationTaskCompletionSource.TrySetCanceled();
            initializationTaskCompletionSource = new TaskCompletionSource<object>();
            State = FileSystemState.Uninitialized;
            Debug.Log(Log, "Shutdown Completed", Verbosity.Message);
            ShutdownCompleted?.InvokeCritical();
        }

        private static void ShutdownInternal(in FileSystemShutdownArgs args)
        {
            if (State != FileSystemState.Initialized)
            {
                return;
            }

            State = FileSystemState.Shutdown;
            Debug.Log(Log, "Shutdown Started", Verbosity.Message);
            try
            {
                ShutdownStarted?.InvokeCritical();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            Storage.Shutdown(args);
            activeProfile.Unload();
            sharedProfile.Unload();
            Storage = null;
            RootFolder = null;
            profilePathData = null;
            profileCache = null;
            validator = null;
            defaultProfileName = null;
            sharedProfile = null;
            activeProfile = null;
            version = string.Empty;
            fileSystemData = default;
            initializationTaskCompletionSource.TrySetCanceled();
            initializationTaskCompletionSource = new TaskCompletionSource<object>();
            State = FileSystemState.Uninitialized;
            Debug.Log(Log, "Shutdown Completed", Verbosity.Message);
            ShutdownCompleted?.InvokeCritical();
        }

        #endregion


        #region Profile Creation

        private static async UniTask<ProfileCreationResult> CreateProfileAsyncInternal(ProfileCreationArgs args)
        {
            if (State != FileSystemState.Initialized)
            {
                throw new FileSystemNotInitializedException(nameof(CreateProfileAsync));
            }

            if (profilePathData.Paths.Count >= profileLimit)
            {
                return ProfileCreationResult.ProfileLimitReached;
            }

            if (Regex.IsMatch(args.name ?? string.Empty, $"{defaultProfileName}\\d*$"))
            {
                return ProfileCreationResult.SystemReservedName;
            }

            var profileName = args.name.IsNotNullOrWhitespace()
                ? args.name!
                : CreateFallbackName();

            string CreateFallbackName()
            {
                var result = defaultProfileName + ++fileSystemData.nextProfileIndex;
                sharedProfile.StoreFile(FileSystemDataSave, fileSystemData);
                return result;
            }

            if (profileName.Length > MaxProfileNameLength)
            {
                return ProfileCreationResult.NameToLong;
            }
            if (!Validator.IsValidProfileName(profileName))
            {
                return ProfileCreationResult.NameInvalid;
            }

            var fileSystemName = profileName.Replace(' ', '_');
            var profilePathCandidate = Path.Combine(fileSystemName, ProfileHeader);
            if (profilePathData.Paths.Contains(profilePathCandidate))
            {
                return ProfileCreationResult.NameNotAvailable;
            }

            var profile = new SaveProfile(profileName, fileSystemName, ProfileHeader);

            profilePathData.Paths.AddUnique(profile.Info.ProfileDataPath);

            if (args.activate)
            {
                await UpdateActiveProfileAsyncInternal(profile);
            }

            profile.Save();
            sharedProfile.Save();
            ProfileCreated?.Invoke(profile, args);
            return new ProfileCreationResult(profile, ProfileCreationStatus.Success);
        }

        private static ProfileCreationResult CreateProfileInternal(ProfileCreationArgs args)
        {
            if (State != FileSystemState.Initialized)
            {
                throw new FileSystemNotInitializedException(nameof(CreateProfileAsync));
            }

            if (profilePathData.Paths.Count >= profileLimit)
            {
                return ProfileCreationResult.ProfileLimitReached;
            }

            if (Regex.IsMatch(args.name ?? string.Empty, $"{defaultProfileName}\\d*$"))
            {
                return ProfileCreationResult.SystemReservedName;
            }

            var profileName = args.name.IsNotNullOrWhitespace()
                ? args.name!
                : CreateFallbackName();

            string CreateFallbackName()
            {
                var result = defaultProfileName + ++fileSystemData.nextProfileIndex;
                sharedProfile.SaveFile(FileSystemDataSave, fileSystemData);
                return result;
            }

            if (profileName.Length > MaxProfileNameLength)
            {
                return ProfileCreationResult.NameToLong;
            }
            if (!Validator.IsValidProfileName(profileName))
            {
                return ProfileCreationResult.NameInvalid;
            }

            var fileSystemName = profileName.Replace(' ', '_');
            var profilePathCandidate = Path.Combine(fileSystemName, ProfileHeader);
            if (profilePathData.Paths.Contains(profilePathCandidate))
            {
                return ProfileCreationResult.NameNotAvailable;
            }

            var profile = new SaveProfile(profileName, fileSystemName, ProfileHeader);

            profilePathData.Paths.AddUnique(profile.Info.ProfileDataPath);

            if (args.activate)
            {
                UpdateActiveProfileInternal(profile);
            }

            profile.Save();
            ProfileCreated?.Invoke(profile, args);
            return new ProfileCreationResult(profile, ProfileCreationStatus.Success);
        }

        #endregion


        #region Profile Switching

        private static async UniTask<bool> UpdateActiveProfileAsyncInternal(SaveProfile profile)
        {
            if (profile == null)
            {
                return false;
            }
            if (profile == activeProfile)
            {
                return false;
            }
            if (activeProfile is { IsLoaded: true })
            {
                activeProfile.Unload();
            }
            Debug.Log(Log, $"Switch active profile from: {activeProfile.ToNullString()} to {profile}");
            activeProfile = profile;
            await activeProfile.LoadAsync();
            profileCache.AddOrUpdate(activeProfile.Info.ProfileDataPath, activeProfile);
            fileSystemData.activeProfileFilePath = profile.Info.ProfileDataPath;
            sharedProfile.StoreFile(FileSystemDataSave, fileSystemData);
            if (IsInitialized)
            {
                ProfileChanged?.Invoke(activeProfile);
            }
            sharedProfile.Save();
            return true;
        }

        private static bool UpdateActiveProfileInternal(SaveProfile profile)
        {
            if (profile == null)
            {
                return false;
            }
            if (profile == activeProfile)
            {
                return false;
            }
            if (activeProfile is { IsLoaded: true })
            {
                activeProfile.Unload();
            }
            Debug.Log(Log, $"Switch active profile from: {activeProfile.ToNullString()} to {profile}");
            activeProfile = profile;
            activeProfile.Load();
            profileCache.AddOrUpdate(activeProfile.Info.ProfileDataPath, activeProfile);
            fileSystemData.activeProfileFilePath = profile.Info.ProfileDataPath;
            sharedProfile.StoreFile(FileSystemDataSave, fileSystemData);
            if (IsInitialized)
            {
                ProfileChanged?.Invoke(activeProfile);
            }
            sharedProfile.Save();
            return true;
        }

        #endregion


        #region Profile Deletion (Profile)

        private static async UniTask DeleteProfileAsyncInternal(SaveProfile profile)
        {
            if (profile == null)
            {
                return;
            }
            if (profile == activeProfile)
            {
                return;
            }

            ProfileDeleted?.Invoke(profile);

            profileCache.Remove(profile.Info.ProfileDataPath);
            profilePathData.Paths.Remove(profile.Info.ProfileDataPath);

            if (profile.IsLoaded)
            {
                profile.Unload();
            }

            foreach (var header in profile.Info.FileHeaders)
            {
                var filePath = Path.Combine(profile.FolderName, header.fileName);
                await Storage.DeleteAsync(filePath);
            }

            await Storage.DeleteAsync(profile.Info.ProfileDataPath);
            await Storage.DeleteFolderAsync(profile.FolderName);
        }

        private static async UniTask DeleteProfileAsyncInternal(string profileName)
        {
            var profile = GetProfileByName(profileName);
            await DeleteProfileAsyncInternal(profile as SaveProfile);
        }

        private static void DeleteProfileInternal(SaveProfile profile)
        {
            if (profile == null)
            {
                return;
            }
            if (profile == activeProfile)
            {
                Debug.Log(Log, $"Cannot delete active profile {profile.DisplayName}");
                return;
            }

            ProfileDeleted?.Invoke(profile);

            profileCache.Remove(profile.Info.ProfileDataPath);
            profilePathData.Paths.Remove(profile.Info.ProfileDataPath);

            if (profile.IsLoaded)
            {
                profile.Unload();
            }

            var profileInfo = profile.Info;
            foreach (var header in profileInfo.FileHeaders)
            {
                var filePath = Path.Combine(profile.FolderName, header.fileName);
                Storage.Delete(filePath);
            }

            Storage.Delete(profileInfo.ProfileDataPath);
            Storage.DeleteFolder(profile.FolderName);
        }

        private static void DeleteProfileInternal(string profileName)
        {
            var profile = GetProfileByName(profileName);
            DeleteProfileInternal(profile as SaveProfile);
        }

        private static ISaveProfile GetProfileByName(string profileName)
        {
            foreach (var profile in Profiles)
            {
                if (profile.Info.DisplayName != profileName)
                {
                    continue;
                }
                return profile;
            }
            return null;
        }

        #endregion


        #region Reset Profile

        private static async UniTask ResetProfileAsyncInternal(SaveProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            if (profile.IsLoaded)
            {
                profile.Unload();
            }

            foreach (var header in profile.Info.FileHeaders)
            {
                var filePath = Path.Combine(profile.FolderName, header.fileName);
                await Storage.DeleteAsync(filePath);
            }

            profile.Reset();
            profile.Save();

            ProfileReset?.Invoke(profile);
        }

        private static async UniTask ResetProfileAsyncInternal(string profileName)
        {
            var profile = GetProfileByName(profileName);
            await ResetProfileAsyncInternal(profile as SaveProfile);
        }

        private static void ResetProfileInternal(SaveProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            if (profile.IsLoaded)
            {
                profile.Unload();
            }

            foreach (var header in profile.Info.FileHeaders)
            {
                var filePath = Path.Combine(profile.FolderName, header.fileName);
                Storage.Delete(filePath);
            }

            profile.Reset();
            profile.Save();

            ProfileReset?.Invoke(profile);
        }

        private static void ResetProfileInternal(string profileName)
        {
            var profile = GetProfileByName(profileName);
            ResetProfileInternal(profile as SaveProfile);
        }

        #endregion


        #region Profile Backup

#pragma warning disable CS1998
        private static async UniTask<ProfileBackup> BackupProfileInternalAsync(ISaveProfile profile)
#pragma warning restore CS1998
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}