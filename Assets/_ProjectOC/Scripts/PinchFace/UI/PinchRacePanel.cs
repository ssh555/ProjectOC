using System.Collections;
using System.Collections.Generic;
using ML.Engine.TextContent;
using ML.Engine.UI;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class PinchRacePanel : ML.Engine.UI.UIBasePanel<PinchRacePanel.PinchRacePanelStruct>
    {
        #region ML
        protected override void Enter()
        {
            this.UIBtnList.EnableBtnList();
            base.Enter();
        }

        protected override void Exit()
        {
            this.UIBtnList.DisableBtnList();
            base.Exit();
        }
        #endregion

        #region Internal
        protected override void RegisterInput()
        {
            //创建种族
            //进入种族(int _index)
            
            //最后一个是创建
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Enable();
            this.UIBtnList.BindNavigationInputAction(ProjectOC.Input.InputManager.PlayerInput.PlayerUI.AlterSelected, UIBtnListContainer.BindType.started);
            this.UIBtnList.BindButtonInteractInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.Confirm, UIBtnListContainer.BindType.started);
            
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
        }
        
        protected override void UnregisterInput()
        {
            this.UIBtnList.RemoveAllListener();

            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Disable();

            this.UIBtnList.DeBindInputAction();

            // 杩斿洖
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }
        
        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
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
            this.abpath = "OC/Json/TextContent/PlayerUIPanel";
            this.abname = "PlayerUIPanel";
            this.description = "PlayerUIPanel鏁版嵁鍔犺浇瀹屾垚";
        }

        private UIBtnList UIBtnList;
        protected override void InitBtnInfo()
        {
            UIBtnListInitor uIBtnListInitor = this.transform.GetComponentInChildren<UIBtnListInitor>(true);
            this.UIBtnList = new UIBtnList(uIBtnListInitor);
        }
        private void InitBtnData(PinchRacePanelStruct datas)
        {
            foreach (var tt in datas.Btns)
            {
                this.UIBtnList.SetBtnText(tt.name, tt.description.GetText());
            }
        }
        
        #endregion

        #region PinchRacePanel

        

        #endregion
    }   
}
