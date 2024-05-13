using System.Collections;
using System.Collections.Generic;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using ProjectOC.PinchFace.Config;
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

            CommonType2.Add(PinchPartType2.Body);
            CommonType2.Add(PinchPartType2.HairFront);
            CommonType2.Add(PinchPartType2.Eye);
            CommonType2.Add(PinchPartType2.FaceDress);
            
            this.UIBtnListContainer =
                new UIBtnListContainer(this.transform.GetComponentInChildren<UIBtnListContainerInitor>());
            #region ��ʼ���ɶ�ȡģ�� Config ͼ��
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(playerModelPrefabPath).Completed+=(handle) =>
            {
                uICameraImage = transform.Find("UICameraImage").GetComponentInChildren<UICameraImage>();
                RectTransform _rtTransform = uICameraImage.transform as RectTransform;
                
                RenderTexture _rt = new RenderTexture((int)_rtTransform.rect.width,(int)_rtTransform.rect.height,0);
                uICameraImage.Init(_rt);
                pinchFaceManager.ModelPinch = handle.Result.GetComponentInChildren<CharacterModelPinch>();
                UICameraImage.ModeGameObjectLayer(handle.Result.transform);
                CurType2 = PinchPartType2.Body;
                
                //������������ ͷ��
                foreach (var _type3 in raceData.pinchPartType3s)
                {
                    // int prefabCount = Config.typesDatas[(int)_type3 - 1].typeCount;
                    // if (prefabCount != 0)
                    // {
                    //     
                    // }
                }
            };
            
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<PinchDataConfig>(pinchDataConfigPath).Completed+=(handle) =>
            {
                Config = handle.Result;
            };

            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<SpriteAtlas>(pinchFaceSAPath).Completed+=(handle) =>
            {
                SA_PinchPart = handle.Result;
            };
            #endregion

        }

        public override void OnExit()
        {
            base.OnExit();
            uICameraImage.DisableUICameraImage();
        }

        #endregion

        #region Internal

        protected override void RegisterInput()
        {
            base.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Enable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
        }
        protected override void UnregisterInput()
        {
            base.UnregisterInput();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Disable();
            this.UIBtnListContainer.DisableUIBtnListContainer();
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            //�����ǰ��btnList[7]
            if (CurrentState == CurrentMouseState.Left)
            {
                Destroy(pinchFaceManager.ModelPinch.gameObject);
                ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
            }
            else if (CurrentState == CurrentMouseState.Right)
            {
                ReturnBtnList(1);
            }
        }
        
        //btnList0 ���
        //btnList1 2
        //btnList3 ��ɴ��� 
        
        //btnList4--��  ��ʽ����ɫ
        protected override void InitBtnInfo()
        {
            UIBtnListContainer.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);
            foreach (var _btnList in UIBtnListContainer.UIBtnLists)
            {
                _btnList.BindButtonInteractInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.Confirm,UIBtnListContainer.BindType.started);
            }
            UIBtnListContainer.AddOnSelectButtonChangedAction(() =>
            {
                //�Ҳ������������£���Ӣ���л�ֱ�ӻ�RacePinchData
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
                        CurType2 = CommonType2[_curPos];
                    }
                }

                
            });
        }
        
        //�����Ҳ�BtnList����������
        public void ReGenerateBtnListContainer(List<UIBtnListInitor> _btnLists)
        {
            //�Ȼ�ȡǰ�澲̬�ķֲ�
            UIBtnListContainer.DisableUIBtnListContainer();
            rightBtnLists = _btnLists;
            
            UIBtnListContainerInitor newBtnListContainers =  this.transform.GetComponentInChildren<UIBtnListContainerInitor>();
            int defaultIndex = 4;
            int newLinkDataCount = _btnLists.Count - 1;
            
            //��4����������һ��
            //���һ������Ҫ��������
            for (int i = 0; i < newLinkDataCount; i++)
            {
                // _listContainer.LinkTwoEdge();
                UIBtnListContainerInitor.LinkData _linkData = new UIBtnListContainerInitor
                    .LinkData(i+defaultIndex,i+defaultIndex+1,UIBtnListContainerInitor.EdgeType.�²�˳ʱ��,UIBtnListContainerInitor.EdgeType.�ϲ���ʱ��,UIBtnListContainerInitor.LinkType.��������);
                newBtnListContainers.btnListContainerInitData.AddLinkData(_linkData);
            }
            this.UIBtnListContainer = new UIBtnListContainer(newBtnListContainers);
            //btnListContainerInitor ɾ����ԭ
            newBtnListContainers.btnListContainerInitData.RemoveLinkData(defaultIndex-1,newLinkDataCount);
            
            
            //�󶨺���
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
        [ShowInInspector]
        private UIBtnListContainer UIBtnListContainer;
        public Transform containerTransf,commonBtnList;
        private RacePinchData raceData;
        private List<PinchPart> pinchParts = new List<PinchPart>();
        private Dictionary<PinchPartType3, SelectedButton> type3ButtonDic = new Dictionary<PinchPartType3, SelectedButton>();
        private Dictionary<PinchPartType3, PinchPartType2> type3Type2Dic => pinchFaceManager.pinchPartType3Dic;
        public List<UIBtnListInitor> rightBtnLists = new List<UIBtnListInitor>();
        
        public PinchDataConfig Config;
        public SpriteAtlas SA_PinchPart;
        public UICameraImage uICameraImage;

        
        private PinchPartType2 curType2;
        private string pinchFaceSAPath = "SA_UI_PinchFace/SA_PinchFace.spriteatlasv2";
        private string playerModelPrefabPath = "Prefabs_PinchPart/PinchPart/Prefab_PinchModel.prefab";
        private string pinchDataConfigPath = "PinchAsset_PinchFaceSetting/PinchDataConfig.asset";
        private const int commonType3Count = 4;
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
        private List<PinchPartType2> CommonType2 = new List<PinchPartType2>();
        
        public void InitRaceData(RacePinchData _racePinchData)
        {
            raceData = _racePinchData;
            //���͡�ͷ�����۾�����ױ
            int _pinchPartTypeCount = _racePinchData.pinchPartType3s.Count + commonType3Count;
            for (int i = 0; i < _pinchPartTypeCount; i++)
            {
                pinchParts.Add(null);
            }
            
            
            //�������List<PinchPartType3>
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
            //common���Action
            for (int i = 0; i < commonType3Count; i++)
            {
                int _index = i;
                commonBtnList.GetChild(i).GetComponent<SelectedButton>()
                    .onClick.AddListener(() =>
                    {
                        foreach (Transform _container in containerTransf)
                        {
                            _container.gameObject.SetActive(false);
                            Destroy(_container.gameObject);
                        }
                        GenerateChangeSetting(_index,true);
                    });
            }
        }

        private void LeftButton_BtnAction(int _index)
        {
            //ɾ������ChangeSetting
            foreach (Transform _container in containerTransf)
            {
                 _container.gameObject.SetActive(false);
                 Destroy(_container.gameObject);
            }
            GenerateChangeSetting(_index);
        }

        private void GenerateChangeSetting(int _Dataindex,PinchPartType2 _type2,PinchPartType3 _type3)
        {
            
            string pathFore = "PinchAsset_PinchFaceSetting/PinchTypeConfig";
            string templatePath = $"{pathFore}/{(int)_type2-1}_{_type2.ToString()}_PinchType2Template.prefab";

            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(templatePath)
                .Completed += (handle) =>
            {
                var _comps = handle.Result.GetComponents<IPinchSettingComp>();
                pinchParts[_Dataindex] = new PinchPart(this, _type3, _type2, _comps, containerTransf);
                Destroy(handle.Result);
            };
            //����Type3���Ƿ���com
        }
        private void GenerateChangeSetting(int _index,bool isCommon = false)
        {
            //todo ��ʱ�ֱ�����Common����ķ���ʹ�����ܻ��
            if (isCommon)
            {
                int realIndex = raceData.pinchPartType3s.Count + _index;
                //���͡�ͷ�����۾�����ױ
                switch (_index)
                {
                    case 0:
                        GenerateChangeSetting(realIndex,PinchPartType2.Body, PinchPartType3.B_Body);
                        break;
                    case 1:
                        GenerateChangeSetting(realIndex,PinchPartType2.HairFront, PinchPartType3.HF_HairFront);
                        break;
                    
                    case 2:
                        GenerateChangeSetting(realIndex,PinchPartType2.Eye, PinchPartType3.O_Orbit);
                        break;
                    
                    case 3:
                        GenerateChangeSetting(realIndex,PinchPartType2.FaceDress, PinchPartType3.FD_FaceDress);
                        break;
                }
                
            }
            else
            {
                PinchPartType3 _type3 = raceData.pinchPartType3s[_index];
                //����TypeѰ�Ҷ�Ӧ�ļ������������Component����Prefab
                PinchPartType2 _type2 = type3Type2Dic[_type3];
                GenerateChangeSetting(_index,_type2, _type3);
            }
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