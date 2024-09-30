using System.Collections.Generic;
using Baracuda.Utility.Editor.Drawer;
using Cysharp.Threading.Tasks;
using UnityEngine;
using GUIUtility = Baracuda.Utility.Editor.Utilities.GUIUtility;

namespace Baracuda.Serialization.Editor
{
    public class FileSystemSettingsProvider : UnityEditor.SettingsProvider
    {
        private UnityEditor.SerializedObject _serializedObject;
        private UnityEditor.SerializedObject _argsObject;
        private UnityEditor.SerializedProperty _argsProperty;
        private UnityEditor.Editor _argsEditor;

        private UnityEditor.SerializedObject _shutdownArgsObject;
        private UnityEditor.SerializedProperty _shutdownArgsProperty;

        private FoldoutHandler Foldout { get; } = new(nameof(FileSystemSettingsProvider));

        public FileSystemSettingsProvider(string path, UnityEditor.SettingsScope scopes,
            IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);

            _serializedObject ??= new UnityEditor.SerializedObject(FileSystemEditorSettings.instance);

            UnityEditor.EditorGUILayout.Space(20);

            var initializationFlags = FileSystemEditorSettings.instance.InitializationFlags;
            initializationFlags =
                (InitializeFlags)UnityEditor.EditorGUILayout.EnumFlagsField("Initialization", initializationFlags);
            FileSystemEditorSettings.instance.InitializationFlags = initializationFlags;

            var shutdownFlags = FileSystemEditorSettings.instance.ShutdownFlags;
            shutdownFlags = (ShutdownFlags)UnityEditor.EditorGUILayout.EnumFlagsField("Shutdown", shutdownFlags);
            FileSystemEditorSettings.instance.ShutdownFlags = shutdownFlags;

            GUI.enabled = false;
            UnityEditor.EditorGUILayout.TextField("File System State", FileSystem.State.ToString());
            UnityEditor.EditorGUILayout.TextField("File System Root", FileSystem.RootFolder);
            GUI.enabled = true;

            UnityEditor.EditorGUILayout.Space();
            DrawButtons();
            UnityEditor.EditorGUILayout.Space();

            var labelWidth = UnityEditor.EditorGUIUtility.labelWidth;
            UnityEditor.EditorGUIUtility.labelWidth = 230;
            if (Foldout["Editor Settings"])
            {
                UnityEditor.EditorGUILayout.Space();
                DrawSetupArguments();
                UnityEditor.EditorGUILayout.Space();
            }
            UnityEditor.EditorGUIUtility.labelWidth = labelWidth;

            FileSystemEditorSettings.instance.SaveSettings();

            if (GUI.changed)
            {
                Foldout.SaveState();
            }
        }

        private void DrawButtons()
        {
            GUILayout.BeginHorizontal("HelpBox");
            GUI.enabled = FileSystem.State == FileSystemState.Uninitialized;
            if (GUILayout.Button("Initialize"))
            {
                FileSystem.InitializeAsync(FileSystemEditorSettings.instance.Settings).Forget();
            }
            GUI.enabled = FileSystem.State == FileSystemState.Initialized;
            if (GUILayout.Button("Shutdown"))
            {
                FileSystem.ShutdownAsync();
            }
            GUI.enabled = true;
            if (GUILayout.Button("Storage"))
            {
                Application.OpenURL(Application.persistentDataPath);
            }
            GUILayout.EndHorizontal();
        }

        private void DrawSetupArguments()
        {
            var arguments = FileSystemEditorSettings.instance.FileSystemSettings;
            arguments = (FileSystemSettingsAsset)UnityEditor.EditorGUILayout.ObjectField("Settings", arguments, typeof(FileSystemSettingsAsset), false);

            FileSystemEditorSettings.instance.FileSystemSettings = arguments;

            if (arguments != null)
            {
                _argsEditor ??= UnityEditor.Editor.CreateEditor(arguments);
            }
            else
            {
                _argsEditor = null;
            }

            if (_argsEditor != null)
            {
                GUIUtility.IncreaseIndent();
                GUIUtility.Space();
                _argsEditor.OnInspectorGUI();
                GUIUtility.Space();
                GUIUtility.DecreaseIndent();
                UnityEditor.EditorUtility.SetDirty(_argsEditor);
            }
        }

        [UnityEditor.SettingsProviderAttribute]
        public static UnityEditor.SettingsProvider CreateSettingsProvider()
        {
            return new FileSystemSettingsProvider("Project/Baracuda/File System", UnityEditor.SettingsScope.Project);
        }

        [UnityEditor.MenuItem("Tools/File System/Settings", priority = 5000)]
        public static void OpenSettings()
        {
            UnityEditor.SettingsService.OpenProjectSettings("Project/Baracuda/File System");
        }

        [UnityEditor.MenuItem("Tools/File System/Open Persistent Data Path", priority = 5000)]
        public static void OpenSettingsPath()
        {
            Application.OpenURL(Application.persistentDataPath);
        }

        [UnityEditor.MenuItem("Tools/File System/Initialize", priority = 5001)]
        private static void InitializeFileSystem()
        {
            FileSystem.Initialize();
        }

        [UnityEditor.MenuItem("Tools/File System/Shutdown", priority = 5002)]
        private static void ShutdownFileSystem()
        {
            FileSystem.Shutdown();
        }
    }
}