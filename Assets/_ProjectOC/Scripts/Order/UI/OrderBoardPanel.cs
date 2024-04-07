using ML.Engine.Manager;
using ML.Engine.Timer;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using ProjectOC.MissionNS;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;
using UnityEngine.UI;
using static OrderBoardPanel;

public class OrderBoardPanel : UIBasePanel<OrderBoardPanelStruct>
{
    #region Unity
    protected override void Awake()
    {
        base.Awake();
        this.Function = transform.Find("FunctionType").Find("Function");
        this.FunctionPanel = transform.Find("FunctionPanel");


    }

    #endregion

    #region Override

    protected override void Exit()
    {
        base.Exit();
        ClearTemp();
    }

    #endregion

    #region Internal
    protected override void UnregisterInput()
    {
        ProjectOC.Input.InputManager.PlayerInput.BeastPanel.Disable();

        

        // 返回
        ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
    }

    protected override void RegisterInput()
    {
        ProjectOC.Input.InputManager.PlayerInput.BeastPanel.Enable();

        

        // 返回
        ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
    }
    private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        UIMgr.PopPanel();
    }
    #endregion


    #region UI
    #region temp
    /*    private Sprite icon_genderfemaleSprite, icon_gendermaleSprite;*/

    private void ClearTemp()
    {
/*        GameManager.DestroyObj(icon_genderfemaleSprite);
        GameManager.DestroyObj(icon_gendermaleSprite);*/
    }

    #endregion

    #region UI对象引用
    private int FunctionIndex = 1;
    private Transform Function;
    private Transform FunctionPanel;

    #endregion

    public override void Refresh()
    {

        if (this.objectPool.IsLoadFinish())
        {
            return;
        }

        
    }
    #endregion


    #region Resource

    #region TextContent
    [System.Serializable]
    public struct OrderBoardPanelStruct
    {
        
    }
    protected override void InitTextContentPathData()
    {
        this.abpath = "OC/Json/TextContent/ResonanceWheel";
        this.abname = "OrderBoardPanel";
        this.description = "OrderBoardPanel数据加载完成";
    }
    #endregion

    protected override void InitObjectPool()
    {
        
        base.InitObjectPool();
    }

    #endregion
}
