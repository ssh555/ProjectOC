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
        //�洢Type3 Prefab����
        public List<int> typesData;
        //���� Ĭ����ֵ
        public Dictionary<BoneWeightType, BoneTransfData> boneWeightData;
        public class BoneTransfData
        {
            public Vector3 pos, scale;
        }
        
        
        
        
        
        
        [Button("��ȡ��Ϣ")]
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
            //����Texture2D�������ж�Prefab����
            int enumsValue = (System.Enum.GetValues(typeof(PinchPartType3))).Length -1;
            typesData = new List<int>(enumsValue);
            
            string rootFolderPath = "Assets/_ProjectOC/OCResources/UI/PinchFace/Texture";
            TraverseFolders(rootFolderPath);
        }
        private void TraverseFolders(string folderPath)
        {
            // ��ȡ�ļ����е��������ļ���
            string[] subDirectories = Directory.GetDirectories(folderPath);

            // ���û�����ļ��У�˵����ǰ�ļ�����������ļ���
            if (subDirectories.Length == 0)
            {
                // ��ȡ��ǰ�ļ��е�����
                string folderName = new DirectoryInfo(folderPath).Name;

                // ��ȡ��ǰ�ļ����µ��ļ�����
                int fileCount = Directory.GetFiles(folderPath, "*.png").Length
                                + Directory.GetFiles(folderPath, "*.jpg").Length
                                + Directory.GetFiles(folderPath, "*.jpeg").Length;

                int num = int.Parse(folderName.Split('_')[0]);
                typesData.Add(fileCount);
                return;
            }

            // ��������ļ��У���������ÿ�����ļ���
            foreach (string subDirectory in subDirectories)
            {
                TraverseFolders(subDirectory);
            }
        }
    }   
}
