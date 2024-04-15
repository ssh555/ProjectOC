using Cysharp.Threading.Tasks;
using ML.Engine.Manager;
using ML.Engine.SaveSystem;
using ML.Engine.TextContent;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static ML.Engine.UI.OptionPanel;
using static ML.Engine.UI.UIBtnListContainer;

namespace ML.Engine.UI
{
    public class TestPanelA : ML.Engine.UI.UIBasePanel
    {
        #region Override
        protected override void Awake()
        {
            base.Awake();
            this.ButtonList = transform.Find("ButtonList");
            this.DeleteBtnInputField = this.ButtonList.Find("DeleteBtn").Find("InputField").GetComponent<TMP_InputField>();
        }

        protected override void Enter()
        {
            this.TestBtnList.EnableBtnList();
            base.Enter();
        }

        protected override void Exit()
        {
            this.TestBtnList.DisableBtnList();
            base.Exit();
        }

        #endregion

        #region Internal
        protected override void UnregisterInput()
        {
            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;

            this.TestBtnList.RemoveAllListener();
            //解绑按钮导航和按钮确认InputAction的回调函数
            this.TestBtnList.DeBindInputAction();

            this.UIBtnListContainer.DisableUIBtnListContainer();
        }

        protected override void RegisterInput()
        {
            UIBtnListContainer.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, BindType.started);
            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;


            this.TestBtnList.SetBtnAction("AddBtn",
            () =>
            {
                this.UIBtnListContainer.AddBtnList("ML/BaseUIPanel/GridNavagation/TestButtonList.prefab", ML.Engine.Input.InputManager.Instance.Common.Common.Confirm,
                    BindType.started, new List<UnityAction>() { ()=> { Debug.Log("回调1"); }, () => { Debug.Log("回调2"); }, () => { Debug.Log("回调3"); } }
                    );
            });

            this.TestBtnList.SetBtnAction("DeleteBtn",
            () =>
            {
                int number1;
                if (!int.TryParse(DeleteBtnInputField.text, out number1))
                {
                    Debug.Log("请重新输入");
                    return;
                }
                this.UIBtnListContainer.DeleteBtnList(number1);
            });
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            GameManager.Instance.UIManager.PopPanel();
        }

        #endregion

        
        private Transform ButtonList;
        private UIBtnList TestBtnList;
        private TMP_InputField DeleteBtnInputField;
        [ShowInInspector]
        private UIBtnListContainer UIBtnListContainer;
        protected override void InitBtnInfo()
        {
            this.UIBtnListContainer = new UIBtnListContainer(this.transform.GetComponentInChildren<UIBtnListContainerInitor>());
            this.TestBtnList = new UIBtnList(ButtonList.GetComponentInChildren<UIBtnListInitor>());

            this.UIBtnListContainer.AddOnSelectButtonChangedAction(() => { Debug.Log("SelectButtonChanged!"); });
            this.UIBtnListContainer.AddOnSelectButtonListChangedAction(() => { Debug.Log("SelectButtonListChanged!"); });
        }

    }
}