using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInfiniteInventory : ML.Engine.UI.UIBasePanel
{

    #region Input
    /// <summary>
    /// ����Drop��Destroy������ӦCancel
    /// ������Ӧ��Destroy����Ϊtrue
    /// Cancel�Ͳ���ӦDrop �� ����
    /// </summary>
    private bool ItemIsDestroyed = false;
    #endregion

    #region Unity
    private void Awake()
    {
        this.enabled = false;
    }

    #endregion

    #region Override
    public override void OnEnter()
    {
        base.OnEnter();
        this.Enter();
    }

    public override void OnExit()
    {
        base.OnExit();
        this.Exit();
    }

    public override void OnPause()
    {
        base.OnPause();
        this.Exit();
    }

    public override void OnRecovery()
    {
        base.OnRecovery();
        this.Enter();
    }

    #endregion

    #region Internal
    public ML.Engine.InventorySystem.InfiniteInventory inventory;

    private void Enter()
    {
        this.RegisterInput();
        ProjectOC.Input.InputManager.PlayerInput.UIInventory.Enable();
        ML.Engine.Manager.GameManager.Instance.SetAllGameTimeRate(0);
    }

    private void Exit()
    {
        ProjectOC.Input.InputManager.PlayerInput.UIInventory.Disable();
        ML.Engine.Manager.GameManager.Instance.SetAllGameTimeRate(1);
        this.UnregisterInput();
    }

    private void UnregisterInput()
    {
        // �л���Ŀ
        ProjectOC.Input.InputManager.PlayerInput.UIInventory.LastTerm.performed -= LastTerm_performed;
        ProjectOC.Input.InputManager.PlayerInput.UIInventory.NextTerm.performed -= NextTerm_performed;
        // �л�Item
        ProjectOC.Input.InputManager.PlayerInput.UIInventory.AlterItem.performed -= AlterItem_performed;
        // ʹ��
        ML.Engine.Input.InputManager.Instance.Common.Common.Comfirm.performed -= Comfirm_performed;
        // ����
        ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        // ����
        ProjectOC.Input.InputManager.PlayerInput.UIInventory.DropAndDestroy.canceled -= DropAndDestroy_canceled;
        // ����
        ProjectOC.Input.InputManager.PlayerInput.UIInventory.DropAndDestroy.performed -= DropAndDestroy_performed;
    }

    private void RegisterInput()
    {
        // �л���Ŀ
        ProjectOC.Input.InputManager.PlayerInput.UIInventory.LastTerm.performed += LastTerm_performed;
        ProjectOC.Input.InputManager.PlayerInput.UIInventory.NextTerm.performed += NextTerm_performed;
        // �л�Item
        ProjectOC.Input.InputManager.PlayerInput.UIInventory.AlterItem.performed += AlterItem_performed;
        // ʹ��
        ML.Engine.Input.InputManager.Instance.Common.Common.Comfirm.performed += Comfirm_performed;
        // ����
        ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
        // ����
        ProjectOC.Input.InputManager.PlayerInput.UIInventory.DropAndDestroy.canceled += DropAndDestroy_canceled;
        // ����
        ProjectOC.Input.InputManager.PlayerInput.UIInventory.DropAndDestroy.performed += DropAndDestroy_performed;
    }

    private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        UIMgr.PopPanel();
    }

    private void Comfirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
    }

    private void AlterItem_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
    }

    private void LastTerm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
    }

    private void NextTerm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
    }

    private void DropAndDestroy_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(this.ItemIsDestroyed)
        {
            this.ItemIsDestroyed = false;
        }
        else
        {
        }
    }

    private void DropAndDestroy_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        this.ItemIsDestroyed = true;
    }

    #endregion

    #region UI

    #endregion

    #region TextContent

    #endregion
}
