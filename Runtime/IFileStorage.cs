using Cysharp.Threading.Tasks;
using System;

namespace Baracuda.Serialization
{
    public interface IFileStorage
    {
        #region Properties

        /// <summary>
        ///     When enabled the file storage will force synchronous operations
        /// </summary>
        public bool ForceSynchronous { get; }

        #endregion


        #region Asynchronous

        UniTask<FileData<T>> LoadAsync<T>(string key, LoadArgs args = new());

        UniTask<FileData<object>> LoadAsync(string key, Type type, LoadArgs args = new());

        UniTask<FileData> LoadAsync(string key, LoadArgs args = new());

        #endregion


        #region Asynchronous Delete And Sate

        UniTask DeleteAsync(string key);

        UniTask DeleteFolderAsync(string folderName);

        #endregion


        #region Synchronous Loading

        FileData<T> Load<T>(string key, in LoadArgs args = new());

        FileData<object> Load(string key, Type type, in LoadArgs args = new());

        FileData Load(string key, in LoadArgs args = new());

        #endregion


        #region Synchronous Saving

        SaveResult Save<T>(string key, T file, in SaveArgs args = new());

        SaveResult Save(string key, object file, in SaveArgs args = new());

        SaveResult Save(string key, string data, in SaveArgs args = new());

        void SaveBackend();

        #endregion


        #region Synchronous Delete And Sate

        void Delete(string key);

        void DeleteFolder(string folderName);

        #endregion


        #region Setup

        public void Initialize(in FileStorageArguments args);

        #endregion


        #region Shutdown

        public void Shutdown(FileSystemShutdownArgs args);

        public UniTask ShutdownAsync(FileSystemShutdownArgs args);

        #endregion
    }
}