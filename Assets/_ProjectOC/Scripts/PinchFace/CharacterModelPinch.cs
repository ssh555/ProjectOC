using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmazingAssets.TerrainToMesh;
using ML.Engine.UI;
using ProjectOC.PinchFace;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using ML.MathExtension;
using ML.PlayerCharacterNS;
using ProjectOC.ManagerNS;
using UnityEngine.Rendering;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectOC.PinchFace
{
    public class CharacterModelPinch : MonoBehaviour
    {
        #region Unity

        

        #endregion
        
        #region ��������
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

        /// <summary>
        /// ��ֹ�����޸�
        /// </summary>
        /// <param name="_type2"></param>
        /// <returns></returns>
        private int Type2ToTypeReplaceGoIndex(PinchPartType2 _type2)
        {
            return (int)_type2 - 1;
        }
        
        //temp
        private int curHairIndex = -1;
        public AsyncOperationHandle<GameObject> ChangeType(PinchPartType3 _type3, int typeIndex,bool inCamera = false)
        {
            
            PinchPartType2 _type2 = pinchFaceManager.pinchPartType3Dic[_type3];
            AsyncOperationHandle<GameObject> _handle = GeneratePinchTypePrefab(_type3, typeIndex);
            if (_type3 == PinchPartType3.HF_HairFront)
            {
                curHairIndex = typeIndex;
            }
            #region ��β����  ���⴦��
            if(_type2 == PinchPartType2.EarTop)
            {
                if (topEar != null)
                {
                    topEar.Release();
                }
            
                BezierComponent _bezier = boneTransfsDictionary[PinchPartType2.EarTop].GetComponent<BezierComponent>();
                _handle.Completed += (handle) =>
                {
                    topEar = new PinchJiao(_bezier,_handle.Result,typeIndex,inCamera);
                };
                return _handle;
            }
            else if (_type2 == PinchPartType2.HairBraid)
            {
                if (braid != null)
                {
                    braid.Release();
                }
                Transform _HeadBone = boneDic["Head"];
                _handle.Completed += (handle) =>
                {
                    braid = new PinchBraid(_HeadBone,_handle.Result,typeIndex,Vector2.zero);
                };
                return _handle;
                
            }
            #endregion
            if (replaceGo[Type2ToTypeReplaceGoIndex(_type2)] != null)
            {
                UnEquipItem(_type2);
            }

            
            _handle.Completed += (handle)=>
            {
                //�ӳ�һִ֡��
                StartCoroutine(EquipExecuteAfterOneFrame());
                IEnumerator EquipExecuteAfterOneFrame()
                {
                    yield return null;
                    EquipItem(_type2,handle.Result,inCamera);
                }
            };
            return _handle;
        }

        
        
        public void EquipItem(PinchPartType2 _type2, GameObject _PinchGo,bool inCamera)
        {
            // sourceClothing�·� targetAvatar ��ɫ
            replaceGo[Type2ToTypeReplaceGoIndex(_type2)] = Stitch(_PinchGo, avatar.gameObject,inCamera);
            
            //to-do: ���¹�������Ŀ¼
            Destroy(_PinchGo);
        }

        public void UnEquipItem(PinchPartType2 _type2)
        {
            if (_type2 == PinchPartType2.EarTop)
            {
                if (topEar != null)
                {
                    topEar.Release();
                    topEar = null;    
                }
            }
            
            if (_type2 == PinchPartType2.Tail && boneWeightDictionary.ContainsKey(BoneWeightType.Tail))
                boneWeightDictionary.Remove(BoneWeightType.Tail);
            
            
            if (boneTransfsDictionary.TryGetValue(_type2, out var keyBone))
            {
                foreach (Transform bone in keyBone)
                {
                    //ȥ������ ���Ƴ�Parent.Transform��Ȼ�� ɾ�� gameObject 
                    DisCatalog(boneDic,bone);
                    bone.SetParent(this.transform.parent);
                    Destroy(bone.gameObject);
                }

            }
            if (replaceGo[Type2ToTypeReplaceGoIndex(_type2)] != null)
            {
                GameObject _PinchGo = replaceGo[Type2ToTypeReplaceGoIndex(_type2)];
                _PinchGo.transform.SetParent(this.transform.parent);
                Destroy(_PinchGo);   
            }
        }

        public void UnEquipAllItem(bool UnEquipHair = false)
        {
            //��һ����0
            int maxCount = replaceGo.Count;
            if (!UnEquipHair)
            {
                maxCount = (int)PinchPartType2.HairFront;
            }
                
            for (int i = 1; i < maxCount; i++)
            {
                if(replaceGo[i] != null)
                    continue;
                
                UnEquipItem((PinchPartType2)i);
            }
        }
        
        public GameObject Stitch(GameObject sourceClothing, GameObject targetCharacter,bool inCamera)
        {
            GameObject targetClothingBone = null;
            targetClothingBone = new GameObject(sourceClothing.name);
            targetClothingBone.layer = LayerMask.NameToLayer("UICamera");;
            targetClothingBone.transform.SetParent(targetCharacter.transform);
            //if() ��Ҫ��ӹ���
            //׷�ӹ����������ֵ�  ,S��T ������source��target
            //ӳ�����
            
            Transform SkeyBone = BreadthFirstSearchForKey(sourceClothing.transform,AddStr);
            //�����������ʾ��Key������˵����Ҫ���׷���¹���
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
 
            //���smr�������ֵ��ӳ�����
            SkinnedMeshRenderer[] smrs = sourceClothing.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var smr in smrs)
            {
                // Destroy(smr);
                SkinnedMeshRenderer targetRenderer = AddSkinnedMeshRenderer(smr,targetClothingBone);
                if (inCamera)
                {
                    UICameraImage.ModeGameObjectLayer(targetRenderer.transform);
                }
                targetRenderer.bones = TranslateTransforms (smr.bones, boneDic);
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
            
            target.sharedMaterials = source.materials;
            //todo ?AB���������ʶ�ʧ
            target.sharedMesh = source.sharedMesh;
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
                }
                else
                {
                    Debug.Log("can't find: "+ sources [index].name);
                }
            }
            return targets;
        }

        public void ChangeTransform(PinchPartType2 boneType2,Vector2 param)
        {
            if(boneType2 == PinchPartType2.EarTop)
            {
                if(topEar != null)
                    topEar.ModifyValue(param);
            }
            else if (boneType2 == PinchPartType2.HairFront)
            {
                if(braid != null)
                    braid.ModifyValue(param);
            }
        }
        
        public void ChangeTexture(PinchPartType3 _type3 , int textureIndex)
        {
            //����Mat �� _MainTex ����
            //����Ҳͨ��
            //string texABPath = "SA_UI_PinchFace/PinchPart/5_Common_PinchType1/19_Eye_PinchType2/44_O_Orbit_PinchType3/O_Orbit_0.jpg";
            string prePath = "Prefabs_PinchPart/PinchPart";
            string typePath = pinchFaceManager.pinchFaceHelper.GetType3PrefabPath(_type3);
            string texturePath = $"{_type3.ToString()}_{textureIndex}.png";
            string totalPath = $"{prePath}/{typePath}/{texturePath}";
            
            PinchPartType2 _type2 = pinchFaceManager.pinchPartType3Dic[_type3];
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<Texture>(totalPath)
                .Completed += (handle) =>
            {
                //todo �����ĳ� MainTex
                if (_type2 == PinchPartType2.Eye)
                {
                    foreach (Material _material in characterSkinMeshRenderer.materials)
                    {
                        if (_material.name.Contains("Mat_Eye"))
                        {
                            _material.SetTexture("_BaseMap",handle.Result);   
                        }
                    }
                }
            };


        }

        
        /// <summary>
        /// �� 0.5 -- 1.5  ����Ĭ��BoneScale
        /// </summary>
        /// <param name="boneWeightType"></param>
        /// <param name="scaleValue"></param>
        public void ChangeBoneScale(BoneWeightType boneWeightType, Vector3 scaleValue,bool isScale = true)
        {
            if (boneWeightDictionary.ContainsKey(boneWeightType))
            {
                if (!isScale)
                {
                    //�⻷
                    Vector3 _defaultPos = boneWeightDictionary[boneWeightType].defaultPos;
                    boneWeightDictionary[boneWeightType].boneTransform.localPosition = _defaultPos + scaleValue;  
                    return;
                }
                
                //�ϰ���Ҫ����������������Ҫ��������
                //������һ������Լ��������������
                
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
        string matColorName = "_ColorChange";
        //ͷ��˫ɫ��
        // string matColorName2 = "Mat_ColorChange2";
        private Material GetMaterial(PinchPartType2 _type2)
        {
            GameObject _replaceGo = replaceGo[Type2ToTypeReplaceGoIndex(_type2)];
            if (_replaceGo == null)
            {
                Debug.LogWarning($"û����������:{_type2.ToString()}");
                return null;
            }
            return GetMaterial(_replaceGo);
        }

        private Material GetMaterial(GameObject _replaceGo)
        {
            Material[] mats = _replaceGo.GetComponentInChildren<SkinnedMeshRenderer>().materials;
            if (mats == null)
            {
                mats = _replaceGo.GetComponentInChildren<MeshRenderer>().materials;
            }
            foreach (var _mat in mats)
            {
                if (_mat.name.Contains(matColorName))
                    return _mat;
            }
            return null;
        }
        
        private string changeColorShaderKey = "_BaseColor";
        private string changeColroShaderKey2 = "_Color2";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_type2"></param>
        /// <param name="_color"></param>
        /// <param name="_index">��ɫ���</param>
        /// <param name="colorType">��ɫģʽ</param>
        public void ChangeColor(PinchPartType2 _type2, Color _color,int _index)
        {
            //���⴦��
            // if (_type2 == PinchPartType2.EarTop)
            // {
            //     Material _targetMat = GetMaterial();
            //     _targetMat.SetColor(changeColorShaderKey,_color);   
            //     return;
            // }
            //
            if (_type2 == PinchPartType2.HairFront)
            {
                ChangeColor( PinchPartType2.HairBack, _color, _index);
            }
            
            Material _targetMat = GetMaterial(_type2);
            if (_targetMat == null)
                return;
            
            Debug.LogError($"Mat:{_targetMat.name} Shader:{_targetMat.shader.name}");
            if (_index == 0)
            {
                _targetMat.SetColor(changeColorShaderKey,_color);   
            }
            else
            {
                _targetMat.SetColor(changeColroShaderKey2,_color);
            }

            
            
        }

        public void ChangeColorType(PinchPartType2 _type2,int colorType,float _param1,float _param2)
        {
            if (_type2 == PinchPartType2.HairFront)
            {
                ChangeColorType( PinchPartType2.HairBack, colorType,_param1,_param2);
            }
            
            Material _targetMat = GetMaterial(_type2);
            if (_targetMat == null)
                return;
            if (colorType != -1)
            {
                int _curColor = 1 << colorType;
                if (_type2 == PinchPartType2.HairFront &&
                    (pinchFaceManager.Config.HairFrontChangeColorTypes[curHairIndex] & _curColor) != 0
                    || (_type2 == PinchPartType2.HairBack &&
                        (pinchFaceManager.Config.HairBackChangeColorTypes[curHairIndex] & _curColor) != 0))
                {
                    //��Shader��Intֵ����
                    if (colorType > 2)
                        colorType--;
                    _targetMat.SetInt("_ColorType",colorType);
                }
                else
                {
                    _targetMat.SetInt("_ColorType",0);
                }
                
                
                _targetMat.SetFloat("_smoothStepThreshold",_param1);
                _targetMat.SetFloat("_smoothStrength",_param2);
            }
        }
        
        #endregion

        #region public����

        //

        public Color GetType2Color(PinchPartType2 _type2,int index = 0)
        {
            Material _targetMat = GetMaterial(_type2);
            if (_targetMat == null)
                return Color.clear;
            Color resColor = Color.clear;
            
                if (index == 0 && _targetMat.HasColor(changeColorShaderKey))
                    resColor = _targetMat.GetColor(changeColorShaderKey);
                else if(index == 1 && _targetMat.HasColor(changeColroShaderKey2))
                    resColor = _targetMat.GetColor(changeColroShaderKey2);
                
            return resColor;
        }
        

        #endregion
        
        #region �������ݴ���
            string AddStr = "Add_";
            string WeightStr = "Weight_";
            private Transform avatar;
            private SkinnedMeshRenderer characterSkinMeshRenderer;
            [SerializeField, ReadOnly, FoldoutGroup("�����ֵ�")]
            List<GameObject> replaceGo = new List<GameObject>();
            [ShowInInspector, ReadOnly, FoldoutGroup("�����ֵ�")]
            private Dictionary<string, Transform> boneDic = new Dictionary<string, Transform>();
            [ShowInInspector, ReadOnly, FoldoutGroup("�����ֵ�")]
            Dictionary<PinchPartType2, Transform> boneTransfsDictionary = new Dictionary<PinchPartType2, Transform>();
            [ShowInInspector, ReadOnly, FoldoutGroup("�����ֵ�")]
            public Dictionary<BoneWeightType, BoneData> boneWeightDictionary = new Dictionary<BoneWeightType, BoneData>();
            private List<PinchPart.PinchPartData> PinchPartDatas = new List<PinchPart.PinchPartData>();
            
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
                //���ݳ�ʼ��
                pinchFaceManager = LocalGameManager.Instance.PinchFaceManager;
                CameraView.Init(pinchFaceManager);
                
                Array enumValues = Enum.GetValues(typeof(PinchPartType2));
                for(int i = 0;i<enumValues.Length;i++)
                    replaceGo.Add(null);
                avatar = transform.Find("AnMiXiuBone");
                characterSkinMeshRenderer = transform.Find("AnMiXiu_Mesh").GetComponent<SkinnedMeshRenderer>();
                CataBonelog(boneDic,avatar);
                //�����ֵ��ʼ��
                boneWeightDictionary.Add(BoneWeightType.Head,new BoneData(boneDic["Head"]));
                //boneWeightDictionary.Add(BoneWeightType.Chest,transform);
                //boneWeightDictionary.Add(BoneWeightType.Arm,transform);
                boneWeightDictionary.Add(BoneWeightType.Waist,new BoneData(boneDic["Spine"]));
                boneWeightDictionary.Add(BoneWeightType.Leg,new BoneData(boneDic["Weight_Thin"]));
                boneWeightDictionary.Add(BoneWeightType.HeadTop,new BoneData(boneDic["Add_HeadTop"]));
                boneWeightDictionary.Add(BoneWeightType.Root,new BoneData(boneDic["Root"]));


                if (pinchFaceManager.Config != null)
                {
                    ChangeType(PinchPartType3.HF_HairFront, 0);
                    //ChangeType(PinchPartType3.HD_Dai, 0);
                    ChangeType(PinchPartType3.HB_HairBack, 0);
                    // ChangeType(PinchPartType3.HB_HairBraid, 0);
                }
                
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
                        //Debug.LogWarning($"�޷�ƥ��PinchType2 Enum �� {tempName}");
                    }
                }
                else if(transform.name.StartsWith(WeightStr))
                {
                    //Ŀǰֻ��Tail ��ͨ������ƥ���
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
                        //Ŀǰֻ��Tail ��ͨ������ƥ���
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

        #region CharacterModelPinch

        private PinchFaceManager pinchFaceManager;
        
        [LabelText("�������"), SerializeField,  FoldoutGroup("����|�ű�����")]
        public PinchCharacterCameraView CameraView;

        #endregion
#if UNITY_EDITOR
        // void OnDrawGizmos()
        // {
        //     if (braid != null)
        //     {
        //         Vector3 center = braid.CalculateCenter(braid.headBone,braid.offset);
        //         Gizmos.DrawWireSphere(center,braid.radius);
        //     }
        // }
#endif
    }
}