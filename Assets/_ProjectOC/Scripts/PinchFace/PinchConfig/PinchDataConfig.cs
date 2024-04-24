using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.PinchFace.Config
{
    [CreateAssetMenu(fileName = "NewPinchDataConfig", menuName = "ML/PinchDataConfig")]
    public class PinchDataConfig : SerializedScriptableObject
    {
        //存储Type3 Prefab数量
        public List<int> typesData;
        //骨骼 默认数值
        public Dictionary<BoneWeightType, BoneTransfData> boneWeightData;
        public class BoneTransfData
        {
            public Vector3 pos, scale;
        }
        
        
        
        
        
        
        [Button("读取信息")]
        public void LoadConfig()
        {
            ReadType3Cofig();
            ReadBoneWeightConfig();
        }

        private void ReadBoneWeightConfig()
        {
            //
        }

        private void ReadType3Cofig()
        {
            //根据Texture2D数量来判断Prefab数量
            int enumsValue = (System.Enum.GetValues(typeof(PinchPartType3))).Length -1;
            typesData = new List<int>(enumsValue);
            
            string rootFolderPath = "Assets/_ProjectOC/OCResources/UI/PinchFace/Texture";
            TraverseFolders(rootFolderPath);
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

                int num = int.Parse(folderName.Split('_')[0]);
                typesData.Add(fileCount);
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
