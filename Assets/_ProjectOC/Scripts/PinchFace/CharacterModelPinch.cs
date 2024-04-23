using System;
using System.Collections;
using System.Collections.Generic;
using AmazingAssets.TerrainToMesh;
using ProjectOC.PinchFace;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using ML.MathExtension;
namespace ProjectOC.PinchFace
{
    public class CharacterModelPinch : MonoBehaviour
    {
        #region ��������

        public void GeneratePinchTypePrefab(PinchPartType3 _type3,int typeIndex)
        {
            
        }
        
        public void ChangeType(PinchPartType2 boneType2, int typeIndex)
        {
            if (replaceGo[(int)boneType2] != null)
            {
                UnEquipItem(boneType2,replaceGo[(int)boneType2]);
            }
            
            EquipItem(boneType2,tempTail[typeIndex]);
        }

        public Slider _slider;
        public void TempChangeType(int index)
        {
            ChangeType(PinchPartType2.HeadTop,index);
            return;
            
            if (index < 2)
            {
                ChangeType(PinchPartType2.Tail, index);
            }
            else if (index == 2)
            {
                ChangeType(PinchPartType2.TopHorn, index);
            }
            else if (index == 3)
            {
                ChangeTransform(PinchPartType2.EarTop,index,Vector2.right * _slider.value);
            }
            else
            {
                topEar.Release();
            }
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
            if (boneTransfsDictionary.TryGetValue(boneType2, out var keyBone))
            {
                foreach (Transform bone in keyBone)
                {
                    //ȥ������ ���Ƴ�Parent.Transform��Ȼ�� ɾ�� gameObject 
                    DisCatalog(boneCatalog,bone);
                    bone.SetParent(this.transform.parent);
                    Destroy(bone.gameObject);
                }

            }
            
            if (_PinchGo != null)
            {
                _PinchGo.transform.SetParent(this.transform.parent);
                Destroy(_PinchGo);   
            }
        }

        public GameObject Stitch(GameObject sourceClothing, GameObject targetCharacter)
        {
            GameObject targetClothingBone = null;
            targetClothingBone = new GameObject(sourceClothing.name);
            targetClothingBone.transform.SetParent(targetCharacter.transform);
            //if() ��Ҫ��ӹ���
            //׷�ӹ����������ֵ�  ,S��T ������source��target
            //ӳ�����
            
            Transform SkeyBone = BreadthFirstSearchForKey(sourceClothing.transform,AddStr);
            //�����������ʾ��Key������˵����Ҫ���׷���¹���
            if (SkeyBone != null && SkeyBone.childCount > 0)
            {
                //SkeyBone.name = SkeyBone.name.Replace(keyStr, "");
                Transform TKeyBone = boneCatalog[SkeyBone.name];
            
                // set parents,localPosition
                foreach (Transform _transBone in SkeyBone)
                {
                    GameObject SNewBone = GameObject.Instantiate(_transBone.gameObject);
                    SNewBone.name = _transBone.name;
            
            
                    SNewBone.transform.SetParent(TKeyBone);
                    SNewBone.transform.localPosition = _transBone.localPosition;
                    SNewBone.transform.localRotation = _transBone.localRotation;
                    SNewBone.transform.localScale = _transBone.localScale;
                    CataBonelog(boneCatalog,SNewBone.transform);
                }
                
            }
 
            //���smr�������ֵ��ӳ�����
            SkinnedMeshRenderer[] smrs = sourceClothing.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var smr in smrs)
            {
                Debug.LogWarning(smr.bones.Length);
                SkinnedMeshRenderer targetRenderer = AddSkinnedMeshRenderer(smr,targetClothingBone);
                targetRenderer.bones = TranslateTransforms (smr.bones, boneCatalog);
            }
            return targetClothingBone;
        }
        private SkinnedMeshRenderer AddSkinnedMeshRenderer (SkinnedMeshRenderer source, GameObject parent)
        {
            SkinnedMeshRenderer target = null;
            //����ж�������½����������SkinMesh
            if (parent.GetComponent<SkinnedMeshRenderer>() != null)
            {
                GameObject _newGO = new GameObject(source.gameObject.name);
                _newGO.transform.SetParent(parent.transform);
                target = _newGO.AddComponent<SkinnedMeshRenderer>();
            }
            else
            {
                target = parent.AddComponent<SkinnedMeshRenderer> ();
            }

            target.sharedMesh = source.sharedMesh;
            target.materials = source.materials;
            return target;
        }
        private Transform[] TranslateTransforms (Transform[] sources, Dictionary<string,Transform> _boneCatalog)
        {
            Transform[] targets = new Transform[sources.Length];
            for (int index = 0; index < sources.Length; index++)
            {
                //�п��ܻ�����֮ǰ�Ĺ������ݣ����ණ��ɾ����
                if(sources[index] == null)
                    continue;
                Transform transf  = _boneCatalog[sources[index].name];

                if (transf != null)
                {
                    targets[index] = transf;
                    Debug.Log(sources[index].name);
                }
                else
                {
                    Debug.Log("can't find: "+ sources [index].name);
                }
            }
            return targets;
        }

        public void ChangeTransform(PinchPartType2 boneType2,int index,Vector2 param)
        {
            if(boneType2 == PinchPartType2.EarTop)
            {

                if (topEar == null || topEar.index != index)
                {
                    BezierComponent _bezier = 
                        boneTransfsDictionary[PinchPartType2.EarTop].GetComponent<BezierComponent>();
                    topEar = new PinchJiao(_bezier,tempTail[index],index);
                }
                topEar.ModifyValue(param);
            }
            else if (boneType2 == PinchPartType2.HairBraid)
            {
                if (braid == null || braid.index != index)
                {
                    braid = new PinchBraid(tempTail[index], index);
                }
                braid.ModifyValue(param);
            }
        }
        
        public void ChangeTexture(PinchPartType2 boneType2, int textureIndex)
        {
            //����Mat �� _MainTex ����
        }

        public void ChangeBoneScale(BoneWeightType boneWeightType, Vector3 scaleValue)
        {
            if (boneWeightDictionary.ContainsKey(boneWeightType))
            {
                //�ϰ���Ҫ����������������Ҫ��������
                //������һ������Լ��������������
                if (boneWeightType == BoneWeightType.Arm)
                {
                       
                }
                else
                {
                    boneWeightDictionary[boneWeightType].localScale = scaleValue;                    
                }
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
        public 
                

        #endregion
        
        #region TransformCatalog
            string AddStr = "Add_";
            string WeightStr = "Weight_";
            private Transform avatar;
            [SerializeField] 
            List<GameObject> replaceGo = new List<GameObject>();
            
            public 
                List<GameObject> tempTail = new List<GameObject>();
            [ShowInInspector]
            private Dictionary<string, Transform> boneCatalog = new Dictionary<string, Transform>();
            [ShowInInspector]
            Dictionary<PinchPartType2, Transform> boneTransfsDictionary = new Dictionary<PinchPartType2, Transform>();
            [ShowInInspector]
            Dictionary<BoneWeightType, Transform> boneWeightDictionary = new Dictionary<BoneWeightType, Transform>();
            
            PinchJiao topEar;
            PinchBraid braid;
            
            void Awake()
            {
                Array enumValues = Enum.GetValues(typeof(PinchPartType2));
                for(int i = 0;i<enumValues.Length;i++)
                    replaceGo.Add(null);
                
                avatar = transform.Find("AnMiXiuBone");
                CataBonelog(boneCatalog,avatar);
                
                //�����ֵ��ʼ��
                boneWeightDictionary.Add(BoneWeightType.Head,boneCatalog["Head"]);
                //boneWeightDictionary.Add(BoneWeightType.Chest,transform);
                //boneWeightDictionary.Add(BoneWeightType.Arm,transform);
                boneWeightDictionary.Add(BoneWeightType.Waist,boneCatalog["Spine"]);
                boneWeightDictionary.Add(BoneWeightType.Leg,boneCatalog["Weight_Thin"]);
                boneWeightDictionary.Add(BoneWeightType.HeadTop,boneCatalog["Add_HeadTop"]);
                boneWeightDictionary.Add(BoneWeightType.Root,boneCatalog["Root"]);
                
                this.enabled = false;
            }
            
            
            //������������ֵ�    containThis : �費��Ҫ����������ֵ�
            private void CataBonelog (Dictionary<string,Transform>dic, Transform transform,bool containThis = true)
            {
                if (transform.name.StartsWith(AddStr))
                {
                    string tempName = transform.name.Replace(AddStr, "");
                    PinchPartType2 pinchPartType2;
                    if (Enum.TryParse(tempName, out pinchPartType2))
                    {
                        boneTransfsDictionary[pinchPartType2] = transform;
                        // boneTransfsDictionary.Add(pinchPartType2,transform);
                    }
                    else
                    {
                        Debug.LogWarning($"�޷�ƥ��PinchType2 Enum �� {tempName}");
                    }
                }
                else if(transform.name.StartsWith(WeightStr))
                {
                    //Ŀǰֻ��Tail ��ͨ������ƥ���
                    string tempName = transform.name.Replace(WeightStr, "");
                    BoneWeightType boneWeightType;
                    if (Enum.TryParse(tempName, out boneWeightType))
                    {
                        boneWeightDictionary[boneWeightType] = transform;
                    }
                }
                
                
                if (containThis)
                {
                    dic[transform.name] = transform;
                }
                
                foreach (Transform child in transform)
                    CataBonelog (dic,child);
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