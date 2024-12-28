using System;
using System.Collections.Generic;
using System.Linq;
using Baracuda.Utility.Utilities;
using UnityEngine;
using GUIUtility = Baracuda.Utility.Editor.Utilities.GUIUtility;

namespace Baracuda.Serialization.Editor
{
    public class SaveDataEditorWindow : UnityEditor.EditorWindow
    {
        private Vector2 _scrollPosition;
        private bool _humanizeNames = true;
        private bool _showDefaultValues = true;
        private bool _showTags = true;
        private readonly List<SaveDataDrawer> _saveDataDrawers = new();
        private readonly List<UnityEditor.Editor> _editors = new();
        private string _searchFilter;

        [UnityEditor.MenuItem("Window/Save Data Editor", priority = 10_000)]
        public static void OpenWindow()
        {
            GetWindow<SaveDataEditorWindow>("Save Data");
        }

        private void OnEnable()
        {
            _saveDataDrawers.Clear();
            foreach (var saveData in SaveData.Instances)
            {
                _saveDataDrawers.Add(new SaveDataDrawer(saveData));
            }

            _humanizeNames = UnityEditor.EditorPrefs.GetBool("_humanizeNames", _humanizeNames);
            _showDefaultValues = UnityEditor.EditorPrefs.GetBool("_showDefaultValues", _showDefaultValues);
            _showTags = UnityEditor.EditorPrefs.GetBool("_showTags", _showTags);

            SaveData.Instances.Changed.AddListener(OnInstancesChanged);
        }

        private void OnDisable()
        {
            SaveData.Instances.Changed.RemoveListener(OnInstancesChanged);
            UnityEditor.EditorPrefs.SetBool("_humanizeNames", _humanizeNames);
            UnityEditor.EditorPrefs.SetBool("_showDefaultValues", _showDefaultValues);
            UnityEditor.EditorPrefs.SetBool("_showTags", _showTags);
        }

        private void OnInstancesChanged()
        {
            _saveDataDrawers.Clear();
            foreach (var saveData in SaveData.Instances)
            {
                _saveDataDrawers.Add(new SaveDataDrawer(saveData));
            }
        }

        private void OnGUI()
        {
            if (FileSystem.IsInitialized is false)
            {
                return;
            }

            if (GUIUtility.RefreshButton())
            {
                _saveDataDrawers.Clear();
                foreach (var saveData in SaveData.Instances)
                {
                    _saveDataDrawers.Add(new SaveDataDrawer(saveData));
                }
            }

            _humanizeNames = UnityEditor.EditorGUILayout.Toggle("Humanize Names", _humanizeNames);
            _showDefaultValues = UnityEditor.EditorGUILayout.Toggle("Show Default Values", _showDefaultValues);
            _showTags = UnityEditor.EditorGUILayout.Toggle("Show Tags", _showTags);
            UnityEditor.EditorGUILayout.Space();

            if (SaveData.Instances.Count == 0)
            {
                UnityEditor.EditorGUILayout.LabelField("No SaveData instances found.");
                return;
            }

            _searchFilter = GUIUtility.SearchBar(_searchFilter);
            var search = _searchFilter.IsNotNullOrWhitespace();
            _scrollPosition = UnityEditor.EditorGUILayout.BeginScrollView(_scrollPosition);

            GUIUtility.DrawLine();
            foreach (var drawer in _saveDataDrawers)
            {
                if (search)
                {
                    if (!drawer.Label.Contains(_searchFilter, StringComparison.InvariantCultureIgnoreCase) &&
                        !drawer.HumanizedLabel.Contains(_searchFilter, StringComparison.InvariantCultureIgnoreCase) &&
                        !drawer.TypeName.Contains(_searchFilter, StringComparison.InvariantCultureIgnoreCase) &&
                        !(drawer.SaveData.FileOptions.Tags?.Any(tag => tag.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase)) ?? false))
                    {
                        continue;
                    }
                }

                UnityEditor.EditorGUIUtility.labelWidth = 300;
                drawer.DrawValue(_humanizeNames);
                if (_showDefaultValues)
                {
                    drawer.DrawDefaultValue();
                }
                if (_showTags)
                {
                    drawer.DrawTags();
                }
                GUIUtility.DrawLine();
            }
            UnityEditor.EditorGUILayout.EndScrollView();
        }
    }
}