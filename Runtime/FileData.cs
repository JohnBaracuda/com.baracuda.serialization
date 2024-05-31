using System;
using UnityEngine;

namespace Baracuda.Serialization
{
    /// <summary>
    ///     Stores file value with type information.
    /// </summary>
    public readonly struct FileData<T>
    {
        public readonly bool IsValid;
        private readonly T _value;
        public readonly Exception Exception;

        public T Read()
        {
            return _value;
        }

        public bool TryRead(out T result)
        {
            result = _value;
            return IsValid;
        }

        public FileData(T value, bool isValid, Exception exception)
        {
            _value = value;
            IsValid = isValid && value != null;
            Exception = exception;
        }

        public static implicit operator T(FileData<T> fileData)
        {
            return fileData._value;
        }

        public static implicit operator bool(FileData<T> fileData)
        {
            return fileData.IsValid;
        }

        public static FileData<T> FromFailure()
        {
            return new FileData<T>(default(T), false, null);
        }

        public static FileData<T> FromSuccess(T value)
        {
            return new FileData<T>(value, true, null);
        }

        public static FileData<T> FromException(Exception exception)
        {
            return new FileData<T>(default(T), false, exception);
        }
    }

    /// <summary>
    ///     Stores file value without type information.
    /// </summary>
    public readonly struct FileData
    {
        public readonly bool IsValid;
        public readonly string RawData;
        public readonly Exception Exception;

        public T Read<T>()
        {
            return JsonUtility.FromJson<T>(RawData);
        }

        public bool TryRead<T>(out T result)
        {
            if (!IsValid)
            {
                result = default(T);
                return false;
            }
            result = Read<T>();
            return true;
        }

        private FileData(string rawData, bool isValid, Exception exception)
        {
            RawData = rawData;
            IsValid = isValid;
            Exception = exception;
        }

        public static implicit operator bool(FileData result)
        {
            return result.IsValid;
        }

        public static FileData FromFailure()
        {
            return new FileData(default(string), false, null);
        }

        public static FileData FromSuccess(string rawData)
        {
            return new FileData(rawData, true, null);
        }

        public static FileData FromException(Exception exception)
        {
            return new FileData(null, false, exception);
        }
    }
}