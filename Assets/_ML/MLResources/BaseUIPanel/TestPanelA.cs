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
    public class TestPanelA : ML.Engine.UI.UIBasePanel
    {
        #region Internal
        protected override void UnregisterInput()
        {
            this.UIBtnListContainer.DisableUIBtnListContainer();
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
