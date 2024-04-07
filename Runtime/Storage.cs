using System;

namespace Baracuda.Serialization
{
    [Serializable]
    public struct Storage<T>
    {
        public T value;

        public Storage(T value)
        {
            this.value = value;
        }
    }

    [Serializable]
    public class ManagedStorage<T>
    {
        public T value;

        public ManagedStorage(T value)
        {
            this.value = value;
        }

        public ManagedStorage()
        {
        }
    }
}