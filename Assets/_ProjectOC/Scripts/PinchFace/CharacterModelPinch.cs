using System;
using System.Collections;
using System.Collections.Generic;
using ProjectOC.PinchFace;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class CharacterModelPinch : MonoBehaviour
    {
        [SerializeField] 
        List<GameObject> replaceGo = new List<GameObject>();

        [SerializeField] 
        List<Transform> boneWeight = new List<Transform>();

        [SerializeField]
        List<Transform> boneTransf = new List<Transform>();

        public 
        List<GameObject> tempTail = new List<GameObject>();

        private Dictionary<string, Transform> boneCatalog = new Dictionary<string, Transform>();
        #region 捏脸函数
        public void ChangeType(PinchPartType2 boneType2, int typeIndex)
        {
            if (replaceGo[(int)boneType2] != null)
            {
                UnEquipItem(boneType2,replaceGo[(int)boneType2]);
            }
            EquipItem(boneType2,tempTail[typeIndex]);
        }
        
        public void TempChangeType(int index)
        {
            ChangeType(PinchPartType2.HeadTop, index);
        }
        public void EquipItem(PinchPartType2 boneType2, GameObject _PinchGo)
        {
            //生成 gameObejct Asset
            //装备
            //删除gameObject Instance
            _PinchGo = GameObject.Instantiate(_PinchGo);
            // sourceClothing衣服 targetAvatar 角色
            
            
            Destroy(_PinchGo);
        }

        public void UnEquipItem(PinchPartType2 boneType2, GameObject _PinchGo)
        {
            //先移出Parent.Transform，然后 删除 gameObject 
        }

        void EquipBone(PinchPartType2 boneType2)
        {
            
        }

        
        public void ChangeTexture(PinchPartType2 boneType2, int textureIndex)
        {
            //更换Mat 的 _MainTex 参数
        }

        public void ChangeBoneScale(PinchPartType2 boneType2, Vector3 scaleValue)
        {
            //上半身要缩放两跟骨骼，需要单独处理
            if (boneType2 == PinchPartType2.Arm)
            {
                
            }
            else
            {
                //身体骨骼部分的排序
                boneTransf[(int)boneType2 - (int)(PinchPartType2.Body)].localScale = scaleValue;    
            }
        }

        public void ChangeBoneScale(Transform boneTransf, Vector3 scaleValue)
        {
            boneTransf.localScale = scaleValue;
        }

        public void ChangeColor(PinchPartType2 boneType2, Color _color)
        {
            //从boneType找到对应的两个 Mat，更改Mat的_Color 属性
        }
        
                

        #endregion
        
        #region TransformCatalog
        
            void Awake()
            {
                Transform _avatar = transform.Find("AnMiXiuBone");
                Catalog(boneCatalog,_avatar);
            }
            
            //将子物体加入字典
            private void Catalog (Dictionary<string,Transform>dic, Transform transform,bool containThis = true)
            {
                if (containThis)
                {
                    if(dic.ContainsKey(transform.name))
                    {
                        dic.Remove(transform.name); 
                        dic.Add(transform.name, transform);
                    }
                    else
                    {
                        dic.Add(transform.name, transform);
                    }
                }
                
                foreach (Transform child in transform)
                    Catalog (dic,child);
            }
            
            #endregion
    
        
    }
}