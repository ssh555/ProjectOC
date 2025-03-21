using System.Collections;
using System.Collections.Generic;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using ProjectOC.Player;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.U2D;

namespace ProjectOC.PinchFace
{
    public class UIPinchFacePanel : UIBasePanel<UIPinchFacePanel.PinchFacePanelStruct>
    {
        #region ML
        protected override void Awake()
        {
            base.Awake();
            pinchFaceManager = LocalGameManager.Instance.PinchFaceManager;
            
            
            this.UIBtnListContainer =
                new UIBtnListContainer(this.transform.GetComponentInChildren<UIBtnListContainerInitor>());
            #region 初始PinchParts 生成读取模型 Config 图集
            GenerateCharacterModel();
            

            void GenerateCharacterModel()
            {
                GameManager.Instance.ABResourceManager.InstantiateAsync(pinchFaceManager.playerModelPrefabPath).Completed+=(handle) =>
                {
                    uICameraImage = transform.Find("UICameraImage").GetComponentInChildren<UICameraImage>();
                    RectTransform _rtTransform = uICameraImage.transform as RectTransform;

                    RenderTexture _rt = new RenderTexture((int)_rtTransform.rect.width,(int)_rtTransform.rect.height,0);
                    uICameraImage.Init(_rt);
                    pinchFaceManager.ModelPinch = handle.Result.GetComponentInChildren<CharacterModelPinch>();
                    UICameraImage.ModeGameObjectLayer(handle.Result.transform);
                    CurType2 = PinchPartType2.Body;
                    //生成随机PinchPart和CommonPart
                    Init_RedeceCheckCallBack();
                };
            }
            
            
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<SpriteAtlas>(pinchFaceSAPath).Completed+=(handle) =>
            {
                SA_PinchPart = handle.Result;
            };
            #endregion

        }
        
        void Init_RedeceCheckCallBack()
        {
            IsInit--;
            if (IsInit == 0)
            {
                //生成随机PinchPart和CommonPart
                if (isInhrit)
                {
                    foreach (var _pinchPartData in PinchPartDatas)
                    {
                        _pinchPartData.ApplyPinchSetting(modelPinch);
                    }
                }
                else
                {
                    RandomPinchPart();
                }
                
                
                
            }
        }

        
        
        
        public override void OnExit()
        {
            base.OnExit();
            uICameraImage.DisableUICameraImage();
            Destroy(pinchFaceManager.ModelPinch.gameObject);
        }

        #endregion

        #region Internal

        protected override void RegisterInput()
        {
            base.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Enable();
            UIBtnListContainer.UIBtnLists[3].GetBtn(0).onClick.AddListener(ApplyPinchType);
            UIBtnListContainer.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.MainInteract.performed += ApplyPinchType;
            ML.Engine.Input.InputManager.Instance.Common.Common.SubInteract.performed += RandomPinchPart;
        }
        protected override void UnregisterInput()
        {
            base.UnregisterInput();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Disable();
            UIBtnListContainer.UIBtnLists[3].DisableBtnList();
            this.UIBtnListContainer.DisableUIBtnListContainer();
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.MainInteract.performed -= ApplyPinchType;
            ML.Engine.Input.InputManager.Instance.Common.Common.SubInteract.performed -= RandomPinchPart;
        }

        void ApplyPinchType(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            ApplyPinchType();
        }
        void ApplyPinchType()
        {
            GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.PopUpUI, 
                new UIManager.PopUpUIData($"要保存当前角色形象吗?","今后创建该种族角色或更改角色外观时，可读取已保存的角色形象，快捷完成指令", null, 
                    PushNameInput,ApplyAndPop
                ));
            //延迟一帧推入，以防推入后直接被Pop
            void PushNameInput()
            {
                StartCoroutine(DelayPushNotice());
                IEnumerator DelayPushNotice()
                {
                    yield return null;
                    GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.InputFieldUI,
                        new UIManager.InputFieldUIData("请输入角色形象名称", "", null,
                            PushFaceTemplate, null));
                }
            }

            void PushFaceTemplate(string _str1,string str2)
            {
                //本身就是异步，不需要推迟一帧
                pinchFaceManager.GenerateFaceTemplateUI(raceData,false,PinchPartDatas,_str1);
            }
            void ApplyAndPop()
            {
                GameManager.Instance.UIManager.PopPanel();
                List<PinchPart.PinchPartData> pinchPartDatas = new List<PinchPart.PinchPartData>();
                foreach (var _pinchPart in pinchParts)
                {
                    pinchPartDatas.Add(_pinchPart.pinchPartData);
                }
                pinchFaceManager.TargetModelPinch.SetPinchPartDatas(pinchPartDatas,true);
            }
        }
        void RandomPinchPart(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            RandomPinchPart();
        }
        /// <summary>
        /// True继承，已经有的部分不会随机
        /// </summary>
        /// <param name="_inherit"></param>
        void RandomPinchPart()
        {
            //生成随机的组件 头发
            foreach (var _pinchPart in pinchParts)
            {
                PinchPartType3 _type3 = _pinchPart.PinchPartType3;
                //如果没有继承,随机Type
                int typePrefabCount = pinchFaceManager.Config.typesDatas[(int)_type3 - 1].typeCount;
                if (typePrefabCount != 0)
                {
                    foreach (var _pinchSetting in _pinchPart.pinchSettingComps)
                    {
                        if (_pinchSetting.GetType() == typeof(ChangeTypePinchSetting))
                        {
                            /////通过PinchPart调用
                            ChangeTypePinchSetting _typePinch = _pinchSetting as ChangeTypePinchSetting;
                            int _typeIndex = Random.Range(0, typePrefabCount);
                            _pinchPart.Action_TypeBtn(_typePinch,_typeIndex);
                            break;
                        }
                    }
                } 
            }
        }
        
        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            //如果当前在btnList[7]
            if (CurrentState == CurrentMouseState.Left)
            {
                GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.PopUpUI, 
                    new UIManager.PopUpUIData("确认终止角色创建吗?", "你将丢失当前未保存的数据", null, 
                        () => {
                            ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
                        },null));
            }
            else if (CurrentState == CurrentMouseState.Right)
            {
                ReturnBtnList(1);
            }
        }
        
        //btnList0 随机
        //btnList1 2
        //btnList3 完成创建 
        
        //btnList4--？  样式、颜色
        protected override void InitBtnInfo()
        {
            UIBtnListContainer.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);
            foreach (var _btnList in UIBtnListContainer.UIBtnLists)
            {
                _btnList.BindButtonInteractInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.Confirm,UIBtnListContainer.BindType.started);
            }
            UIBtnListContainer.AddOnSelectButtonChangedAction(() =>
            {
                int curListIndex = UIBtnListContainer.CurSelectUIBtnListIndex;
                int _curPos = UIBtnListContainer.UIBtnLists[curListIndex].GetCurSelectedPos1();
                if (curListIndex == 1)
                {
                    CurType2 = pinchFaceManager.pinchPartType3Dic[raceData.pinchPartType3s[_curPos]];
                }
                else if (curListIndex == 2)
                {
                    _curPos = UIBtnListContainer.UIBtnLists[2].GetCurSelectedPos1();
                    if (_curPos != -1)
                    {
                        int _index = _curPos + raceData.pinchPartType3s.Count;
                        CurType2 = pinchParts[_index].PinchPartType2;
                    }
                }
            });
            base.InitBtnInfo();
        }
        
        //返回右侧BtnList，重新生成
        public void ReGenerateBtnListContainer(List<UIBtnListInitor> _btnLists)
        {
            //先获取前面静态的分布
            UIBtnListContainer.DisableUIBtnListContainer();
            rightBtnLists = _btnLists;
            
            UIBtnListContainerInitor newBtnListContainers =  this.transform.GetComponentInChildren<UIBtnListContainerInitor>();
            int defaultIndex = 4;
            int newLinkDataCount = _btnLists.Count - 1;
            
            //从4连到倒数第一个
            //最后一个不需要加连接线
            for (int i = 0; i < newLinkDataCount; i++)
            {
                // _listContainer.LinkTwoEdge();
                UIBtnListContainerInitor.LinkData _linkData = new UIBtnListContainerInitor
                    .LinkData(i+defaultIndex,i+defaultIndex+1,UIBtnListContainerInitor.EdgeType.下侧顺时针,UIBtnListContainerInitor.EdgeType.上侧逆时针,UIBtnListContainerInitor.LinkType.上下相连);
                newBtnListContainers.btnListContainerInitData.AddLinkData(_linkData);
            }
            
            UIBtnListContainer = new UIBtnListContainer(newBtnListContainers);
            //btnListContainerInitor 删除复原
            newBtnListContainers.btnListContainerInitData.RemoveLinkData(defaultIndex-1,newLinkDataCount);
            
            //绑定函数
            InitBtnInfo();
        }
        #endregion

        
        #region TextContent
        public struct PinchFacePanelStruct
        {
            public TextTip[] Btns;
        }
        protected override void OnLoadJsonAssetComplete(UIPinchFacePanel.PinchFacePanelStruct datas)
        {
            InitBtnData(datas);
        }

        protected override void InitTextContentPathData()
        {
            this.abpath = "OCTextContent/PlayerUIPanel";
            this.abname = "PlayerUIPanel";
            this.description = "PinchRace";
        }
        private void InitBtnData(UIPinchFacePanel.PinchFacePanelStruct datas)
        {
            // foreach (var tt in datas.Btns)
            // {
            //     this.UIBtnList.SetBtnText(tt.name, tt.description.GetText());
            // }
        }
        #endregion


        #region FacePanel

        private PinchFaceManager pinchFaceManager;
        private CharacterModelPinch modelPinch => pinchFaceManager.ModelPinch;
        public UIBtnListContainer UIBtnListContainer { private set; get; }
        public Transform containerTransf,commonBtnList;
        private RacePinchData raceData;
        [ShowInInspector]
        private List<PinchPart> pinchParts;
        private Dictionary<PinchPartType3, SelectedButton> type3ButtonDic = new Dictionary<PinchPartType3, SelectedButton>();
        private Dictionary<PinchPartType3, PinchPartType2> type3Type2Dic => pinchFaceManager.pinchPartType3Dic;
        public List<UIBtnListInitor> rightBtnLists = new List<UIBtnListInitor>();
        
        
        public SpriteAtlas SA_PinchPart;
        public UICameraImage uICameraImage;

        
        
        private string pinchFaceSAPath = "SA_UI_PinchFace/SA_PinchFace.spriteatlasv2";
        private const int commonType3Count = 4;
        private int IsInit = 2;
        private PinchPartType2 curType2;
        public PinchPartType2 CurType2
        {
            get
            {
                return curType2;
            }
            set
            {
                curType2 = value;
                modelPinch.CameraView.CameraLookAtSwitch(uICameraImage,curType2);
            }
        }
        
        
        enum CurrentMouseState
        {
            Left,
            Right
        }

        private CurrentMouseState CurrentState = CurrentMouseState.Left;
        public List<PinchPart.PinchPartData> PinchPartDatas;
        private bool isInhrit = false;
        public void InitRaceData(RacePinchData _racePinchData,List<PinchPart.PinchPartData> _PinchPartDatas)
        {
            //初始化raceData
            raceData = _racePinchData;
            Dictionary<PinchPartType3, PinchPart.PinchPartData> dataDic = new Dictionary<PinchPartType3, PinchPart.PinchPartData>();
            if (_PinchPartDatas != null)
            {
                isInhrit = true;
                foreach (var _pinchPartData in _PinchPartDatas)
                {
                    dataDic.Add(_pinchPartData.PinchPartType3,_pinchPartData);
                }
            }
            
            
            
            //体型、头发、眼睛、面妆
            int _pinchPartTypeCount = _racePinchData.pinchPartType3s.Count + commonType3Count;
            if (pinchParts == null)
            {
                pinchParts = new List<PinchPart>();
                for (int i = 0; i < _pinchPartTypeCount; i++)
                {
                    pinchParts.Add(null);
                } 
            }

            int setDataCounter = 0;
            
            for(int i = 0;i<raceData.pinchPartType3s.Count;i++)
            {
                PinchPartType3 _type3 = raceData.pinchPartType3s[i];
                PinchPartType2 _type2 = pinchFaceManager.pinchPartType3Dic[_type3];
                InitPinchPartTypes(i,  _type2,_type3);
            }

            int _pinchPart3Count = raceData.pinchPartType3s.Count;
            InitPinchPartTypes(_pinchPart3Count+0, PinchPartType2.Body, PinchPartType3.B_Body);
            InitPinchPartTypes(_pinchPart3Count+1, PinchPartType2.HairFront, PinchPartType3.HF_HairFront);
            InitPinchPartTypes(_pinchPart3Count+2, PinchPartType2.Eye, PinchPartType3.O_Orbit);
            InitPinchPartTypes(_pinchPart3Count+3, PinchPartType2.FaceDress, PinchPartType3.FD_FaceDress);
            //如果是继承则不需要异步初始化，直接回调
            if (_PinchPartDatas != null)
            {
                SavePinchPartData();
                Init_RedeceCheckCallBack();
            }
            
                
            void InitPinchPartTypes(int _index,PinchPartType2 _type2, PinchPartType3 _type3)
            {
                if (dataDic.ContainsKey(_type3))
                {
                    pinchParts[_index] = new PinchPart(this, _type3, _type2, dataDic[_type3], containerTransf);
                }
                else
                {
                    setDataCounter++;
                    
                    if( _PinchPartDatas != null)
                        Debug.LogError($"Has no: {_type3}");
                    string pathFore = "PinchAsset_PinchFaceSetting/PinchTypeConfig";
                    string templatePath = $"{pathFore}/{(int)_type2-1}_{_type2.ToString()}_PinchType2Template.prefab";

                    GameManager.Instance.ABResourceManager.InstantiateAsync(templatePath)
                        .Completed += (handle) =>
                    {
                        var _comps = handle.Result.GetComponents<IPinchSettingComp>();
                        pinchParts[_index] = new PinchPart(this, _type3, _type2, _comps, containerTransf);
                        Destroy(handle.Result);

                        setDataCounter--;
                        if (setDataCounter == 0)
                        {
                            SavePinchPartData();
                            Init_RedeceCheckCallBack();    
                        }
                    };
                }
            }

            void SavePinchPartData()
            {
                List<PinchPart.PinchPartData> tempPinchPartDatas = new List<PinchPart.PinchPartData>();
                foreach (var _pinchPart in pinchParts)
                {
                    tempPinchPartDatas.Add(_pinchPart.pinchPartData);
                }
                PinchPartDatas = tempPinchPartDatas;
            }
            
            //UI生成
            //生成左侧List<PinchPartType3>
            for(int i = 0;i<raceData.pinchPartType3s.Count;i++)
            {
                int _buttonIndex = i;
                PinchPartType3 _partType3 = _racePinchData.pinchPartType3s[i];
                this.UIBtnListContainer.AddBtn(1, "Prefabs_PinchPart/UIPanel/Prefab_Pinch_BaseUISelectedBtn.prefab"
                    ,BtnText:_partType3.ToString()
                    ,BtnSettingAction:(_btn)=>
                    {
                        type3ButtonDic.Add(_partType3,_btn);
                        pinchFaceManager.pinchFaceHelper.RefreshPanelLayout(this.transform);
                    }
                    ,BtnAction: () =>
                    {
                        LeftButton_BtnAction(_buttonIndex);
                    });
            }
            //common添加Action
            for (int i = 0; i < commonType3Count; i++)
            {
                int _index = i;
                commonBtnList.GetChild(i).GetComponent<SelectedButton>()
                    .onClick.AddListener(() =>
                    {
                        LeftButton_BtnAction(_index,true);
                    });
            }
        }

        private void LeftButton_BtnAction(int _index,bool _isCommon = false)
        {
            //删除原来的所有ChangeSetting
            foreach (Transform _container in containerTransf)
            {
                 _container.gameObject.SetActive(false);
                 //有没有可能Destory延迟执行，在Generate生成后执行
                 //防止影响排序
                 _container.name = "99999";

                 Destroy(_container.gameObject);
            }
            GenerateChangeSetting(_index, _isCommon);
        }


        private void GenerateChangeSetting(int _index,bool isCommon = false)
        {
            //todo 临时粗暴处理，Common后面的分类和处理可能会变
            if (isCommon)
            {
                _index += raceData.pinchPartType3s.Count;
            }
            
            pinchParts[_index].GeneratePinchPartSetting();
        }

    
        public void ReturnBtnList(int _index)
        {
            UIBtnListContainer.MoveToBtnList(UIBtnListContainer.UIBtnLists[_index]);
            if (_index > 3)
            {
                CurrentState = CurrentMouseState.Right;
            }
            else
            {
                CurrentState = CurrentMouseState.Left;
            }
        }
        #endregion
    }
}