using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// This file adds a menu item just below "Create->C# Script" for creating some different script templates.
/// You can add your own templates by copy-pasting the code in this file and editing the paths.a
/// </summary>
public static class CustomScriptTemplates {
    /// <summary>
    /// Path to the template file for a ScriptableObject.
    /// Using .cs.txt extension to match Unity's built-in templates, but this is _not_ neccessary.
    /// </summary>
    private static string TemplatePathScriptableObject {
        get {
            return Application.dataPath + "/EditorDefaultResources/ScriptableObjectTemplate.cs.txt";
        }
    }

    /// <summary>
    /// Path to the template file for a Plain C# class.
    /// </summary>
    private static string TemplatePathPlainClass {
        get {
            return Application.dataPath + "/EditorDefaultResources/PlainClassTemplate.cs.txt";
        }
    }

    /// <summary>
    /// ProjectWindowUtil.CreateScriptAsset is the method that makes the magic happen.
    /// It has two parameters:
    /// - templatePath, the absolute path to the template file.
    /// - destName, the suggested file name for the new asset.
    ///
    /// It seems like this method is usually called from c++; it's a private method, and nothing in ProjectWindowUtil calls it, but if you add a breakpoint
    /// in it when hitting Create-C# script, the breakpoint is hit, with no stack trace.
    /// </summary>
    private static MethodInfo CreateScriptAsset {
        get {
            var projectWindowUtilType = typeof(ProjectWindowUtil);
            return  projectWindowUtilType.GetMethod("CreateScriptAsset", BindingFlags.NonPublic | BindingFlags.Static);
        }
    }

    /// <summary>
    /// Adds a menu item for creating a new ScriptableObject file.
    /// </summary>
    [MenuItem("Assets/Create/Script Template/ScriptableObject", priority = 81)] // Create/C# Script has priority 80, so this puts it just below that.
    public static void CreateScriptableObject() {
        CreateScriptAsset.Invoke(null, new object[] { TemplatePathScriptableObject, "NewScriptableObject.cs" });
    }

    /// <summary>
    /// Validates that the ScriptableObject template exists.
    /// It's pretty important to check that the template path exist. If you call this method with an empty template Unity creates an invisible asset you cannot
    /// interact with, which will stick around until you restart Unity. 
    /// </summary>
    [MenuItem("Assets/Create/Script Template/ScriptableObject", true, priority = 81)]
    public static bool CreateScriptableObjectValidate() {
        return File.Exists(TemplatePathScriptableObject) && CreateScriptAsset != null;
    }

    /// <summary>
    /// Adds a menu item for creating a new plain class file.
    /// </summary>
    [MenuItem("Assets/Create/Script Template/Class", priority = 81)]
    public static void CreatePlainClass() {
        CreateScriptAsset.Invoke(null, new object[] { TemplatePathPlainClass, "NewClass.cs" });
    }

    /// <summary>
    /// Validates that the plain class template exists. 
    /// </summary>
    [MenuItem("Assets/Create/Script Template/Class", true, priority = 81)]
    public static bool CreatePlainClassValidate() {
        return File.Exists(TemplatePathPlainClass) && CreateScriptAsset != null;
    }
}