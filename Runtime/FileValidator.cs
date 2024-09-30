using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Baracuda.Utility.Utilities;
using UnityEngine;

namespace Baracuda.Serialization
{
    public class FileValidator
    {
        private readonly string _extension;
        private readonly string[] _availableExtensions;
        private readonly bool _skipFileEndingsArray;
        private readonly bool _logMissingFileExtensionWarning;

        internal FileValidator(IFileSystemSettings settings)
        {
            _extension = IsValidFileEnding(settings.FileEnding) ? settings.FileEnding : ".sav";
            _skipFileEndingsArray = !settings.EnforceFileEndings.Enabled;
            _availableExtensions = settings.EnforceFileEndings.ValueOrDefault(Array.Empty<string>());
            ArrayUtility.Add(ref _availableExtensions, _extension);
            _logMissingFileExtensionWarning = settings.LogMissingFileExtensionWarning;
        }


        #region Profile Name

        public bool IsValidProfileName(string input)
        {
            foreach (var c in input)
            {
                if (!char.IsLetterOrDigit(c) && c != ' ')
                {
                    return false;
                }
            }
            return true;
        }

        #endregion


        #region File Ending

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SanitizeFileName(ref string fileName)
        {
            var fileExtension = Path.GetExtension(fileName);

            if (fileExtension.IsNullOrWhitespace())
            {
                if (_logMissingFileExtensionWarning)
                {
                    Debug.LogWarning(FileSystem.Log, $"File Name: {fileName} has no extension! Adding {_extension}");
                }
                fileName = CombineStringsNoAlloc(fileName, _extension);
                return;
            }

            if (_skipFileEndingsArray)
            {
                return;
            }

            if (_availableExtensions.Contains(fileExtension))
            {
                return;
            }

            Debug.LogWarning(FileSystem.Log,
                $"File Name: {fileName} has an invalid extension! replacing {fileExtension} with {_extension}");
            fileName = Path.ChangeExtension(fileName, _extension);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsValidFileEnding(string fileEnding)
        {
            if (fileEnding.IsNullOrWhitespace())
            {
                return false;
            }
            var pattern = @"^\.[a-zA-Z0-9]+$";
            var isValid = Regex.IsMatch(fileEnding, pattern);

            return isValid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CombineStringsNoAlloc(string string1, string string2)
        {
            var categoryBuffer = (ReadOnlySpan<char>)string1;
            var messageBuffer = (ReadOnlySpan<char>)string2;

            Span<char> resultBuffer = stackalloc char[categoryBuffer.Length + messageBuffer.Length];

            var index = 0;
            categoryBuffer.CopyTo(resultBuffer.Slice(index, categoryBuffer.Length));
            index += categoryBuffer.Length;
            messageBuffer.CopyTo(resultBuffer.Slice(index, messageBuffer.Length));

            return resultBuffer.ToString();
        }

        #endregion


        #region Scriptable Object

        [Conditional("DEBUG")]
        public void ValidateScriptableObjectData(string data, ScriptableObject asset)
        {
            if (data.Contains("instanceID"))
            {
                Debug.LogWarning(FileSystem.Log, $"Saved scriptable object [{asset}] with runtime asset reference!",
                    asset);
            }
        }

        #endregion
    }
}