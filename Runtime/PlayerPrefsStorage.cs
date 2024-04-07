using Cysharp.Threading.Tasks;
using System;
using System.IO;
using UnityEngine;

namespace Baracuda.Serialization
{
    public partial class PlayerPrefsStorage : IFileStorage
    {
        #region Properties

        public bool ForceSynchronous { get; private set; }

        #endregion


        #region Setup

        private string _root;

        public void Initialize(in FileStorageArguments args)
        {
            _root = args.RootFolder;
            ForceSynchronous = args.ForceSynchronous;
        }

        #endregion


        #region Async

        public UniTask<FileData<T>> LoadAsync<T>(string key, LoadArgs args = new())
        {
            var result = Load<T>(key, args);
            return UniTask.FromResult(result);
        }

        public UniTask<FileData<object>> LoadAsync(string key, Type type, LoadArgs args = new())
        {
            var result = Load(key, type, args);
            return UniTask.FromResult(result);
        }

        public UniTask<FileData> LoadAsync(string key, LoadArgs args = new())
        {
            var result = Load(key, args);
            return UniTask.FromResult(result);
        }

        public UniTask DeleteAsync(string key)
        {
            Delete(key);
            return UniTask.CompletedTask;
        }

        public UniTask DeleteFolderAsync(string folderName)
        {
            DeleteFolder(folderName);
            return UniTask.CompletedTask;
        }

        #endregion


        #region Sync

        public FileData<T> Load<T>(string key, in LoadArgs args = new())
        {
            try
            {
                var path = Path.Combine(_root, key);

                if (PlayerPrefs.HasKey(path) is false)
                {
                    return FileData<T>.FromFailure();
                }

                var data = PlayerPrefs.GetString(path);
                Debug.Log("IO", $"Loading data from path {path}");

                var file = Json.FromJson<T>(data);
                return FileData<T>.FromSuccess(file);
            }
            catch (Exception exception)
            {
                return FileData<T>.FromException(exception);
            }
        }

        public FileData<object> Load(string key, Type type, in LoadArgs args = new())
        {
            try
            {
                var path = Path.Combine(_root, key);

                if (PlayerPrefs.HasKey(path) is false)
                {
                    return FileData<object>.FromFailure();
                }

                type ??= typeof(object);
                var data = PlayerPrefs.GetString(path);
                var file = Json.FromJson(data, type);
                Debug.Log("IO", $"Loading data from path {path}");

                return FileData<object>.FromSuccess(file);
            }
            catch (Exception exception)
            {
                return FileData<object>.FromException(exception);
            }
        }

        public FileData Load(string key, in LoadArgs args = new())
        {
            try
            {
                var path = Path.Combine(_root, key);

                if (PlayerPrefs.HasKey(path) is false)
                {
                    return FileData.FromFailure();
                }

                var data = PlayerPrefs.GetString(path);
                Debug.Log("IO", $"Loading data from path {path}");

                return FileData.FromSuccess(data);
            }
            catch (Exception exception)
            {
                return FileData.FromException(exception);
            }
        }

        public SaveResult Save<T>(string key, T file, in SaveArgs args = new())
        {
            try
            {
                var path = Path.Combine(_root, key);
                var data = Json.ToJson(file);

                PlayerPrefs.SetString(path, data);
                Debug.Log("IO", $"Saving data to path {path}");

                return SaveResult.FromSuccess();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return SaveResult.FromException(exception);
            }
        }

        public SaveResult Save(string key, object file, in SaveArgs args = new())
        {
            try
            {
                var path = Path.Combine(_root, key);
                var data = Json.ToJson(file);

                PlayerPrefs.SetString(path, data);
                Debug.Log("IO", $"Saving data to path {path}");

                return SaveResult.FromSuccess();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return SaveResult.FromException(exception);
            }
        }

        public SaveResult Save(string key, string data, in SaveArgs args = new())
        {
            try
            {
                var path = Path.Combine(_root, key);

                PlayerPrefs.SetString(path, data);
                Debug.Log("IO", $"Saving data to path {path}");

                return SaveResult.FromSuccess();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return SaveResult.FromException(exception);
            }
        }

        public void SaveBackend()
        {
            PlayerPrefs.Save();
        }

        public void Delete(string key)
        {
            var path = Path.Combine(_root, key);
            PlayerPrefs.DeleteKey(path);
        }

        public void DeleteFolder(string folderName)
        {
        }

        #endregion


        #region Shutdown

        public void Shutdown(FileSystemShutdownArgs args)
        {
            PlayerPrefs.Save();
        }

        public UniTask ShutdownAsync(FileSystemShutdownArgs args)
        {
            Shutdown(args);
            return UniTask.CompletedTask;
        }

        #endregion
    }
}