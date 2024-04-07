namespace Baracuda.Serialization
{
    public delegate void ProfileChangedDelegate(ISaveProfile profile);

    public delegate void ProfileCreatedDelegate(ISaveProfile profile, ProfileCreationArgs args);

    public delegate void ProfileDeletedDelegate(ISaveProfile profile);

    public delegate void ProfileResetDelegate(ISaveProfile profile);
}