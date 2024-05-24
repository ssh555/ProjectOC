using UnityEngine;
using UnityEditor;
using System.IO;

namespace ProjectOC.PinchFace
{
    public class StringReplaceTool : EditorWindow
    {
        private string originStr = "";
        private string replacedStr = "";

        [MenuItem("Tools/Asset Process/String Replace Tool %&s")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(StringReplaceTool), false, "String Replace Tool");
        }

        private void OnGUI()
        {
            GUILayout.Label("Enter the string to be replaced:");
            originStr = EditorGUILayout.TextField("Original String:", originStr);

            GUILayout.Label("Enter the replacement string:");
            replacedStr = EditorGUILayout.TextField("Replaced String:", replacedStr);

            if (GUILayout.Button("Replace"))
            {
                ReplaceStringInAssetNames();
            }
        }

        private void ReplaceStringInAssetNames()
        {
            // if (string.IsNullOrEmpty(originStr) || string.IsNullOrEmpty(replacedStr))
            // {
            //     Debug.LogError("Both original and replaced strings must be provided.");
            //     return;
            // }

            string[] guids = Selection.assetGUIDs;
            if (guids == null || guids.Length == 0)
            {
                Debug.LogError("No assets selected.");
                return;
            }

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string assetName = Path.GetFileNameWithoutExtension(assetPath);
                string newAssetName = assetName.Replace(originStr, replacedStr);

                if (newAssetName != assetName)
                {
                    string newPath = Path.GetDirectoryName(assetPath) + "/" + newAssetName + Path.GetExtension(assetPath);
                    AssetDatabase.RenameAsset(assetPath, newAssetName);
                    //AssetDatabase.ImportAsset(newPath);
                }
            }

            Debug.Log("String replacement completed.");
        }
    }
}