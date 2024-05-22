using ML.Engine.Timer;
using ML.Engine.UI;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// �Ǳ��̰�ť
/// </summary>
public class PanelButton : SelectedButton
{
    private InputAction BindInputAction = null;
    #region ���������
    [LabelText("�����Ƿ��ǳ���")]
    public bool isHold = false;
    #endregion

    /// <summary>
    /// �ڸð�ť��ʼ��ʱ���� ��ʼ����Ҫ�󶨵�InputAction
    /// </summary>
    public void SetBindInput(InputAction inputAction)
    {
        this.BindInputAction = inputAction;
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
    }

    public void DeBindInput()
    {
        if (isHold)
        {
            this.BindInputAction.started -= InputAction_started;
            this.BindInputAction.canceled -= InputAction_canceled;
        }
        else
        {
            this.BindInputAction.performed -= InputAction_performed;
        }

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
                RefreshBtn(obj);
            };
        }
    }
    private void InputAction_performed(InputAction.CallbackContext obj)
    {
        RefreshBtn(obj);
    }

    private void InputAction_canceled(InputAction.CallbackContext obj)
    {
        ML.Engine.Manager.GameManager.Instance.CounterDownTimerManager.RemoveTimer(timer);
        timer = null;
    }

    /// <summary>
    /// �ú���Ϊ inputaction ����Ӧ����
    /// </summary>
    /// <param name="obj"></param>
    private void RefreshBtn(InputAction.CallbackContext obj)
    {

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
