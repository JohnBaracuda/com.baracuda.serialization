using System;
using System.Runtime.CompilerServices;
using Baracuda.Utility.Utilities;

namespace Baracuda.Serialization
{
    public static class SaveDataExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToIntValueString<T>(this SaveData<T> saveData) where T : unmanaged, Enum
        {
            return EnumUtility.ToInt(saveData.Value).ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToIntValue<T>(this SaveData<T> saveData) where T : unmanaged, Enum
        {
            return EnumUtility.ToInt(saveData.Value);
        }
    }
}