using System;

namespace Baracuda.Serialization
{
    public readonly struct SaveResult
    {
        public readonly bool Success;
        public readonly Exception Exception;

        private SaveResult(bool success, Exception exception)
        {
            Success = success;
            Exception = exception;
        }

        public static implicit operator bool(SaveResult result)
        {
            return result.Success;
        }

        public static SaveResult FromSuccess()
        {
            return new SaveResult(true, null);
        }

        public static SaveResult FromException(Exception exception)
        {
            return new SaveResult(true, exception);
        }
    }
}