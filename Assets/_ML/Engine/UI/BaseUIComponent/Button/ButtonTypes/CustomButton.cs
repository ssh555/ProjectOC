using ML.Engine.Timer;
using ML.Engine.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ML.Engine.UI
{
    [System.Serializable]
    /// <summary>
    /// 该脚本为自定义按钮 通常需求为选中自身时有自己独有的输入去更新按钮 
    /// </summary>
    public class CustomButton : SelectedButton
    {
        private InputAction BindInputAction = null;
        #region 面板配置项
        [LabelText("是否为长按输入")]
        public bool isHold;
        #endregion
        [ShowInInspector]
        private Action<InputAction.CallbackContext> BindRefreshBtnAction;
        [ShowInInspector]
        private event Action<InputAction.CallbackContext> RefreshBtnAction;

        /// <summary>
        /// 在该按钮初始化时调用 初始化需要绑定的InputAction 以及响应该InputAction的回调函数
        /// </summary>
        public void InitBindData(InputAction inputAction, Action<InputAction.CallbackContext> OnInputActionPerformed)
        {
            this.BindInputAction = inputAction;
            this.BindRefreshBtnAction = OnInputActionPerformed;
        }

        public void BindInput()
        {
            if (isHold)
            {
                this.BindInputAction.started += InputAction_started;
                this.BindInputAction.canceled += InputAction_canceled;
            }
            else
            {
                this.BindInputAction.performed += InputAction_performed;
            }
            RefreshBtnAction += BindRefreshBtnAction;
        }

        public void DeBindInput()
        {
            if (isHold)
            {
                this.BindInputAction.started -= InputAction_started;
                this.BindInputAction.canceled -= InputAction_canceled;
                if(timer != null)
                {
                    //确保timer被释放
                    ML.Engine.Manager.GameManager.Instance.CounterDownTimerManager.RemoveTimer(timer);
                    timer = null;
                }
            }
            else
            {
                this.BindInputAction.performed -= InputAction_performed;
            }
            RefreshBtnAction -= BindRefreshBtnAction;
        }

        private float TimeInterval = 0.2f;
        private CounterDownTimer timer = null;
        private void InputAction_started(InputAction.CallbackContext obj)
        {
            if (timer == null)
            {
                timer = new CounterDownTimer(TimeInterval, true, true, 1, 2);
                timer.OnEndEvent += () =>
                {
                    RefreshBtnAction?.Invoke(obj);
                };
            }
        }
        private void InputAction_performed(InputAction.CallbackContext obj)
        {
            RefreshBtnAction?.Invoke(obj);
        }

        private void InputAction_canceled(InputAction.CallbackContext obj)
        {
            ML.Engine.Manager.GameManager.Instance.CounterDownTimerManager.RemoveTimer(timer);
            timer = null;
        }
        public override void OnSelect(BaseEventData eventData)
        {
            BindInput();
            base.OnSelect(eventData);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            DeBindInput();
            base.OnDeselect(eventData);
        }
    }
}


#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(CustomButton))]
public class PanelButtonEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        // 绘制基类的 Inspector 面板
        base.OnInspectorGUI();
    }
}

#endif