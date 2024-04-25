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
            
            uICameraImage = transform.Find("UICameraImage").GetComponentInChildren<UICameraImage>();
            uICameraImage.Init();
            GameObject characterModel = null;
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("Prefabs_PinchPart/PinchPart/Model_AnMiXiu.prefab").Completed+=(handle) =>
            {
                characterModel = handle.Result;
                //uICameraImage.LookAtGameObject(characterModel); //自动移动位置
                pinchFaceManager.ModelPinch = characterModel.GetComponentInChildren<CharacterModelPinch>();
            };
            
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<PinchDataConfig>("PinchAsset_PinchFaceSetting/PinchDataConfig.asset").Completed+=(handle) =>
            {
                Config = handle.Result;
            };

            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<SpriteAtlas>("SA_UI_PinchFace/SA_PinchFace.spriteatlasv2").Completed+=(handle) =>
            {
                SA_PinchPart = handle.Result;
            };
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
            UIBtnListContainer.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);
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
            //如果当前在btnList[7]
            if (CurrentState == CurrentMouseState.Left)
            {
                ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
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
            this.UIBtnListContainer =
                new UIBtnListContainer(this.transform.GetComponentInChildren<UIBtnListContainerInitor>());
            foreach (var _btnList in UIBtnListContainer.UIBtnLists)
            {
                _btnList.BindButtonInteractInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.Confirm,UIBtnListContainer.BindType.started);
            }
        }
        
        //返回右侧BtnList，重新生成
        public void ReGenerateBtnListContainer(List<UIBtnListInitor> _btnLists)
        {
            UIBtnListContainer.DisableUIBtnListContainer();
            rightBtnLists = _btnLists;
            UIBtnListContainerInitor newBtnListContainers = this.transform.GetComponentInChildren<UIBtnListContainerInitor>();
            
            //从4连到倒数第一个
            //最后一个不需要加连接线
            for (int i = 0; i < _btnLists.Count-1; i++)
            {
                // _listContainer.LinkTwoEdge();
                UIBtnListContainerInitor.LinkData _linkData = new UIBtnListContainerInitor
                    .LinkData(i+4,i+5,UIBtnListContainerInitor.EdgeType.下侧顺时针,UIBtnListContainerInitor.EdgeType.上侧逆时针,UIBtnListContainerInitor.LinkType.上下相连);
                newBtnListContainers.btnListContainerInitData.AddLinkData(_linkData);
            }
            this.UIBtnListContainer = new UIBtnListContainer(newBtnListContainers);
            UIBtnListContainer.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);

            foreach (var _btnList in UIBtnListContainer.UIBtnLists)
            {
                _btnList.BindButtonInteractInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.Confirm,UIBtnListContainer.BindType.started);
            }
            
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
        [ShowInInspector]
        private UIBtnListContainer UIBtnListContainer;
        public Transform containerTransf;
        private RacePinchData raceData;
        private List<PinchPart> pinchParts = new List<PinchPart>();
        private Dictionary<PinchPartType3, SelectedButton> type3ButtonDic = new Dictionary<PinchPartType3, SelectedButton>();
        private Dictionary<PinchPartType3, PinchPartType2> type3Type2Dic => pinchFaceManager.pinchPartType3Dic;
        public List<UIBtnListInitor> rightBtnLists = new List<UIBtnListInitor>();
        
        public PinchDataConfig Config;
        public SpriteAtlas SA_PinchPart;

        public UICameraImage uICameraImage;
        enum CurrentMouseState
        {
            Left,
            Right
        }

        private CurrentMouseState CurrentState = CurrentMouseState.Left; 
        
        public void InitRaceData(RacePinchData _racePinchData)
        {
            raceData = _racePinchData;
            //体型、头发、眼睛、面妆
            int _pinchPartTypeCount = _racePinchData.pinchPartType3s.Count + 4;
            for (int i = 0; i < _pinchPartTypeCount; i++)
            {
                pinchParts.Add(null);
            }
            
            
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
        }

        private void LeftButton_BtnAction(int _index)
        {
            //删除所有ChangeSetting
            foreach (Transform _container in containerTransf)
            {
                 _container.gameObject.SetActive(false);
                 Destroy(_container.gameObject);
            }
            GenerateChangeSetting(_index);
        }

        private void GenerateChangeSetting(int _index)
        {
            PinchPartType3 _type3 = raceData.pinchPartType3s[_index];
            //根据Type寻找对应文件，根据上面的Component生成Prefab
            PinchPartType2 _type2 = type3Type2Dic[_type3];
            
            string pathFore = "PinchAsset_PinchFaceSetting/PinchTypeConfig";
            string templatePath = $"{pathFore}/{(int)_type2-1}_{_type2.ToString()}_PinchType2Template.prefab";

            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(templatePath)
                .Completed += (handle) =>
            {
                var _comps = handle.Result.GetComponents<IPinchSettingComp>();
                pinchParts[_index] = new PinchPart(this, _type3, _type2, _comps, containerTransf);
                Destroy(handle.Result);
            };
            //加载Type3，是否有com
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