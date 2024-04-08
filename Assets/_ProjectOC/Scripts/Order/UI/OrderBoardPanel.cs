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
        this.Function = transform.Find("FunctionType").Find("Content").Find("Function");
        this.FunctionPanel = transform.Find("FunctionPanel");
        this.FunctionType = this.Function.transform.childCount;

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
        ProjectOC.Input.InputManager.PlayerInput.OrderBoardPanel.Disable();

        // 切换类目
        ProjectOC.Input.InputManager.PlayerInput.OrderBoardPanel.LastTerm.performed -= LastTerm_performed;
        ProjectOC.Input.InputManager.PlayerInput.OrderBoardPanel.NextTerm.performed -= NextTerm_performed;

        // 返回
        ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
    }

    protected override void RegisterInput()
    {
        ProjectOC.Input.InputManager.PlayerInput.OrderBoardPanel.Enable();

        // 切换类目
        ProjectOC.Input.InputManager.PlayerInput.OrderBoardPanel.LastTerm.performed += LastTerm_performed;
        ProjectOC.Input.InputManager.PlayerInput.OrderBoardPanel.NextTerm.performed += NextTerm_performed;

        // 返回
        ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

    }

    private void LastTerm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        FunctionIndex = (FunctionIndex + FunctionType - 1) % FunctionType;
        Debug.Log("LastTerm_performed " + FunctionType+" "+ FunctionIndex);
        this.Refresh();
    }

    private void NextTerm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        FunctionIndex = (FunctionIndex + 1) % FunctionType;
        Debug.Log("NextTerm_performed " + FunctionType + " " + FunctionIndex);
        this.Refresh();
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
    private int FunctionType;
    private Transform Function;
    private Transform FunctionPanel;

    #endregion

    public override void Refresh()
    {

        if (!this.objectPool.IsLoadFinish())
        {
            return;
        }
        Debug.Log("sdfs");
        #region FunctionType

        for (int i = 0; i < FunctionType; i++)
        {
            if(FunctionIndex == i)
            {
                Function.GetChild(i).Find("Selected").gameObject.SetActive(true);
                FunctionPanel.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                Function.GetChild(i).Find("Selected").gameObject.SetActive(false);
                FunctionPanel.GetChild(i).gameObject.SetActive(false);
            }
            
        }

        #endregion


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
        this.abpath = "OC/Json/TextContent/Order";
        this.abname = "OrderBoardPanel";
        this.description = "OrderBoardPanel数据加载完成";
    }
    #endregion

    protected override void InitObjectPool()
    {
        
        base.InitObjectPool();
    }

    private UIBtnList ClanBtnList;
    protected override void InitBtnInfo()
    {
        UIBtnListInitor ClanBtnListInitor = FunctionPanel.GetChild(1).GetComponentInChildren<UIBtnListInitor>();
        ClanBtnList = new UIBtnList(ClanBtnListInitor);
    }

    #endregion
}
