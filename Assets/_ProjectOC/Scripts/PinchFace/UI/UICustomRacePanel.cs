using System;
using System.Collections;
using System.Collections.Generic;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectOC.PinchFace
{
    public class UICustomRacePanel : ML.Engine.UI.UIBasePanel<UICustomRacePanel.PinchCustomRacePanelStruct>
    {
        #region Unity

        protected override void Awake()
        {
            base.Awake();
            pinchFaceManager = LocalGameManager.Instance.PinchFaceManager;
            selectBtnTemplate = transform.Find("RightPanel/Container/Prefab_Pinch_BaseUISelectedBtn")
                .GetComponent<SelectedButton>();
            rightContainerTransf = transform.Find("RightPanel/Container");
            GenerateCharacterModel();
            
            void GenerateCharacterModel()
            {
                IsInit++;
                ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(pinchFaceManager.playerModelPrefabPath).Completed+=(handle) =>
                {
                    uICameraImage = transform.Find("UICameraImage").GetComponentInChildren<UICameraImage>();
                    RectTransform _rtTransform = uICameraImage.transform as RectTransform;

                    RenderTexture _rt = new RenderTexture((int)_rtTransform.rect.width,(int)_rtTransform.rect.height,0);
                    uICameraImage.Init(_rt);
                    CharacterModelPinch modelPinch = handle.Result.GetComponentInChildren<CharacterModelPinch>();
                    pinchFaceManager.ModelPinch = modelPinch;
                    UICameraImage.ModeGameObjectLayer(handle.Result.transform);
                    //看向
                    // CurType2 = PinchPartType2.Body;
                    modelPinch.CameraView.CameraLookAtSwitch(uICameraImage,PinchPartType2.Body);
                    pinchFaceManager.RandomPinchPart(PinchPartType3.HF_HairFront, true);
                };
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Destroy(pinchFaceManager.ModelPinch.gameObject);
            uICameraImage.DisableUICameraImage();
        }
        #endregion


        #region Internal

        protected override void RegisterInput()
        {
            base.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Enable();
            UIBtnListContainer.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
            
            UIBtnListContainer.UIBtnLists[6].SetBtnAction(0,0,() =>
            {
                //完成种族创建，加入
                raceData.raceName = "raceName: " + pinchFaceManager.RacePinchDatas.Count;
                raceData.raceDescription = "raceDescription: " + pinchFaceManager.RacePinchDatas.Count;
                pinchFaceManager.RacePinchDatas.Add(raceData);
                ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
                pinchFaceManager.GeneratePinchRaceUI();
            });
        }
        protected override void UnregisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Disable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }
        
        //btnList0 随机种族
        //btnList1--5  种族部件
        //btnList6  完成创建
        //btnList7  选择部件
        protected override void InitBtnInfo()
        {
            this.UIBtnListContainer = new UIBtnListContainer(this.transform.GetComponentInChildren<UIBtnListContainerInitor>());
            foreach (var _btnList in UIBtnListContainer.UIBtnLists)
            {
                _btnList.BindButtonInteractInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.Confirm,UIBtnListContainer.BindType.started);
            }
            
            //0-4,5是通用不需要加
            for (int i = 0; i < pinchFaceManager.pinchPartType1Inclusion.Count-1; i++)
            {
                foreach (var _type2 in pinchFaceManager.pinchPartType1Inclusion[i])
                {
                    this.UIBtnListContainer.AddBtn(i+1,pinchButtonPath
                        ,BtnText: _type2.ToString()
                        ,BtnAction:LeftButton_BtnAction
                        ,BtnSettingAction:(btn) =>
                        {
                            //To-Do 应该是异步加载完刷新一次的，而不是每次生成完Button都刷新，不过看上去也不卡
                            leftButtonDic.Add(_type2, btn);
                            pinchFaceManager.pinchFaceHelper.RefreshPanelLayout(this.transform);
                            SetLeftBtnText(btn, isNake:true);
                        });
                }
            }
            this.UIBtnListContainer.AddOnSelectButtonChangedAction(SelectButtonChangedAction);
        }
        void SelectButtonChangedAction()
        {
            //右侧种族描述更新，中英文切换直接换RacePinchData
            int _curPos = UIBtnListContainer.UIBtnLists[7].GetCurSelectedPos1();
            if (_curPos != -1)
            {
                PinchPartType curPinchPartType = pinchFaceManager.pinchPartType2Dic[curType2];
                if (curPinchPartType.couldNaked)
                {
                    _curPos--;
                    
                }
                if (_curPos == -1)
                {
                    pinchFaceManager.ModelPinch.UnEquipItem(curType2);
                }
                else
                {
                    PinchPartType3 _type3 = curPinchPartType.pinchPartType3s[_curPos];
                    pinchFaceManager.RandomPinchPart(_type3,true);
                }
                
                
            }
            
        }
        
        
        private void LeftButton_BtnAction()
        {
            //UIBtnListContainer.UIBtnLists[7].DeleteAllButton();
            int _curListIndex = UIBtnListContainer.CurSelectUIBtnListIndex;
            Vector2Int _curPos = UIBtnListContainer.UIBtnLists[_curListIndex].GetCurSelectedPos2();
            int _curPosOne = _curPos.x * OneRowCount + _curPos.y;
            
            curType2 = pinchFaceManager.pinchPartType1Inclusion[_curListIndex-1][_curPosOne];
            PinchPartType2 _type2 = curType2;
            PinchPartType _ppt = pinchFaceManager.pinchPartType2Dic[_type2];

            
            if (_ppt.couldNaked)
            {
                SelectedButton _btn = Instantiate(selectBtnTemplate,rightContainerTransf);
                _btn.gameObject.SetActive(true);
                _btn.name = "RightBtn_-1";
                _btn.GetComponentInChildren<TextMeshProUGUI>().text = "Naked";
                _btn.onClick.AddListener(()=>RightButton_BtnAction(_type2,-1));
            }

            for (int i = 0;i < _ppt.pinchPartType3s.Count; i++)
            {
                // Debug.Log(_ppt.pinchPartType3s[i].ToString());
                int _index = i;
                SelectedButton _btn = Instantiate(selectBtnTemplate,rightContainerTransf);
                _btn.gameObject.SetActive(true);
                _btn.name = $"RightBtn_{_index}";
                _btn.GetComponentInChildren<TextMeshProUGUI>().text = _ppt.pinchPartType3s[_index].ToString();
                _btn.onClick.AddListener(()=>RightButton_BtnAction(_type2,_index));
                //SetLeftBtnText(_btn, isNake:true);
            }
            CurrentState = CurrentMouseState.Right;
            StartCoroutine(GenerateRightBtn());
            // UIBtnListContainer.UIBtnLists[7].InitBtnInfo();
            // UIBtnListContainer.MoveToBtnList(UIBtnListContainer.UIBtnLists[7]);
        }
        //todo 似乎同步生成btnlist + 跳转 会触发
        IEnumerator GenerateRightBtn()
        {
            // 等待当前帧渲染完成
            yield return null;
        
            UIBtnListContainer.UIBtnLists[7].InitBtnInfo();
            UIBtnListContainer.MoveToBtnList(UIBtnListContainer.UIBtnLists[7]);
        }
                

        private void RightButton_BtnAction(PinchPartType2 _type2,int _type3)
        {
            //修改Panel text、图片  ,加入种族
            PinchPartType ppt = pinchFaceManager.pinchPartType2Dic[_type2];
            SelectedButton _btn = leftButtonDic[_type2];
            
            
            
            if (_type3 == -1)
            {
                SetLeftBtnText(_btn, _type2.ToString(),true);
            }
            else
            {
                // Debug.Log("");
                SetLeftBtnText(_btn, ppt.pinchPartType3s[_type3].ToString(),false);
                AddType3(_type2, _type3);
            }
            
            BackActionOfList7();
        }
        //右侧面部Button相关函数
        private void AddType3(PinchPartType2 _type2,int _type3)
        {
            PinchPartType dicType = pinchFaceManager.pinchPartType2Dic[_type2];
            foreach (var _dicType3 in dicType.pinchPartType3s)
            {
                if (raceData.pinchPartType3s.Contains(_dicType3))
                {
                    raceData.pinchPartType3s.Remove(_dicType3);
                }
            }
            
            //删除前面的0
            // _type3--;
            if (_type3 >= 0)
            {
                // if (pinchFaceManager.pinchPartType2Dic[_type2].couldNaked)
                //     _type3--;
                
                raceData.pinchPartType3s.Add(dicType.pinchPartType3s[_type3]);    
            }
        }

        
        private void BackActionOfList7()
        {            
            // 返回 selectType
            CurrentState = CurrentMouseState.Left;
            
            // int _listIndex = (int)pinchFaceManager.pinchPartType2Dic[curType2].pinchPartType1;
            // UIBtnListContainer.CurSelectUIBtnList = UIBtnListContainer.UIBtnLists[_listIndex];
            UIBtnListContainer.MoveToBtnList(UIBtnListContainer.UIBtnLists[1]);
            
            for(int i = rightContainerTransf.childCount-1;i>=0;i--)
            {
                Transform _btn = rightContainerTransf.GetChild(i);
                if (_btn.gameObject.GetInstanceID() != selectBtnTemplate.gameObject.GetInstanceID())
                {
                    _btn.SetParent(this.transform);
                    Destroy(_btn.gameObject);
                }
            }
            curType2 = PinchPartType2.None;
        }
        
        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            //如果当前在btnList[7]
            if (CurrentState == CurrentMouseState.Left)
            {
                ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
            }
            else
            {
                BackActionOfList7();
            }
        }
        
        
        enum CurrentMouseState
        {
            Left,
            Right
        }

        private CurrentMouseState CurrentState = CurrentMouseState.Left;
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
  

        #region TextContent
        public struct PinchCustomRacePanelStruct
        {
            public TextTip[] Btns;
        }
        protected override void OnLoadJsonAssetComplete(PinchCustomRacePanelStruct datas)
        {
            InitBtnData(datas);
        }

        protected override void InitTextContentPathData()
        {
            this.abpath = "OCTextContent/PlayerUIPanel";
            this.abname = "PlayerUIPanel";
            this.description = "PinchRace";
        }
        private void InitBtnData(PinchCustomRacePanelStruct datas)
        {
            // foreach (var tt in datas.Btns)
            // {
            //     this.UIBtnList.SetBtnText(tt.name, tt.description.GetText());
            // }
        }
        #endregion

        #region CustomRace
        
        private PinchFaceManager pinchFaceManager;
        private UIBtnListContainer UIBtnListContainer;
        private const int OneRowCount = 5;
        private PinchPartType2 curType2 = PinchPartType2.None;
        private Dictionary<PinchPartType2, SelectedButton> leftButtonDic = new Dictionary<PinchPartType2, SelectedButton>();
        private RacePinchData raceData = new RacePinchData();
        private string pinchButtonPath = "Prefabs_PinchPart/UIPanel/Prefab_Pinch_BaseUISelectedBtn.prefab";
        
        public UICameraImage uICameraImage;
        private int IsInit = 0;
        private SelectedButton selectBtnTemplate;
        private Transform rightContainerTransf;
        private void SetLeftBtnText(SelectedButton _btn,string _str = "",bool isNake = false)
        {
            if (_str != "")
            {
                _btn.GetComponentInChildren<TextMeshProUGUI>().text = _str;    
            }
            
            Image _image = _btn.transform.Find("Image").GetComponent<Image>();
            if (isNake)
            {
                _image.color = Color.gray;
            }
            else
            {
                _image.color = new Color32(10,150,10,255);
            }
        }
        #endregion
    }
}