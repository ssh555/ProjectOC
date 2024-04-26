using Cysharp.Threading.Tasks;
using ML.Engine.Manager;
using ML.Engine.SaveSystem;
using ML.Engine.TextContent;
using ML.Engine.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ML.Engine.UI.UIBtnListContainer;

namespace ProjectOC.MainInteract.UI
{
    public class ControllerPanel : ML.Engine.UI.UIBasePanel
    {
        #region Override
        protected override void Awake()
        {
            base.Awake();
            this.ButtonList = transform.Find("ButtonList");
        }

        #endregion

        #region Internal
        protected override void UnregisterInput()
        {
            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
            this.UIBtnListContainer.DisableUIBtnListContainer();
        }

        protected override void RegisterInput()
        {
            UIBtnListContainer.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, BindType.started);
            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

            this.UIBtnListContainer.UIBtnLists[0].BindButtonInteractInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, BindType.started, 
                inputCondition: (obj) =>
                {
                    var vector2 = obj.ReadValue<UnityEngine.Vector2>();
                    float angle = Mathf.Atan2(vector2.x, vector2.y);
                    angle = angle * 180 / Mathf.PI;
                    if (angle < 0)
                    {
                        angle = angle + 360;
                    }

                    if (angle < 45 || angle > 315)
                    {
                        return false;
                    }
                    else if (angle > 135 && angle < 225)
                    {
                        return false;
                    }
                    return true;
                });

            this.UIBtnListContainer.UIBtnLists[1].BindButtonInteractInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.Confirm, BindType.started);
            this.UIBtnListContainer.UIBtnLists[2].BindButtonInteractInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.Confirm, BindType.started);

        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            GameManager.Instance.UIManager.PopPanel();
        }

        #endregion
        [ShowInInspector]
        private UIBtnListContainer UIBtnListContainer;

        private Transform ButtonList;
        protected override void InitBtnInfo()
        {
            this.UIBtnListContainer = new UIBtnListContainer(this.transform.GetComponentInChildren<UIBtnListContainerInitor>());


            this.UIBtnListContainer.AddOnSelectButtonChangedAction(() => { Debug.Log("SelectButtonChanged!"); });
            this.UIBtnListContainer.AddOnSelectButtonListChangedAction(() => { Debug.Log("SelectButtonListChanged!"); });

            this.UIBtnListContainer.UIBtnLists[0].SetAllBtnAction(() => { 
                GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.FloatTextUI, new UIManager.FloatTextUIData("切换开关！"));
                GameManager.Instance.EventManager.ExecuteFunction("InteractUpgrade(asd,5)");
            });
            this.UIBtnListContainer.UIBtnLists[1].SetAllBtnAction(() => { 
                GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.FloatTextUI, new UIManager.FloatTextUIData("确认改键！"));
                GameManager.Instance.EventManager.ExecuteFunction("Attack(true)");
            });
            this.UIBtnListContainer.UIBtnLists[2].SetAllBtnAction(() => { 
                GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.FloatTextUI, new UIManager.FloatTextUIData("确认改键！"));
                GameManager.Instance.EventManager.ExecuteFunction("UseItem(8)");
            });

        }
    }

}
