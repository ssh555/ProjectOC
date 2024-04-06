using Cysharp.Threading.Tasks;
using ML.Engine.Manager;
using ML.Engine.SaveSystem;
using ML.Engine.TextContent;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ML.Engine.UI.OptionPanel;
using static ML.Engine.UI.UIBtnListContainer;

namespace ML.Engine.UI
{
    public class TestPanel1 : ML.Engine.UI.UIBasePanel
    {
        private Transform BtnList1, BtnList2, BtnList3;
        private List<Transform> transforms = new List<Transform>();
        
        protected override void Awake()
        {
            base.Awake();
            

            
        }


        protected override void Exit()
        {
            this.UIBtnListContainer.DisableUIBtnListContainer();
            base.Exit();
        }

        #region Internal
        protected override void UnregisterInput()
        {
            // ·µ»Ø
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }

        protected override void RegisterInput()
        {
            UIBtnListContainer.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, BindType.started);
            // ·µ»Ø
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            GameManager.Instance.UIManager.PopPanel();
        }

        #endregion

        private UIBtnListContainer UIBtnListContainer;
        protected override void InitBtnInfo()
        {
            UIBtnListContainerInitor uIBtnListContainerInitor = this.transform.GetComponentInChildren<UIBtnListContainerInitor>();
            this.UIBtnListContainer = new UIBtnListContainer(uIBtnListContainerInitor.transform, uIBtnListContainerInitor.btnListContainerInitData);
        }




    }

}
