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
        #region ��������
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
            //���� gameObejct Asset
            //װ��
            //ɾ��gameObject Instance
            _PinchGo = GameObject.Instantiate(_PinchGo);
            // sourceClothing�·� targetAvatar ��ɫ
            
            
            Destroy(_PinchGo);
        }

        public void UnEquipItem(PinchPartType2 boneType2, GameObject _PinchGo)
        {
            //���Ƴ�Parent.Transform��Ȼ�� ɾ�� gameObject 
        }

        void EquipBone(PinchPartType2 boneType2)
        {
            
        }

        
        public void ChangeTexture(PinchPartType2 boneType2, int textureIndex)
        {
            //����Mat �� _MainTex ����
        }

        public void ChangeBoneScale(PinchPartType2 boneType2, Vector3 scaleValue)
        {
            //�ϰ���Ҫ����������������Ҫ��������
            if (boneType2 == PinchPartType2.Arm)
            {
                
            }
            else
            {
                //����������ֵ�����
                boneTransf[(int)boneType2 - (int)(PinchPartType2.Body)].localScale = scaleValue;    
            }
        }

        public void ChangeBoneScale(Transform boneTransf, Vector3 scaleValue)
        {
            boneTransf.localScale = scaleValue;
        }

        public void ChangeColor(PinchPartType2 boneType2, Color _color)
        {
            //��boneType�ҵ���Ӧ������ Mat������Mat��_Color ����
        }
        
                

        #endregion
        
        #region TransformCatalog
        
            void Awake()
            {
                Transform _avatar = transform.Find("AnMiXiuBone");
                Catalog(boneCatalog,_avatar);
            }
            
            //������������ֵ�
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