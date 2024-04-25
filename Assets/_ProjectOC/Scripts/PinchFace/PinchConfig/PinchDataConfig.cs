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
        
        [Space(40)]
        //���� Ĭ����ֵ
        public Dictionary<BoneWeightType, BoneTransfData> boneWeightData;
        public class BoneTransfData
        {
            public Vector3 pos, scale;

            public BoneTransfData(Vector3 _pos, Vector3 _scale)
            {
                pos = _pos;
                scale = _scale;
            }
        }
        
        
        
        
        
        
        
        [Button("��ȡ��Ϣ")]
        public void LoadConfig()
        {
            ReadType3Cofig();
            ReadBoneWeightConfig();
        }

        private void ReadBoneWeightConfig()
        {
            boneWeightData = new Dictionary<BoneWeightType, BoneTransfData>();
            
            string addressPath = "Assets/_ProjectOC/OCResources/PinchFace/Prefabs/PinchPart/Model_AnMiXiu.prefab";
            GameObject characterModel = AssetDatabase.LoadAssetAtPath<GameObject>(addressPath);
            if (characterModel == null)
            {
                Debug.LogWarning("can't get object");
            }
            Dictionary<string, Transform> boneDic = new Dictionary<string, Transform>();
            CollectTransforms(characterModel.transform, boneDic);
            

            StorageBoneData(BoneWeightType.Head,"Head");
            //StorageBoneData(BoneWeightType.Chest,transform);
            //StorageBoneData(BoneWeightType.Arm,transform);
            StorageBoneData(BoneWeightType.Waist,"Spine");
            StorageBoneData(BoneWeightType.Leg,"Weight_Thin");
            StorageBoneData(BoneWeightType.HeadTop,"Add_HeadTop");
            StorageBoneData(BoneWeightType.Root,"Root");
            

            void StorageBoneData(BoneWeightType _boneWeightType, string _boneName)
            {
                Transform _boneTrans = boneDic[_boneName];
                BoneTransfData _oneData = new BoneTransfData(_boneTrans.localPosition, _boneTrans.localScale);
                boneWeightData.Add(_boneWeightType,_oneData);
            }

            void CollectTransforms(Transform parent, Dictionary<string, Transform> transformDict)
            {
                foreach (Transform child in parent)
                {
                    if (!transformDict.ContainsKey(child.name))
                    {
                        transformDict.Add(child.name, child);
                    }
                    // �ݹ������Transform
                    CollectTransforms(child, transformDict);
                }
            }
        }
        
        
        private void ReadType3Cofig()
        {
            //����Texture2D�������ж�Prefab����
            int enumsValue = (System.Enum.GetValues(typeof(PinchPartType3))).Length -1;
            typesDatas = new List<Type3Data>(enumsValue);
            string rootFolderPath = "Assets/_ProjectOC/OCResources/PinchFace/UI/PinchPart";
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
