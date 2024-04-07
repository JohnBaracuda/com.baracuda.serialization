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
        public void SaveFile<T>(string fileName, T value, StoreOptions options = default);

        /// <summary>
        ///     Store the file to the profile but don't save it yet.
        /// </summary>
        public void StoreFile<T>(string fileName, T value, StoreOptions options = default);

        /// <summary>
        ///     Load the file from the file profile.
        /// </summary>
        public T LoadFile<T>(string fileName, StoreOptions options = default);

        /// <summary>
        ///     Try to load the file from the file profile.
        /// </summary>
        public bool TryLoadFile<T>(string fileName, out T value, StoreOptions options = default);

        /// <summary>
        ///     Returns true if the profile has data for the file;
        /// </summary>
        public bool HasFile(string fileName);

        /// <summary>
        ///     Delete the file from the profile and the file system. Deleting a file will trigger a save.
        /// </summary>
        public void DeleteFile(string fileName);

        /// <summary>
        ///     Save stored files and the profile.
        /// </summary>
        public void Save();
    }
}