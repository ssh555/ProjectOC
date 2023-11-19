using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
namespace ML.Editor.Utility.Enum
{
    [CreateAssetMenu(fileName = "SingleEnumAssetScript", menuName = "ML/Enum/SingleEnumAsset", order = 1)]
    public class SingleEnumAsset : SerializedScriptableObject
    {
        #region Config
        [LabelText("命名空间"), ShowInInspector, FoldoutGroup("配置"), PropertyOrder(-1)]
        public string enumNamespace = "";

        private string _generateCSFile = "";
        [LabelText("生成的CS脚本路径"), ShowInInspector, FoldoutGroup("配置"), PropertyOrder(-1), FilePath(Extensions = ".cs")]
        public string GenerateCSFile
        {
            get
            {
                if (_generateCSFile != "")
                {
                    return _generateCSFile;
                }
                return UnityEditor.AssetDatabase.GetAssetPath(this).Replace(".asset", ".cs");
            }
            set
            {
                if (ML.Editor.AssetDatabaseExtension.IsAssetPathValid(value) || value == "")
                {
                    _generateCSFile = value;
                }
            }
        }

        [LabelText("DefaultName"), FoldoutGroup("配置")]
        public string NoneName = "None";

        [LabelText("枚举列表"), FoldoutGroup("配置"), Space(5)]
        public List<string> EnumList = new List<string>();
        #endregion


        /// <summary>
        /// 将此Asset生成对应的C#脚本文件
        /// </summary>
        [Button("生成脚本")]
        public void CreateScript()
        {
            // The path where you want to create the script
            string assetPath = this.GenerateCSFile;
            bool isHasNamespace = this.enumNamespace != "";

            string code = "";
            foreach(var _enum in this.EnumList)
            {
                code += ((_enum == null || _enum == "") ? "" : ($"\t{_enum},\n") + (isHasNamespace ? "\t" : ""));
            }


            // The contents of the new script
            string scriptContents =
                (isHasNamespace ? $"namespace {this.enumNamespace}\n{{\n" : "") + (isHasNamespace ? "\t" : "") +
                "[System.Serializable]\n" + (isHasNamespace ? "\t" : "") +
                $"public enum {System.IO.Path.GetFileNameWithoutExtension(assetPath)}\n" + (isHasNamespace ? "\t" : "") +
                "{\n" + (isHasNamespace ? "\t" : "") +
                $"\t{this.NoneName},\n" + (isHasNamespace ? "\t" : "") +
                code +
                "}\n" +
                (isHasNamespace ? "}\n" : "");

            // Create the new script
            System.IO.File.WriteAllText(assetPath, scriptContents);

            // Import the new asset into the asset database
            UnityEditor.AssetDatabase.ImportAsset(assetPath, UnityEditor.ImportAssetOptions.ForceUpdate);
        }

    }
}

#endif