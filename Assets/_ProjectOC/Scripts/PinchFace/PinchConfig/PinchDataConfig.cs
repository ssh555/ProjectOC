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
        //�洢Type3 Prefab����
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
        
        
        [Button("��ȡ��Ϣ")]
        public void LoadConfig()
        {
            ReadType3Cofig();
        }
        
        
        private void ReadType3Cofig()
        {
            //����Texture2D�������ж�Prefab����
            int enumsValue = (System.Enum.GetValues(typeof(PinchPartType3))).Length -1;
            typesDatas = new List<Type3Data>(enumsValue);
            string rootFolderPath = "Assets/_ProjectOC/OCResources/PinchFace/UI/PinchPart";
            TraverseFolders(rootFolderPath);
            //����
            typesDatas.Sort((a, b) => ((int)a._type3).CompareTo((int)b._type3));
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

                string type3Name = $"{folderName.Split('_')[1]}_{folderName.Split('_')[2]}";
                typesDatas.Add(
                    new Type3Data((PinchPartType3)Enum.Parse(typeof(PinchPartType3),type3Name),fileCount));
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
