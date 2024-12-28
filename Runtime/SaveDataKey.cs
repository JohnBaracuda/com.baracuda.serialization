using System;
using System.Collections.Generic;
using System.Linq;
using Baracuda.Utility.Utilities;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Scripting;

namespace Baracuda.Serialization
{
    [Preserve]
    [Serializable]
    public struct SaveDataKey : IEquatable<SaveDataKey>
    {
        #region Fields

        [SerializeField] private int value;

        public readonly int Value => value;

        public SaveDataKey(int value)
        {
            this.value = value;
        }

        [BurstDiscard]
        public static SaveDataKey Create(string name)
        {
            var value = name.ComputeFnv1AHash();
            var key = new SaveDataKey(value);
            Registry.AddKey(name, value);
            return key;
        }

        [BurstDiscard]
        public static implicit operator SaveDataKey(string name)
        {
            return Create(name);
        }

        [BurstDiscard]
        public static implicit operator string(SaveDataKey key)
        {
            return key.ToString();
        }

        [BurstDiscard]
        public override string ToString()
        {
            return Registry.GetName(this);
        }

        #endregion


        #region IEquatable

        public bool Equals(SaveDataKey other)
        {
            return value == other.value;
        }

        public override bool Equals(object obj)
        {
            return obj is SaveDataKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return value;
        }

        #endregion


        #region Registry

        public static class Registry
        {
            public static IReadOnlyDictionary<int, string> Keys => keyNames;

            public static IEnumerable<string> Names => Keys.Values;
            public static IEnumerable<string> HumanizedNames => Keys.Values.Select(item => item.Humanize());

            private static readonly Dictionary<int, string> keyNames = new()
            {
                { 0, "Invalid" }
            };

            public static void AddKey(string name, in int key)
            {
                keyNames.TryAdd(key, name);
            }

            public static string GetName(in SaveDataKey key)
            {
                return keyNames[key.Value];
            }
        }

        #endregion
    }
}