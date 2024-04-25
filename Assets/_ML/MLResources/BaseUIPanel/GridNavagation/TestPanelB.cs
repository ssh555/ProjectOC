using Cysharp.Threading.Tasks;
using ML.Engine.Manager;
using ML.Engine.SaveSystem;
using ML.Engine.TextContent;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ML.Engine.UI.UIBtnListContainer;
using static ML.Engine.UI.UIBtnListContainerInitor;

namespace ML.Engine.UI
{
    public class TestPanelB : ML.Engine.UI.UIBasePanel
    {
        #region Override
        protected override void Awake()
        {
            base.Awake();
            this.ButtonList = transform.Find("ButtonList");
            this.AddBtnInputField = this.ButtonList.Find("AddBtn").Find("InputField").GetComponent<TMP_InputField>();
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
                int number1;
                if (!int.TryParse(AddBtnInputField.text, out number1))
                {
                    Debug.Log("请重新输入");
                    return;
                }

                this.UIBtnListContainer.AddBtn(number1, "ML/BaseUIPrefab/Prefab_BaseUISelectedBtn.prefab");
                
            });

            this.TestBtnList.SetBtnAction("AddBtnList",
            () =>
            {
                LinkData linkData = new LinkData(this.UIBtnListContainer.UIBtnListNum-1, this.UIBtnListContainer.UIBtnListNum,EdgeType.下侧顺时针,EdgeType.上侧逆时针,LinkType.上下相连);

                this.UIBtnListContainer.AddBtnListBType("ML/BaseUIPanel/GridNavagation/TestButtonListB.prefab", linkData);

            });

            this.TestBtnList.SetBtnAction("DeleteBtn",
            () =>
            {
                string[] numbers = DeleteBtnInputField.text.Split(',');
                int number1, number2;
                if (numbers.Length == 2)
                {
                    if (!int.TryParse(numbers[0], out number1))
                    {
                        Debug.Log("请重新输入");
                        return;
                    }

                    if (!int.TryParse(numbers[1], out number2))
                    {
                        Debug.Log("请重新输入");
                        return;
                    }
                }
                else
                {
                    Debug.Log("请重新输入");
                    return;
                }

                
                this.UIBtnListContainer.DeleteBtn(number1, number2);

            });
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            GameManager.Instance.UIManager.PopPanel();
        }

        #endregion
        [ShowInInspector]
        private UIBtnListContainer UIBtnListContainer;
        private Transform ButtonList;
        private UIBtnList TestBtnList;
        private TMP_InputField AddBtnInputField, DeleteBtnInputField;
        protected override void InitBtnInfo()
        {
            this.UIBtnListContainer = new UIBtnListContainer(this.transform.GetComponentInChildren<UIBtnListContainerInitor>());
            this.TestBtnList = new UIBtnList(ButtonList.GetComponentInChildren<UIBtnListInitor>());

            this.UIBtnListContainer.AddOnSelectButtonChangedAction(() => { Debug.Log("SelectButtonChanged!"); });
            this.UIBtnListContainer.AddOnSelectButtonListChangedAction(() => { Debug.Log("SelectButtonListChanged!"); });
        }
    }

}
