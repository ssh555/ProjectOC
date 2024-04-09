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
            replaceGo[(int)boneType2] = Stitch(_PinchGo, avatar.gameObject);
            
            //to-do: 更新骨骼解算目录
            Destroy(_PinchGo);
        }

        public void UnEquipItem(PinchPartType2 boneType2, GameObject _PinchGo)
        {
            Transform keyBone = boneTransf[(int)boneType2];
            //去除骨骼 ，移出Parent.Transform，然后 删除 gameObject 
            DisCatalog( boneCatalog,keyBone);
            keyBone.SetParent(this.transform.parent);
            Destroy(keyBone.gameObject);
            _PinchGo.transform.SetParent(this.transform.parent);
            Destroy(_PinchGo);
        }

        public GameObject Stitch(GameObject sourceClothing, GameObject targetCharacter)
        {
            GameObject targetClothingBone = null;
            //if() 需要添加骨骼
            //追加骨骼，存入字典  ,S、T 来区分source、target
            //映射骨骼
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

            //添加smr组件并赋值、映射骨骼
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
                avatar = transform.Find("AnMiXiuBone");
                Catalog(boneCatalog,avatar);
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