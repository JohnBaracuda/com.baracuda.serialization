using Cysharp.Threading.Tasks;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Baracuda.Serialization
{
    public partial class FileStorage
    {
        #region Public

        public bool ForceSynchronous { get; private set; }

        public UniTask<FileData<T>> LoadAsync<T>(string key, LoadArgs args = new())
        {
            return LoadAsyncInternal<T>(key).AsUniTask();
        }

        public UniTask<FileData<object>> LoadAsync(string key, Type type, LoadArgs args = new())
        {
            return LoadAsyncInternal(key, type).AsUniTask();
        }

        public UniTask<FileData> LoadAsync(string key, LoadArgs args = new())
        {
            return LoadAsyncInternal(key).AsUniTask();
        }

        public UniTask DeleteAsync(string fileName)
        {
            Delete(fileName);
            return UniTask.CompletedTask;
        }

        public UniTask DeleteFolderAsync(string folderName)
        {
            return DeleteFolderAsyncInternal(folderName).AsUniTask();
        }

        public void SaveBackend()
        {
            _fileOperations.Save();
        }

        #endregion


        #region Loading

        private async Task<FileData<T>> LoadAsyncInternal<T>(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_dataPath, fileName);

                var data = await ReadInternalAsync(filePath);
                if (data == null)
                {
                    return FileData<T>.FromFailure();
                }

                var file = Json.FromJson<T>(data);

                return FileData<T>.FromSuccess(file);
            }
            catch (Exception exception)
            {
                LogExceptionInternal(exception);
                return FileData<T>.FromException(exception);
            }
        }

        private async Task<FileData<object>> LoadAsyncInternal(string fileName, Type type)
        {
            try
            {
                var filePath = Path.Combine(_dataPath, fileName);

                var data = await ReadInternalAsync(filePath);
                if (data == null)
                {
                    return FileData<object>.FromFailure();
                }
                type ??= typeof(object);

                var file = Json.FromJson(data, type);

                return FileData<object>.FromSuccess(file);
            }
            catch (Exception exception)
            {
                LogExceptionInternal(exception);
                return FileData<object>.FromException(exception);
            }
        }

        private async Task<FileData> LoadAsyncInternal(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_dataPath, fileName);
                var data = await ReadInternalAsync(filePath);
                return data == null ? FileData.FromFailure() : FileData.FromSuccess(data);
            }
            catch (Exception exception)
            {
                LogExceptionInternal(exception);
                return FileData.FromException(exception);
            }
        }

        #endregion


        #region Meta Information

        private Task DeleteFolderAsyncInternal(string path)
        {
            try
            {
                var folderPath = Path.Combine(_dataPath, path);
                _fileOperations.DeleteDirectory(folderPath);
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return Task.CompletedTask;
            }
        }

        #endregion
    }
}