using System;
using System.IO;
using System.Reflection;
using System.Text;
using Squla.Core.IOC.Builder;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Squla.Editor
{
    public class ComponentCreator : EditorWindow
    {
        private static ComponentCreator singletonWindow;

        public static bool IsOpen {
            get { return singletonWindow != null; }
        }

        [MenuItem("Assets/Create/Squla/Component")]
        private static void SqulaComponentLoad()
        {
            ShowWindow().GetDirectory();
        }

        [MenuItem("Squla/Tools/Create Component")]
        public static ComponentCreator ShowWindow()
        {
            var editor =(ComponentCreator) GetWindow(typeof(ComponentCreator));
            editor.minSize = new Vector2(350f, 110f);
            editor.CenterOnMainWin();
	        editor.namespaceName = Selection.activeObject.name;
            return (ComponentCreator) editor;
        }

        [DidReloadScripts]
        public static void OnCompileScripts()
        {
            if (IsOpen && singletonWindow.compiling) {
                singletonWindow.compiling = false;
                singletonWindow.CreatePrefabs();
            }
        }

        private void GetDirectory()
        {
            var selected = Selection.activeObject;
            directory = AssetDatabase.GetAssetPath(selected);
            if (!Directory.Exists(directory) && File.Exists(directory)) {
                directory = Path.GetDirectoryName(directory);
            }
        }

        public string directory;
        public string namespaceName;
        private string componentName;
        private bool includeComponentModel = true;
        private string componentModelName;

        private bool compiling;

        void OnGUI()
        {
            GUILayout.Label(directory);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Namespace", GUILayout.Width(150f));
            GUILayout.Label("Squla.App.", GUILayout.Width(60f));
            namespaceName = GUILayout.TextField(namespaceName);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Component", GUILayout.Width(150f));
            componentName = GUILayout.TextField(componentName);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Component Model", GUILayout.Width(150f));
            componentModelName = GUILayout.TextField(componentModelName);
            includeComponentModel = GUILayout.Toggle(includeComponentModel, new GUIContent("", "Create this file?"),
                GUILayout.Width(15f));
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);

            if (compiling) {
                GUI.enabled = false;
                GUILayout.Button("Waiting for Compilation to create Prefabs...");
                GUI.enabled = true;
            } else if (GUILayout.Button("Generate Component")) {
                GenerateFolderStructure();
            }
        }

        void OnEnable()
        {
            singletonWindow = this;
        }

        private void OnDisable()
        {
            singletonWindow = null;
        }

        void GenerateFolderStructure()
        {
            string componentDirectory = this.directory + "/" + componentName;
            Directory.CreateDirectory(componentDirectory);

            Directory.CreateDirectory(string.Format("{0}/Scripts", componentDirectory));
            Directory.CreateDirectory(string.Format("{0}/Prefabs", componentDirectory));
            Directory.CreateDirectory(string.Format("{0}/Tests", componentDirectory));

            string component = GetTemplateData("ComponentTemplate");
            string componentModel = GetTemplateData("ComponentModelTemplate");
            string componentTest = GetTemplateData("ComponentUxTemplate");

            StreamWriter writer =
                new StreamWriter(string.Format("{0}/Scripts/{1}.cs", componentDirectory, componentName));
            writer.Write(component);
            writer.Close();

            if (includeComponentModel) {
                writer = new StreamWriter(string.Format("{0}/Scripts/{1}.cs", componentDirectory, componentModelName));
                writer.Write(componentModel);
                writer.Close();
            }

            writer = new StreamWriter(string.Format("{0}/Tests/ux_{1}.cs", componentDirectory, componentName));
            writer.Write(componentTest);
            writer.Close();

            compiling = true;
            AssetDatabase.Refresh();
//            AssetDatabase.ImportAsset(componentDirectory);
        }

        public void CreatePrefabs()
        {
            string componentDirectory = this.directory + "/" + componentName;

            // Create the main component prefab
            GameObject componentGO = new GameObject(componentName, typeof(RectTransform));
            RectTransform rectT = (RectTransform) componentGO.transform;
            rectT.anchorMin = new Vector2(0f,0f);
            rectT.anchorMax = new Vector2(1f,1f);

            componentGO.AddComponent(Type.GetType(string.Format("Squla.App.{0}.{1}, Assembly-CSharp", namespaceName,
                componentName)));

            var prefab = PrefabUtility.SaveAsPrefabAsset(componentGO, componentDirectory + "/Prefabs/prefab_" + componentName + ".prefab");
            DestroyImmediate(componentGO);
            componentGO = prefab;

            // Create ux prefab
            var componentUXGO = new GameObject(componentName, typeof(RectTransform));
            rectT = (RectTransform) componentUXGO.transform;
            rectT.anchorMin = new Vector2(0f,0f);
            rectT.anchorMax = new Vector2(1f,1f);

            // Add child to the test gameobject
            var uxChild = new GameObject("UXContainer", typeof(RectTransform));
            uxChild.transform.SetParent(componentUXGO.transform, false);
            ((RectTransform)uxChild.transform).sizeDelta = Vector2.one * 100f;

            // Add simple builder
            var simpleBuilder = componentUXGO.AddComponent<SimpleBuilder>();
            simpleBuilder.itemsToBuild = new SimpleBuilderItem[] {
                new SimpleBuilderItem() {
                    prefab = componentGO,
                    target = (RectTransform)uxChild.transform,
                    name = componentName
                }
            };

            componentUXGO.AddComponent(Type.GetType(string.Format("Squla.TDD.ux_{0}, Assembly-CSharp",
                componentName)));

            PrefabUtility.SaveAsPrefabAsset(componentUXGO, componentDirectory + "/Tests/test_prefab_" + componentName + ".prefab");
            DestroyImmediate(componentUXGO);

            // Selects the new created folder
            Selection.activeObject = AssetDatabase.LoadAssetAtPath(componentDirectory, typeof(UnityEngine.Object));
            Close();
        }

        private string GetTemplateData(string templateName)
        {
            string[] guids = AssetDatabase.FindAssets("t:TextAsset " + templateName);

            if (guids.Length > 0) {
                TextAsset text = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
                StringBuilder sb = new StringBuilder(text.text);
                sb.Replace("{Component}", componentName)
                    .Replace("{ComponentLowerCase}",
                        Char.ToLowerInvariant(componentName[0]) + componentName.Substring(1))
                    .Replace("{Namespace}", namespaceName)
                    .Replace("{ComponentModel}", componentModelName);
                return sb.ToString();
            }

            Debug.LogError("Asset " + templateName + " not found");
            return string.Empty;
        }
    }
}