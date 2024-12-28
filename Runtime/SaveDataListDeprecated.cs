using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Baracuda.Utility.Collections;
using Baracuda.Utility.Types;
using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEngine;

namespace Baracuda.Serialization
{
    public abstract class SaveDataListDeprecated<TValue> : SaveDataAsset, IList<TValue>, IReadOnlyList<TValue>
    {
        [SerializeField] private List<TValue> defaultList;
        [NonSerialized] private readonly Broadcast<TValue> _addedEvent = new();
        [NonSerialized] private readonly Broadcast<TValue> _removedEvent = new();
        [NonSerialized] private List<TValue> _list;


        #region Events

        [PublicAPI]
        public event Action<TValue> Added
        {
            add => _addedEvent.AddListener(value);
            remove => _addedEvent.RemoveListener(value);
        }

        [PublicAPI]
        public event Action<TValue> Removed
        {
            add => _removedEvent.AddListener(value);
            remove => _removedEvent.RemoveListener(value);
        }

        #endregion


        #region Map Access

        public void Add(TValue value)
        {
            _list.Add(value);
            _addedEvent.Raise(value);
            Profile.SaveFile(Key, _list);
        }

        public void Clear()
        {
            _list.Clear();
            Profile.SaveFile(Key, _list);
        }

        public void AddUnique(TValue value)
        {
            if (_list.AddUnique(value))
            {
                _addedEvent.Raise(value);
                Profile.SaveFile(Key, _list);
            }
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(TValue item)
        {
            if (_list.Remove(item))
            {
                _removedEvent.Raise(item);
                Profile.SaveFile(Key, _list);
                return true;
            }
            return false;
        }

        public int Count => _list.Count;
        public bool IsReadOnly => false;

        public bool Contains(TValue value)
        {
            return _list.Contains(value);
        }

        #endregion


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

        [Button("Reset")]
        public void ResetPersistentData()
        {
            _list = new List<TValue>();
            foreach (var value in defaultList)
            {
                _list.Add(value);
            }
            Profile.SaveFile(Key, _list);
        }

        #endregion


        #region Initialization

        private void OnEnable()
        {
            FileSystem.InitializationCompleted -= OnFileSystemInitialized;
            FileSystem.InitializationCompleted += OnFileSystemInitialized;

            if (FileSystem.IsInitialized)
            {
                OnFileSystemInitialized();
            }
        }

        private void OnFileSystemInitialized()
        {
            UpdateSaveDataKey();
            if (Profile.HasFile(Key) is false)
            {
                _list = new List<TValue>();
                foreach (var value in defaultList)
                {
                    _list.Add(value);
                }
                Profile.SaveFile(Key, _list);
            }
            else
            {
                _list = Profile.LoadFile<List<TValue>>(Key);
            }
        }

        private void OnDisable()
        {
            FileSystem.InitializationCompleted -= OnFileSystemInitialized;
        }

        #endregion


        public IEnumerator<TValue> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(TValue item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, TValue item)
        {
            _list.Insert(index, item);
            _addedEvent.Raise(item);
            Profile.SaveFile(Key, _list);
        }

        public void RemoveAt(int index)
        {
            var item = _list[index];
            _list.RemoveAt(index);
            _removedEvent.Raise(item);
            Profile.SaveFile(Key, _list);
        }

        public TValue this[int index]
        {
            get => _list[index];
            set
            {
                _list[index] = value;
                Profile.SaveFile(Key, _list);
            }
        }
    }
}