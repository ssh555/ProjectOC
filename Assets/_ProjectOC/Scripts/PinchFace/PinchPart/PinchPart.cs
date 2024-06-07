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

     


        #region ���úͱ���
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
        //�ⲿ����

        #region Init
        public PinchPart(UIPinchFacePanel _PinchFacePanel,PinchPartType3 _type3,PinchPartType2 _type2,Transform _containerTransf)
        {
            //PinchType2 ��ͨ��Assets
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
            
            boneWeightDic.Add(BoneWeightType.Head,"ͷ��");
            boneWeightDic.Add(BoneWeightType.Chest,"�ز�");
            boneWeightDic.Add(BoneWeightType.Waist,"����");
            boneWeightDic.Add(BoneWeightType.Arm,"��֫");
            boneWeightDic.Add(BoneWeightType.Leg,"��֫");
            boneWeightDic.Add(BoneWeightType.HeadTop,"ͷ��");
            boneWeightDic.Add(BoneWeightType.Tail,"β��");
            boneWeightDic.Add(BoneWeightType.Root,"����");
            
            PinchFaceManager = LocalGameManager.Instance.PinchFaceManager;
            
        }
        
        //����_pinchSettingComps Buttons������

        
        // public void PinchFaceCallBack()
        // {
        //     foreach (var _pinchSettingComp in pinchSettingComps)
        //     {
        //         if (_pinchSettingComp.GetType() == typeof(ChangeBoneWeightPinchSetting))
        //         {
        //             ChangeBoneWeightPinchSetting _bonePinchSetting = _pinchSettingComp as ChangeBoneWeightPinchSetting;
        //             foreach (var _boneWeightData in _bonePinchSetting.BoneWeightDatas)
        //             {
        //                 //todo �ϱ۵Ĺ�����Դ��û�д���������ʽ�����������ɾ��
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
        /// �������SettingComp���ɶ�ӦUI
        /// </summary>
        /// <param name="SettingComp"></param>
        private void ProcessSetttingComp(IPinchSettingComp _comp)
        {
            if(_comp.GetType()== typeof(ChangeTypePinchSetting))
            {
                GenerateHeadUI("��ʽ");
                GenerateTypeUI(_comp as ChangeTypePinchSetting);
            }
            else if (_comp.GetType()== typeof(ChangeBoneWeightPinchSetting))
            {
                ChangeBoneWeightPinchSetting _weightComp = _comp as ChangeBoneWeightPinchSetting;
                GenerateHeadUI("�ߴ�");
                GenerateBoneWeightUI(_weightComp);
            }
            else if (_comp.GetType()== typeof(ChangeColorPinchSetting))
            {
                ChangeColorPinchSetting _colorComp = _comp as ChangeColorPinchSetting;
                //�����ͷ������ֹ�д�ɫ
                if (PinchPartType3 == PinchPartType3.HF_HairFront)
                {
                    GenerateHeadUI("��ɫ��ʽ");
                    GenerateColorTypeUI(_colorComp);
                    GenerateHeadUI("��ɫ1");
                    GenerateColorUI1(_comp as ChangeColorPinchSetting,0);
                    GenerateHeadUI("��ɫ2");
                    GenerateColorUI1(_comp as ChangeColorPinchSetting,1);
                    GenerateColorGradualSlider(_comp as ChangeColorPinchSetting);
                }
                else
                {
                    GenerateHeadUI("��ɫ");
                    GenerateColorUI1(_comp as ChangeColorPinchSetting);
                }
            }
            else if (_comp.GetType()== typeof(ChangeTexturePinchSetting))
            {
                GenerateHeadUI("����");
                GenerateTextureUI(_comp as ChangeTexturePinchSetting);
            }
            else if (_comp.GetType()== typeof(ChangeTransformPinchSetting))
            {
                ChangeTransformPinchSetting transComp = _comp as ChangeTransformPinchSetting;
                GenerateHeadUI("λ�ñ任");
                GenerateTransformTypeUI(transComp);
            }
        }
        
        
        
        /// <summary>
        /// ������λ����
        /// </summary>
        /// <param name="������"></param>
        
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
        /// Type,����type3 Prefab �±������ɣ���������Ҫ��comp���Index
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
                //��ѯ��ӦĿ¼�����е�Texture������
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
        /// /// ���а�ť��������Ӧ�����������ʼ����ʱ��Ҫ���һ������,�Լ��������
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
                //����ͷ�������������ɫ���ͺ�UI
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
                        //���ҷ�ɫ��ʽ

                        ChangeColorPinchSetting _colorPinchSetting =
                            handle.Result.GetComponent<ChangeColorPinchSetting>();
                        //Braid �ݲ���Ҫ
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
        /// ���ɹ�������UI
        /// ���岿�֣����ܻ����ɶ���
        /// ��ʼ���ص�ʱ����ģ��ԭʼ������ֵ���ڶ��μ����Ǹ�������ֵ��Ӧ����Ҫ������ֵ
        /// </summary>
        /// <param name="boneWeightTypes"></param>
        
        public void GenerateBoneWeightUI(ChangeBoneWeightPinchSetting _boneWeightPinchSetting)
        {
            int _counter = sortCount;
            GenerateUIPre();
            
            //û�ж������Type�������� ������
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(uiPrefabPaths[2])
                .Completed += (handle) =>
            {
                //���� ���ɵ�Slider
                Transform _trans = handle.Result.transform;
                GameObject sliderTemplate = _trans.Find("Container/Pinch_SliderTemplate").gameObject;
                for (int i = 0; i < _boneWeightPinchSetting.BoneWeightDatas.Count; i++)
                {
                    int _index = i;
                    ChangeBoneWeightPinchSetting.BoneWeightData _boneWeightData = _boneWeightPinchSetting.BoneWeightDatas[i];
                    BoneWeightType _boneWeightType = _boneWeightData.boneWeightType;
                    //����
                    GenerateSliderAndSetting(boneWeightDic[_boneWeightType], Action_ScaleSlider,
                        PinchFaceManager.pinchFaceHelper.RemapValue(_boneWeightPinchSetting.BoneWeightDatas[i].currentScaleValue.x, 
                        _boneWeightData.scaleValueRange,new Vector2(1, 100)));
                    //ƫ��
                    if ((_boneWeightData.boneWeightChangeType &
                         ChangeBoneWeightPinchSetting.BoneWeightChangeType.Offset) != 0)
                    {
                        GenerateSliderAndSetting(boneWeightDic[_boneWeightType]+"ƫ��", Action_OffsetSilder
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
                    
                    
                    //���ó�ʼֵ
                    //β�͵Ĺ��������������ϣ��ָ�Ĭ��ֵ
                    if (_boneWeightType == BoneWeightType.Tail)
                    {
                        //
                    }
                    else
                    {
                        
                    }
                    
                    //�����ƶ���
                }
                
                sliderTemplate.gameObject.SetActive(false);
                GenerateUICallBack(_trans,_counter);
            };
        }
        
        
        
        /// <summary>
        /// Ԥ����ColorButton,�����ټ�һ�������ĸ�Mat��Color����Color���
        /// </summary>
        /// <param name="_type3"></param>
        /// <param name="_color"></param>
        /// <param name="count">�����-1,˵���Ǵ�һ���滻����һ�֣���������</param>
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
                //����Btn
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
                
                //�л�Ϊ�ڶ��ֱ༭ģʽ
                _container2.GetComponentInChildren<SelectedButton>().onClick.AddListener(() =>
                {
                    GenerateColorUI2(_colorSetting,colorPanelIndex,_counter);
                    //ɾ����������
                    GameObject.Destroy(_trans.gameObject);
                });
                if (count != -1)
                {
                    //select btn����ĳһ��
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
            //to-do color2 ��counterӦ���Ǽ̳�ɾ����_counter
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(uiPrefabPaths[4])
                .Completed += (handle) =>
            {
                //���� type3��btn
                Transform _trans = handle.Result.transform;
                Transform _container2 = _trans.Find("Container2");
                Image _colorView = _container2.Find("ColorView").GetComponent<Image>();
                //����Slider Action
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
                
                //����ȷ��btn Action
                _colorView.color = _curColor;
                _container2.GetComponentInChildren<SelectedButton>().onClick.AddListener(() =>
                {
                    //�л�Ϊ�ڶ��ֱ༭ģʽ
                    GenerateColorUI1(_colorSetting,colorPanelIndex,_counter);
                    //ɾ����������
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
            "��ɫ",
            "��ɫ1",
            "��ɫ2",
            "����1",
            "����2"
        }; 
        /// <summary>
        /// ͷ����ɫ����
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
        /// ÿ���л����͵�ʱ���л��ɵ�ǰ���͵ķ�ɫģʽ  ɾ��ԭ����Btn
        /// true,��һ�ν��� ����Ҫ��������BtnListContainer��false ������Ϊ�л������������ɷ�ɫ��ʽBtnListContainer
        /// </summary>
        private void RefreshColorType()
        {
            ChangeColorPinchSetting _colorSetting =
                pinchSettingComps.Find(_comp => _comp.GetType() == typeof(ChangeColorPinchSetting)) as
                    ChangeColorPinchSetting;
            //��ʼ���ɣ���û�������Ҳ�Btn
            if (colorTypeTransf == null)
            {
                return;
            }
            
            SelectedButton btnTemplate = colorTypeTransf.Find("Container/TemplateBtn").GetComponent<SelectedButton>();
            //ɾ��
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
            
            
            //ǰ�����󷢡���ë�����ӵ�ColorSetting
            
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
                            //����ColorSetting������,01Type/ 23ColorType/ 45Color1/ 67Color2/ 8ColorSlider
                            //ColorType������ɵ�ʱ�������Color2��Slider��û����
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
                    //�̳�,
                    if (couldInherit)
                        inheritIndex = CurColorType;
                }
                btnTemplate.transform.parent.Find($"ColorType_{inheritIndex}").GetComponent<SelectedButton>().onClick.Invoke();
                //��������BtnList,��ʼ��ʱ��û��Container
                // if (PinchFacePanel.UIBtnListContainer.UIBtnLists.Count > 4)
                // {
                //     Debug.Log("Regenerate btn 5");
                //     PinchFacePanel.UIBtnListContainer.UIBtnLists[5].InitBtnInfo();
                // }
                //todo Ӧ������������BtnList����������������BtnList Container
                
                //UIBtnListInitor[] btnLists = PinchFacePanel.transform.GetComponentsInChildren<UIBtnListInitor>(); 
                PinchFacePanel.ReGenerateBtnListContainer(PinchFacePanel.rightBtnLists);
                PinchFaceManager.pinchFaceHelper.RefreshPanelLayout(PinchFacePanel.transform);
                PinchFacePanel.ReturnBtnList(4);
            }
        }
        
        //��ʱֻ��ͷ��������Ҫ
        public void GenerateColorGradualSlider(ChangeColorPinchSetting _colorSetting)
        {
            int _counter = sortCount;
            GenerateUIPre();
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(uiPrefabPaths[2])
                .Completed += (handle) =>
            {
                //���� ���ɵ�Slider
                string shaderParam1 = "_smoothStepThreshold",shaderParam2 = "_smoothStrength";
                Transform _trans = handle.Result.transform;
                GameObject sliderTemplate = _trans.Find("Container/Pinch_SliderTemplate").gameObject;
                GameObject _slider2 = GameObject.Instantiate(sliderTemplate, sliderTemplate.transform.parent);
                

                float startValue1 = ModelPinch.GetMaterial(PinchPartType2).GetFloat(shaderParam1) * 100f;
                float startValue2 = ModelPinch.GetMaterial(PinchPartType2).GetFloat(shaderParam2) * 100f;
                SettingSlider(sliderTemplate, "����λ��",Action_ScaleSlider1,startValue1);
                SettingSlider(_slider2, "����̶�",Action_ScaleSlider2,startValue2);
                
                
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
                //��ѯ��ӦĿ¼�����е�Texture������
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
        /// ���а�ť��������Ӧ�����������ʼ����ʱ��Ҫ����
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
                //���� type3��btn
                Transform _trans = handle.Result.transform;
                Transform _slider1 = _trans.Find("Container/Pinch_Slider1");
                Transform _slider2 = _trans.Find("Container/Pinch_Slider2");
                float _value1 = _transformSetting.curValue.x,
                    _value2 = _transformSetting.curValue.y;
                
                SettingSlider(_slider1.gameObject, "λ�õ���X",Action_ScaleSlider1
                    ,PinchFaceManager.pinchFaceHelper.RemapValue(_transformSetting.curValue.x,_transformSetting.pinchRangeX, new Vector2(1, 100)));
                if (PinchPartType2 == PinchPartType2.EarTop)
                {
                    _slider2.gameObject.SetActive(false);
                }
                else
                {
                    SettingSlider(_slider2.gameObject, "λ�õ���Y",Action_ScaleSlider2
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
            //����
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
        
        #region ���ݴ洢���
        
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
                    //ÿ��PinchType
                    if (_comp.GetType() == typeof(ChangeTypePinchSetting))
                    {
                        (_comp as ChangeTypePinchSetting).ApplyType(_modelPinch,this);
                        return;
                    }
                }
                //û��Type�����
                foreach (var _comp in pinchSettingComps)
                {
                    _comp.Apply(PinchPartType2,PinchPartType3,_modelPinch);
                }
            }

            
            
        }
        #endregion
    }
}
