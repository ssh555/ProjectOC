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
using ML.PlayerCharacterNS;
using ProjectOC.ManagerNS;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectOC.PinchFace
{
    public class CharacterModelPinch : MonoBehaviour
    {
        #region 捏脸函数

        public AsyncOperationHandle<GameObject> GeneratePinchTypePrefab(PinchPartType3 _type3,int typeIndex)
        {
            //Prefabs_PinchPart/PinchPart/
            //0_HeadPart_PinchType1/0_HeadTop_PinchType2/0_HT_Ring_PinchType3/HT_Ring_0.prefab
            string prePath = "Prefabs_PinchPart/PinchPart";
            string typePath = pinchFaceManager.pinchFaceHelper.GetType3PrefabPath(_type3);
            string prefabPath = $"{_type3.ToString()}_{typeIndex}.prefab";
            string totalPath = $"{prePath}/{typePath}/{prefabPath}";
            AsyncOperationHandle<GameObject>  handle = ML.Engine.Manager.GameManager.Instance.
                ABResourceManager.InstantiateAsync(totalPath);
            return handle;
        }
        
        public void ChangeType(PinchPartType3 _type3, int typeIndex)
        {
            PinchPartType2 _type2 = pinchFaceManager.pinchPartType3Dic[_type3];
            if (replaceGo[(int)_type2] != null)
            {
                UnEquipItem(_type2,replaceGo[(int)_type2]);
            }

            GeneratePinchTypePrefab(_type3, typeIndex).Completed += (handle)=>
            {
                EquipItem(_type2,handle.Result);
            };
        }
        
        public void TempChangeType(int index)
        {
            // ChangeType(PinchPartType2.HeadTop,index);
            // return;
            //
            // if (index < 2)
            // {
            //     ChangeType(PinchPartType2.Tail, index);
            // }
            // else if (index == 2)
            // {
            //     ChangeType(PinchPartType2.TopHorn, index);
            // }
            // else if (index == 3)
            // {
            //     ChangeTransform(PinchPartType2.EarTop,index,Vector2.right * _slider.value);
            // }
            // else
            // {
            //     topEar.Release();
            // }
        }
        public void EquipItem(PinchPartType2 boneType2, GameObject _PinchGo)
        {
            // sourceClothing衣服 targetAvatar 角色
            replaceGo[(int)boneType2] = Stitch(_PinchGo, avatar.gameObject);
            
            //to-do: 更新骨骼解算目录
            Destroy(_PinchGo);
        }

        public void UnEquipItem(PinchPartType2 boneType2, GameObject _PinchGo)
        {
            if (boneType2 == PinchPartType2.Tail && boneWeightDictionary.ContainsKey(BoneWeightType.Tail))
                boneWeightDictionary.Remove(BoneWeightType.Tail);
            
            
            if (boneTransfsDictionary.TryGetValue(boneType2, out var keyBone))
            {
                foreach (Transform bone in keyBone)
                {
                    //去除骨骼 ，移出Parent.Transform，然后 删除 gameObject 
                    DisCatalog(boneDic,bone);
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
            targetClothingBone.layer = LayerMask.NameToLayer("UICamera");;
            targetClothingBone.transform.SetParent(targetCharacter.transform);
            //if() 需要添加骨骼
            //追加骨骼，存入字典  ,S、T 来区分source、target
            //映射骨骼
            
            Transform SkeyBone = BreadthFirstSearchForKey(sourceClothing.transform,AddStr);
            //如果部件中显示有Key骨骼，说明需要添加追加新骨骼
            if (SkeyBone != null && SkeyBone.childCount > 0)
            {
                //SkeyBone.name = SkeyBone.name.Replace(keyStr, "");
                Transform TKeyBone = boneDic[SkeyBone.name];
            
                // set parents,localPosition
                foreach (Transform _transBone in SkeyBone)
                {
                    GameObject SNewBone = GameObject.Instantiate(_transBone.gameObject);
                    SNewBone.name = _transBone.name;
            
            
                    SNewBone.transform.SetParent(TKeyBone);
                    SNewBone.transform.localPosition = _transBone.localPosition;
                    SNewBone.transform.localRotation = _transBone.localRotation;
                    SNewBone.transform.localScale = _transBone.localScale;
                    CataBonelog(boneDic,SNewBone.transform);
                }
                
            }
 
            //添加smr组件并赋值、映射骨骼
            SkinnedMeshRenderer[] smrs = sourceClothing.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var smr in smrs)
            {
                SkinnedMeshRenderer targetRenderer = AddSkinnedMeshRenderer(smr,targetClothingBone);
                targetRenderer.bones = TranslateTransforms (smr.bones, boneDic);
            }
            return targetClothingBone;
        }
        private SkinnedMeshRenderer AddSkinnedMeshRenderer (SkinnedMeshRenderer source, GameObject parent)
        {
            SkinnedMeshRenderer target = null;
            //如果有多个，则新建子物体添加SkinMesh
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
                //有可能还存有之前的骨骼数据，有脏东西删不掉
                if(sources[index] == null)
                    continue;
                Transform transf  = _boneCatalog[sources[index].name];

                if (transf != null)
                {
                    targets[index] = transf;
                    //Debug.Log(sources[index].name);
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
            //更换Mat 的 _MainTex 参数
        }

        
        /// <summary>
        /// 从 0.5 -- 1.5  乘以默认BoneScale
        /// </summary>
        /// <param name="boneWeightType"></param>
        /// <param name="scaleValue"></param>
        public void ChangeBoneScale(BoneWeightType boneWeightType, Vector3 scaleValue,bool isScale = true)
        {
            if (boneWeightDictionary.ContainsKey(boneWeightType))
            {
                if (!isScale)
                {
                    //光环
                    Vector3 _defaultPos = boneWeightDictionary[boneWeightType].defaultPos;
                    boneWeightDictionary[boneWeightType].boneTransform.localPosition = _defaultPos + scaleValue;  
                    return;
                }
                
                //上半身要缩放两跟骨骼，需要单独处理
                //考虑用一根骨骼约束驱动两根骨骼
                
                if (boneWeightType == BoneWeightType.Arm)
                {
                    
                }
                else if (boneWeightType == BoneWeightType.Leg || boneWeightType == BoneWeightType.Waist)
                {
                    scaleValue = new Vector3(scaleValue.x, 1, scaleValue.z);
                }

                Vector3 _defaultScale = boneWeightDictionary[boneWeightType].defaultScale;
                boneWeightDictionary[boneWeightType].boneTransform.localScale = 
                    new Vector3(_defaultScale.x * scaleValue.x,_defaultScale.y * scaleValue.y,_defaultScale.z * scaleValue.z);                    
   
            }
        }

        public void ChangeColor(PinchPartType2 boneType2, Color _color)
        {
            //从boneType找到对应的两个 Mat，更改Mat的_Color 属性
            List<Material> mats = new List<Material>();
            
            SkinnedMeshRenderer smr = replaceGo[(int)boneType2].GetComponent<SkinnedMeshRenderer>();
            mats.AddRange(smr.materials);
            if (mats.Count == 0)
            {
                MeshRenderer meshRender =  replaceGo[(int)boneType2].GetComponent<MeshRenderer>();
                mats.AddRange(meshRender.materials);
            }
            
            // mats.Find(_mat =>()
            // {
            //     
            // })
            
        }


        #endregion
        
        #region TransformCatalog
            string AddStr = "Add_";
            string WeightStr = "Weight_";
            private Transform avatar;
            [SerializeField, ReadOnly, FoldoutGroup("骨骼字典")]
            List<GameObject> replaceGo = new List<GameObject>();
            
            [SerializeField, ReadOnly, FoldoutGroup("骨骼字典")]
                List<GameObject> tempTail = new List<GameObject>();
            [ShowInInspector, ReadOnly, FoldoutGroup("骨骼字典")]
            private Dictionary<string, Transform> boneDic = new Dictionary<string, Transform>();
            [ShowInInspector, ReadOnly, FoldoutGroup("骨骼字典")]
            Dictionary<PinchPartType2, Transform> boneTransfsDictionary = new Dictionary<PinchPartType2, Transform>();
            [ShowInInspector, ReadOnly, FoldoutGroup("骨骼字典")]
            Dictionary<BoneWeightType, BoneData> boneWeightDictionary = new Dictionary<BoneWeightType, BoneData>();
            
            public class BoneData
            {
                public Transform boneTransform;
                public Vector3 defaultPos;
                public Vector3 defaultScale;

                public BoneData(Transform _trans)
                {
                    boneTransform = _trans;
                    defaultPos = _trans.localPosition;
                    defaultScale = _trans.localScale;
                }
            }
            
            
            PinchJiao topEar;
            PinchBraid braid;
            
            void Awake()
            {
                pinchFaceManager = LocalGameManager.Instance.PinchFaceManager;
                CameraView.Init(pinchFaceManager);
                
                Array enumValues = Enum.GetValues(typeof(PinchPartType2));
                for(int i = 0;i<enumValues.Length;i++)
                    replaceGo.Add(null);
                
                avatar = transform.Find("AnMiXiuBone");
                CataBonelog(boneDic,avatar);
                
                //骨骼字典初始化
                boneWeightDictionary.Add(BoneWeightType.Head,new BoneData(boneDic["Head"]));
                //boneWeightDictionary.Add(BoneWeightType.Chest,transform);
                //boneWeightDictionary.Add(BoneWeightType.Arm,transform);
                boneWeightDictionary.Add(BoneWeightType.Waist,new BoneData(boneDic["Spine"]));
                boneWeightDictionary.Add(BoneWeightType.Leg,new BoneData(boneDic["Weight_Thin"]));
                boneWeightDictionary.Add(BoneWeightType.HeadTop,new BoneData(boneDic["Add_HeadTop"]));
                boneWeightDictionary.Add(BoneWeightType.Root,new BoneData(boneDic["Root"]));
                
                this.enabled = false;
            }
            
            
            //将子物体加入字典    containThis : 需不需要把自身加入字典
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
                        //Debug.LogWarning($"无法匹配PinchType2 Enum ： {tempName}");
                    }
                }
                else if(transform.name.StartsWith(WeightStr))
                {
                    //目前只有Tail 是通过这里匹配的
                    string tempName = transform.name.Replace(WeightStr, "");
                    BoneWeightType boneWeightType;
                    if (Enum.TryParse(tempName, out boneWeightType))
                    {
                        boneWeightDictionary[boneWeightType] = new BoneData(transform);
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
                    if(transform.name.StartsWith(WeightStr))
                    {
                        //目前只有Tail 是通过这里匹配的
                        string tempName = transform.name.Replace(WeightStr, "");
                        BoneWeightType boneWeightType;
                        if (Enum.TryParse(tempName, out boneWeightType))
                        {
                            boneWeightDictionary.Remove(boneWeightType);
                        }
                    }
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

        #region CharacterModelPinch

        private PinchFaceManager pinchFaceManager;
        
        [LabelText("相机配置"), SerializeField,  FoldoutGroup("物体|脚本引用")]
        public PinchCharacterCameraView CameraView;

        #endregion
    }
}