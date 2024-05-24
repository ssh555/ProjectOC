using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.InventorySystem.UI;
using ProjectOC.ManagerNS;
using ProjectOC.MineSystem;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ProjectOC.MineSystem.MineSystemData;
using static SelectMineralSourcesPanel;
public class SelectMineralSourcesPanel : UIBasePanel<SelectMineralSourcesPanelStruct>
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
        this.slider.onValueChanged.AddListener((value) => { this.cursorNavigation.CurZoomRate = value; });
    }

    #endregion

    #region Override
    public override void OnEnter()
    {
        base.OnEnter();
        this.InitData();
        this.cursorNavigation.OnScaleChanged += RefreshOnZoomMap;
        this.cursorNavigation.OnCenterPosChanged += DetectMainIslandCurRegion;
    }
    public override void OnExit()
    {
        base.OnExit();
        this.cursorNavigation.OnScaleChanged -= RefreshOnZoomMap;
        this.cursorNavigation.OnCenterPosChanged -= DetectMainIslandCurRegion;
        ClearTemp();
    }                                                           

    protected override void Exit()
    {
        base.Exit();
        this.MapLayerUIBtnList.DisableBtnList();
        this.cursorNavigation.DisableGraphCursorNavigation();
    }

    protected override void Enter()
    {
        base.Enter();
        this.MapLayerUIBtnList.EnableBtnList();
        this.cursorNavigation.EnableGraphCursorNavigation(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, ML.Engine.Input.InputManager.Instance.Common.Common.LastTerm, ML.Engine.Input.InputManager.Instance.Common.Common.NextTerm);
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

        //初始化主岛的位置
        this.MainIsland.anchoredPosition = MM.MainIslandData.CurPos;
    }
    protected override void UnregisterInput()
    {
        ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;

        // 返回
        ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        this.MapLayerUIBtnList.RemoveAllListener();
        this.MapLayerUIBtnList.DeBindInputAction();
    }

    protected override void RegisterInput()
    {
        ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;

        // 返回
        ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
        this.MapLayerUIBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.StartMenu.SwichBtn, UIBtnListContainer.BindType.started);
    }

    private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(MM.MineralMapData != null)                                               
        {
            GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_Mine_UIPanel/Prefab_Mine_UI_SmallMapPanel.prefab").Completed += (handle) =>
            {
                var panel = handle.Result.GetComponent<SmallMapPanel>();
                panel.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false);
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
            };
        }
    }

    private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        UIMgr.PopPanel();
    }
    #endregion

    #region 光标位置检测
    private RectTransform referenceRectTransform;
    private int PreSelectRegion = -1;
    [ShowInInspector]
    private int CurSelectRegion = -1;

    private void DetectMainIslandCurRegion()
    {
        Vector3 worldPosition = this.cursorNavigation.Center.position;
        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(referenceRectTransform, worldPosition, null, out localPosition);
        Vector2 referenceSize = referenceRectTransform.rect.size;
        Vector2 anchorPosition = new Vector2(localPosition.x / referenceSize.x + 0.5f, localPosition.y / referenceSize.y + 0.5f);
        anchorPosition = new Vector2(anchorPosition.x, 1 - anchorPosition.y);
        int width = MM.BigMapTableData.GetLength(0);
        Vector2Int gridPos = new Vector2Int(
        Mathf.Clamp((int)(anchorPosition.x * (width)), 0, width - 1),
        Mathf.Clamp((int)(anchorPosition.y * (width)), 0, width - 1));
        CurSelectRegion = MM.BigMapTableData[gridPos.y, gridPos.x];
        if (PreSelectRegion != CurSelectRegion)
        {
            PreSelectRegion = CurSelectRegion;
            MM.ChangeCurMineralMapData(CurSelectRegion);
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
            NormalRegions.GetChild(i).Find("Normal").gameObject.SetActive(isUnlocked);
            NormalRegions.GetChild(i).Find("Locked").gameObject.SetActive(!isUnlocked);
            NormalRegions.GetChild(i).Find("Selected").gameObject.SetActive(isUnlocked && i + 1 == CurSelectRegion);
        }
        #endregion
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
    public struct SelectMineralSourcesPanelStruct
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

    private UIBtnList MapLayerUIBtnList = null;
    protected override void InitBtnInfo()
    {
        this.cursorNavigation.InitUIBtnList();
        MapLayerUIBtnList = new UIBtnList(this.transform.Find("MapLayer").Find("ButtonList").GetComponent<UIBtnListInitor>());
        MapLayerUIBtnList.OnSelectButtonChanged += () =>
        {
            MM.CurMapLayerIndex = MapLayerUIBtnList.GetCurSelectedPos1();
            MM.ChangeCurMineralMapData(CurSelectRegion);
            this.Refresh();
        };
    }
    #endregion
}
