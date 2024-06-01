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
        //�洢Type3 Prefab����, ͷ����ɫ����
        public List<Type3Data> typesDatas;

        public List<int> HairFrontChangeColorTypes = new List<int>();
        public List<int> HairBackChangeColorTypes = new List<int>();
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

#if UNITY_EDITOR
        [Button("��ȡ��Ϣ")]
        public void LoadConfig()
        {
            ReadType3Cofig();
            ReadHairColorTypeConfig();
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

        private void ReadHairColorTypeConfig()
        {
            HairFrontChangeColorTypes.Clear();
            HairBackChangeColorTypes.Clear();
            string hairPrefabPath = "Assets/_ProjectOC/OCResources/PinchFace/Prefabs/PinchPart/5_Common_PinchType1";
            int _hairCount = 0;
            foreach (var _type3Data in typesDatas)
            {
                if (_type3Data._type3 == PinchPartType3.HF_HairFront)
                {
                    _hairCount = _type3Data.typeCount;
                    break;
                }
            }

            for (int i = 0; i < _hairCount; i++)
            {
                string _path0 = $"{hairPrefabPath}/15_HairFront_PinchType2/40_HF_HairFront_PinchType3/HF_HairFront_{i}.prefab";
                string _path1 = $"{hairPrefabPath}/16_HairBack_PinchType2/41_HB_HairBack_PinchType3/HB_HairBack_{i}.prefab";
                ReadPrefabConfig(_path0, 0);
                ReadPrefabConfig(_path1, 1);

                void ReadPrefabConfig(string _path, int _id)
                {
                    GameObject _prefab = AssetDatabase.LoadAssetAtPath<GameObject>(_path);
                    if (_prefab != null)
                    {
                        GameObject instance = PrefabUtility.InstantiatePrefab(_prefab) as GameObject;
                        Debug.Log(instance.name);
                        ChangeColorPinchSetting _colorPinchSetting = instance.GetComponent<ChangeColorPinchSetting>();
                        if (_id == 0)
                        {
                            HairFrontChangeColorTypes.Add((int)_colorPinchSetting.colorChangeType);
                        }
                        else
                        {
                            HairBackChangeColorTypes.Add((int)_colorPinchSetting.colorChangeType);
                        }
                        DestroyImmediate(instance);
                        
                    }
                    else
                    {
                        Debug.Log($"Can't find Prefab: {_path}");
                    }
                }
            }
        }
#endif
    }   
}
