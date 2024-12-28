using System;
using System.IO;
using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Baracuda.Serialization
{
    public abstract class SaveDataValueAsset<TValue> : SaveDataAsset
    {
        [FormerlySerializedAs("defaultPersistentValue")]
        [SerializeField] private TValue defaultValue;
        [NonSerialized] private SaveData<TValue> _saveData;

        [PublicAPI]
        [ShowNativeProperty]
        public TValue Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        [PublicAPI]
        public void SetValue(TValue value)
        {
            _saveData.SetValue(value);
        }

        [PublicAPI]
        public TValue GetValue()
        {
            return _saveData.GetValue();
        }

        [PublicAPI]
        public event Action<TValue> Changed
        {
            add => _saveData.ObservableValue.AddListener(value);
            remove => _saveData.ObservableValue.RemoveListener(value);
        }


        #region Persistent Data

        private ISaveProfile Profile => StorageLevel switch
        {
            StorageLevel.Profile => FileSystem.Profile,
            StorageLevel.SharedProfile => FileSystem.PersistentProfile,
            var _ => throw new ArgumentOutOfRangeException()
        };

        [Button]
        private void OpenInFileSystem()
        {
            var dataPath = Application.persistentDataPath;
            var systemPath = FileSystem.RootFolder;
            var profilePath = Profile.Info.FolderName;
            var folderPath = Path.Combine(dataPath, systemPath, profilePath);
            Application.OpenURL(folderPath);
        }

        #endregion


        #region Initialization

        protected virtual void OnEnable()
        {
            UpdateSaveDataKey();
            FileSystem.InitializationCompleted -= OnFileSystemInitialized;
            FileSystem.InitializationCompleted += OnFileSystemInitialized;

            if (FileSystem.IsInitialized)
            {
                OnFileSystemInitialized();
            }
        }

        private void OnFileSystemInitialized()
        {
            _saveData ??= SaveData<TValue>.WithKey(Key)
                .WithAlias(name)
                .WithDefaultValue(defaultValue)
                .WithProfile(() => Profile)
                .Build();
        }

        #endregion
    }
}