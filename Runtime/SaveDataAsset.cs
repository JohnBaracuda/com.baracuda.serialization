﻿using System;
using System.Diagnostics;
using Baracuda.Bedrock.Attributes;
using Baracuda.Bedrock.Utilities;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Baracuda.Serialization
{
    public abstract class SaveDataAsset : ScriptableObject
    {
        [AssetGUID]
        [SerializeField] private string key;
        [ListDrawerSettings(DefaultExpandedState = false)]
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