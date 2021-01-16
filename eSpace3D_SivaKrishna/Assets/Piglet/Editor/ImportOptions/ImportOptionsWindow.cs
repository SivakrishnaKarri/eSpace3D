using System;
using UnityEditor;
using UnityEngine;

namespace Piglet
{
    /// <summary>
    /// Editor window for setting import options regarding
    /// drag-and-drop import of .gltf/.glb/.zip files in
    /// the Project Browser.
    /// </summary>
    public class OptionsWindow : EditorWindow
    {
        private ImportOptions _importOptions;

        private class Styles
        {
            public GUIStyle Button;
            public GUIStyle Title;
            public GUIStyle ToggleLevel1;
            public GUIStyle ToggleLevel2;
            public GUIStyle ToggleLevel3;
        }

        private Styles _styles;

        private void InitStyles()
        {
            if (_styles != null)
                return;

            _styles = new Styles();

            _styles.Button = new GUIStyle(GUI.skin.button);
            _styles.Button.fontSize = 12;

            _styles.Title = new GUIStyle(GUI.skin.label);
            _styles.Title.alignment = TextAnchor.MiddleLeft;
            _styles.Title.margin = new RectOffset(
                _styles.Title.margin.left, 0, 15, 15);
            _styles.Title.fontSize = 18;

            _styles.ToggleLevel1 = new GUIStyle(GUI.skin.toggle);
            _styles.ToggleLevel1.padding.left += 5;
            _styles.ToggleLevel1.fontSize = 12;

            _styles.ToggleLevel2 = new GUIStyle(_styles.ToggleLevel1);
            _styles.ToggleLevel2.margin.left += 20;

            _styles.ToggleLevel3 = new GUIStyle(_styles.ToggleLevel2);
            _styles.ToggleLevel3.margin.left += 20;
        }

        private void OnEnable()
        {
            _importOptions = Resources.Load<ImportOptions>("ImportOptions");
        }

        [MenuItem("Window/Piglet Options")]
        public static void ShowWindow()
        {
            EditorWindow window = GetWindow(typeof(OptionsWindow),
                false, "Piglet Options");

            window.minSize = new Vector2(310f, 240f);
            window.maxSize = window.minSize;
        }

        void OnGUI()
        {
            InitStyles();

            const int MARGIN = 15;

            Rect contentRect = new Rect(
                MARGIN, MARGIN,
                position.width - 2 * MARGIN,
                position.height - 2 * MARGIN);

            GUILayout.BeginArea(contentRect);

                GUILayout.Label("Piglet Options", _styles.Title);

                _importOptions.EnableDragAndDropImport
                    = GUILayout.Toggle(_importOptions.EnableDragAndDropImport,
                    new GUIContent("Enable drag-and-drop glTF import",
                        "Enable/disable automatic glTF imports when dragging " +
                        ".gltf/.glb/.zip files onto the Project Browser window"),
                    _styles.ToggleLevel1);

                GUI.enabled = _importOptions.EnableDragAndDropImport;

                     _importOptions.PromptBeforeOverwritingFiles
                        = GUILayout.Toggle(
                            _importOptions.PromptBeforeOverwritingFiles,
                            new GUIContent("Prompt before overwriting files",
                                "Show confirmation prompt if glTF import directory " +
                                "already exists"),
                            _styles.ToggleLevel2);

                     _importOptions.LogProgress
                        = GUILayout.Toggle(
                            _importOptions.LogProgress,
                            new GUIContent("Print progress messages in Console",
                               "Log progress messages to Unity Console window during " +
                               "glTF imports (useful for debugging)"),
                            _styles.ToggleLevel2);

                     _importOptions.SelectPrefabAfterImport
                        = GUILayout.Toggle(
                            _importOptions.SelectPrefabAfterImport,
                            new GUIContent("Select prefab in Project Browser",
                                "After a glTF import has completed, select/highlight " +
                                "the generated prefab in the Project Browser window"),
                            _styles.ToggleLevel2);

                     _importOptions.AddPrefabToScene
                        = GUILayout.Toggle(
                            _importOptions.AddPrefabToScene,
                            new GUIContent("Add prefab instance to scene",
                                "After a glTF import has completed, add the generated prefab to " +
                                "the current Unity scene, as a child of the currently selected " +
                                "game object. If no game object is selected in the scene, add " +
                                "the prefab at the root of the scene instead."),
                            _styles.ToggleLevel2);

                     GUI.enabled = _importOptions.EnableDragAndDropImport
                         && _importOptions.AddPrefabToScene;

                         _importOptions.SelectPrefabInScene
                            = GUILayout.Toggle(
                                _importOptions.SelectPrefabInScene,
                                new GUIContent("Select prefab instance in scene",
                                    "Select/highlight the prefab in the scene hierarchy " +
                                    "after adding it to the scene"),
                                _styles.ToggleLevel3);

                     GUI.enabled = _importOptions.EnableDragAndDropImport;

                     _importOptions.OpenPrefabAfterImport
                        = GUILayout.Toggle(
                            _importOptions.OpenPrefabAfterImport,
                            new GUIContent("Open prefab in Prefab View",
                                "After a glTF import has completed, open the generated " +
                                "prefab in the Prefab View. (This is equivalent to " +
                                "double-clicking the prefab in the Project Browser.)"),
                            _styles.ToggleLevel2);

                GUI.enabled = true;

                GUILayout.Space(15);

                if (GUILayout.Button(new GUIContent("Reset to Defaults",
                    "Reset all options to their default values"),
                    _styles.Button, GUILayout.Width(150)))
                {
                    _importOptions.Reset();
                }

                GUILayout.EndArea();

            // Tell Unity that _importOptions needs to be saved
            // to disk on the next call to AssetDatabase.SaveAssets().
            //
            // Note: With respect to the GUI code above, it's not very
            // convenient to check if the _importOptions values have
            // actually changed since they were first loaded from disk.
            // Instead I just set the dirty flag unconditionally,
            // with the hope that this does not hurt Editor performance.

            EditorUtility.SetDirty(_importOptions);

        }
    }
}