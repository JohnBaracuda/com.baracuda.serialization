using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Baracuda.Utility.Collections;
using Baracuda.Utility.Types;
using Baracuda.Utility.Utilities;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace Baracuda.Serialization
{
    public sealed class SaveData<T> : SaveData, IObservableValue<T>
    {
        #region Public API

        [PublicAPI]
        public override SaveDataKey Key { get; }

        [PublicAPI]
        public T Value
        {
            get => GetValueInternal();
            set => SetValueInternal(value);
        }

        [PublicAPI]
        public void SetValue(T value)
        {
            SetValueInternal(value);
        }

        [PublicAPI]
        public T GetValue()
        {
            return GetValueInternal();
        }

        [PublicAPI]
        public void ResetToDefault()
        {
            SetValueInternal(DefaultValue);
        }

        [PublicAPI]
        public override bool IsCreated { get; }

        [PublicAPI]
        public ObservableValue<T> ObservableValue { get; }

        [PublicAPI]
        public T DefaultValue { get; }

        [PublicAPI]
        public override FileOptions FileOptions { get; }

        [PublicAPI]
        public void AddObserver(Action<T> observer)
        {
            ObservableValue.AddObserver(observer);
        }

        [PublicAPI]
        public void RemoveObserver(Action<T> observer)
        {
            ObservableValue.RemoveObserver(observer);
        }

        [PublicAPI]
        public static implicit operator T(SaveData<T> saveData)
        {
            return saveData.GetValue();
        }

        #endregion


        #region Implementation

        private readonly Func<ISaveProfile> _profile;
        private readonly Func<T, T> _validation;

        private readonly string _alias;
        private bool _hasCachedValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetValueInternal(T value)
        {
            if (FileSystem.IsInitialized is false)
            {
                return;
            }

            Assert.IsTrue(IsCreated);
            if (EqualityComparer<T>.Default.Equals(value, GetValue()))
            {
                return;
            }

            if (_validation != null)
            {
                value = _validation(value);
            }

            _hasCachedValue = true;
            ObservableValue.Value = value;
            _profile().SaveFile(Key, value, FileOptions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T GetValueInternal()
        {
            if (FileSystem.IsInitialized is false)
            {
                return DefaultValue;
            }
            Assert.IsTrue(IsCreated);

            if (_hasCachedValue)
            {
                return ObservableValue.Value;
            }

            if (_profile().TryLoadFile<T>(Key, out var value, FileOptions))
            {
                _hasCachedValue = true;
                ObservableValue.Value = _validation != null ? _validation(value) : value;
                return ObservableValue.Value;
            }

            if (_alias.IsNotNullOrWhitespace() && _profile().TryLoadFile(_alias, out value, FileOptions))
            {
                _hasCachedValue = true;
                ObservableValue.Value = _validation != null ? _validation(value) : value;
                return ObservableValue.Value;
            }

            _hasCachedValue = true;
            ObservableValue.Value = DefaultValue;
            return ObservableValue.Value;
        }

        #endregion


        #region Builder

        [MustUseReturnValue]
        public static Builder WithKey(SaveDataKey key)
        {
#if UNITY_EDITOR
            if (Instances.Any(instance => instance.Key == key))
            {
                Debug.LogWarning($"Duplicate save data key! {key.ToString()}");
            }
#endif
            return new Builder
            {
                Key = key
            };
        }

        public SaveData(SaveDataKey key, Func<ISaveProfile> profile, FileOptions fileOptions, T @default, Func<T, T> validation, string alias) : base(typeof(T))
        {
            Key = key;
            _profile = profile;
            FileOptions = fileOptions;
            DefaultValue = @default;
            IsCreated = true;
            _validation = validation;
            _alias = alias;
            ObservableValue = new ObservableValue<T>(DefaultValue);
            if (FileSystem.IsInitialized)
            {
                ObservableValue.SetValue(GetValueInternal());
            }
            else
            {
                FileSystem.InitializationCompleted += LazyInitialize;

                void LazyInitialize()
                {
                    FileSystem.InitializationCompleted -= LazyInitialize;
                    ObservableValue.SetValue(GetValueInternal());
                }
            }
        }

        public ref struct Builder
        {
            public SaveDataKey Key;
            private string _alias;
            private Func<ISaveProfile> _profile;
            private T _default;
            private string[] _tags;
            private Func<T, T> _validation;
            private bool _bindToResetAPI;

            [MustUseReturnValue]
            public Builder WithProfile(Func<ISaveProfile> profile)
            {
                _profile = profile;
                return this;
            }

            [MustUseReturnValue]
            public Builder WithValidation(Func<T, T> validation)
            {
                _validation = validation;
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
                _alias = alias;
                return WithTag(alias);
            }

            [MustUseReturnValue]
            public Builder WithDefaultValue(T @default)
            {
                _default = @default;
                return this;
            }

            [MustUseReturnValue]
            public readonly SaveData<T> Build()
            {
                var profile = _profile ?? (() => FileSystem.Profile);
                var saveData = new SaveData<T>(Key, profile, new FileOptions(_tags), _default, _validation, _alias);
                Instances.Add(saveData);
                return saveData;
            }

            public static implicit operator SaveData<T>(Builder builder)
            {
                return builder.Build();
            }
        }

        #endregion
    }

    public abstract class SaveData : IDisposable
    {
        [PublicAPI]
        public abstract FileOptions FileOptions { get; }

        [PublicAPI]
        public abstract SaveDataKey Key { get; }

        [PublicAPI]
        public abstract bool IsCreated { get; }

        [PublicAPI]
        public Type DataType { get; }

        [PublicAPI]
        public static ObservableList<SaveData> Instances { get; } = new();

        protected SaveData(Type dataType)
        {
            DataType = dataType;
        }

        public void Dispose()
        {
            Instances.Remove(this);
        }
    }
}