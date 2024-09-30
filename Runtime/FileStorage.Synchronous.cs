using System;
using System.IO;
using Baracuda.Utility.Reflection;
using UnityEngine.Assertions;

namespace Baracuda.Serialization
{
    public partial class FileStorage
    {
        #region Public

        public FileData<T> Load<T>(string key, in LoadArgs args = new())
        {
            return LoadInternal<T>(key);
        }

        public FileData<object> Load(string key, Type type, in LoadArgs args = new())
        {
            return LoadInternal(key, type);
        }

        public FileData Load(string key, in LoadArgs args = new())
        {
            return LoadInternal(key);
        }

        public SaveResult Save<T>(string key, T file, in SaveArgs args = new())
        {
            return SaveInternal(key, file);
        }

        public SaveResult Save(string key, object file, in SaveArgs args = new())
        {
            return SaveInternal(key, file);
        }

        public SaveResult Save(string key, string data, in SaveArgs args = new())
        {
            return SaveInternal(key, data);
        }

        public void Delete(string key)
        {
            DeleteInternal(key);
        }

        public void DeleteFolder(string folderName)
        {
            DeleteFolderInternal(folderName);
        }

        #endregion


        #region Loading

        private FileData<T> LoadInternal<T>(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_dataPath, fileName);

                var data = ReadInternal(filePath);

                var file = Json.FromJson<T>(data);

                return FileData<T>.FromSuccess(file);
            }
            catch (Exception exception)
            {
                return FileData<T>.FromException(exception);
            }
        }

        private FileData<object> LoadInternal(string fileName, Type type)
        {
            try
            {
                var filePath = Path.Combine(_dataPath, fileName);
                var data = ReadInternal(filePath);

                type ??= typeof(object);
                var file = Json.FromJson(data, type);

                return FileData<object>.FromSuccess(file);
            }
            catch (Exception exception)
            {
                return FileData<object>.FromException(exception);
            }
        }

        private FileData LoadInternal(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_dataPath, fileName);
                var data = ReadInternal(filePath);
                return FileData.FromSuccess(data);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return FileData.FromException(exception);
            }
        }

        #endregion


        #region Saving

        private SaveResult SaveInternal<T>(string fileName, T file)
        {
            try
            {
                Assert.IsTrue(file != null);
                Assert.IsTrue(file.GetType().IsUnitySerializable());

                var filePath = Path.Combine(_dataPath, fileName);

                var data = Json.ToJson(file);

                WriteInternal(new FileBuffer(filePath, data));
                return SaveResult.FromSuccess();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return SaveResult.FromException(exception);
            }
        }

        private SaveResult SaveInternal(string fileName, string data)
        {
            try
            {
                Assert.IsTrue(data != null);

                var filePath = Path.Combine(_dataPath, fileName);

                WriteInternal(new FileBuffer(filePath, data));
                return SaveResult.FromSuccess();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return SaveResult.FromException(exception);
            }
        }

        #endregion


        #region Delete

        private void DeleteInternal(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_dataPath, fileName);

                _fileOperations.DeleteFile(filePath);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private void DeleteFolderInternal(string path)
        {
            try
            {
                var folderPath = Path.Combine(_dataPath, path);

                _fileOperations.DeleteDirectory(folderPath);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        #endregion
    }
}