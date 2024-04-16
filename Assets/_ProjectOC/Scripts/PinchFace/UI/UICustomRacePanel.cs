using System.Collections;
using System.Collections.Generic;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class UICustomRacePanel : ML.Engine.UI.UIBasePanel<UICustomRacePanel.PinchCustomRacePanelStruct>
    {
        #region Unity

        protected override void Awake()
        {
            base.Awake();
            pinchFaceManager = LocalGameManager.Instance.PinchFaceManager;

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
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Disable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }
        
        protected override void InitBtnInfo()
        {
            this.UIBtnListContainer = new UIBtnListContainer(this.transform.GetComponentInChildren<UIBtnListContainerInitor>());
            foreach (var _pinchFaceType2 in pinchFaceManager.pinchPartType2Dic)
            {
                this.UIBtnListContainer.AddBtn((int)_pinchFaceType2.Value -1,"Assets/_ML/MLResources/BaseUIPrefab/BaseUISelectedBtn.prefab"
                    ,BtnText: _pinchFaceType2.Key.ToString()
                    ,BtnAction:() =>
                    {
                        Debug.Log("aa");
                    });
            }
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
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
            this.abpath = "OC/Json/TextContent/PlayerUIPanel";
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

        #endregion
    }
}