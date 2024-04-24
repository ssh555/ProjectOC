using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using ProjectOC.PinchFace;

namespace  ProjectOC.PinchFace
    {
    public class ToolProcessScriptObject : EditorWindow
    {
        [MenuItem("Tools/Process ScriptObjects")]
         
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(ToolProcessScriptObject));
        }

        void OnGUI()
        {
            if (GUILayout.Button("生成对应的文件目录"))
            {
                Process();
            }
        }

        void Process()
        {
            string folderPath = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");

            if (string.IsNullOrEmpty(folderPath))
            {
                return;
            }

            string[] fullAssetPaths = Directory.GetFiles(folderPath, "*.asset", SearchOption.AllDirectories);
            string[] assetPaths = new string[fullAssetPaths.Length];
            for (int i = 0; i < assetPaths.Length; i++)
            {
                assetPaths[i] = "Assets" + fullAssetPaths[i].Substring(Application.dataPath.Length);
                assetPaths[i] = assetPaths[i].Replace('\\', '/');
            }
            
            foreach (string assetPath in assetPaths)
            {
                PinchPartType scriptObject = AssetDatabase.LoadAssetAtPath<PinchPartType>(assetPath);

                if (scriptObject != null)
                {
                    ProcessScriptObject(scriptObject, assetPath);
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("Processing completed.");
        }

        void ProcessScriptObject(PinchPartType scriptObject, string assetPath)
        {
            foreach (var _type3 in scriptObject.pinchPartType3s)
            {
                int type1Index = (int)scriptObject.pinchPartType1 - 1;
                int type2Index = (int)scriptObject.pinchPartType2 - 1;
                int type3Index = (int)_type3 - 1;
                // string folderPath = "Assets/_ProjectOC/OCResources/Prefabs/Character/PinchFace";
                string folderPath = "Assets/_ProjectOC/OCResources/UI/PinchFace";
                string Type1Name =  $"{type1Index}_{scriptObject.pinchPartType1.ToString()}_PinchType1";
                string Type2Name =  $"{type2Index}_{scriptObject.pinchPartType2.ToString()}_PinchType2";
                string Type3Name =  $"{type3Index}_{_type3.ToString()}_PinchType3";
            
            
                //Common不用手动创建
                string Type1FolderPath = Path.Combine(folderPath, Type1Name);
                string Type2FolderPath = Path.Combine(Type1FolderPath, Type2Name);
                string Type3FolderPath = Path.Combine(Type2FolderPath, Type3Name);
                Debug.Log($"{Type1FolderPath}");
                if (!Directory.Exists(Type1FolderPath))
                {
                    Directory.CreateDirectory(Type1FolderPath);
                }
            
                if (!Directory.Exists(Type2FolderPath))
                {
                    Directory.CreateDirectory(Type2FolderPath);
                }
            
                if (!Directory.Exists(Type3FolderPath))
                {
                    Directory.CreateDirectory(Type3FolderPath);
                }
            }
        }
    }
}