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

        
        public 
        List<GameObject> tempTail = new List<GameObject>();
        [ShowInInspector]
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
            ChangeType(PinchPartType2.Tail, index);
        }
        public void EquipItem(PinchPartType2 boneType2, GameObject _PinchGo)
        {
            //生成 gameObejct Asset
            //装备
            //删除gameObject Instance
            _PinchGo = GameObject.Instantiate(_PinchGo);
            // sourceClothing衣服 targetAvatar 角色
            replaceGo[(int)boneType2] = Stitch(_PinchGo, avatar.gameObject);
            
            //to-do: 更新骨骼解算目录
            Destroy(_PinchGo);
        }

        public void UnEquipItem(PinchPartType2 boneType2, GameObject _PinchGo)
        {
            if (boneTransfsDictionary.TryGetValue(boneType2, out var keyBone))
            {
                foreach (Transform bone in keyBone)
                {
                    //去除骨骼 ，移出Parent.Transform，然后 删除 gameObject 
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
            //if() 需要添加骨骼
            //追加骨骼，存入字典  ,S、T 来区分source、target
            //映射骨骼
            
            Transform SkeyBone = BreadthFirstSearchForKey(sourceClothing.transform,AddStr);
            //如果部件中显示有Key骨骼，说明需要添加追加新骨骼
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
                    Catalog(boneCatalog,SNewBone.transform);
                }
                
            }
 
            //添加smr组件并赋值、映射骨骼
            SkinnedMeshRenderer[] smrs = sourceClothing.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var smr in smrs)
            {
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
                //有可能还存有之前的骨骼数据，有脏东西删不掉
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
            //更换Mat 的 _MainTex 参数
        }

        public void ChangeBoneScale(BoneWeightType boneWeightType, Vector3 scaleValue)
        {
            if (boneWeightDictionary.ContainsKey(boneWeightType))
            {
                //上半身要缩放两跟骨骼，需要单独处理
                //考虑用一根骨骼约束驱动两根骨骼
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
            //从boneType找到对应的两个 Mat，更改Mat的_Color 属性
        }
        
                

        #endregion
        
        #region TransformCatalog
            string AddStr = "Add_";
            string WeightStr = "Weight_";
            [ShowInInspector]
            Dictionary<PinchPartType2, Transform> boneTransfsDictionary = new Dictionary<PinchPartType2, Transform>();
            [ShowInInspector]
            Dictionary<BoneWeightType, Transform> boneWeightDictionary = new Dictionary<BoneWeightType, Transform>();
            void Awake()
            {
                Array enumValues = Enum.GetValues(typeof(PinchPartType2));
                for(int i = 0;i<enumValues.Length;i++)
                    replaceGo.Add(null);
                
                avatar = transform.Find("AnMiXiuBone");
                Catalog(boneCatalog,avatar);
                // boneWeightDictionary.Add(BoneWeightType.Head,transform);
                // boneWeightDictionary.Add(BoneWeightType.Chest,transform);
                // boneWeightDictionary.Add(BoneWeightType.Waist,transform);
                // boneWeightDictionary.Add(BoneWeightType.Leg,transform);
                // boneWeightDictionary.Add(BoneWeightType.Root,transform);
                this.enabled = false;
            }
            
            
            //将子物体加入字典    containThis : 需不需要把自身加入字典
            private void Catalog (Dictionary<string,Transform>dic, Transform transform,bool containThis = true)
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
                        Debug.LogWarning($"无法匹配PinchType2 Enum ： {tempName}");
                    }
                }
                else if(transform.name.StartsWith(WeightStr))
                {
                    //目前只有Tail 是通过这里匹配的
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
            

            // 广度优先搜索KeyBone
            private Transform BreadthFirstSearchForKey(Transform root,string _str)
            {
                Queue<Transform> queue = new Queue<Transform>();
                queue.Enqueue(root);

                while (queue.Count > 0)
                {
                    Transform currentNode = queue.Dequeue();
                    Transform currentTransform = currentNode.transform;

                    // 检查当前Transform的名字是否以 "Key_" 开头
                    if (currentTransform.name.StartsWith(_str))
                    {
                        return currentTransform;
                    }

                    // 将当前Transform的子Transform加入队列
                    foreach (Transform child in currentTransform)
                    {
                        queue.Enqueue(child);
                    }
                }

                // 如果没找到符合条件的Transform，返回null
                return null;
            }
            #endregion
    }
}