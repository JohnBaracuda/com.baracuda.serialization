namespace Baracuda.Serialization
{
    public readonly struct ProfileCreationResult
    {
        public readonly ISaveProfile Profile;
        public readonly ProfileCreationStatus Status;

        public bool Success => Status == ProfileCreationStatus.Success;

        public ProfileCreationResult(ISaveProfile profile, ProfileCreationStatus status)
        {
            Profile = profile;
            Status = status;
        }

        public ProfileCreationResult(ProfileCreationStatus status)
        {
            Profile = null;
            Status = status;
        }

        internal static ProfileCreationResult NameInvalid => new(ProfileCreationStatus.NameInvalid);
        internal static ProfileCreationResult NameToLong => new(ProfileCreationStatus.NameToLong);
        internal static ProfileCreationResult NameNotAvailable => new(ProfileCreationStatus.NameNotAvailable);
        internal static ProfileCreationResult ProfileLimitReached => new(ProfileCreationStatus.ProfileLimitReached);
        internal static ProfileCreationResult SystemReservedName => new(ProfileCreationStatus.SystemReservedName);
    }
}