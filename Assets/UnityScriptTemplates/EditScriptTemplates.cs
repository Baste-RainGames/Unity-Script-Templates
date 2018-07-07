using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using UnityEditor;
using UnityEngine;

namespace UnityScriptTemplates {
    public class EditScriptTemplates : EditorWindow {
        [MenuItem("Window/Edit Script Templates")]
        public static void OpenWindow() {
            GetWindow<EditScriptTemplates>();
        }

        public List<ScriptTemplateFile> customScriptTemplates;
        public List<ScriptTemplateFile> builtInScriptTemplates;

        private ScriptTemplateFile selectedTemplate;
        private bool hasWriteAccessToBuiltins;
        private string builtInPath;
        private string customPath;
        private Vector2 scroll;

        private void OnEnable() {
            builtInPath = EditorApplication.applicationContentsPath + "/Resources/ScriptTemplates";
            customPath = Application.dataPath + "/EditorDefaultResources/ScriptTemplates";
            customScriptTemplates = FindScriptTemplates(customPath, false);
            builtInScriptTemplates = FindScriptTemplates(builtInPath, true);

            if (builtInScriptTemplates.Count > 0) {
                try {
                    builtInScriptTemplates[0].SaveToFile();
                    hasWriteAccessToBuiltins = true;
                }
                catch(UnauthorizedAccessException) {
                    hasWriteAccessToBuiltins = false;
                }
            }
        }

        private void OnGUI() {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("Custom script templates (" + customPath + ")");
            foreach (var scriptTemplate in customScriptTemplates) {
                if (GUILayout.Button(scriptTemplate.name)) {
                    selectedTemplate = scriptTemplate;
                }
            }

            EditorGUILayout.LabelField("Built in script templates (" + builtInPath + ")");
            foreach (var scriptTemplate in builtInScriptTemplates) {
                if (GUILayout.Button(scriptTemplate.name)) {
                    selectedTemplate = scriptTemplate;
                }
            }

            if (selectedTemplate != null) {
                var disabled = !hasWriteAccessToBuiltins && selectedTemplate.isBuiltIn;
                if (disabled) {
                    EditorGUILayout.LabelField("Cannot edit built in templates! You're probably not running Unity as administrator/root");
                    EditorGUILayout.LabelField("Either relaunch as administrator/root, or edit the files directly in their folder");
                }

                EditorGUI.BeginDisabledGroup(disabled);
                selectedTemplate.text = EditorGUILayout.TextArea(selectedTemplate.text);

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Save to file", GUILayout.Width(120f))) {
                    selectedTemplate.SaveToFile();
                }

                if (GUILayout.Button("Discard changes", GUILayout.Width(120f))) {
                    selectedTemplate.DiscardChanges();
                    GUI.FocusControl(null); //The text area doesn't update while not selected
                    Repaint();
                }

                GUILayout.FlexibleSpace();

                EditorGUILayout.LabelField("Indent style:", GUILayout.Width(100f));
                if (GUILayout.Button("Spaces")) {
                    selectedTemplate.text = selectedTemplate.text.Replace("\t", "    ");
                }

                if (GUILayout.Button("Tabs")) {
                    selectedTemplate.text = selectedTemplate.text.Replace("    ", "\t");
                }

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private static List<ScriptTemplateFile> FindScriptTemplates(string templateFolder, bool isInternal) {
            return Directory.GetFiles(templateFolder)
                            .Where(file => file.EndsWith(".cs.txt"))
                            .Select(file => new ScriptTemplateFile(file, isInternal))
                            .ToList();
        }
    }

    public class ScriptTemplateFile {
        private readonly string path;
        public readonly string name;
        public readonly bool isBuiltIn;

        private string fileText;
        public string text;

        public ScriptTemplateFile(string path, bool isBuiltIn) {
            this.path = path;
            this.isBuiltIn = isBuiltIn;
            name = Path.GetFileNameWithoutExtension(path);
            fileText = File.ReadAllText(path);
            text = fileText;
        }

        public void SaveToFile() {
            File.WriteAllText(path, text);
            fileText = text;
            if (!isBuiltIn) {
                var assetDatabasePath = "Assets" + path.Substring(Application.dataPath.Length);
                AssetDatabase.ImportAsset(assetDatabasePath);
            }
        }

        public void DiscardChanges() {
            text = fileText;
        }
    }
}