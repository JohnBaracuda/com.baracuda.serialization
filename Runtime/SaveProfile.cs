using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Baracuda.Utilities;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace Baracuda.Serialization
{
    [Serializable]
    internal class SaveProfile : ISaveProfile
    {
        #region Fields

        [SerializeField] private string profileDisplayName;
        [SerializeField] private string profileFolderName;
        [SerializeField] private string profileFileName;
        [SerializeField] private string createdTimeStamp;

        [SerializeField] private List<Header> files;

        private Dictionary<string, SaveData> _loadedSaveDataCache;
        private Dictionary<string, FileData> _loadedFileDataCache;
        private HashSet<string> _dirtySaveDataKeys;

        private bool _isDirty;

        #endregion


        #region Properties

        public string DisplayName => profileDisplayName;
        public string FolderName => profileFolderName;

        public DateTime CreatedTimeStamp =>
            DateTime.TryParse(createdTimeStamp, out var timeStamp) ? timeStamp : DateTime.Now;

        public bool IsLoaded { get; private set; }
        public string ProfileFilePath => Path.Combine(profileFolderName, profileFileName);
        public SaveProfileData Info => new(DisplayName, FolderName, CreatedTimeStamp, ProfileFilePath, files);

        #endregion


        #region Save & Store File

        public void SaveFile<T>([NotNull] string fileName, T value, StoreOptions options = default)
        {
            if (fileName.IsNullOrWhitespace())
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            FileSystem.Validator.SanitizeFileName(ref fileName);

            SaveData<T> saveData;

            if (_loadedSaveDataCache.TryGetValue(fileName, out var save))
            {
                saveData = save as SaveData<T>;
                if (saveData is not null)
                {
                    saveData.value = value;
                    saveData.lastSaveTimeStamp = DateTime.Now.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    saveData = new SaveData<T>
                    {
                        value = value
                    };
                    Debug.LogWarning("Save Profile", $"{fileName} was previously saved with a different type!");
                }
            }
            else
            {
                saveData = new SaveData<T>
                {
                    value = value,
                    fileName = fileName,
                    createdTimeStamp = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                    lastSaveTimeStamp = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                    qualifiedType = typeof(SaveData<T>).AssemblyQualifiedName,
                    fileSystemVersion = FileSystem.Version,
                    applicationVersion = Application.version,
                    tags = options.Tags
                };
                _loadedSaveDataCache.Add(fileName, saveData);
            }

            var filePath = Path.Combine(profileFolderName, fileName);
            FileSystem.Storage.Save(filePath, saveData);
            var header = new Header
            {
                fileName = fileName,
                qualifiedTypeName = typeof(SaveData<T>).AssemblyQualifiedName
            };
            if (files.AddUnique(header))
            {
                var profileFilePath = ProfileFilePath;
                FileSystem.Validator.SanitizeFileName(ref profileFilePath);
                FileSystem.Storage.Save(profileFilePath, this);
            }
        }

        public void StoreFile<T>([NotNull] string fileName, T value, StoreOptions options = default)
        {
            if (fileName.IsNullOrWhitespace())
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            FileSystem.Validator.SanitizeFileName(ref fileName);

            SaveData<T> saveData;

            if (_loadedSaveDataCache.TryGetValue(fileName, out var save))
            {
                saveData = save as SaveData<T>;
                if (saveData is not null)
                {
                    saveData.value = value;
                    saveData.lastSaveTimeStamp = DateTime.Now.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    Debug.LogWarning("Save Profile", $"{fileName} was previously saved with a different type!");
                }
            }
            else
            {
                saveData = new SaveData<T>
                {
                    value = value,
                    fileName = fileName,
                    lastSaveTimeStamp = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                    createdTimeStamp = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                    qualifiedType = typeof(SaveData<T>).AssemblyQualifiedName,
                    fileSystemVersion = FileSystem.Version,
                    applicationVersion = Application.version,
                    tags = options.Tags
                };
                _loadedSaveDataCache.Add(fileName, saveData);
            }

            _dirtySaveDataKeys.Add(fileName);
            _loadedFileDataCache.Remove(fileName);
            var header = new Header
            {
                fileName = fileName,
                qualifiedTypeName = typeof(SaveData<T>).AssemblyQualifiedName
            };
            if (files.AddUnique(header))
            {
                Debug.Log("Save Profile", $"Added unique header for [{fileName}]");
                _isDirty = true;
            }
        }

        #endregion


        #region Load File

        public T LoadFile<T>([NotNull] string fileName, StoreOptions options = default)
        {
            if (fileName.IsNullOrWhitespace())
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            FileSystem.Validator.SanitizeFileName(ref fileName);

            if (_loadedFileDataCache.TryGetValue(fileName, out var data))
            {
                var saveData = data.Read<SaveData<T>>();
                _loadedSaveDataCache.Add(fileName, saveData);
                _loadedFileDataCache.Remove(fileName);
                var value = saveData.value;
                return value;
            }

            if (_loadedSaveDataCache.TryGetValue(fileName, out var file))
            {
                var value = file is SaveData<T> save ? save.value : default;
                return value;
            }

            return default;
        }

        public bool TryLoadFile<T>([NotNull] string fileName, out T value, StoreOptions options = default)
        {
            if (fileName.IsNullOrWhitespace())
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            FileSystem.Validator.SanitizeFileName(ref fileName);

            if (_loadedFileDataCache.TryGetValue(fileName, out var data))
            {
                var saveData = data.Read<SaveData<T>>();
                if (saveData is not null)
                {
                    _loadedSaveDataCache.AddOrUpdate(fileName, saveData);
                    _loadedFileDataCache.Remove(fileName);
                    value = saveData.value;
                    return true;
                }
            }

            if (_loadedSaveDataCache.TryGetValue(fileName, out var file))
            {
                if (file is SaveData<T> save)
                {
                    value = save.value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        #endregion


        #region Has & Delete File

        public bool HasFile([NotNull] string fileName)
        {
            if (fileName.IsNullOrWhitespace())
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            Assert.IsTrue(IsLoaded);
            FileSystem.Validator.SanitizeFileName(ref fileName);
            foreach (var header in files)
            {
                if (header.fileName == fileName)
                {
                    return true;
                }
            }

            return false;
        }

        public void DeleteFile([NotNull] string fileName)
        {
            if (fileName.IsNullOrWhitespace())
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            FileSystem.Validator.SanitizeFileName(ref fileName);
            _loadedSaveDataCache.Remove(fileName);
            _loadedFileDataCache.Remove(fileName);
            for (var index = files.Count - 1; index >= 0; index--)
            {
                if (files[index].fileName == fileName)
                {
                    files.RemoveAt(index);
                    _isDirty = true;
                    break;
                }
            }
            FileSystem.Storage.Delete(fileName);
            Save();
        }

        #endregion


        #region Profile Save, Load & Unload

        public void Save()
        {
            foreach (var key in _dirtySaveDataKeys)
            {
                var saveData = _loadedSaveDataCache[key];
                var fileName = saveData.fileName;
                var filePath = Path.Combine(profileFolderName, fileName);
                FileSystem.Storage.Save(filePath, saveData);
            }
            if (_isDirty)
            {
                var profileFilePath = ProfileFilePath;
                FileSystem.Validator.SanitizeFileName(ref profileFilePath);
                FileSystem.Storage.Save(profileFilePath, this);
            }
        }

        public async UniTask LoadAsync()
        {
            if (IsLoaded)
            {
                return;
            }
            foreach (var header in files)
            {
                if (header.fileName.IsNullOrWhitespace())
                {
                    Debug.LogError(FileSystem.Log, $"Filename [{header.fileName.ToNullString()}] is not invalid!");
                }

                var filePath = Path.Combine(profileFolderName, header.fileName);
                var type = Type.GetType(header.qualifiedTypeName);
                if (type != null && type.GetGenericTypeDefinition() == typeof(SaveData<>))
                {
                    var typedFileData = await FileSystem.Storage.LoadAsync(filePath, type);
                    _loadedSaveDataCache.AddOrUpdate(header.fileName, (SaveData)typedFileData.Read());
                }
                else
                {
                    var fileData = await FileSystem.Storage.LoadAsync(filePath);
                    _loadedFileDataCache.AddOrUpdate(header.fileName, fileData);
                }
            }
            IsLoaded = true;
        }

        public void Load()
        {
            if (IsLoaded)
            {
                return;
            }
            foreach (var header in files)
            {
                if (header.fileName.IsNullOrWhitespace())
                {
                    Debug.LogError(FileSystem.Log, $"Filename [{header.fileName.ToNullString()}] is not invalid!");
                }

                var filePath = Path.Combine(profileFolderName, header.fileName);
                var type = Type.GetType(header.qualifiedTypeName);
                if (type != null && type.GetGenericTypeDefinition() == typeof(SaveData<>))
                {
                    var typedFileData = FileSystem.Storage.Load(filePath, type);
                    _loadedSaveDataCache.AddOrUpdate(header.fileName, (SaveData)typedFileData.Read());
                }
                else
                {
                    var fileData = FileSystem.Storage.Load(filePath);
                    _loadedFileDataCache.AddOrUpdate(header.fileName, fileData);
                }
            }
            IsLoaded = true;
        }

        public void Unload()
        {
            _loadedSaveDataCache.Clear();
            _loadedFileDataCache.Clear();
            _dirtySaveDataKeys.Clear();
        }

        public void Reset()
        {
            files.Clear();
            _loadedSaveDataCache.Clear();
            _loadedFileDataCache.Clear();
            _dirtySaveDataKeys.Clear();
        }

        #endregion


        #region Constructor

        public SaveProfile(string displayName, string folderName, string fileName) : this()
        {
            profileDisplayName = displayName;
            profileFolderName = folderName;
            profileFileName = fileName;
            createdTimeStamp = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }

        private SaveProfile()
        {
            _loadedSaveDataCache = new Dictionary<string, SaveData>();
            _loadedFileDataCache = new Dictionary<string, FileData>();
            _dirtySaveDataKeys = new HashSet<string>();
            files = new List<Header>();
        }

        #endregion
    }
}