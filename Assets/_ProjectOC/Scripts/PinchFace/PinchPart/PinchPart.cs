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


        #region ���úͱ���
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
        Dictionary<BoneWeightType, string> boneWeightDic = new Dictionary<BoneWeightType, string>();
        
        private int sortCount = 0;
        #endregion
        //�ⲿ����

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
        /// �������SettingComp���ɶ�ӦUI
        /// </summary>
        /// <param name="SettingComp"></param>
        public void ProcessSetttingComp(IPinchSettingComp _comp)
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
                //�����ֹ�д�ɫ
                if ((_colorComp.colorChangeType & ChangeColorPinchSetting.ColorChangeType.PureColor) != ChangeColorPinchSetting.ColorChangeType.PureColor)
                {
                    GenerateHeadUI("��ɫ��ʽ");
                    GenerateColorTypeUI(_colorComp);
                }
                GenerateHeadUI("��ɫ");
                GenerateColorUI1(_comp as ChangeColorPinchSetting);

            }
            else if (_comp.GetType()== typeof(ChangeTexturePinchSetting))
            {
                GenerateHeadUI("����");
                GenerateTextureUI(_comp as ChangeTexturePinchSetting);
            }
            else if (_comp.GetType()== typeof(ChangeTransformPinchSetting))
            {
                ChangeTransformPinchSetting transComp = _comp as ChangeTransformPinchSetting;
                GenerateHeadUI("λ��");
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
                        ModelPinch.ChangeType(PinchPartType3,_index);
                     });
                 }
                 btnTemplate.gameObject.SetActive(false);
                GenerateUICallBack(_trans,_counter);
            };
        }

        
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="boneWeightTypes"></param>
        /// ���岿�֣����ܻ����ɶ���
        /// ��ʼ���ص�ʱ����ģ��ԭʼ������ֵ���ڶ��μ����Ǹ�������ֵ��Ӧ����Ҫ������ֵ
        public void GenerateBoneWeightUI(ChangeBoneWeightPinchSetting _boneWeightPinchSetting)
        {
            int _counter = sortCount;
            GenerateUIPre();
            
            //��ʱBoneWeightType Text�ֵ�

            
            //û�ж������Type�������� ������
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(uiPrefabPaths[2])
                .Completed += (handle) =>
            {
                //���� ���ɵ�Slider
                Transform _trans = handle.Result.transform;
                SelectedButton sliderTemplate = _trans.GetComponentInChildren<SelectedButton>();
                for (int i = 0; i < _boneWeightPinchSetting.BoneWeightDatas.Count; i++)
                {
                    var btn = GameObject.Instantiate(sliderTemplate.transform.parent.gameObject, sliderTemplate.transform.parent)
                        .GetComponent<SelectedButton>();
                    
                    CustomSelectedSlider _slider = btn.GetComponentInChildren<CustomSelectedSlider>();
                    
                    ChangeBoneWeightPinchSetting.BoneWeightData _boneWeightData = _boneWeightPinchSetting.BoneWeightDatas[i];
                    BoneWeightType _boneWeightType = _boneWeightData.boneWeightType;
                    Debug.Log($" slider:{_slider != null}   boneType:{_boneWeightType}");
                    _slider.ChangeText(boneWeightDic[_boneWeightType]);
                    
                    //���ó�ʼֵ
                    //β�͵Ĺ��������������ϣ��ָ�Ĭ��ֵ
                    if (_boneWeightType == BoneWeightType.Tail)
                    {
                        //
                    }
                    else
                    {
                        
                    }
                    
                    //_value 1~100 ->
                    _slider.slider.onValueChanged.AddListener((_value)=>
                    {
                        //_value Remap
                        float _realValue = PinchFaceManager.pinchFaceHelper.RemapValue(_value, new Vector2(1, 100),
                            _boneWeightData.scaleValueRange);
                        Vector3 _boneWeight = _realValue*Vector3.one;
                        
                        ModelPinch.ChangeBoneScale(_boneWeightData.boneWeightType,_boneWeight);
                    });
                    
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
        public void GenerateColorUI1(ChangeColorPinchSetting _colorSetting)
        {
            
            int _counter = sortCount;
            GenerateUIPre();
            
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(uiPrefabPaths[3])
                .Completed += (handle) =>
            {
                //���� type3��btn
                Transform _trans = handle.Result.transform;
                Transform _container1 = _trans.Find("Container1");
                Transform _container2 = _trans.Find("Container2");
                SelectedButton[] _buttons = _container1.GetComponentsInChildren<SelectedButton>();
                foreach (var _colorGrid in _buttons)
                {
                    Color _gridColor = _colorGrid.transform.Find("Image").GetComponent<Image>().color;
                    _colorGrid.onClick.AddListener(() =>
                    {
                        ModelPinch.ChangeColor(PinchPartType2,_gridColor);
                        _container2.Find("ColorView").GetComponent<Image>().color = _gridColor;
                    });
                }
                
                _container2.GetComponentInChildren<SelectedButton>().onClick.AddListener(() =>
                {
                    //�л�Ϊ�ڶ��ֱ༭ģʽ
                });
                GenerateUICallBack(_trans,_counter);
            };
        }
        /// <summary>
        /// RGB/HSV
        /// </summary>
        /// <param name="_type3"></param>
        /// <param name="_color"></param>
        public void GenerateColorUI2(ChangeColorPinchSetting _colorSetting)
        {
            int _counter = 0;
            //to-do color2 ��counterӦ���Ǽ̳�ɾ����_counter
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(uiPrefabPaths[4])
                .Completed += (handle) =>
            {
                //���� type3��btn
                Transform _trans = handle.Result.transform;
                GenerateUICallBack(_trans,_counter);
            };
        }

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
                //���� type3��btn
                Transform _trans = handle.Result.transform;
                
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
                //���� type3��btn
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
                //���� type3��btn
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
            
            //����
            isInit--;
            if (isInit == 0)
            {
                PinchFaceManager.pinchFaceHelper.SortUIAfterGenerate(_uiTransf,containerTransf,PinchFacePanel);
            }
            
        }
    }
}
