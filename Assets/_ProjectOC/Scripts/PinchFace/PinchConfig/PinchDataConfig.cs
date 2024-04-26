using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace ProjectOC.PinchFace.Config
{
    [CreateAssetMenu(fileName = "NewPinchDataConfig", menuName = "ML/PinchDataConfig")]
    public class PinchDataConfig : SerializedScriptableObject
    {
        //存储Type3 Prefab数量
        public List<Type3Data> typesDatas;
        public class Type3Data
        {
            public PinchPartType3 _type3;
            public int typeCount;

            public Type3Data(PinchPartType3 _name, int _count)
            {
                _type3 = _name;
                typeCount = _count;
            }
        }
        
        
        [Button("读取信息")]
        public void LoadConfig()
        {
            ReadType3Cofig();
        }
        
        
        private void ReadType3Cofig()
        {
            //根据Texture2D数量来判断Prefab数量
            int enumsValue = (System.Enum.GetValues(typeof(PinchPartType3))).Length -1;
            typesDatas = new List<Type3Data>(enumsValue);
            string rootFolderPath = "Assets/_ProjectOC/OCResources/PinchFace/UI/PinchPart";
            TraverseFolders(rootFolderPath);
            //排序
            typesDatas.Sort((a, b) => ((int)a._type3).CompareTo((int)b._type3));
        }
        private void TraverseFolders(string folderPath)
        {
            // 获取文件夹中的所有子文件夹
            string[] subDirectories = Directory.GetDirectories(folderPath);

            // 如果没有子文件夹，说明当前文件夹是最里层文件夹
            if (subDirectories.Length == 0)
            {
                // 获取当前文件夹的名称
                string folderName = new DirectoryInfo(folderPath).Name;

                // 获取当前文件夹下的文件数量
                int fileCount = Directory.GetFiles(folderPath, "*.png").Length
                                + Directory.GetFiles(folderPath, "*.jpg").Length
                                + Directory.GetFiles(folderPath, "*.jpeg").Length;

                string type3Name = $"{folderName.Split('_')[1]}_{folderName.Split('_')[2]}";
                typesDatas.Add(
                    new Type3Data((PinchPartType3)Enum.Parse(typeof(PinchPartType3),type3Name),fileCount));
                return;
            }

            // 如果有子文件夹，继续遍历每个子文件夹
            foreach (string subDirectory in subDirectories)
            {
                TraverseFolders(subDirectory);
            }
        }
    }   
}
