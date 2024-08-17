using System;
using Baracuda.Bedrock.Utilities;
using UnityEngine;

namespace Baracuda.Serialization.Editor
{
    [Flags]
    internal enum InitializeFlags
    {
        None = 0,
        DelayedCall = 1,
        AfterAssembliesLoaded = 2,
        BeforeSceneLoad = 4,
        AfterSceneLoad = 8,
        InitializeOnEnterEditMode = 16,
        InitializeOnEnterPlayMode = 32,
        OnLoad = 64
    }

    [Flags]
    internal enum ShutdownFlags
    {
        None = 0,
        ShutdownOnExitEditMode = 1,
        ShutdownOnExitPlayMode = 2
    }

    [UnityEditor.FilePathAttribute("ProjectSettings/FileSystemEditorSettings.asset",
        UnityEditor.FilePathAttribute.Location.ProjectFolder)]
    [UnityEditor.InitializeOnLoadAttribute]
    public class FileSystemEditorSettings : UnityEditor.ScriptableSingleton<FileSystemEditorSettings>
    {
        [SerializeField] private InitializeFlags initialization;
        [SerializeField] private ShutdownFlags shutdown;
        [SerializeField] private FileSystemArgumentsAsset fileSystemArguments;

        public IFileSystemArgs Args => fileSystemArguments ? fileSystemArguments : default(FileSystemArgs);

        internal InitializeFlags InitializationFlags
        {
            get => initialization;
            set => initialization = value;
        }

        internal ShutdownFlags ShutdownFlags
        {
            get => shutdown;
            set => shutdown = value;
        }

        public FileSystemArgumentsAsset FileSystemArguments
        {
            get => fileSystemArguments;
            set => fileSystemArguments = value;
        }

        public void SaveSettings()
        {
            Save(true);
        }

        static FileSystemEditorSettings()
        {
            UnityEditor.EditorApplication.delayCall += Initialize;
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void OnAfterAssembliesLoaded()
        {
            var isUninitialized = FileSystem.State is FileSystemState.Uninitialized;
            if (isUninitialized && instance.initialization.HasFlagFast(InitializeFlags.AfterAssembliesLoaded))
            {
                FileSystem.Shutdown();
                FileSystem.Initialize(instance.Args);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoad()
        {
            var isUninitialized = FileSystem.State is FileSystemState.Uninitialized;
            if (isUninitialized && instance.initialization.HasFlagFast(InitializeFlags.BeforeSceneLoad))
            {
                var shutdownArgs = new FileSystemShutdownArgs
                {
                    forceSynchronousShutdown = instance.Args.ForceSynchronousShutdown
                };

                FileSystem.Shutdown(shutdownArgs);
                FileSystem.Initialize(instance.Args);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnAfterSceneLoad()
        {
            var isUninitialized = FileSystem.State is FileSystemState.Uninitialized;
            if (isUninitialized && instance.initialization.HasFlagFast(InitializeFlags.AfterSceneLoad))
            {
                var shutdownArgs = new FileSystemShutdownArgs
                {
                    forceSynchronousShutdown = instance.Args.ForceSynchronousShutdown
                };

                FileSystem.Shutdown(shutdownArgs);
                FileSystem.Initialize(instance.Args);
            }
        }

        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            var isUninitialized = FileSystem.State is FileSystemState.Uninitialized;
            switch (state)
            {
                case UnityEditor.PlayModeStateChange.EnteredEditMode:
                    if (isUninitialized &&
                        instance.initialization.HasFlagFast(InitializeFlags.InitializeOnEnterEditMode))
                    {
                        FileSystem.Initialize(instance.Args);
                    }
                    break;

                case UnityEditor.PlayModeStateChange.ExitingEditMode:
                    if (instance.shutdown.HasFlagFast(ShutdownFlags.ShutdownOnExitEditMode))
                    {
                        var shutdownArgs = new FileSystemShutdownArgs
                        {
                            forceSynchronousShutdown = instance.Args.ForceSynchronousShutdown
                        };

                        FileSystem.Shutdown(shutdownArgs);
                    }
                    break;

                case UnityEditor.PlayModeStateChange.EnteredPlayMode:
                    if (isUninitialized &&
                        instance.initialization.HasFlagFast(InitializeFlags.InitializeOnEnterPlayMode))
                    {
                        FileSystem.Initialize(instance.Args);
                    }
                    break;

                case UnityEditor.PlayModeStateChange.ExitingPlayMode:
                    if (instance.shutdown.HasFlagFast(ShutdownFlags.ShutdownOnExitPlayMode))
                    {
                        var shutdownArgs = new FileSystemShutdownArgs
                        {
                            forceSynchronousShutdown = instance.Args.ForceSynchronousShutdown
                        };

                        FileSystem.Shutdown(shutdownArgs);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private static void Initialize()
        {
            if (instance.InitializationFlags.HasFlagFast(InitializeFlags.DelayedCall))
            {
                FileSystem.Initialize(instance.Args);
            }
        }
    }
}