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
        [SerializeField] private StorageLevel storageLevel = StorageLevel.Profile;

        [PublicAPI]
        public string Key => key;

        [PublicAPI]
        public StorageLevel StorageLevel => storageLevel;

        [Conditional("UNITY_EDITOR")]
        protected void UpdateSaveDataKey()
        {
#if UNITY_EDITOR
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