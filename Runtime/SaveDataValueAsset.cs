using System;
using System.Collections.Generic;
using System.IO;
using Baracuda.Utility.Types;
using Baracuda.Utility.Utilities;
using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Baracuda.Serialization
{
    public abstract class SaveDataValueAsset<TValue> : SaveDataAsset
    {
        [FormerlySerializedAs("defaultPersistentValue")]
        [SerializeField] private TValue defaultValue;
        [NonSerialized] private readonly Broadcast<TValue> _changedEvent = new();
        [NonSerialized] private TValue _cachedValue;
        [NonSerialized] private bool _isValueCached;

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
            if (FileSystem.IsInitialized is false)
            {
                return;
            }

            Assert.IsFalse(Key.IsNullOrWhitespace(), $"Save data asset key of {name} is not set!");
            if (EqualityComparer<TValue>.Default.Equals(value, GetValue()))
            {
                return;
            }
            _cachedValue = value;
            _isValueCached = true;
            Profile.SaveFile(Key, value, new StoreOptions(Tags));
            _changedEvent.Raise(value);
        }

        [PublicAPI]
        public TValue GetValue()
        {
            if (FileSystem.IsInitialized is false)
            {
                return defaultValue;
            }

            Assert.IsFalse(Key.IsNullOrWhitespace(), $"Save data asset key of {name} is not set!");
            if (_isValueCached)
            {
                return _cachedValue;
            }
            _cachedValue = Profile.LoadFile<TValue>(Key, new StoreOptions(Tags));
            return _cachedValue;
        }

        [PublicAPI]
        public event Action<TValue> Changed
        {
            add => _changedEvent.AddListener(value);
            remove => _changedEvent.RemoveListener(value);
        }

        [PublicAPI]
        public void SetValueDirty()
        {
            SetValue(_isValueCached ? _cachedValue : Value);
        }


        #region Persistent Data

        private ISaveProfile Profile => StorageLevel switch
        {
            StorageLevel.Profile => FileSystem.Profile,
            StorageLevel.SharedProfile => FileSystem.SharedProfile,
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

        [Button("Reset")]
        public override void ResetPersistentData()
        {
            Value = defaultValue;
            _changedEvent.Raise(Value);
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
            if (Profile.HasFile(Key) is false)
            {
                _cachedValue = defaultValue;
                _isValueCached = true;
            }
            else
            {
                _cachedValue = Profile.LoadFile<TValue>(Key);
                _isValueCached = true;
            }

            _changedEvent.Raise(Value);
        }

        #endregion
    }
}