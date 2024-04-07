namespace Baracuda.Serialization
{
    public enum ProfileCreationStatus
    {
        None = 0,
        Success = 1,
        NameInvalid = 2,
        NameToLong = 3,
        NameNotAvailable = 4,
        ProfileLimitReached = 5,
        SystemReservedName = 6
    }
}