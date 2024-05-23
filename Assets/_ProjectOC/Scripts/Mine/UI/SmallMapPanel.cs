using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using ProjectOC.MineSystem;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SmallMapPanel;
public class SmallMapPanel : UIBasePanel<SmallMapPanelStruct>
{
    #region Unity
    protected override void Awake()
    {
        base.Awake();
        this.cursorNavigation = this.transform.Find("GraphCursorNavigation").GetComponent<GraphCursorNavigation>();
    }

    #endregion

    #region Override
    public override void OnEnter()
    {
        base.OnEnter();
        this.InitData();
        this.cursorNavigation.EnableGraphCursorNavigation(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, ML.Engine.Input.InputManager.Instance.Common.Common.LastTerm, ML.Engine.Input.InputManager.Instance.Common.Common.NextTerm);
        this.cursorNavigation.OnScaleChanged += RefreshOnZoomMap;
    }
    public override void OnExit()
    {
        base.OnExit();
        this.cursorNavigation.DisableGraphCursorNavigation();
        this.cursorNavigation.OnScaleChanged -= RefreshOnZoomMap;
        ClearTemp();
    }

    #endregion

    #region Internal
    private Dictionary<SelectedButton,string> BtnToMapRegionIdDic = new Dictionary<SelectedButton,string>();
    private void InitData()
    {
        
    }
    protected override void UnregisterInput()
    {
        ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;

        // 返回
        ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
    }

    protected override void RegisterInput()
    {
        ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;

        // 返回
        ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
    }

    private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
    }

    private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        UIMgr.PopPanel();
    }
    #endregion


    #region UI
    #region temp
    private void ClearTemp()
    {
/*        GameManager.DestroyObj(icon_genderfemaleSprite);
        GameManager.DestroyObj(icon_gendermaleSprite);*/
    }
    #endregion

    #region UI对象引用
    private MineSystemManager MM => LocalGameManager.Instance.MineSystemManager;
    [ShowInInspector]
    private GraphCursorNavigation cursorNavigation;

    private RectTransform MainIsland;
    private RectTransform Target;
    private Slider slider;

    private Transform BigMapInstanceTrans;
    private Transform NormalRegions;
    #endregion

    public override void Refresh()
    {
        if (!this.objectPool.IsLoadFinish())
        {
            return;
        }
    }

    private void RefreshOnZoomMap()
    {
        this.slider.value = this.cursorNavigation.CurZoomRate;
        MM.GridScale = this.cursorNavigation.CurZoomscale;
    }
    #endregion


    #region Resource
    #region TextContent
    [System.Serializable]
    public struct SmallMapPanelStruct
    {
        public KeyTip AcceptedOrderCancel;

    }
    protected override void InitTextContentPathData()
    {
        this.abpath = "OCTextContent/Order";
        this.abname = "OrderBoardPanel";
        this.description = "OrderBoardPanel数据加载完成";
    }
    #endregion
    protected override void InitBtnInfo()
    {
        this.cursorNavigation.InitUIBtnList();
    }
    #endregion
}
