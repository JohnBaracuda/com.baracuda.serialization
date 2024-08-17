using System;
using System.Globalization;
using Baracuda.Bedrock.Reflection;
using UnityEngine;

namespace Baracuda.Serialization
{
    [Serializable]
    internal sealed class FileHeader
    {
        #region Fields

        [SerializeField] private string fileName;
        [SerializeField] private string fileType;
        [SerializeField] private FileGroup fileGroup;
        [SerializeField] private string lastModificationTimeStamp;
        [SerializeField] private bool encrypted;
        [SerializeField] private string[] tags;
        [SerializeField] private string version;

        #endregion


        #region Properties

        public string FileName
        {
            get => fileName;
            set => fileName = value;
        }
        public string FileType
        {
            get => fileType;
            set => fileType = value;
        }
        public FileGroup FileGroup
        {
            get => fileGroup;
            set => fileGroup = value;
        }
        public string LastModificationTimeStamp
        {
            get => lastModificationTimeStamp;
            set => lastModificationTimeStamp = value;
        }
        public bool Encrypted
        {
            get => encrypted;
            set => encrypted = value;
        }
        public string[] Tags
        {
            get => tags;
            set => tags = value;
        }
        public string Version
        {
            get => version;
            set => version = value;
        }

        #endregion


        #region Ctor

        public FileHeader(string name, Type type, StoreOptions options)
        {
            fileName = name;
            fileType = type.AssemblyQualifiedName;
            fileGroup = GetFileGroup(type);
            lastModificationTimeStamp = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            encrypted = options.Encryption.ValueOrDefault();
            tags = options.Tags;
            version = FileSystem.Version;

            static FileGroup GetFileGroup(Type type)
            {
                if (typeof(ScriptableObject).IsAssignableFrom(type))
                {
                    return FileGroup.ScriptableObject;
                }
                if (type.IsUnitySerializable())
                {
                    return FileGroup.Serializable;
                }
                return FileGroup.Invalid;
            }
        }

        public void Update(StoreOptions options)
        {
            encrypted = options.Encryption.ValueOrDefault();
            tags = options.Tags;
            version = FileSystem.Version;
            lastModificationTimeStamp = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }

        public void Update()
        {
            version = FileSystem.Version;
            lastModificationTimeStamp = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }

        #endregion
    }
}