using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TextFileAttribute))]
public class TextFilePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        TextFileAttribute textAttribute = (TextFileAttribute) attribute;

        EditorGUI.BeginProperty(position, label, property);

        Rect space = position;
        space.width = position.width * 0.3f;
        GUI.Label(space, label);

        position.x += space.width;
        position.width -= space.width;

        space = position;
        space.width = 45;

        if (GUI.Button(space, "New")) {
            // Find file
            string newPath = EditorUtility.SaveFilePanel("Create Data", textAttribute.relativePath + property.stringValue,
                "Data", textAttribute.fileType);
            StreamWriter writer =
                new StreamWriter(newPath);
            writer.Write(GetTemplateData());
            writer.Close();
            property.stringValue = newPath.Split(new[] {textAttribute.relativePath.Split('/').Last()}, StringSplitOptions.None).Last();
            property.stringValue = property.stringValue.Split('.').First();
            EditorUtility.OpenWithDefaultApp(newPath);
        }

        position.x += space.width;
        position.width -= space.width;

        space = position;
        space.width = 45;

        if (GUI.Button(space, "Open")) {
            if (File.Exists(textAttribute.relativePath + property.stringValue + ".json")) {
                EditorUtility.OpenWithDefaultApp(textAttribute.relativePath + property.stringValue + ".json");
            } else {
                EditorUtility.DisplayDialog("File not found", "File was not found", "ok");
            }
        }

        position.x += space.width;
        position.width -= space.width + 18;

        property.stringValue = GUI.TextField(position, property.stringValue);

        space = position;
        space.x += position.width;
        space.width = 18;
        if (GUI.Button(space, "", GUI.skin.GetStyle("IN ObjectField"))) {
            // Find file
            string newPath = EditorUtility.OpenFilePanel("Test Data", textAttribute.relativePath + property.stringValue,
                textAttribute.fileType);

            property.stringValue = newPath.Split(new[] {"/" + textAttribute.relativePath.Split('/').Last() + "/"}, StringSplitOptions.None).Last();
            property.stringValue = "/" + property.stringValue.Split('.').First();
        }

        EditorGUI.EndProperty();
    }

    private string GetTemplateData()
    {
        string[] guids = AssetDatabase.FindAssets("t:TextAsset ComponentDataTemplate");

        if (guids.Length > 0) {
            TextAsset text = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
            return text.text;
        }

        Debug.LogError("Asset ComponentDataTemplate not found");
        return string.Empty;
    }
}