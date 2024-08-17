using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Baracuda.Bedrock.Collections;
using Baracuda.Bedrock.Types;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Baracuda.Serialization
{
    public abstract class SaveDataList<TValue> : SaveDataAsset, IList<TValue>, IReadOnlyList<TValue>
    {
        [LabelText("Default Value")]
        [SerializeField] private List<TValue> defaultList;
        [NonSerialized] private readonly Broadcast<TValue> _addedEvent = new();
        [NonSerialized] private readonly Broadcast<TValue> _removedEvent = new();
        [ShowInInspector]
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
            Profile.SaveFile(Key, _list);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void AddUnique(TValue value)
        {
            if (_list.AddUnique(value))
            {
                Profile.SaveFile(Key, _list);
            }
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool ICollection<TValue>.Remove(TValue item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; }
        public bool IsReadOnly { get; }

        public void Remove(TValue value)
        {
            if (_list.Remove(value))
            {
                Profile.SaveFile(Key, _list);
            }
        }

        public bool Contains(TValue value)
        {
            return _list.Contains(value);
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
            Profile.SaveFile(Key, _list);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
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