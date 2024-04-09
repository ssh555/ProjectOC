using System;
using System.Collections;
using System.Collections.Generic;
using AmazingAssets.TerrainToMesh;
using ProjectOC.PinchFace;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class CharacterModelPinch : MonoBehaviour
    {
        private Transform avatar;
        [SerializeField] 
        List<GameObject> replaceGo = new List<GameObject>();

        [SerializeField] 
        List<Transform> boneWeight = new List<Transform>();

        [SerializeField]
        List<Transform> boneTransf = new List<Transform>();

        public 
        List<GameObject> tempTail = new List<GameObject>();
        [ShowInInspector]
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
            replaceGo[(int)boneType2] = Stitch(_PinchGo, avatar.gameObject);
            
            //to-do: ���¹�������Ŀ¼
            Destroy(_PinchGo);
        }

        public void UnEquipItem(PinchPartType2 boneType2, GameObject _PinchGo)
        {
            Transform keyBone = boneTransf[(int)boneType2];
            //ȥ������ ���Ƴ�Parent.Transform��Ȼ�� ɾ�� gameObject 
            DisCatalog( boneCatalog,keyBone);
            keyBone.SetParent(this.transform.parent);
            Destroy(keyBone.gameObject);
            _PinchGo.transform.SetParent(this.transform.parent);
            Destroy(_PinchGo);
        }

        public GameObject Stitch(GameObject sourceClothing, GameObject targetCharacter)
        {
            GameObject targetClothingBone = null;
            //if() ��Ҫ��ӹ���
            //׷�ӹ����������ֵ�  ,S��T ������source��target
            //ӳ�����
            string keyStr = "Key_";
            Transform SkeyBone = BreadthFirstSearchForKey(sourceClothing.transform,keyStr);
            if (SkeyBone != null)
            {
                SkeyBone.name = SkeyBone.name.Replace(keyStr, "");
                Transform TKeyBone = boneCatalog[SkeyBone.name];
            
                // set parents,localPosition
                foreach (var _trans in SkeyBone)
                {
                    
                }

                
                Transform SNewBone = SkeyBone.GetChild(0);
                targetClothingBone =GameObject.Instantiate(SNewBone.gameObject);
                targetClothingBone.name = SNewBone.name;
            
            
                targetClothingBone.transform.SetParent(TKeyBone);
                targetClothingBone.transform.localPosition = SNewBone.transform.localPosition;
                targetClothingBone.transform.localRotation = SNewBone.transform.localRotation;
                targetClothingBone.transform.localScale = SNewBone.transform.localScale;
                Catalog(boneCatalog,targetClothingBone.transform);
            }

            //���smr�������ֵ��ӳ�����
            SkinnedMeshRenderer[] smrs = sourceClothing.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var smr in smrs)
            {
                if (targetClothingBone == null)
                {
                    targetClothingBone = new GameObject(smr.gameObject.name);
                    targetClothingBone.transform.SetParent(targetCharacter.transform);
                }
                
                SkinnedMeshRenderer targetRenderer = AddSkinnedMeshRenderer(smr,targetClothingBone);
                targetRenderer.bones = TranslateTransforms (smr.bones, boneCatalog);
            }
            return targetClothingBone;
        }
        private SkinnedMeshRenderer AddSkinnedMeshRenderer (SkinnedMeshRenderer source, GameObject parent)
        {
            SkinnedMeshRenderer target = parent.AddComponent<SkinnedMeshRenderer> ();
            target.sharedMesh = source.sharedMesh;
            target.materials = source.materials;
            return target;
        }
        private Transform[] TranslateTransforms (Transform[] sources, Dictionary<string,Transform> _boneCatalog)
        {
            Transform[] targets = new Transform[sources.Length];
            for (int index = 0; index < sources.Length; index++)
            {
                if(sources[index] == null)
                    continue;
                Transform transf  = _boneCatalog[sources[index].name];

                if(transf != null)
                    targets[index] = transf;
                else
                {
                    Debug.Log("can't find: "+ sources [index].name);
                }
            }
            return targets;
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
                avatar = transform.Find("AnMiXiuBone");
                Catalog(boneCatalog,avatar);
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

            private void DisCatalog(Dictionary<string,Transform>dic, Transform transform)
            {
                if (dic.ContainsKey(transform.name))
                {
                    dic.Remove(transform.name);
                }

                foreach (Transform child in transform)
                {
                    DisCatalog(dic,child);
                }
            }
            

            // �����������KeyBone
            private Transform BreadthFirstSearchForKey(Transform root,string _str)
            {
                Queue<Transform> queue = new Queue<Transform>();
                queue.Enqueue(root);

                while (queue.Count > 0)
                {
                    Transform currentNode = queue.Dequeue();
                    Transform currentTransform = currentNode.transform;

                    // ��鵱ǰTransform�������Ƿ��� "Key_" ��ͷ
                    if (currentTransform.name.StartsWith(_str))
                    {
                        return currentTransform;
                    }

                    // ����ǰTransform����Transform�������
                    foreach (Transform child in currentTransform)
                    {
                        queue.Enqueue(child);
                    }
                }

                // ���û�ҵ�����������Transform������null
                return null;
            }
            #endregion
    
        
    }
}