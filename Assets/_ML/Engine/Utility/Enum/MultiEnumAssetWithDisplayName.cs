using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using ML.Editor;

namespace ML.Editor.Utility.Enum
{
    [System.Serializable]
    public struct EnumKV
    {
        [LabelText("枚举名"), SerializeField]
        public string name;
        [LabelText("显示名"), SerializeField]
        public string displayName;
    }

    [CreateAssetMenu(fileName = "MultiEnumAssetWithDisplayNameScript", menuName = "ML/Enum/MultiEnumAssetWithDisplayName", order = 1)]
    public class MultiEnumAssetWithDisplayName : SerializedScriptableObject
    {
        public MultiEnumAssetWithDisplayName()
        {
            this.AllName.name = this.AllName.displayName = "All";
            this.NoneName.name = this.NoneName.displayName = "None";
        }

        #region Config
        [LabelText("命名空间"), ShowInInspector, FoldoutGroup("配置"), PropertyOrder(-1)]
        public string enumNamespace = "";

        private string _generateCSFile = "";
        [LabelText("生成的CS脚本路径"), ShowInInspector, FoldoutGroup("配置"), PropertyOrder(-1), FilePath(Extensions = ".cs")]
        public string GenerateCSFile
        {
            get
            {
                if(_generateCSFile != "")
                {
                    return _generateCSFile;
                }
                return UnityEditor.AssetDatabase.GetAssetPath(this).Replace(".asset", ".cs");
            }
            set
            {
                if(AssetDatabaseExtension.IsAssetPathValid(value) || value == "")
                {
                    _generateCSFile = value;
                }
            }
        }

        [LabelText("全选 Name"), FoldoutGroup("配置"), Space(5)]
        public EnumKV AllName;
        [LabelText("全部不选 Name"), FoldoutGroup("配置")]
        public EnumKV NoneName;
        #endregion

        #region 枚举值
        private EnumKV[] Enums = new EnumKV[32];
        [LabelText("Enum 0"), PropertySpace(5), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum0
        {
            get => Enums[0];
            set => Enums[0] = value;
        }
        [LabelText("Enum 1"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum1
        {
            get => Enums[1];
            set => Enums[1] = value;
        }
        [LabelText("Enum 2"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum2
        {
            get => Enums[2];
            set => Enums[2] = value;
        }
        [LabelText("Enum 3"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum3
        {
            get => Enums[3];
            set => Enums[3] = value;
        }
        [LabelText("Enum 4"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum4
        {
            get => Enums[4];
            set => Enums[4] = value;
        }
        [LabelText("Enum 5"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum5
        {
            get => Enums[5];
            set => Enums[5] = value;
        }
        [LabelText("Enum 6"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum6
        {
            get => Enums[6];
            set => Enums[6] = value;
        }
        [LabelText("Enum 7"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum7
        {
            get => Enums[7];
            set => Enums[7] = value;
        }
        [LabelText("Enum 8"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum8
        {
            get => Enums[8];
            set => Enums[8] = value;
        }
        [LabelText("Enum 9"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum9
        {
            get => Enums[9];
            set => Enums[9] = value;
        }
        [LabelText("Enum 10"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum10
        {
            get => Enums[10];
            set => Enums[10] = value;
        }
        [LabelText("Enum 11"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum11
        {
            get => Enums[11];
            set => Enums[11] = value;
        }
        [LabelText("Enum 12"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum12
        {
            get => Enums[12];
            set => Enums[12] = value;
        }
        [LabelText("Enum 13"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum13
        {
            get => Enums[13];
            set => Enums[13] = value;
        }
        [LabelText("Enum 14"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum14
        {
            get => Enums[14];
            set => Enums[14] = value;
        }
        [LabelText("Enum 15"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum15
        {
            get => Enums[15];
            set => Enums[15] = value;
        }
        [LabelText("Enum 16"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum16
        {
            get => Enums[16];
            set => Enums[16] = value;
        }
        [LabelText("Enum 17"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum17
        {
            get => Enums[17];
            set => Enums[17] = value;
        }
        [LabelText("Enum 18"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum18
        {
            get => Enums[18];
            set => Enums[18] = value;
        }
        [LabelText("Enum 19"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum19
        {
            get => Enums[19];
            set => Enums[19] = value;
        }
        [LabelText("Enum 20"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum20
        {
            get => Enums[20];
            set => Enums[20] = value;
        }
        [LabelText("Enum 21"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum21
        {
            get => Enums[21];
            set => Enums[21] = value;
        }
        [LabelText("Enum 22"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum22
        {
            get => Enums[22];
            set => Enums[22] = value;
        }
        [LabelText("Enum 23"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum23
        {
            get => Enums[23];
            set => Enums[23] = value;
        }
        [LabelText("Enum 24"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum24
        {
            get => Enums[24];
            set => Enums[24] = value;
        }
        [LabelText("Enum 25"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum25
        {
            get => Enums[25];
            set => Enums[25] = value;
        }
        [LabelText("Enum 26"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum26
        {
            get => Enums[26];
            set => Enums[26] = value;
        }
        [LabelText("Enum 27"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum27
        {
            get => Enums[27];
            set => Enums[27] = value;
        }
        [LabelText("Enum 28"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum28
        {
            get => Enums[28];
            set => Enums[28] = value;
        }
        [LabelText("Enum 29"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum29
        {
            get => Enums[29];
            set => Enums[29] = value;
        }
        [LabelText("Enum 30"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum30
        {
            get => Enums[30];
            set => Enums[30] = value;
        }
        [LabelText("Enum 31"), FoldoutGroup("枚举值"), ShowInInspector]
        public EnumKV Enum31
        {
            get => Enums[31];
            set => Enums[31] = value;
        }
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
            for(int i = 0; i < 32; ++i)
            {
                code +=
                    ((Enums[i].name == null || Enums[i].name == "" || Enums[i].displayName == null || Enums[i].displayName == "") ? "" : ($"\t[LabelText(\"{Enums[i].displayName}\")]\n") + (isHasNamespace ? "\t" : "")) +
                    ((Enums[i].name == null || Enums[i].name == "") ? "" : ($"\t{Enums[i].name} = 1 << {i},\n") + (isHasNamespace ? "\t" : ""));
            }


            // The contents of the new script
            string scriptContents =
                "using Sirenix.OdinInspector;\n\n" +
                (isHasNamespace ? $"namespace {this.enumNamespace}\n{{\n" : "") + (isHasNamespace ? "\t" : "") +
                "[System.Flags, System.Serializable]\n" + (isHasNamespace ? "\t" : "") +
                $"public enum {System.IO.Path.GetFileNameWithoutExtension(assetPath)}\n" + (isHasNamespace ? "\t" : "") +
                "{\n" + (isHasNamespace ? "\t" : "") +
                ((this.AllName.name == null || this.AllName.name == "" || this.AllName.displayName == null || this.AllName.displayName == "") ? "" : ($"\t[LabelText(\"{this.AllName.displayName}\")]\n") + (isHasNamespace ? "\t" : "")) +
                $"\t{this.AllName.name} = int.MaxValue,\n" + (isHasNamespace ? "\t" : "") +
                ((this.NoneName.name == null || this.NoneName.name == "" || this.NoneName.displayName == null || this.NoneName.displayName == "") ? "" : ($"\t[LabelText(\"{this.NoneName.displayName}\")]\n") + (isHasNamespace ? "\t" : "")) +
                $"\t{this.NoneName.name} = 0,\n" + (isHasNamespace ? "\t" : "") +
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