using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using ML.Engine.SaveSystem;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using ProjectOC.PinchFace;
using ProjectOC.PinchFace.Config;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Object = System.Object;

namespace  ProjectOC.PinchFace
{
    public class PinchPart :ISaveData
    {
        public string SavePath { get; set; }
        public string SaveName { get; set; }
        public bool IsDirty { get; set; }
        public ML.Engine.Manager.GameManager.Version Version { get; set; }

        public object Clone()
        {
            return null;
        }


        #region 引用和变量
        private PinchFaceManager PinchFaceManager;
        private CharacterModelPinch ModelPinch => PinchFaceManager.ModelPinch;
        private UIPinchFacePanel PinchFacePanel;
        
        private SpriteAtlas SA_PinchPart=>PinchFacePanel.SA_PinchPart;
        private PinchDataConfig Config => PinchFacePanel.Config;
        public List<IPinchSettingComp> pinchSettingComps = new List<IPinchSettingComp>();
        private Transform containerTransf;
        private List<string> uiPrefabPaths = new List<string>();
        private int isInit = 0;
        private PinchPartType2 PinchPartType2;
        private PinchPartType3 PinchPartType3;
        
        private int sortCount = 0;
        private int BtnListCount = 0;
        #endregion
        //外部引用

        private void Init()
        {
            uiPrefabPaths.Add("OC/UI/PinchFace/Setting/Pinch_SettingHead.prefab");
            uiPrefabPaths.Add("OC/UI/PinchFace/Setting/Pinch_TypeSettingPanel.prefab");
            uiPrefabPaths.Add("OC/UI/PinchFace/Setting/Pinch_BoneWeightSettingPanel.prefab");
            uiPrefabPaths.Add("OC/UI/PinchFace/Setting/Pinch_ColorSettingPanel1.prefab");
            uiPrefabPaths.Add("OC/UI/PinchFace/Setting/Pinch_ColorSettingPanel2.prefab");
            uiPrefabPaths.Add("OC/UI/PinchFace/Setting/Pinch_ColorTypeSetting.prefab");
            uiPrefabPaths.Add("OC/UI/PinchFace/Setting/Pinch_TextureSettingPanel.prefab");
            uiPrefabPaths.Add("OC/UI/PinchFace/Setting/Pinch_TransformSettingPanel.prefab");

            PinchFaceManager = LocalGameManager.Instance.PinchFaceManager;
            
        }
        
        //控制_pinchSettingComps Buttons的生成
        public PinchPart(UIPinchFacePanel _PinchFacePanel,PinchPartType3 _type3,PinchPartType2 _type2, IPinchSettingComp[] _settingComps,Transform _containerTransf)
        {
            Init();
            
            PinchPartType3 = _type3;
            PinchPartType2 = _type2;
            pinchSettingComps = _settingComps.ToList();
            containerTransf = _containerTransf;
            PinchFacePanel = _PinchFacePanel;
            
            foreach (var _comp in _settingComps)
            {
                ProcessSetttingComp(_comp);
            }
        }

        /// <summary>
        /// 负责根据SettingComp生成对应UI
        /// </summary>
        /// <param name="SettingComp"></param>
        public void ProcessSetttingComp(IPinchSettingComp _comp)
        {
            if(_comp.GetType()== typeof(ChangeTypePinchSetting))
            {
                GenerateHeadUI("样式");
                GenerateTypeUI(PinchPartType3);
            }
            else if (_comp.GetType()== typeof(ChangeBoneWeightPinchSetting))
            {
                ChangeBoneWeightPinchSetting _weightComp = _comp as ChangeBoneWeightPinchSetting;
                GenerateHeadUI("尺寸");
                GenerateBoneWeightUI(_weightComp);
            }
            else if (_comp.GetType()== typeof(ChangeColorPinchSetting))
            {
                ChangeColorPinchSetting _colorComp = _comp as ChangeColorPinchSetting;
                //如果不止有纯色
                if ((_colorComp.colorChangeType & ChangeColorPinchSetting.ColorChangeType.PureColor) != ChangeColorPinchSetting.ColorChangeType.PureColor)
                {
                    GenerateHeadUI("分色样式");
                    GenerateColorTypeUI(_colorComp);
                }
                GenerateHeadUI("颜色");
                GenerateColorUI1(PinchPartType3,Color.white);

            }
            else if (_comp.GetType()== typeof(ChangeTexturePinchSetting))
            {
                GenerateHeadUI("纹理");
                GenerateTextureUI(PinchPartType3);
            }
            else if (_comp.GetType()== typeof(ChangeTransformPinchSetting))
            {
                ChangeTransformPinchSetting transComp = _comp as ChangeTransformPinchSetting;
                GenerateHeadUI("位置");
                GenerateTransformTypeUI(transComp);
            }
        }
        
        
        
        /// <summary>
        /// 生成栏位标题
        /// </summary>
        /// <param name="标题名"></param>
        
        public void GenerateHeadUI(string _str)
        {
            int _counter = sortCount;
            GenerateUIPre();
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(uiPrefabPaths[0])
                .Completed += (handle) =>
            {
                Transform _trans = handle.Result.transform;
                _trans.GetComponentInChildren<TextMeshProUGUI>().text = _str;
                GenerateUICallBack(_trans, _counter);
            };
        }
        
        /// <summary>
        /// Type,根据type3 Prefab 下标来生成，甚至不需要在comp里加Index
        /// </summary>
        /// <param name="_type3"></param>
        public void GenerateTypeUI(PinchPartType3 _type3)
        {
            int _counter = sortCount;
            GenerateUIPre();
            
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(uiPrefabPaths[1])
                .Completed += (handle) =>
            {
                Transform _trans = handle.Result.transform;
                //查询对应目录下所有的Texture，加载
                 string pathFore = "OC/UI/PinchFace/Texture";
                 //string type3Path = PinchFaceManager.pinchFaceHelper.GetType3PrefabPath(PinchPartType2,_type3);
                 //加载对应的Type3 icon button
                 SelectedButton btnTemplate = _trans.GetComponentInChildren<SelectedButton>();
                 int prefabCount = Config.typesData[(int)_type3 - 1];
                 Debug.LogWarning($"{_type3.ToString()}:{prefabCount}");
                 for (int i = 0; i <prefabCount; i++)
                 {
                     var btn = GameObject.Instantiate(btnTemplate.gameObject, btnTemplate.transform.parent).GetComponent<SelectedButton>();
                     btn.name = $"TypeBtn{i}";
                     
                     string spriteName = $"{_type3.ToString()}_{i}"; 
                     btn.transform.Find("Image").GetComponent<Image>().sprite = SA_PinchPart.GetSprite(spriteName);
                     btn.onClick.AddListener(() =>
                     {
                        ModelPinch.ChangeType(PinchPartType2,i);
                     });
                 }
                 btnTemplate.gameObject.SetActive(false);
                 //ML.Engine.Manager.GameManager.DestroyObj();
                GenerateUICallBack(_trans,_counter);
            };
        }

        
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="boneWeightTypes"></param>
        /// 身体部分，可能会生成多条
        /// 初始加载的时候是模型原始缩放数值，第二次加载是更换后数值，应该需要加载数值
        public void GenerateBoneWeightUI(ChangeBoneWeightPinchSetting _boneWeightPinchSetting)
        {
            List<BoneWeightType> boneWeightTypes = new List<BoneWeightType>();
            int _counter = sortCount;
            GenerateUIPre();
            
            if (PinchPartType3 == PinchPartType3.B_Body)
            {
                boneWeightTypes.Add(BoneWeightType.Root);
                boneWeightTypes.Add(BoneWeightType.Head);
                // boneWeightTypes.Add(BoneWeightType.Chest);
                boneWeightTypes.Add(BoneWeightType.Waist);
                // boneWeightTypes.Add(BoneWeightType.Arm);
                boneWeightTypes.Add(BoneWeightType.Leg);
            }
            else
            {
                boneWeightTypes.Add(_boneWeightPinchSetting.boneWeightType);
            }
            GenerateBoneWeightUI(boneWeightTypes,_counter);
        }
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="boneWeightTypes"></param>
        /// 身体部分，可能会生成多条,_value为当前存档数据
        public void GenerateBoneWeightUI(List<BoneWeightType> boneWeightTypes,int _counter,List<int> _values = null,List<ChangeBoneWeightPinchSetting.BoneWeightChangeType> _ChangeTypes = null)
        {
            //没有定义骨骼Type，就生成 缩放型
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(uiPrefabPaths[2])
                .Completed += (handle) =>
            {
                //加入 生成的Slider
                Transform _trans = handle.Result.transform;
                GenerateUICallBack(_trans,_counter);
            };
        }

        
        
        
        /// <summary>
        /// 预定的ColorButton,可能再加一个控制哪个Mat的Color或者Color序号
        /// </summary>
        /// <param name="_type3"></param>
        /// <param name="_color"></param>
        public void GenerateColorUI1(PinchPartType3 _type3, Color _color)
        {
            int _counter = sortCount;
            GenerateUIPre();
            
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(uiPrefabPaths[3])
                .Completed += (handle) =>
            {
                //加入 type3的btn
                Transform _trans = handle.Result.transform;
                GenerateUICallBack(_trans,_counter);
            };
        }
        /// <summary>
        /// RGB/HSV
        /// </summary>
        /// <param name="_type3"></param>
        /// <param name="_color"></param>
        public void GenerateColorUI2(PinchPartType3 _type3, Color _color)
        {
            int _counter = 0;
            //to-do color2 的counter应该是继承删除的_counter
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(uiPrefabPaths[4])
                .Completed += (handle) =>
            {
                //加入 type3的btn
                Transform _trans = handle.Result.transform;
                GenerateUICallBack(_trans,_counter);
            };
        }

        /// <summary>
        /// 头发上色类型
        /// </summary>
        /// <param name="_type3"></param>
        /// <param name="_color"></param>
        public void GenerateColorTypeUI(ChangeColorPinchSetting _colorSetting)
        {
            int _counter = sortCount;
            GenerateUIPre();
            
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(uiPrefabPaths[5])
                .Completed += (handle) =>
            {
                //加入 type3的btn
                Transform _trans = handle.Result.transform;
                GenerateUICallBack(_trans,_counter);
            };
        }
        
        public void GenerateTextureUI(PinchPartType3 _type3)
        {
            int _counter = sortCount;
            GenerateUIPre();
            
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(uiPrefabPaths[6])
                .Completed += (handle) =>
            {
                //加入 type3的btn
                Transform _trans = handle.Result.transform;
                GenerateUICallBack(_trans,_counter);
            };
        }
        
        public void GenerateTransformTypeUI(ChangeTransformPinchSetting _transformSetting)
        {
            int _counter = sortCount;
            GenerateUIPre();
            
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(uiPrefabPaths[7])
                .Completed += (handle) =>
            {
                //加入 type3的btn
                Transform _trans = handle.Result.transform;
                GenerateUICallBack(_trans,_counter);
            };
        }

        private void GenerateUIPre()
        {
            sortCount++;
            isInit++;
        }
        private void GenerateUICallBack(Transform _uiTransf,int _counter)
        {
            _uiTransf.SetParent(containerTransf);
            _uiTransf.localScale = Vector3.one;
            _uiTransf.name = _counter.ToString();
            
            //排序
            isInit--;
            if (isInit == 0)
            {
                PinchFaceManager.pinchFaceHelper.SortUIAfterGenerate(_uiTransf,containerTransf,PinchFacePanel);
            }
            
        }
    }
}
