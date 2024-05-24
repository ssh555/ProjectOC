using System.Collections;
using System.Collections.Generic;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class UIPinchRacePanel : ML.Engine.UI.UIBasePanel<UIPinchRacePanel.PinchRacePanelStruct>
    {
        #region ML

        protected override void Awake()
        {
            base.Awake();
            raceNameText = transform.Find("RaceInfo/RaceText").GetComponent<TextMeshProUGUI>();
            raceDescription = transform.Find("RaceInfo/RaceDesciption").GetComponent<TextMeshProUGUI>();
            raceBtnTemplate = transform
                .Find("RaceButtonGroup/ScrollView/ButtonList/Container/Prefab_PinchRaceButtonTemplate")
                .GetComponent<SelectedButton>();
            pinchFaceManager = LocalGameManager.Instance.PinchFaceManager;
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
                    //随机生成皮肤
                    int _curPos = UIBtnListContainer.UIBtnLists[0].GetCurSelectedPos1();
                    if (_curPos != -1)
                    {
                        RacePinchData raceData = RacePinchDatas[_curPos];
                        // pinchFaceManager.ModelPinch.UnEquipAllItem();
                        foreach (var _type3 in raceData.pinchPartType3s)
                        {
                            pinchFaceManager.RandomPinchPart(_type3,true);
                        }
                    }
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
            for(int i = RacePinchDatas.Count-1;i >= 0;i--)
            {
                int tmpI = i;
                SelectedButton _btn =  Instantiate(raceBtnTemplate,raceBtnTemplate.transform.parent);
                _btn.gameObject.SetActive(true);
                _btn.name = $"Race_{RacePinchDatas[tmpI].raceName}";
                _btn.transform.GetComponentInChildren<TextMeshProUGUI>().text = RacePinchDatas[i].raceName;
                if (RacePinchDatas[tmpI].isDefault)
                {
                    _btn.transform.Find("NotDefaultImage").gameObject.SetActive(false);
                }
                _btn.onClick.AddListener(() =>
                {
                    ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
                    pinchFaceManager.GeneratePinchFaceUI(RacePinchDatas[tmpI]);
                });
            }
            
            
            UIBtnListContainer.UIBtnLists[0].InitBtnInfo();
            
            UIBtnListContainer.UIBtnLists[1].SetBtnAction(0,0,() =>
            {
                //创建种族
                ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
                LocalGameManager.Instance.PinchFaceManager.GenerateCustomRaceUI();
            });
            
 
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Enable();
            UIBtnListContainer.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
        }
        
        protected override void UnregisterInput()
        {
            base.UnregisterInput();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Disable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }
        
        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            Destroy(pinchFaceManager.ModelPinch.gameObject);
            ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
        }

        
        #endregion
        
        #region TextContent
        [System.Serializable]
        public struct PinchRacePanelStruct
        {
            public TextTip[] Btns;
        }
        protected override void OnLoadJsonAssetComplete(PinchRacePanelStruct datas)
        {
            InitBtnData(datas);
        }

        protected override void InitTextContentPathData()
        {
            this.abpath = "OCTextContent/PlayerUIPanel";
            this.abname = "PlayerUIPanel";
            this.description = "PinchRace";
        }
        
        protected override void InitBtnInfo()
        {
            this.UIBtnListContainer = new UIBtnListContainer(this.transform.GetComponentInChildren<UIBtnListContainerInitor>());
            foreach (var _btnList in UIBtnListContainer.UIBtnLists)
            {
                _btnList.BindButtonInteractInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.Confirm,UIBtnListContainer.BindType.started);
            }
            this.UIBtnListContainer.AddOnSelectButtonChangedAction(SelectButtonChangedAction);

            base.InitBtnInfo();
        }

        void SelectButtonChangedAction()
        {
            //右侧种族描述更新，中英文切换直接换RacePinchData
            int _curPos = UIBtnListContainer.UIBtnLists[0].GetCurSelectedPos1();
            if (_curPos == -1)
            {
                raceNameText.text = "";
                raceDescription.text = "";
            }
            else
            {
                RacePinchData raceData = RacePinchDatas[_curPos];
                raceNameText.SetText(raceData.raceName);
                raceDescription.SetText(raceData.raceDescription);
                //随机生成
                if (pinchFaceManager.ModelPinch != null)
                {
                    pinchFaceManager.ModelPinch.UnEquipAllItem();
                    foreach (var _type3 in raceData.pinchPartType3s)
                    {
                        pinchFaceManager.RandomPinchPart(_type3,true);
                    }
                }

            }
            
        }
        
        private void InitBtnData(PinchRacePanelStruct datas)
        {
            // foreach (var tt in datas.Btns)
            // {
            //     this.UIBtnList.SetBtnText(tt.name, tt.description.GetText());
            // }
        }
        
        #endregion


        #region PinchRacePanel

        private PinchFaceManager pinchFaceManager;
        [ShowInInspector]
        private UIBtnListContainer UIBtnListContainer;
        
        public List<RacePinchData> RacePinchDatas=>pinchFaceManager.RacePinchDatas;

        private TextMeshProUGUI raceNameText, raceDescription;
        private SelectedButton raceBtnTemplate;
        
        
        public UICameraImage uICameraImage;
        private int IsInit = 0;
    
        
        #endregion


    }   
}
