using System;
using System.Diagnostics;
using Baracuda.Utility.Attributes;
using Baracuda.Utility.Utilities;
using JetBrains.Annotations;
using UnityEngine;

namespace Baracuda.Serialization
{
    public abstract class SaveDataAsset : ScriptableObject
    {
        [AssetGUID]
        [SerializeField] private string key;
        [SerializeField] private string[] tags;
        [SerializeField] private StorageLevel storageLevel = StorageLevel.Profile;

        [PublicAPI]
        public string Key => key;

        [PublicAPI]
        public string[] Tags => tags;

        [PublicAPI]
        public StorageLevel StorageLevel => storageLevel;

        public abstract void ResetPersistentData();

        [Conditional("UNITY_EDITOR")]
        protected void UpdateSaveDataKey()
        {
#if UNITY_EDITOR
            tags ??= Array.Empty<string>();
            ArrayUtility.AddUnique(ref tags, name);

            if (key.IsNullOrWhitespace())
            {
                var path = UnityEditor.AssetDatabase.GetAssetPath(this);
                var guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
                key = guid;
            }
#endif
        }
    }
}