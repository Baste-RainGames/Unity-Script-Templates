using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        //serialized by Unity so selection can survive script reload
        public bool selectedIsBuiltIn;
        public int selectedIndex = -1;

        private GenericMenu selectTemplateMenu;
        private ScriptTemplateFile selectedTemplate;
        private bool hasWriteAccessToBuiltins;

        private string builtInPath;
        private string customPath;
        private Vector2 scroll;

        private void OnEnable() {
            builtInPath = EditorApplication.applicationContentsPath + "/Resources/ScriptTemplates";
            customPath = Application.dataPath + "/EditorDefaultResources/ScriptTemplates";
            builtInScriptTemplates = FindScriptTemplates(builtInPath, true);
            customScriptTemplates = FindScriptTemplates(customPath, false);

            var listToSelectIn = selectedIsBuiltIn ? builtInScriptTemplates : customScriptTemplates;
            if (selectedIndex > 0 && selectedIndex < listToSelectIn.Count)
                selectedTemplate = listToSelectIn[selectedIndex];

            if (builtInScriptTemplates.Count > 0) {
                try {
                    builtInScriptTemplates[0].SaveToFile();
                    hasWriteAccessToBuiltins = true;
                }
                catch (UnauthorizedAccessException) {
                    hasWriteAccessToBuiltins = false;
                }
            }

            selectTemplateMenu = new GenericMenu();
            foreach (var template in customScriptTemplates) {
                selectTemplateMenu.AddItem(new GUIContent("Custom/" + template.name), false, TemplateSelected, template);
            }

            foreach (var template in builtInScriptTemplates) {
                selectTemplateMenu.AddItem(new GUIContent("Built in/" + template.name), false, TemplateSelected, template);
            }
        }

        private void TemplateSelected(object template) {
            selectedTemplate = (ScriptTemplateFile) template;
        }

        private void OnGUI() {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawTemplateSelection();

            if (selectedTemplate != null) {
                EditorGUILayout.Space();
                DrawSelectedTemplate();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawTemplateSelection() {
            EditorGUILayout.LabelField("Selected template:");
            string text;
            if (selectedTemplate == null) {
                text = "";
            }
            else {
                if (selectedTemplate.isBuiltIn)
                    text = "Built in/" + selectedTemplate.name;
                else
                    text = "Custom/" + selectedTemplate.name;
            }

            if (GUILayout.Button(text, GUILayout.Width(450f))) {
                selectTemplateMenu.ShowAsContext();
            }

            if (selectedTemplate != null) {
                EditorGUILayout.TextField("Path: " + selectedTemplate.path, GUI.skin.label); //textField so it's selectable for copy+pasting
            }

        }

        private void DrawSelectedTemplate() {
            var disabled = !hasWriteAccessToBuiltins && selectedTemplate.isBuiltIn;
            if (disabled) {
                var style = new GUIStyle(GUI.skin.label) {
                    normal = {
                        textColor = Color.red
                    }
                };
                EditorGUILayout.LabelField("Cannot edit built in templates! You're probably not running Unity as administrator/root", style);
                EditorGUILayout.LabelField("Either relaunch as administrator/root, or edit the files directly in their folder", style);
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

            EditorGUILayout.LabelField("Set indent style:", GUILayout.Width(100f));
            if (GUILayout.Button("Spaces")) {
                selectedTemplate.text = selectedTemplate.text.Replace("\t", "    ");
            }

            if (GUILayout.Button("Tabs")) {
                selectedTemplate.text = selectedTemplate.text.Replace("    ", "\t");
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }

        private static List<ScriptTemplateFile> FindScriptTemplates(string templateFolder, bool isInternal) {
            return Directory.GetFiles(templateFolder)
                            .Where(file => file.EndsWith(".cs.txt"))
                            .Select(file => new ScriptTemplateFile(file, isInternal))
                            .ToList();
        }
    }

    public class ScriptTemplateFile {
        public readonly string path;
        public readonly string name;
        public readonly bool isBuiltIn;

        private string fileText;
        public string text;

        public ScriptTemplateFile(string path, bool isBuiltIn) {
            this.path = path.Replace("\\", "/");
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