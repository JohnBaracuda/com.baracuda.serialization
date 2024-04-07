using Baracuda.Utilities.Pools;
using System;
using System.Collections.Generic;

namespace Baracuda.Serialization
{
    public readonly ref struct SaveProfileData
    {
        public readonly string ProfileDataPath;
        public readonly string DisplayName;
        public readonly string FolderName;
        public readonly DateTime CreatedTimeStamp;
        internal readonly IReadOnlyList<Header> FileHeaders;

        internal SaveProfileData(string displayName, string folderName, DateTime createdTimeStamp,
            string profileDataPath, IReadOnlyList<Header> headers)
        {
            DisplayName = displayName;
            FolderName = folderName;
            CreatedTimeStamp = createdTimeStamp;
            ProfileDataPath = profileDataPath;
            FileHeaders = headers;
        }

        public override string ToString()
        {
            var builder = StringBuilderPool.Get();

            builder.Append("Display Name: ");
            builder.Append(DisplayName);
            builder.Append('\n');

            builder.Append("Folder Name: ");
            builder.Append(FolderName);
            builder.Append('\n');

            builder.Append("Profile Data Path: ");
            builder.Append(ProfileDataPath);
            builder.Append('\n');

            builder.Append("File Count: ");
            builder.Append(FileHeaders.Count);
            builder.Append('\n');

            builder.Append("Created Time: ");
            builder.Append(CreatedTimeStamp);

            return StringBuilderPool.BuildAndRelease(builder);
        }
    }
}