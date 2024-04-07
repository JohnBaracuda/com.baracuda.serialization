using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Baracuda.Serialization
{
    public static class Json
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T FromJson<T>(string data)
        {
            return JsonUtility.FromJson<T>(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object FromJson(string data, Type type)
        {
            return JsonUtility.FromJson(data, type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToJson(object value)
        {
            return JsonUtility.ToJson(value, true);
        }
    }
}