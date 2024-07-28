using JetBrains.Annotations;

namespace Baracuda.Serialization
{
    public interface ISaveProfile
    {
        /// <summary>
        ///     Information about the save profile.
        /// </summary>
        public SaveProfileData Info { get; }

        /// <summary>
        ///     Store and save a file to the profile.
        /// </summary>
        public void SaveFile<T>([NotNull] string fileName, T value, StoreOptions options = default);

        /// <summary>
        ///     Store the file to the profile but don't save it yet.
        /// </summary>
        public void StoreFile<T>([NotNull] string fileName, T value, StoreOptions options = default);

        /// <summary>
        ///     Load the file from the file profile.
        /// </summary>
        public T LoadFile<T>([NotNull] string fileName, StoreOptions options = default);

        /// <summary>
        ///     Try to load the file from the file profile.
        /// </summary>
        public bool TryLoadFile<T>([NotNull] string fileName, out T value, StoreOptions options = default);

        /// <summary>
        ///     Returns true if the profile has value for the file;
        /// </summary>
        public bool HasFile([NotNull] string fileName);

        /// <summary>
        ///     Delete the file from the profile and the file system. Deleting a file will trigger a save.
        /// </summary>
        public void DeleteFile([NotNull] string fileName);

        /// <summary>
        ///     Save stored files and the profile.
        /// </summary>
        public void Save();
    }
}