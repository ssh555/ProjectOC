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
        [LabelText("�����ռ�"), ShowInInspector, FoldoutGroup("����"), PropertyOrder(-1)]
        public string enumNamespace = "";

        private string _generateCSFile = "";
        [LabelText("���ɵ�CS�ű�·��"), ShowInInspector, FoldoutGroup("����"), PropertyOrder(-1), FilePath(Extensions = ".cs")]
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

        [LabelText("DefaultName"), FoldoutGroup("����")]
        public string NoneName = "None";

        [LabelText("ö���б�"), FoldoutGroup("����"), Space(5)]
        public List<string> EnumList = new List<string>();
        #endregion


        /// <summary>
        /// ����Asset���ɶ�Ӧ��C#�ű��ļ�
        /// </summary>
        [Button("���ɽű�")]
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