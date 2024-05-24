using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.Timer;
using ML.Engine.UI;
using ML.Engine.Utility;
using Newtonsoft.Json;
using ProjectOC.ManagerNS;
using ProjectOC.MineSystem;
using ProjectOC.MissionNS;
using ProjectOC.Order;
using ProjectOC.Player;
using ProjectOC.TechTree;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.U2D;
using UnityEngine.UI;
using static IslandRudderPanel;
using static ML.Engine.UI.UIBtnList;
using static ML.Engine.UI.UIBtnListContainer;
using static OrderBoardPanel;
using static ProjectOC.MineSystem.MineSystemData;
using static ProjectOC.Order.OrderManager;
using static UnityEditor.PlayerSettings;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class IslandRudderPanel : UIBasePanel<IslandRudderPanelStruct>
{
    #region Unity
    protected override void Awake()
    {
        base.Awake();
        this.cursorNavigation = this.transform.Find("GraphCursorNavigation").GetComponent<GraphCursorNavigation>();
        var Content = this.cursorNavigation.transform.Find("Scroll View").Find("Viewport").Find("Content");
        this.MainIsland = Content.Find("MainIsland").GetComponent<RectTransform>();
        this.Target = Content.Find("Target").GetComponent<RectTransform>();
        this.slider = this.transform.Find("MapLayer").Find("Slider").GetComponent<Slider>();
        
    }

    #endregion

    #region Override

    public override void OnEnter()
    {
        base.OnEnter();
        MM.RefreshUI += this.RefreshMainIsland;
        this.InitData();
        this.cursorNavigation.EnableGraphCursorNavigation(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, ML.Engine.Input.InputManager.Instance.Common.Common.LastTerm, ML.Engine.Input.InputManager.Instance.Common.Common.NextTerm);
        this.cursorNavigation.OnScaleChanged += RefreshOnZoomMap;
        this.cursorNavigation.ChangeRaycastCenter(this.MainIsland);
        this.cursorNavigation.UIBtnList.OnSelectButtonChanged += () =>
        {

        };
    }
    public override void OnExit()
    {
        base.OnExit();
        MM.RefreshUI -= this.RefreshMainIsland;
        this.cursorNavigation.DisableGraphCursorNavigation();
        this.cursorNavigation.OnScaleChanged -= RefreshOnZoomMap;
    }

    protected override void Exit()
    {
        base.Exit();
        ClearTemp();
    }

    #endregion

    #region Internal
    private Dictionary<SelectedButton,string> BtnToMapRegionIdDic = new Dictionary<SelectedButton,string>();
    private void InitData()
    {


        GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_Mine_UIPrefab/Prefab_MineSystem_UI_BigMap.prefab").Completed += (handle) =>
        {
            BigMapInstanceTrans = handle.Result.transform;
            NormalRegions = BigMapInstanceTrans.Find("NormalRegion");
            BigMapInstanceTrans.SetParent(this.cursorNavigation.Content.Find("BigMap"));
            BigMapInstanceTrans.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            referenceRectTransform = handle.Result.transform.Find("NormalRegion").transform as RectTransform;
            this.cursorNavigation.CurZoomscale = MM.GridScale;
            RefreshOnZoomMap();
            this.Refresh();
        };
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
        if(!MM.SetNewNavagatePoint(this.cursorNavigation.CenterPos))
        {
            GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.PopUpUI, new UIManager.PopUpUIData("已有导航点，是否重新设置？", null, null, () => {
                MM.SetNewNavagatePoint(this.cursorNavigation.CenterPos, true);
            }));
        }
          
    }

    private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        UIMgr.PopPanel();
    }
    #endregion

    #region 主岛位置检测
    private RectTransform referenceRectTransform;
    private int PreRegion = -1;
    private int CurRegion = -1;

    private void DetectMainIslandCurRegion()
    {
        Vector3 worldPosition = MainIsland.transform.position;
        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(referenceRectTransform, worldPosition, null, out localPosition);
        Vector2 referenceSize = referenceRectTransform.rect.size;
        Vector2 anchorPosition = new Vector2(localPosition.x / referenceSize.x + 0.5f, localPosition.y / referenceSize.y + 0.5f);
        anchorPosition = new Vector2(anchorPosition.x, 1 - anchorPosition.y);
        int width = MM.BigMapTableData.GetLength(0);
        Vector2Int gridPos = new Vector2Int(
        Mathf.Clamp((int)(anchorPosition.x * (width)), 0, width - 1),
        Mathf.Clamp((int)(anchorPosition.y * (width)), 0, width - 1));
        CurRegion = MM.BigMapTableData[gridPos.y, gridPos.x];
        if (PreRegion != CurRegion)
        {
            //进入新区域
            MM.UnlockMapRegion(CurRegion);
            PreRegion = CurRegion;
            this.Refresh();
        }
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

        #region 更新迷雾
        for (int i = 0;i< NormalRegions.childCount;i++)
        {
            var isUnlocked = MM.CheckRegionIsUnlocked(i + 1);
            Debug.Log(isUnlocked);
            NormalRegions.GetChild(i).Find("Normal").gameObject.SetActive(isUnlocked);
            NormalRegions.GetChild(i).Find("Locked").gameObject.SetActive(!isUnlocked);
        }
        #endregion

    }

    private void RefreshMainIsland()
    {
        this.MainIsland.anchoredPosition = MM.MainIslandData.CurPos;
        this.Target.anchoredPosition = MM.MainIslandData.TargetPos;
        DetectMainIslandCurRegion();
    }

    private void RefreshOnZoomMap()
    {
        this.slider.value = this.cursorNavigation.curZoomRate;
        MM.GridScale = this.cursorNavigation.CurZoomscale;
    }
    #endregion


    #region Resource

    #region TextContent
    [System.Serializable]
    public struct IslandRudderPanelStruct
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
