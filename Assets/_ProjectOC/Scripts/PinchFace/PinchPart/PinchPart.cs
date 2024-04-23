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
        
        private int sortCount = 0;
        private int BtnListCount = 0;
        #endregion
        //�ⲿ����

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
                GenerateTypeUI(PinchPartType3);
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
                GenerateColorUI1(PinchPartType3,Color.white);

            }
            else if (_comp.GetType()== typeof(ChangeTexturePinchSetting))
            {
                GenerateHeadUI("����");
                GenerateTextureUI(PinchPartType3);
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
        public void GenerateTypeUI(PinchPartType3 _type3)
        {
            int _counter = sortCount;
            GenerateUIPre();
            
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(uiPrefabPaths[1])
                .Completed += (handle) =>
            {
                Transform _trans = handle.Result.transform;
                //��ѯ��ӦĿ¼�����е�Texture������
                 string pathFore = "OC/UI/PinchFace/Texture";
                 //string type3Path = PinchFaceManager.pinchFaceHelper.GetType3PrefabPath(PinchPartType2,_type3);
                 //���ض�Ӧ��Type3 icon button
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
        /// ���岿�֣����ܻ����ɶ���
        /// ��ʼ���ص�ʱ����ģ��ԭʼ������ֵ���ڶ��μ����Ǹ�������ֵ��Ӧ����Ҫ������ֵ
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
        /// ���岿�֣����ܻ����ɶ���,_valueΪ��ǰ�浵����
        public void GenerateBoneWeightUI(List<BoneWeightType> boneWeightTypes,int _counter,List<int> _values = null,List<ChangeBoneWeightPinchSetting.BoneWeightChangeType> _ChangeTypes = null)
        {
            //û�ж������Type�������� ������
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(uiPrefabPaths[2])
                .Completed += (handle) =>
            {
                //���� ���ɵ�Slider
                Transform _trans = handle.Result.transform;
                GenerateUICallBack(_trans,_counter);
            };
        }

        
        
        
        /// <summary>
        /// Ԥ����ColorButton,�����ټ�һ�������ĸ�Mat��Color����Color���
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
                //���� type3��btn
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
        
        public void GenerateTextureUI(PinchPartType3 _type3)
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
