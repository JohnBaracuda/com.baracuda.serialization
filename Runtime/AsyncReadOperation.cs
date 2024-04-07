using System;
using System.Threading;
using System.Threading.Tasks;

namespace Baracuda.Serialization
{
    /// <summary>
    ///     Represents an asynchronous read operation that may be cancelled or force completed at any point
    /// </summary>
    internal class AsyncReadOperation
    {
        private static readonly LogCategory log = "IO";

        private readonly string _filePath;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly TaskCompletionSource<byte[]> _completionSource = new();
        private readonly IFileOperations _fileOperations;
        private bool _isReading;

        public AsyncReadOperation(string filePath, IFileOperations fileOperations)
        {
            _filePath = filePath;
            _fileOperations = fileOperations;
        }

        public void SetResult(byte[] result)
        {
            try
            {
                if (_completionSource.Task.IsCompleted)
                {
                    return;
                }

                _completionSource.SetResult(result);
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }
            catch (Exception exception)
            {
                Debug.LogException(log, exception);
            }
        }

        public async Task<byte[]> ReadAsync()
        {
            if (_completionSource.Task.IsCompleted || _isReading)
            {
                return await _completionSource.Task;
            }

            try
            {
                _isReading = true;
                Debug.Log("IO", $"Read async started: {_filePath}");
                var content = await _fileOperations.ReadAllBytesAsync(_filePath, _cancellationTokenSource.Token);
                Debug.Log("IO", $"Read async completed: {_filePath}");

                _completionSource.TrySetResult(content);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning(log, $"Read of {_filePath} was force completed!");
            }

            return await _completionSource.Task;
        }
    }
}