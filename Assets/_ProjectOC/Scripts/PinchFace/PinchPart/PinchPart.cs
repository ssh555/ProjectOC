using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityTransform;
using ML.Engine.SaveSystem;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using ProjectOC.PinchFace;
using ProjectOC.PinchFace.Config;
using ProjectOC.StoreNS.UI;
using Sirenix.OdinInspector;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using UnityEngine.UI;
using Object = System.Object;

namespace  ProjectOC.PinchFace
{
    [System.Serializable]
    public class PinchPart:ISaveData
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
        private PinchDataConfig Config => PinchFaceManager.Config;
        public PinchPartData pinchPartData;
        public PinchPartType2 PinchPartType2 => pinchPartData.PinchPartType2;
        public PinchPartType3 PinchPartType3 => pinchPartData.PinchPartType3;
        
        public List<IPinchSettingComp> pinchSettingComps => pinchPartData.pinchSettingComps;
        private ChangeColorPinchSetting  colorSettingComp = null;
        private ChangeTypePinchSetting typeSettingComp = null;
        private ChangeTexturePinchSetting textureSettingComp = null;
        private ChangeTransformPinchSetting transformSettingComp = null;
        private ChangeBoneWeightPinchSetting boneWeightSettingComp = null;
        
        
        private Transform containerTransf;
        private List<string> uiPrefabPaths = new List<string>();
        private int isInit = 0;

        Dictionary<BoneWeightType, string> boneWeightDic = new Dictionary<BoneWeightType, string>();

        private Transform colorTypeTransf;
        private int sortCount = 0;
        #endregion
        //外部引用

        #region Init
        public PinchPart(UIPinchFacePanel _PinchFacePanel,PinchPartType3 _type3,PinchPartType2 _type2,Transform _containerTransf)
        {
            //PinchType2 的通用Assets
            Init();
            if(pinchPartData == null)
                pinchPartData = new PinchPartData(_type2, _type3);
            containerTransf = _containerTransf;
            PinchFacePanel = _PinchFacePanel;
        }

        public PinchPart(UIPinchFacePanel _PinchFacePanel,PinchPartType3 _type3,PinchPartType2 _type2, IPinchSettingComp[] _settingComps,Transform _containerTransf)
            :this(_PinchFacePanel,_type3,_type2, _containerTransf)
        {
            pinchPartData.pinchSettingComps = _settingComps.ToList();
            foreach (var _pinchSettingComp in pinchPartData.pinchSettingComps)
            {
                switch (_pinchSettingComp)
                {
                    case ChangeColorPinchSetting color:
                        colorSettingComp = color;
                        break;
                    case ChangeTypePinchSetting type:
                        typeSettingComp = type;
                        break;
                    case ChangeTexturePinchSetting texture:
                        textureSettingComp = texture;
                        break;
                    case ChangeTransformPinchSetting transform:
                        transformSettingComp = transform;
                        break;
                    case ChangeBoneWeightPinchSetting boneWeight:
                        boneWeightSettingComp = boneWeight;
                        break;
                }
            }
        }

        public PinchPart(UIPinchFacePanel _PinchFacePanel,PinchPartType3 _type3,PinchPartType2 _type2, PinchPartData _pinchPartData,Transform _containerTransf)
            :this(_PinchFacePanel,_type3,_type2, _containerTransf)
        {
            pinchPartData = _pinchPartData;
        }

        private void Init()
        {
            uiPrefabPaths.Add("Prefabs_PinchPart/UIPanel/Setting/Prefab_Pinch_SettingHead.prefab");
            uiPrefabPaths.Add("Prefabs_PinchPart/UIPanel/Setting/Prefab_Pinch_TypeSettingPanel.prefab");
            uiPrefabPaths.Add("Prefabs_PinchPart/UIPanel/Setting/Prefab_Pinch_BoneWeightSettingPanel.prefab");
            uiPrefabPaths.Add("Prefabs_PinchPart/UIPanel/Setting/Prefab_Pinch_ColorSettingPanel1.prefab");
            uiPrefabPaths.Add("Prefabs_PinchPart/UIPanel/Setting/Prefab_Pinch_ColorSettingPanel2.prefab");
            uiPrefabPaths.Add("Prefabs_PinchPart/UIPanel/Setting/Prefab_Pinch_ColorTypeSetting.prefab");
            uiPrefabPaths.Add("Prefabs_PinchPart/UIPanel/Setting/Prefab_Pinch_TextureSettingPanel.prefab");
            uiPrefabPaths.Add("Prefabs_PinchPart/UIPanel/Setting/Prefab_Pinch_TransformSettingPanel.prefab");
            
            boneWeightDic.Add(BoneWeightType.Head,"头部");
            boneWeightDic.Add(BoneWeightType.Chest,"胸部");
            boneWeightDic.Add(BoneWeightType.Waist,"腰部");
            boneWeightDic.Add(BoneWeightType.Arm,"上肢");
            boneWeightDic.Add(BoneWeightType.Leg,"下肢");
            boneWeightDic.Add(BoneWeightType.HeadTop,"头顶");
            boneWeightDic.Add(BoneWeightType.Tail,"尾巴");
            boneWeightDic.Add(BoneWeightType.Root,"整体");
            
            PinchFaceManager = LocalGameManager.Instance.PinchFaceManager;
            
        }
        
        //控制_pinchSettingComps Buttons的生成

        
        // public void PinchFaceCallBack()
        // {
        //     foreach (var _pinchSettingComp in pinchSettingComps)
        //     {
        //         if (_pinchSettingComp.GetType() == typeof(ChangeBoneWeightPinchSetting))
        //         {
        //             ChangeBoneWeightPinchSetting _bonePinchSetting = _pinchSettingComp as ChangeBoneWeightPinchSetting;
        //             foreach (var _boneWeightData in _bonePinchSetting.BoneWeightDatas)
        //             {
        //                 //todo 上臂的骨骼资源还没有处理，导入正式的人物骨骼后删除
        //                 if (ModelPinch.boneWeightDictionary.ContainsKey(_boneWeightData.boneWeightType))
        //                 {
        //                     _boneWeightData.currentScaleValue = ModelPinch.boneWeightDictionary[_boneWeightData.boneWeightType].defaultScale;
        //                     _boneWeightData.currentOffsetValue = ModelPinch.boneWeightDictionary[_boneWeightData.boneWeightType].defaultPos;  
        //                 }
        //             }
        //         }
        //     } 
        // }
        
        

        
        #endregion
        #region PinchPart&&UI Process

        public void GeneratePinchPartSetting()
        {
            sortCount = 0;
            foreach (var _comp in pinchSettingComps)
            {
                ProcessSetttingComp(_comp);
            }
        }

        /// <summary>
        /// 负责根据SettingComp生成对应UI
        /// </summary>
        /// <param name="SettingComp"></param>
        private void ProcessSetttingComp(IPinchSettingComp _comp)
        {
            if(_comp.GetType()== typeof(ChangeTypePinchSetting))
            {
                GenerateHeadUI("样式");
                GenerateTypeUI(_comp as ChangeTypePinchSetting);
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
                //如果是头发，不止有纯色
                if (PinchPartType3 == PinchPartType3.HF_HairFront)
                {
                    GenerateHeadUI("分色样式");
                    GenerateColorTypeUI(_colorComp);
                    GenerateHeadUI("颜色1");
                    GenerateColorUI1(_comp as ChangeColorPinchSetting,0);
                    GenerateHeadUI("颜色2");
                    GenerateColorUI1(_comp as ChangeColorPinchSetting,1);
                    GenerateColorGradualSlider(_comp as ChangeColorPinchSetting);
                }
                else
                {
                    GenerateHeadUI("颜色");
                    GenerateColorUI1(_comp as ChangeColorPinchSetting);
                }
            }
            else if (_comp.GetType()== typeof(ChangeTexturePinchSetting))
            {
                GenerateHeadUI("纹理");
                GenerateTextureUI(_comp as ChangeTexturePinchSetting);
            }
            else if (_comp.GetType()== typeof(ChangeTransformPinchSetting))
            {
                ChangeTransformPinchSetting transComp = _comp as ChangeTransformPinchSetting;
                GenerateHeadUI("位置变换");
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
        public void GenerateTypeUI(ChangeTypePinchSetting _typeSetting)
        {
            int _counter = sortCount;
            GenerateUIPre();
            
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(uiPrefabPaths[1])
                .Completed += (handle) =>
            {
                Transform _trans = handle.Result.transform;
                //查询对应目录下所有的Texture，加载
                 SelectedButton btnTemplate = _trans.GetComponentInChildren<SelectedButton>();
                 int prefabCount = Config.typesDatas[(int)PinchPartType3 - 1].typeCount;
                 for (int i = 0; i <prefabCount; i++)
                 {
                     int _index = i;
                     var btn = GameObject.Instantiate(btnTemplate.gameObject, btnTemplate.transform.parent).GetComponent<SelectedButton>();
                     btn.name = $"TypeBtn{_index}";
                     
                     string spriteName = $"{PinchPartType3.ToString()}_{_index}"; 
                     btn.transform.Find("Image").GetComponent<Image>().sprite = SA_PinchPart.GetSprite(spriteName);
                     btn.onClick.AddListener(() =>
                     { 
                        Action_TypeBtn(_typeSetting, _index);
                     });
                 }
                 btnTemplate.gameObject.SetActive(false);
                GenerateUICallBack(_trans,_counter);
            };
        }
        /// <summary>
        /// /// 所有按钮、滑动响应分离出来，初始化的时候要随机一个部件,以及做随机用
        /// </summary>
        /// <param name="_typeSetting"></param>
        /// <param name="_index"></param>
        public void Action_TypeBtn(ChangeTypePinchSetting _typeSetting,int _index,bool inCamera = true)
        {
            int _isInit = 0;
            _typeSetting.typeIndex = _index;
            if (PinchPartType3 == PinchPartType3.HF_HairFront)
            {
                _isInit = 2;
                curColorChangeType = ChangeColorPinchSetting.ColorChangeType.None;
                ProcessChangeTypeHandle(ModelPinch.ChangeType(PinchPartType3.HF_HairFront,_index,inCamera));    
                //ProcessChangeTypeHandle(ModelPinch.ChangeType(PinchPartType3.HD_Dai,_index,inCamera));    
                ProcessChangeTypeHandle(ModelPinch.ChangeType(PinchPartType3.HB_HairBack,_index,inCamera));
                ProcessChangeTypeHandle(ModelPinch.ChangeType(PinchPartType3.HB_HairBraid,0,inCamera));
                //根据头发的组件增加颜色类型和UI
            }
            else
            {
                ProcessChangeTypeHandle(ModelPinch.ChangeType(PinchPartType3,_index,inCamera));    
            }

            void ProcessChangeTypeHandle(AsyncOperationHandle<GameObject> _handle)
            {
                if (PinchPartType3 == PinchPartType3.HF_HairFront)
                {
                    _handle.Completed += (handle) =>
                    {
                        //查找分色方式

                        ChangeColorPinchSetting _colorPinchSetting =
                            handle.Result.GetComponent<ChangeColorPinchSetting>();
                        //Braid 暂不需要
                        if (_colorPinchSetting != null)
                            curColorChangeType |= _colorPinchSetting.colorChangeType;
                        _isInit--;
                        if (_isInit == 0)
                        {
                            RefreshColorType();
                            _typeSetting.ApplyOtherComp(ModelPinch, pinchPartData);
                        }
                    };

                }
                else
                {
                    _typeSetting.ApplyOtherComp(ModelPinch,pinchPartData);
                }
            }
        }
        
        
        
        /// <summary>
        /// 生成骨骼调整UI
        /// 身体部分，可能会生成多条
        /// 初始加载的时候是模型原始缩放数值，第二次加载是更换后数值，应该需要加载数值
        /// </summary>
        /// <param name="boneWeightTypes"></param>
        
        public void GenerateBoneWeightUI(ChangeBoneWeightPinchSetting _boneWeightPinchSetting)
        {
            int _counter = sortCount;
            GenerateUIPre();
            
            //没有定义骨骼Type，就生成 缩放型
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(uiPrefabPaths[2])
                .Completed += (handle) =>
            {
                //加入 生成的Slider
                Transform _trans = handle.Result.transform;
                GameObject sliderTemplate = _trans.Find("Container/Pinch_SliderTemplate").gameObject;
                for (int i = 0; i < _boneWeightPinchSetting.BoneWeightDatas.Count; i++)
                {
                    int _index = i;
                    ChangeBoneWeightPinchSetting.BoneWeightData _boneWeightData = _boneWeightPinchSetting.BoneWeightDatas[i];
                    BoneWeightType _boneWeightType = _boneWeightData.boneWeightType;
                    //缩放
                    GenerateSliderAndSetting(boneWeightDic[_boneWeightType], Action_ScaleSlider,
                        PinchFaceManager.pinchFaceHelper.RemapValue(_boneWeightPinchSetting.BoneWeightDatas[i].currentScaleValue.x, 
                        _boneWeightData.scaleValueRange,new Vector2(1, 100)));
                    //偏移
                    if ((_boneWeightData.boneWeightChangeType &
                         ChangeBoneWeightPinchSetting.BoneWeightChangeType.Offset) != 0)
                    {
                        GenerateSliderAndSetting(boneWeightDic[_boneWeightType]+"偏移", Action_OffsetSilder
                            ,PinchFaceManager.pinchFaceHelper.RemapValue(_boneWeightPinchSetting.BoneWeightDatas[i].currentOffsetValue.y, 
                                _boneWeightData.offsetValueRange,new Vector2(1, 100)));
                    }
                    
                    
                    void GenerateSliderAndSetting(string _text,UnityAction<float> _action,float _curValue)
                    {
                        var _sliderGo = GameObject.Instantiate(sliderTemplate, sliderTemplate.transform.parent);
                        CustomSelectedSlider _slider = _sliderGo.GetComponentInChildren<CustomSelectedSlider>();
                        _sliderGo.transform.Find("Button").name = _text;
                        
                        _slider.SetSliderConfig(_text,_action,_curValue);
                    }
                    
                    //_value 1~100 ->
                    void Action_ScaleSlider(float _value)
                    {
                        //_value Remap
                        float _realValue = PinchFaceManager.pinchFaceHelper.RemapValue(_value, new Vector2(1, 100),
                            _boneWeightData.scaleValueRange);
                        Vector3 _boneWeight = _realValue*Vector3.one;

                        _boneWeightPinchSetting.BoneWeightDatas[_index].currentScaleValue = _boneWeight;
                        ModelPinch.ChangeBoneScale(_boneWeightData.boneWeightType,_boneWeight);
                        
                    }
                    void Action_OffsetSilder(float _value)
                    {
                        //_value Remap
                        float _realValue = PinchFaceManager.pinchFaceHelper.RemapValue(_value, new Vector2(1, 100),
                            _boneWeightData.offsetValueRange);
                        Vector3 _boneWeight = _realValue*Vector3.up;
                        _boneWeightPinchSetting.BoneWeightDatas[_index].currentOffsetValue = _boneWeight;
                        ModelPinch.ChangeBoneScale(_boneWeightData.boneWeightType,_boneWeight,false);
                    }
                    
                    
                    //设置初始值
                    //尾巴的骨骼在新增骨骼上，恢复默认值
                    if (_boneWeightType == BoneWeightType.Tail)
                    {
                        //
                    }
                    else
                    {
                        
                    }
                    
                    //处理移动型
                }
                
                sliderTemplate.gameObject.SetActive(false);
                GenerateUICallBack(_trans,_counter);
            };
        }
        
        
        
        /// <summary>
        /// 预定的ColorButton,可能再加一个控制哪个Mat的Color或者Color序号
        /// </summary>
        /// <param name="_type3"></param>
        /// <param name="_color"></param>
        /// <param name="count">如果是-1,说明是从一种替换成另一种，不是新增</param>
        public void GenerateColorUI1(ChangeColorPinchSetting _colorSetting,int colorPanelIndex = 0,int count = -1)
        {
            if (count != -1)
            {
                sortCount = count - 1;   
            }
            int _counter = sortCount;
            GenerateUIPre();
            
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(uiPrefabPaths[3])
                .Completed += (handle) =>
            {
                //设置Btn
                Transform _trans = handle.Result.transform;
                Transform _container1 = _trans.Find("Container1");
                Transform _container2 = _trans.Find("Container2");
                SelectedButton[] _buttons = _container1.GetComponentsInChildren<SelectedButton>();
                foreach (var _colorGrid in _buttons)
                {
                    SelectedButton _curBtn = _colorGrid;
                    Color _gridColor = _curBtn.transform.GetComponent<Image>().color;
                    _colorGrid.onClick.AddListener(() =>
                    {
                        _colorSetting.colors[colorPanelIndex] = _gridColor;
                        ModelPinch.ChangeColor(PinchPartType2,_gridColor,colorPanelIndex);
                        _container2.Find("ColorView").GetComponent<Image>().color = _gridColor;
                    });
                }
                
                //切换为第二种编辑模式
                _container2.GetComponentInChildren<SelectedButton>().onClick.AddListener(() =>
                {
                    GenerateColorUI2(_colorSetting,colorPanelIndex,_counter);
                    //删除并解绑这个
                    GameObject.Destroy(_trans.gameObject);
                });
                if (count != -1)
                {
                    //select btn返回某一个
                    //int return count
                }
                
                GenerateUICallBack(_trans,_counter);
                _container2.Find("ColorView").GetComponent<Image>().color = ModelPinch.GetType2Color(PinchPartType2,colorPanelIndex);


            };
        }
        /// <summary>
        /// RGB/HSV
        /// </summary>
        /// <param name="_type3"></param>
        /// <param name="_color"></param>
        public void GenerateColorUI2(ChangeColorPinchSetting _colorSetting,int colorPanelIndex = 0,int _counter = -1)
        {
            //to-do color2 的counter应该是继承删除的_counter
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(uiPrefabPaths[4])
                .Completed += (handle) =>
            {
                //加入 type3的btn
                Transform _trans = handle.Result.transform;
                Transform _container2 = _trans.Find("Container2");
                Image _colorView = _container2.Find("ColorView").GetComponent<Image>();
                //设置Slider Action
                HSVColorSettingController _hsvColorSetting = _trans.Find("Container1").GetComponentInChildren<HSVColorSettingController>();
                Color _curColor = ModelPinch.GetType2Color(PinchPartType2);
                _hsvColorSetting.SetColorValue(_curColor,true);
                _hsvColorSetting.SetColorValue(_curColor,false);
                _hsvColorSetting.ColorChangeAction +=((_color)=>
                {
                    _colorSetting.colors[colorPanelIndex] = _color;
                    ModelPinch.ChangeColor(PinchPartType2,_color,colorPanelIndex);
                    _colorView.color = _color;
                });
                
                //设置确定btn Action
                _colorView.color = _curColor;
                _container2.GetComponentInChildren<SelectedButton>().onClick.AddListener(() =>
                {
                    //切换为第二种编辑模式
                    GenerateColorUI1(_colorSetting,colorPanelIndex,_counter);
                    //删除并解绑这个
                    GameObject.Destroy(_trans.gameObject);
                });
                //int return count
                GenerateUICallBack(_trans,_counter);
            };
        }

        private int curColorType = 0;
        private int CurColorType
        {
            set
            {
                if (value != curColorType)
                {
                    curColorType = value;
                }
            }
            get
            {
                return curColorType;
            }
        }

        private float gradualParam1 , gradualParam2;
        private ChangeColorPinchSetting.ColorChangeType curColorChangeType = ChangeColorPinchSetting.ColorChangeType.None;
        
        
        string[] colorTypeString =
        {
            "纯色",
            "分色1",
            "分色2",
            "渐变1",
            "渐变2"
        }; 
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
                colorTypeTransf = handle.Result.transform;
                GenerateUICallBack(colorTypeTransf,_counter);
            };
        }
        /// <summary>
        /// 每次切换发型的时候，切换成当前发型的分色模式  删除原来的Btn
        /// true,第一次进入 不需要重新生成BtnListContainer；false 后续因为切换发型重新生成分色方式BtnListContainer
        /// </summary>
        private void RefreshColorType()
        {
            ChangeColorPinchSetting _colorSetting =
                pinchSettingComps.Find(_comp => _comp.GetType() == typeof(ChangeColorPinchSetting)) as
                    ChangeColorPinchSetting;
            //初始生成，还没有生成右侧Btn
            if (colorTypeTransf == null)
            {
                return;
            }
            
            SelectedButton btnTemplate = colorTypeTransf.Find("Container/TemplateBtn").GetComponent<SelectedButton>();
            //删除
            foreach (Transform _btn in btnTemplate.transform.parent)
            {
                if (_btn != btnTemplate.transform)
                {
                    _btn.gameObject.SetActive(false);
                    _btn.GetComponent<SelectedButton>().Interactable = false;
                    _btn.name = "todelete";
                    GameObject.Destroy(_btn.gameObject);
                }
            }
            GenerateColorTypeButtons(_colorSetting);
            
            
            //前发、后发、呆毛、辫子的ColorSetting
            
            void GenerateColorTypeButtons(ChangeColorPinchSetting _colorSetting)
            {
                
                int inheritIndex = 0;
                
                for (int i = 0; i < 5; i++)
                {
                    bool couldInherit = false;
                    int _index = i;
                    if (((int)curColorChangeType & (1<< _index)) != 0)
                    {
                        if (_index == CurColorType)
                            couldInherit = true;
                        
                        GameObject _btn = GameObject.Instantiate(btnTemplate.gameObject,btnTemplate.transform.parent);
                        _btn.name = $"ColorType_{_index}";
                        _btn.GetComponentInChildren<TextMeshProUGUI>().text = colorTypeString[_index];
                        _btn.SetActive(true);
                        _btn.GetComponentInChildren<SelectedButton>().onClick.AddListener(() =>
                        {
                            CurColorType = _index;
                            _colorSetting.CurColorChangeType = CurColorType;
                            ModelPinch.ChangeColorType(PinchPartType2,CurColorType,gradualParam1,gradualParam2);
                            Color _selectColor = new Color(0.3931559f, 0.8773585f, 0.5431628f);
                            foreach (Transform _otherBtn in btnTemplate.transform.parent)
                            {
                                _otherBtn.GetComponent<Image>().color = Color.gray;
                            }
                            _btn.GetComponent<Image>().color =_selectColor;
                            //设置ColorSetting的启用,01Type/ 23ColorType/ 45Color1/ 67Color2/ 8ColorSlider
                            //ColorType生成完成的时候下面的Color2和Slider还没生成
                            containerTransf.GetChild(6).gameObject.SetActive(true);
                            containerTransf.GetChild(7).gameObject.SetActive(true);
                            containerTransf.GetChild(8).gameObject.SetActive(true);
                            if (CurColorType == 0)
                            {
                                containerTransf.GetChild(6).gameObject.SetActive(false);
                                containerTransf.GetChild(7).gameObject.SetActive(false);
                                containerTransf.GetChild(8).gameObject.SetActive(false);
                            }
                            else if (CurColorType == 1 || CurColorType == 2)
                            {
                                containerTransf.GetChild(8).gameObject.SetActive(false);
                            }
                        });
                    }
                    //继承,
                    if (couldInherit)
                        inheritIndex = CurColorType;
                }
                btnTemplate.transform.parent.Find($"ColorType_{inheritIndex}").GetComponent<SelectedButton>().onClick.Invoke();
                //重新生成BtnList,初始的时候没有Container
                // if (PinchFacePanel.UIBtnListContainer.UIBtnLists.Count > 4)
                // {
                //     Debug.Log("Regenerate btn 5");
                //     PinchFacePanel.UIBtnListContainer.UIBtnLists[5].InitBtnInfo();
                // }
                //todo 应该是重新生成BtnList，而不是重新生成BtnList Container
                
                //UIBtnListInitor[] btnLists = PinchFacePanel.transform.GetComponentsInChildren<UIBtnListInitor>(); 
                PinchFacePanel.ReGenerateBtnListContainer(PinchFacePanel.rightBtnLists);
                PinchFaceManager.pinchFaceHelper.RefreshPanelLayout(PinchFacePanel.transform);
                PinchFacePanel.ReturnBtnList(4);
            }
        }
        
        //暂时只有头发部分需要
        public void GenerateColorGradualSlider(ChangeColorPinchSetting _colorSetting)
        {
            int _counter = sortCount;
            GenerateUIPre();
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(uiPrefabPaths[2])
                .Completed += (handle) =>
            {
                //加入 生成的Slider
                string shaderParam1 = "_smoothStepThreshold",shaderParam2 = "_smoothStrength";
                Transform _trans = handle.Result.transform;
                GameObject sliderTemplate = _trans.Find("Container/Pinch_SliderTemplate").gameObject;
                GameObject _slider2 = GameObject.Instantiate(sliderTemplate, sliderTemplate.transform.parent);
                

                float startValue1 = ModelPinch.GetMaterial(PinchPartType2).GetFloat(shaderParam1) * 100f;
                float startValue2 = ModelPinch.GetMaterial(PinchPartType2).GetFloat(shaderParam2) * 100f;
                SettingSlider(sliderTemplate, "渐变位置",Action_ScaleSlider1,startValue1);
                SettingSlider(_slider2, "渐变程度",Action_ScaleSlider2,startValue2);
                
                
                //_value 1~100 ->
                void Action_ScaleSlider1(float _value)
                {
                    //_value Remap
                    float _realValue = PinchFaceManager.pinchFaceHelper.RemapValue(_value, new Vector2(1, 100),
                        Vector2.up*0.8f);
                    gradualParam1 = _realValue;
                    _colorSetting.smoothStepThreshold = gradualParam1;
                    ModelPinch.ChangeColorType(PinchPartType2,CurColorType,gradualParam1,gradualParam2);
                }
                void Action_ScaleSlider2(float _value)
                {
                    //_value Remap
                    float _realValue = PinchFaceManager.pinchFaceHelper.RemapValue(_value, new Vector2(1, 100),
                        Vector2.up*0.5f);
                    gradualParam2 = _realValue;
                    _colorSetting.smoothStrength = gradualParam2;
                    ModelPinch.ChangeColorType(PinchPartType2,CurColorType,gradualParam1,gradualParam2);
                }
                
                
                GenerateUICallBack(_trans,_counter);
            };
        }
        
        
        public void GenerateTextureUI(ChangeTexturePinchSetting _texSetting)
        {
            int _counter = sortCount;
            GenerateUIPre();
            
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(uiPrefabPaths[6])
                .Completed += (handle) =>
            {
                Transform _trans = handle.Result.transform;
                //查询对应目录下所有的Texture，加载
                SelectedButton btnTemplate = _trans.GetComponentInChildren<SelectedButton>();
                int prefabCount = Config.typesDatas[(int)PinchPartType3 - 1].typeCount;
                for (int i = 0; i <prefabCount; i++)
                {
                    int _index = i;
                    var btn = GameObject.Instantiate(btnTemplate.gameObject, btnTemplate.transform.parent).GetComponent<SelectedButton>();
                    btn.name = $"TexBtn{_index}";
                 
                    string spriteName = $"{PinchPartType3.ToString()}_{_index}"; 
                    btn.transform.Find("Image").GetComponent<Image>().sprite = SA_PinchPart.GetSprite(spriteName);
                    btn.onClick.AddListener(()=>
                    {
                        Action_TexBtn(_texSetting, _index);
                    });
                }
                btnTemplate.gameObject.SetActive(false);
                GenerateUICallBack(_trans,_counter);

            };
        }
        
        /// <summary>
        /// 所有按钮、滑动响应分离出来，初始化的时候要触发
        /// </summary>
        /// <param name="_texSetting"></param>
        /// <param name="_index"></param>
        private void Action_TexBtn(ChangeTexturePinchSetting _texSetting,int _index)
        {
            _texSetting.textureIndex = _index; 
            ModelPinch.ChangeTexture(PinchPartType3,_index);
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
                Transform _slider1 = _trans.Find("Container/Pinch_Slider1");
                Transform _slider2 = _trans.Find("Container/Pinch_Slider2");
                float _value1 = _transformSetting.curValue.x,
                    _value2 = _transformSetting.curValue.y;
                
                SettingSlider(_slider1.gameObject, "位置调整X",Action_ScaleSlider1
                    ,PinchFaceManager.pinchFaceHelper.RemapValue(_transformSetting.curValue.x,_transformSetting.pinchRangeX, new Vector2(1, 100)));
                if (PinchPartType2 == PinchPartType2.EarTop)
                {
                    _slider2.gameObject.SetActive(false);
                }
                else
                {
                    SettingSlider(_slider2.gameObject, "位置调整Y",Action_ScaleSlider2
                        ,PinchFaceManager.pinchFaceHelper.RemapValue(_transformSetting.curValue.y,_transformSetting.pinchBraidRangeY, new Vector2(1, 100)));
                }
                
                //_value 1~100 ->
                void Action_ScaleSlider1(float _value)
                {
                    //_value Remap
                    _value1 = PinchFaceManager.pinchFaceHelper.RemapValue(_value, new Vector2(1, 100),_transformSetting.pinchRangeX);
                    Vector2 _resValue = new Vector2(_value1,_value2);
                    _transformSetting.curValue = _resValue;
                    ModelPinch.ChangeTransform(PinchPartType2,_resValue);
                }
                void Action_ScaleSlider2(float _value)
                {
                    //_value Remap
                    _value2 = PinchFaceManager.pinchFaceHelper.RemapValue(_value, new Vector2(1, 100),_transformSetting.pinchBraidRangeY);
                    Vector2 _resValue = new Vector2(_value1,_value2);
                    _transformSetting.curValue = _resValue;
                    ModelPinch.ChangeTransform(PinchPartType2,_resValue);
                }
                
                GenerateUICallBack(_trans,_counter);
            };
        }
        void SettingSlider(GameObject _sliderGo,string _text,UnityAction<float> _action,float curValue)
        {
            CustomSelectedSlider _slider = _sliderGo.GetComponentInChildren<CustomSelectedSlider>();
            _sliderGo.transform.Find("Button").name = _text;
            _slider.SetSliderConfig(_text,_action,curValue);
        }
        private void GenerateUIPre()
        {
            sortCount++;
            isInit++;
        }
        private void GenerateUICallBack(Transform _uiTransf,int _counter,int returnBtnListCount = 4)
        {
            _uiTransf.SetParent(containerTransf);
            _uiTransf.localScale = Vector3.one;
            _uiTransf.name = _counter.ToString();
            //排序
            isInit--;
            if (isInit <= 0)
            {
                PinchFaceManager.pinchFaceHelper.SortUIAfterGenerate(_uiTransf,containerTransf,PinchFacePanel,returnBtnListCount);
                if (PinchPartType3 == PinchPartType3.HF_HairFront)
                {
                    RefreshColorType();
                }
            }

            
        }
        #endregion
        
        #region 数据存储相关
        
        public class PinchPartData
        {
            public PinchPartType2 PinchPartType2 { get; private set; }
            public PinchPartType3 PinchPartType3 { get; private set; }
            public List<IPinchSettingComp> pinchSettingComps = new List<IPinchSettingComp>();
            
            public PinchPartData(PinchPartType2 _type2, PinchPartType3 _type3)
            {
                PinchPartType2 = _type2;
                PinchPartType3 = _type3;
            }
            
            public void ApplyPinchSetting(CharacterModelPinch _modelPinch)
            {
                foreach (var _comp in pinchSettingComps)
                {
                    //每个PinchType
                    if (_comp.GetType() == typeof(ChangeTypePinchSetting))
                    {
                        (_comp as ChangeTypePinchSetting).ApplyType(_modelPinch,this);
                        return;
                    }
                }
                //没有Type的情况
                foreach (var _comp in pinchSettingComps)
                {
                    _comp.Apply(PinchPartType2,PinchPartType3,_modelPinch);
                }
            }

            
            
        }
        #endregion
    }
}
