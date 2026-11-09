#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TitoX.Editor
{
    public class EditorUnusedAssetsCleaner : EditorWindow
    {
        readonly List<string> _unusedAssetPaths = new List<string>();
        Vector2 _scrollPos;
        bool _isScanning;

        bool _includeTextures = true;
        bool _includeAudio = true;

        [SerializeField]
        List<DefaultAsset> _targetFolders = new List<DefaultAsset>();

        SerializedObject _serializedObject;
        SerializedProperty _targetFoldersProperty;

        [MenuItem("Tools/Game Template/Unused Assets Cleaner")]
        static void OpenWindow()
        {
            var window = GetWindow<EditorUnusedAssetsCleaner>("Unused Assets Cleaner");
            window.minSize = new Vector2(350, 300);
            window.Show();
        }

        void OnEnable()
        {
            _serializedObject = new SerializedObject(this);
            _targetFoldersProperty = _serializedObject.FindProperty(nameof(_targetFolders));
        }

        void OnGUI()
        {
            _serializedObject.Update();

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("1. Target Scope Configuration", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Drag and drop target folders below.\nIf the list is EMPTY, the tool will scan the entire 'Assets/' folder.",
                MessageType.Info
            );

            EditorGUILayout.PropertyField(_targetFoldersProperty, new GUIContent("Target Folders"), true);
            _serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("2. Asset Type Filters", EditorStyles.boldLabel);
            _includeTextures = EditorGUILayout.Toggle("Scan Sprites / Textures", _includeTextures);
            _includeAudio = EditorGUILayout.Toggle("Scan Audio Clips (SFX / Music)", _includeAudio);

            EditorGUILayout.Space(8);
            if (GUILayout.Button("Scan for Unused Assets", GUILayout.Height(30)))
            {
                ScanUnusedAssets();
            }

            EditorGUILayout.Space(8);
            if (_unusedAssetPaths.Count > 0)
            {
                GUI.color = new Color(1f, 0.4f, 0.4f);
                if (GUILayout.Button($"DELETE ALL ({_unusedAssetPaths.Count} files)", GUILayout.Height(32)))
                {
                    if (EditorUtility.DisplayDialog("Confirm Deletion",
                        $"Are you sure you want to permanently delete {_unusedAssetPaths.Count} unused assets?\n\nPlease ensure your changes are committed to Git before deleting.",
                        "Delete Permanently", "Cancel"))
                    {
                        DeleteUnusedAssets();
                    }
                }
                GUI.color = Color.white;

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField($"Unreferenced Assets ({_unusedAssetPaths.Count}):", EditorStyles.boldLabel);

                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                foreach (var path in _unusedAssetPaths)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.TextField(path);
                    if (GUILayout.Button("Ping", GUILayout.Width(50)))
                    {
                        var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                        EditorGUIUtility.PingObject(obj);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
            else if (!_isScanning)
            {
                EditorGUILayout.LabelField("No unreferenced assets found within the selected scope.");
            }
        }

        void ScanUnusedAssets()
        {
            _isScanning = true;
            _unusedAssetPaths.Clear();

            var searchFolders = new List<string>();
            foreach (var folderAsset in _targetFolders)
            {
                if (folderAsset != null)
                {
                    string path = AssetDatabase.GetAssetPath(folderAsset);
                    if (AssetDatabase.IsValidFolder(path))
                        searchFolders.Add(path);
                }
            }

            if (searchFolders.Count == 0)
                searchFolders.Add("Assets");

            // Collect all potential dependency containers across the entire project
            string[] containerGuids = AssetDatabase.FindAssets("t:Scene t:Prefab t:ScriptableObject t:Material t:AnimatorController", new[] { "Assets" });
            var allUsedDependencies = new HashSet<string>();

            int totalContainers = containerGuids.Length;
            for (int i = 0; i < totalContainers; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(containerGuids[i]);
                EditorUtility.DisplayProgressBar("1/2: Collecting Project Dependencies", $"Analyzing: {Path.GetFileName(path)}", (float)i / totalContainers);

                string[] deps = AssetDatabase.GetDependencies(path, true);
                foreach (var dep in deps)
                    allUsedDependencies.Add(dep);
            }

            // Filter assets only inside the targeted folders
            var filterList = new List<string>();
            if (_includeTextures) filterList.Add("t:Texture2D");
            if (_includeAudio) filterList.Add("t:AudioClip");

            if (filterList.Count == 0)
            {
                EditorUtility.ClearProgressBar();
                _isScanning = false;
                return;
            }

            string filterString = string.Join(" ", filterList);
            string[] targetGuids = AssetDatabase.FindAssets(filterString, searchFolders.ToArray());

            int totalTargets = targetGuids.Length;
            for (int i = 0; i < totalTargets; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(targetGuids[i]);
                EditorUtility.DisplayProgressBar("2/2: Checking Unused Assets", $"Inspecting: {Path.GetFileName(assetPath)}", (float)i / totalTargets);

                if (!assetPath.StartsWith("Assets/")) continue;

                if (assetPath.Contains("/Resources/") ||
                    assetPath.Contains("/StreamingAssets/") ||
                    assetPath.Contains("/Editor/"))
                {
                    continue;
                }

                if (!allUsedDependencies.Contains(assetPath))
                    _unusedAssetPaths.Add(assetPath);
            }

            EditorUtility.ClearProgressBar();
            _isScanning = false;
        }

        void DeleteUnusedAssets()
        {
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var path in _unusedAssetPaths)
                    AssetDatabase.DeleteAsset(path);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();
            _unusedAssetPaths.Clear();
            EditorUtility.DisplayDialog("Complete", "Unreferenced assets have been successfully removed!", "OK");
        }
    }
}
#endif