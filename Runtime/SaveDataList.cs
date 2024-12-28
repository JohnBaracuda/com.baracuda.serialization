using System;
using System.Collections;
using System.Collections.Generic;
using Baracuda.Utility.Collections;
using Baracuda.Utility.Utilities;
using JetBrains.Annotations;

namespace Baracuda.Serialization
{
    public class SaveDataList<T> : IList<T>, IReadOnlyList<T>
    {
        #region Public API

        [PublicAPI]
        public FileOptions FileOptions { get; }

        [PublicAPI]
        public SaveDataKey Key { get; }

        [PublicAPI]
        public bool IsCreated { get; }

        [PublicAPI]
        public ISaveProfile Profile => _profile();

        [PublicAPI]
        public void Add(T value)
        {
            AddInternal(value);
        }

        [PublicAPI]
        public bool Remove(T value)
        {
            return RemoveInternal(value);
        }

        [PublicAPI]
        public void Clear()
        {
            ClearInternal();
        }

        [PublicAPI]
        public bool Contains(T item)
        {
            return ContainsInternal(item);
        }

        [PublicAPI]
        public void AddUnique(T value)
        {
            AddUniqueInternal(value);
        }

        [PublicAPI]
        public int IndexOf(T item)
        {
            return IndexOfInternal(item);
        }

        [PublicAPI]
        public void Insert(int index, T item)
        {
            InsertInternal(index, item);
        }

        [PublicAPI]
        public void RemoveAt(int index)
        {
            RemoveAtInternal(index);
        }

        [PublicAPI]
        public T this[int index]
        {
            get => GetInternal(index);
            set => SetInternal(index, value);
        }

        [PublicAPI]
        public int Count => GetCountInternal();

        [PublicAPI]
        public bool IsReadOnly => GetIsReadOnlyInternal();

        [PublicAPI]
        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyToInternal(array, arrayIndex);
        }

        [PublicAPI]
        public IEnumerator<T> GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        [PublicAPI]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        #endregion


        #region Builder

        [MustUseReturnValue]
        public static Builder WithKey(SaveDataKey key)
        {
            return new Builder
            {
                Key = key
            };
        }

        [PublicAPI]
        public SaveDataList(SaveDataKey key, Func<ISaveProfile> profile, FileOptions options, List<T> defaultList)
        {
            FileOptions = options;
            Key = key;
            IsCreated = true;
            _defaultList = defaultList;
            _profile = profile;
            LoadOnDemand();
        }

        private bool _isLoaded;

        private bool LoadOnDemand()
        {
            if (FileSystem.IsInitialized is false)
            {
                return false;
            }
            if (_isLoaded)
            {
                return true;
            }
            _list.AddRange(Profile.TryLoadFile<List<T>>(Key, out var data) ? data : _defaultList);
            _isLoaded = true;
            return true;
        }

        public ref struct Builder
        {
            public SaveDataKey Key;
            private Func<ISaveProfile> _profile;
            private List<T> _default;
            private string[] _tags;

            [MustUseReturnValue]
            public Builder WithProfile(Func<ISaveProfile> profile)
            {
                _profile = profile;
                return this;
            }

            [MustUseReturnValue]
            public Builder WithSharedProfile()
            {
                _profile = () => FileSystem.PersistentProfile;
                return this;
            }

            [MustUseReturnValue]
            public Builder WithTags(params string[] tags)
            {
                ArrayUtility.Add(ref _tags, tags);
                return this;
            }

            [MustUseReturnValue]
            public Builder WithAlias(string alias)
            {
                ArrayUtility.Add(ref _tags, alias);
                return this;
            }

            [MustUseReturnValue]
            public Builder WithDefaultValue(List<T> @default)
            {
                _default = @default;
                return this;
            }

            [MustUseReturnValue]
            public Builder WithDefaultValue(params T[] defaultValues)
            {
                _default = new List<T>(defaultValues);
                return this;
            }

            [MustUseReturnValue]
            public readonly SaveDataList<T> Build()
            {
                var profile = _profile ?? (() => FileSystem.Profile);
                var defaultList = _default ?? new List<T>();
                var saveData = new SaveDataList<T>(Key, profile, new FileOptions(_tags), defaultList);
                return saveData;
            }

            public static implicit operator SaveDataList<T>(Builder builder)
            {
                return builder.Build();
            }
        }

        #endregion


        #region Implementation

        private readonly ObservableList<T> _list = new();
        private readonly List<T> _defaultList;
        private readonly Func<ISaveProfile> _profile;

        private void AddInternal(T value)
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }
            _list.Add(value);
            Profile.SaveFile(Key, _list.GetInternalList());
        }

        private bool RemoveInternal(T value)
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }
            if (_list.Remove(value))
            {
                Profile.SaveFile(Key, _list.GetInternalList());
                return true;
            }
            return false;
        }

        private void ClearInternal()
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }

            _list.Clear();
        }

        private bool ContainsInternal(T item)
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }

            return _list.Contains(item);
        }

        private void AddUniqueInternal(T value)
        {
            if (!LoadOnDemand())
            {
                Debug.LogError("Data is not loaded!");
                return;
            }

            if (!_list.Contains(value))
            {
                _list.Add(value);
            }
        }

        private int IndexOfInternal(T item)
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }
            return _list.IndexOf(item);
        }

        private void InsertInternal(int index, T item)
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }
            _list.Insert(index, item);
        }

        private void RemoveAtInternal(int index)
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }
            _list.RemoveAt(index);
        }

        private T GetInternal(int index)
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }
            return _list[index];
        }

        private void SetInternal(int index, T value)
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }
            _list[index] = value;
        }

        private int GetCountInternal()
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }
            return _list.Count;
        }

        private bool GetIsReadOnlyInternal()
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }
            return false;
        }

        private void CopyToInternal(T[] array, int arrayIndex)
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }
            _list.CopyTo(array, arrayIndex);
        }

        private IEnumerator<T> GetEnumeratorInternal()
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }
            return _list.GetEnumerator();
        }

        #endregion
    }
}