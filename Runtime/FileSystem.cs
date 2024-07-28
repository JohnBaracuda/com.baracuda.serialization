using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Baracuda.Serialization
{
    public static partial class FileSystem
    {
        #region Properties

        /// <summary>
        ///     Returns true if the file system is initialized. Check this property before accessing other file system API.
        /// </summary>
        [PublicAPI]
        public static bool IsInitialized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => State == FileSystemState.Initialized;
        }

        /// <summary>
        ///     Awaitable initialization operation.
        /// </summary>
        [PublicAPI]
        public static Task AwaitInitializationAsync()
        {
            return initializationTaskCompletionSource.Task;
        }

        /// <summary>
        ///     The loaded player <see cref="Serialization.ISaveProfile" />.
        /// </summary>
        /// <exception cref="FileSystemNotInitializedException"></exception>
        [PublicAPI]
        public static ISaveProfile Profile => IsInitialized
            ? activeProfile
            : throw new FileSystemNotInitializedException(nameof(Profile));

        /// <summary>
        ///     The persistent shared <see cref="Serialization.ISaveProfile" />.
        ///     This profile contains non profile specific persistent value
        ///     and can be used to store value that is non profile specific.
        /// </summary>
        /// <exception cref="FileSystemNotInitializedException"></exception>
        [PublicAPI]
        public static ISaveProfile SharedProfile => IsInitialized
            ? sharedProfile
            : throw new FileSystemNotInitializedException(nameof(SharedProfile));

        /// <summary>
        ///     List of available profiles that can be loaded.
        /// </summary>
        /// <exception cref="FileSystemNotInitializedException"></exception>
        [PublicAPI]
        public static IReadOnlyCollection<ISaveProfile> Profiles => IsInitialized
            ? profileCache.Values
            : throw new FileSystemNotInitializedException(nameof(SharedProfile));

        /// <summary>
        ///     The <see cref="FileSystemState" /> of the file system.
        /// </summary>
        [PublicAPI]
        public static FileSystemState State { get; private set; }

        /// <summary>
        ///     Event is invoked when the active <see cref="ISaveProfile" /> has changed.
        /// </summary>
        [PublicAPI]
        public static event ProfileChangedDelegate ProfileChanged;

        /// <summary>
        ///     Event is invoked when a new <see cref="ISaveProfile" /> was created.
        /// </summary>
        [PublicAPI]
        public static event ProfileCreatedDelegate ProfileCreated;

        /// <summary>
        ///     Event is invoked before a <see cref="ISaveProfile" /> is deleted.
        /// </summary>
        [PublicAPI]
        public static event ProfileDeletedDelegate ProfileDeleted;

        /// <summary>
        ///     Event is invoked before a <see cref="ISaveProfile" /> is deleted.
        /// </summary>
        [PublicAPI]
        public static event ProfileResetDelegate ProfileReset;

        /// <summary>
        ///     Called before the file system is initialized.
        /// </summary>
        [PublicAPI]
        public static event Action InitializationStarted;

        /// <summary>
        ///     Called after the file system was initialized.
        /// </summary>
        [PublicAPI]
        public static event Action InitializationCompleted;

        /// <summary>
        ///     Called before the file system is shut down.
        /// </summary>
        [PublicAPI]
        public static event Action ShutdownStarted;

        /// <summary>
        ///     Called after the file system was shut down.
        /// </summary>
        [PublicAPI]
        public static event Action ShutdownCompleted;

        /// <summary>
        ///     Validator to check the integrity of files, value & paths.
        /// </summary>
        /// <exception cref="FileSystemNotInitializedException"></exception>
        [PublicAPI]
        public static FileValidator Validator => State != FileSystemState.Uninitialized
            ? validator
            : throw new FileSystemNotInitializedException(nameof(Validator));

        /// <summary>
        ///     The current serialization version of the file system.
        /// </summary>
        /// <exception cref="FileSystemNotInitializedException"></exception>
        [PublicAPI]
        public static string Version => State != FileSystemState.Uninitialized
            ? version
            : throw new FileSystemNotInitializedException(nameof(Version));

        #endregion


        #region Initialize

        public static UniTask InitializeAsync(IFileSystemArgs args = null)
        {
            return InitializeAsyncInternal(args);
        }

        public static void Initialize(IFileSystemArgs args = null)
        {
            InitializeInternal(args);
        }

        #endregion


        #region Shutdown

        public static void Shutdown(in FileSystemShutdownArgs args = new())
        {
            ShutdownInternal(args);
        }

        public static UniTask ShutdownAsync(in FileSystemShutdownArgs args = new())
        {
            return ShutdownAsyncInternal(args);
        }

        #endregion


        #region Profile Switching

        public static UniTask<bool> SwitchProfileAsync(ISaveProfile profile)
        {
            return UpdateActiveProfileAsyncInternal(profile as SaveProfile);
        }

        public static bool SwitchProfile(ISaveProfile profile)
        {
            return UpdateActiveProfileInternal(profile as SaveProfile);
        }

        #endregion


        #region Profile Creation

        public static UniTask<ProfileCreationResult> CreateProfileAsync(ProfileCreationArgs args)
        {
            return CreateProfileAsyncInternal(args);
        }

        public static ProfileCreationResult CreateProfile(ProfileCreationArgs args)
        {
            return CreateProfileInternal(args);
        }

        #endregion


        #region Profile Deleting

        public static UniTask DeleteProfileAsync(ISaveProfile profile)
        {
            return DeleteProfileAsyncInternal(profile as SaveProfile);
        }

        public static void DeleteProfile(ISaveProfile profile)
        {
            DeleteProfileInternal(profile as SaveProfile);
        }

        public static UniTask DeleteProfileAsync(string profileName)
        {
            return DeleteProfileAsyncInternal(profileName);
        }

        public static void DeleteProfile(string profileName)
        {
            DeleteProfileInternal(profileName);
        }

        #endregion


        #region Profile Reset

        public static UniTask ResetProfileAsync(ISaveProfile profile)
        {
            return ResetProfileAsyncInternal(profile as SaveProfile);
        }

        public static void ResetProfile(ISaveProfile profile)
        {
            ResetProfileInternal(profile as SaveProfile);
        }

        public static UniTask ResetProfileAsync(string profileName)
        {
            return ResetProfileAsyncInternal(profileName);
        }

        public static void ResetProfile(string profileName)
        {
            ResetProfileInternal(profileName);
        }

        #endregion


        #region Saving

        /// <summary>
        ///     Save the current file operation backend. This operation may stall the current thread depending on the backend and
        ///     platform. Don't call during gameplay!
        /// </summary>
        public static void Save()
        {
            Storage.SaveBackend();
        }

        #endregion


        #region Profile Backup

        public static UniTask<IReadOnlyCollection<ProfileBackup>> LoadProfileBackupsAsync()
        {
            throw new NotImplementedException();
        }

        public static IReadOnlyCollection<ProfileBackup> LoadProfileBackups()
        {
            throw new NotImplementedException();
        }

        public static UniTask<ProfileBackup> BackupProfileAsync(ISaveProfile profile)
        {
            throw new NotImplementedException();
        }

        public static ProfileBackup BackupProfile(ISaveProfile profile)
        {
            throw new NotImplementedException();
        }

        public static UniTask RestoreProfileAsync(ProfileBackup profileBackup)
        {
            throw new NotImplementedException();
        }

        public static UniTask RestoreProfile(ProfileBackup profileBackup)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}