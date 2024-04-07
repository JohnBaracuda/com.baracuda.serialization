using Baracuda.Utilities;
using Baracuda.Utilities.Types;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Baracuda.Serialization
{
    public partial class FileStorage : IFileStorage
    {
        #region Fields

        private string _dataPath;
        private string _encryptionKey;
        private IEncryptionProvider _encryptionProvider;
        private IFileOperations _fileOperations;
        private LoggingLevel _exceptionLogging;

        private static readonly DictionaryQueue<string, FileBuffer> writeQueue = new();
        private static FileBuffer? writeBuffer;
        private static bool isWritingAsync;
        private static CancellationTokenSource writeCancellationSource = new();
        private static readonly Dictionary<string, AsyncReadOperation> readOperations = new();

        #endregion


        #region Setup

        public void Initialize(in FileStorageArguments args)
        {
            Debug.Log("Storage", "Initialization Started");
            ForceSynchronous = args.ForceSynchronous;
            _encryptionKey = args.EncryptionKey;
            _exceptionLogging = args.ExceptionLogging;
            _encryptionProvider = args.EncryptionProvider ?? new DefaultEncryptionProvider();
            var dataPath = Application.persistentDataPath;
#if UNITY_PS5 && !UNITY_EDITOR
            dataPath = "/savedata";
#endif
            _dataPath = Path.Combine(dataPath, args.RootFolder);
            _fileOperations = args.FileOperations;
            _fileOperations.Initialize();
            _fileOperations.CreateDirectory(_dataPath);
            Debug.Log("Storage", "Initialization Completed");
        }

        #endregion


        #region Shutdown

        public void Shutdown(FileSystemShutdownArgs args)
        {
            ShutdownInternal(args);
        }

        public async UniTask ShutdownAsync(FileSystemShutdownArgs args)
        {
            await ShutdownInternalAsync(args);
        }

        #endregion


        #region Flush

        private void FlushWriteOperations()
        {
            Debug.Log("File System", $"Flushing ({writeQueue.Count + (isWritingAsync ? 1 : 0)}) Write Operations");

            writeCancellationSource.Cancel();
            writeCancellationSource.Dispose();
            writeCancellationSource = new CancellationTokenSource();

            if (isWritingAsync && writeBuffer.HasValue)
            {
                WriteBufferSynchronous(writeBuffer.Value);
            }

            var queuedFiles = writeQueue.Values.ToArray();
            foreach (var buffer in queuedFiles)
            {
                WriteBufferSynchronous(buffer);
            }
        }

        private void FlushReadOperations()
        {
            Debug.Log("File System", $"Flushing ({readOperations.Count}) Read Operations");

            var readOperationPaths = readOperations.Keys.ToArray();
            foreach (var operationPath in readOperationPaths)
            {
                ReadInternal(operationPath);
            }
        }

        #endregion


        #region Read

        private async Task<string> ReadInternalAsync(string filePath)
        {
            if (ForceSynchronous)
            {
                return ReadInternal(filePath);
            }

            if (writeBuffer.HasValue && writeBuffer.Value.FilePath == filePath)
            {
                return writeBuffer.Value.FileData;
            }

            if (writeQueue.TryGetValue(filePath, out var buffer))
            {
                return buffer.FileData;
            }

            if (!readOperations.TryGetValue(filePath, out var readOperation))
            {
                readOperation = new AsyncReadOperation(filePath, _fileOperations);
            }

            var bytes = await readOperation.ReadAsync();
            var result = _encryptionProvider.Decrypt(bytes, _encryptionKey);

            readOperations.TryRemove(filePath);

            return result;
        }

        private string ReadInternal(string filePath)
        {
            if (writeBuffer.HasValue && writeBuffer.Value.FilePath == filePath)
            {
                return writeBuffer.Value.FileData;
            }

            if (writeQueue.TryGetValue(filePath, out var buffer))
            {
                return buffer.FileData;
            }

            Debug.Log("IO", $"Start Reading {filePath}");
            var bytes = _fileOperations.ReadAllBytes(filePath);
            var result = _encryptionProvider.Decrypt(bytes, _encryptionKey);
            Debug.Log("IO", $"Stop Reading {filePath}");

            if (readOperations.TryRemove(filePath, out var readOperation))
            {
                readOperation.SetResult(bytes);
            }

            return result;
        }

        #endregion


        #region Write Queue

        private void WriteInternal(FileBuffer fileBuffer)
        {
            if (ForceSynchronous)
            {
                WriteBufferSynchronous(fileBuffer);
                return;
            }

            writeQueue.Update(fileBuffer.FilePath, fileBuffer);
            UpdateWriteQueue();
        }

        private async void UpdateWriteQueue()
        {
            try
            {
                if (writeBuffer.HasValue)
                {
                    return;
                }

                if (!writeQueue.TryDequeue(out var buffer))
                {
                    return;
                }

                var filePath = buffer.FilePath;
                var fileData = buffer.FileData;
                var bytes = _encryptionProvider.Encrypt(fileData, _encryptionKey);

                if (readOperations.TryGetValue(filePath, out var readOperation))
                {
                    readOperation.SetResult(bytes);
                    readOperations.TryRemove(filePath);
                }

                var directoryName = Path.GetDirectoryName(filePath)!;
                _fileOperations.CreateDirectory(directoryName);

                writeBuffer = buffer;
                isWritingAsync = true;

                Debug.Log("IO", $"Start Writing Async {filePath}");
                await _fileOperations.WriteAllBytesAsync(filePath, bytes, writeCancellationSource.Token)
                    .TimeoutAsync(TimeSpan.FromSeconds(5));
                Debug.Log("IO", $"Stop Writing Async {filePath}");

                isWritingAsync = false;
                writeBuffer = null;

                UpdateWriteQueue();
            }
            catch (TimeoutException timeoutException)
            {
                Debug.LogException(timeoutException);
                isWritingAsync = false;
                writeBuffer = null;
                UpdateWriteQueue();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private void WriteBufferSynchronous(FileBuffer buffer)
        {
            try
            {
                var filePath = buffer.FilePath;
                var fileData = buffer.FileData;
                var bytes = _encryptionProvider.Encrypt(fileData, _encryptionKey);

                var directoryName = Path.GetDirectoryName(filePath)!;
                _fileOperations.CreateDirectory(directoryName);

                if (readOperations.TryGetValue(filePath, out var readOperation))
                {
                    readOperation.SetResult(bytes);
                    readOperations.TryRemove(filePath);
                }

                Debug.Log("IO", $"Start Writing {filePath}");
                _fileOperations.WriteAllBytes(filePath, bytes);
                Debug.Log("IO", $"Stop Writing {filePath}");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        #endregion


        #region Shutdown Internal

        private void ShutdownInternal(FileSystemShutdownArgs args)
        {
            FlushReadOperations();
            FlushWriteOperations();
        }

        private async UniTask ShutdownInternalAsync(FileSystemShutdownArgs args)
        {
            if (args.forceSynchronousShutdown)
            {
                ShutdownInternal(args);
                return;
            }

            FlushReadOperations();
            FlushWriteOperations();

            await UniTask.Yield();
        }

        #endregion


        #region Logging

        private void LogExceptionInternal(Exception exception)
        {
            switch (_exceptionLogging)
            {
                case LoggingLevel.None:
                    break;
                case LoggingLevel.Message:
                    Debug.Log("Storage", exception.ToString());
                    break;
                case LoggingLevel.Warning:
                    Debug.LogWarning("Storage", exception.ToString());
                    break;
                case LoggingLevel.Error:
                    Debug.LogError("Storage", exception.ToString());
                    break;
                case LoggingLevel.Exception:
                    Debug.LogException("Storage", exception);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}