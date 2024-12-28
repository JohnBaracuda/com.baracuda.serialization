using System;
using System.Collections;
using System.Collections.Generic;
using Baracuda.Utility.Collections;
using Baracuda.Utility.Utilities;
using JetBrains.Annotations;

namespace Baracuda.Serialization
{
    public class SaveDataMap<TKey, TValue> : IDictionary<TKey, TValue>
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
        public SaveDataMap(SaveDataKey key, Func<ISaveProfile> profile, FileOptions options, Dictionary<TKey, TValue> defaultDictionary)
        {
            FileOptions = options;
            Key = key;
            IsCreated = true;
            _defaultDictionary = defaultDictionary;
            _profile = profile;
            LoadOnDemand();
        }

        [PublicAPI]
        public void Add(TKey key, TValue value)
        {
            AddInternal(key, value);
        }

        [PublicAPI]
        public bool Remove(TKey key)
        {
            return RemoveInternal(key);
        }

        [PublicAPI]
        public bool ContainsKey(TKey key)
        {
            return ContainsKeyInternal(key);
        }

        [PublicAPI]
        public bool TryGetValue(TKey key, out TValue value)
        {
            return TryGetValueInternal(key, out value);
        }

        [PublicAPI]
        public TValue this[TKey key]
        {
            get => GetInternal(key);
            set => SetInternal(key, value);
        }

        [PublicAPI]
        public ICollection<TKey> Keys => GetKeysInternal();

        [PublicAPI]
        public ICollection<TValue> Values => GetValuesInternal();

        [PublicAPI]
        public void Clear()
        {
            ClearInternal();
        }

        [PublicAPI]
        public int Count => GetCountInternal();

        [PublicAPI]
        public bool IsReadOnly => GetIsReadOnlyInternal();

        [PublicAPI]
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            AddInternal(item.Key, item.Value);
        }

        [PublicAPI]
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return RemoveInternal(item.Key);
        }

        [PublicAPI]
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ContainsInternal(item);
        }

        [PublicAPI]
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            CopyToInternal(array, arrayIndex);
        }

        [PublicAPI]
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
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

        public ref struct Builder
        {
            public SaveDataKey Key;
            private Func<ISaveProfile> _profile;
            private Dictionary<TKey, TValue> _default;
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
            public Builder WithTag(params string[] tags)
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
            public Builder WithDefaultValue(Map<TKey, TValue> @default)
            {
                _default = @default;
                return this;
            }

            [MustUseReturnValue]
            public Builder WithDefaultValue(Dictionary<TKey, TValue> defaultValues)
            {
                _default = defaultValues;
                return this;
            }

            [MustUseReturnValue]
            public readonly SaveDataMap<TKey, TValue> Build()
            {
                var profile = _profile ?? (() => FileSystem.Profile);
                var defaultMap = _default ?? new Map<TKey, TValue>();
                var saveDataMap = new SaveDataMap<TKey, TValue>(Key, profile, new FileOptions(_tags), defaultMap);
                return saveDataMap;
            }

            public static implicit operator SaveDataMap<TKey, TValue>(Builder builder)
            {
                return builder.Build();
            }
        }

        #endregion


        #region Implementation

        private readonly Map<TKey, TValue> _map = new();
        private readonly Dictionary<TKey, TValue> _defaultDictionary;
        private readonly Func<ISaveProfile> _profile;
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

            var loadedMap = Profile.TryLoadFile<Map<TKey, TValue>>(Key, out var data) ? data : _defaultDictionary;
            foreach (var kvp in loadedMap)
            {
                _map[kvp.Key] = kvp.Value;
            }

            _isLoaded = true;
            return true;
        }

        private void AddInternal(TKey key, TValue value)
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }

            _map.Add(key, value);
            Profile.SaveFile(Key, _map);
        }

        private bool RemoveInternal(TKey key)
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }

            if (_map.Remove(key))
            {
                Profile.SaveFile(Key, _map);
                return true;
            }

            return false;
        }

        private bool ContainsKeyInternal(TKey key)
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }

            return _map.ContainsKey(key);
        }

        private bool TryGetValueInternal(TKey key, out TValue value)
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }

            return _map.TryGetValue(key, out value);
        }

        private TValue GetInternal(TKey key)
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }

            return _map[key];
        }

        private void SetInternal(TKey key, TValue value)
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }

            _map[key] = value;
            Profile.SaveFile(Key, _map);
        }

        private ICollection<TKey> GetKeysInternal()
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }

            return _map.Keys;
        }

        private ICollection<TValue> GetValuesInternal()
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }

            return _map.Values;
        }

        private void ClearInternal()
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }

            _map.Clear();
            Profile.SaveFile(Key, _map);
        }

        private int GetCountInternal()
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }

            return _map.Count;
        }

        private bool GetIsReadOnlyInternal()
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }

            return false;
        }

        private bool ContainsInternal(KeyValuePair<TKey, TValue> item)
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }

            return _map.ContainsKey(item.Key) && EqualityComparer<TValue>.Default.Equals(_map[item.Key], item.Value);
        }

        private void CopyToInternal(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }

            ((IDictionary<TKey, TValue>)_map).CopyTo(array, arrayIndex);
        }

        private IEnumerator<KeyValuePair<TKey, TValue>> GetEnumeratorInternal()
        {
            if (!LoadOnDemand())
            {
                throw new Exception("Data is not loaded!");
            }

            return _map.GetEnumerator();
        }

        #endregion
    }
}