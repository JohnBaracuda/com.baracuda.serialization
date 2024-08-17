using System;
using System.Collections.Generic;
using System.IO;
using Baracuda.Bedrock.Collections;
using Baracuda.Bedrock.Types;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Baracuda.Serialization
{
    public abstract class SaveDataMap<TKey, TValue> : SaveDataAsset
    {
        [LabelText("Default Value")]
        [SerializeField] private Map<TKey, TValue> defaultMap;
        [NonSerialized] private readonly Broadcast<KeyValuePair<TKey, TValue>> _changedEvent = new();
        [NonSerialized] private readonly Broadcast<KeyValuePair<TKey, TValue>> _addedEvent = new();
        [NonSerialized] private readonly Broadcast<KeyValuePair<TKey, TValue>> _removedEvent = new();
        [ShowInInspector]
        [NonSerialized] private Map<TKey, TValue> _map;


        #region Events

        [PublicAPI]
        public event Action<KeyValuePair<TKey, TValue>> Changed
        {
            add => _changedEvent.AddListener(value);
            remove => _changedEvent.RemoveListener(value);
        }

        [PublicAPI]
        public event Action<KeyValuePair<TKey, TValue>> Added
        {
            add => _addedEvent.AddListener(value);
            remove => _addedEvent.RemoveListener(value);
        }

        [PublicAPI]
        public event Action<KeyValuePair<TKey, TValue>> Removed
        {
            add => _removedEvent.AddListener(value);
            remove => _removedEvent.RemoveListener(value);
        }

        #endregion


        #region Map Access

        public void Add(TKey key, TValue value)
        {
            _map.Add(key, value);
            Profile.SaveFile(Key, _map);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _map.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get => _map[key];
            set
            {
                _map[key] = value;
                Profile.SaveFile(Key, _map);
            }
        }

        #endregion


        #region Persistent Data

        private ISaveProfile Profile => StorageLevel switch
        {
            StorageLevel.Profile => FileSystem.Profile,
            StorageLevel.SharedProfile => FileSystem.SharedProfile,
            var _ => throw new ArgumentOutOfRangeException()
        };

        [Button]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 8)]
        private void OpenInFileSystem()
        {
            var dataPath = Application.persistentDataPath;
            var systemPath = FileSystem.RootFolder;
            var profilePath = Profile.Info.FolderName;
            var folderPath = Path.Combine(dataPath, systemPath, profilePath);
            Application.OpenURL(folderPath);
        }

        [Button("Reset")]
        [ButtonGroup("Persistent")]
        public override void ResetPersistentData()
        {
            _map = new Map<TKey, TValue>();
            foreach (var (key, value) in defaultMap)
            {
                _map.Add(key, value);
            }
            Profile.SaveFile(Key, _map);
        }

        #endregion


        #region Initialization

        private void OnEnable()
        {
#if UNITY_EDITOR
            UpdateSaveDataKey();
#endif
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
                _map = new Map<TKey, TValue>();
                foreach (var (key, value) in defaultMap)
                {
                    _map.Add(key, value);
                }
                Profile.SaveFile(Key, _map);
            }
            else
            {
                _map = Profile.LoadFile<Map<TKey, TValue>>(Key);
            }
        }

        #endregion
    }
}