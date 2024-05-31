using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.InventorySystem.UI;
using ProjectOC.ManagerNS;
using ProjectOC.MineSystem;
using ProjectOC.ProNodeNS.UI;
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
        uIMineProNode.ProNode.ChangeMine(MM.CurMiningData);
    }                                                           

    protected override void Exit()
    {
        base.Exit();
        this.MapLayerUIBtnList.DisableBtnList();
        this.cursorNavigation.DisableGraphCursorNavigation();
    }

    protected override void Enter()
    {
        this.MapLayerUIBtnList.EnableBtnList();
        this.MapLayerUIBtnList.MoveIndexIUISelected(MM.CurMapLayerIndex);
        base.Enter();
        this.cursorNavigation.EnableGraphCursorNavigation(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, ML.Engine.Input.InputManager.Instance.Common.Common.NextTerm, ML.Engine.Input.InputManager.Instance.Common.Common.LastTerm);
    }
    #endregion

    #region Internal
    private void InitData()
    {
        GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_Mine_UIPrefab/Prefab_MineSystem_UI_BigMap.prefab").Completed += (handle) =>
        {
            BigMapInstanceTrans = handle.Result.transform;
            NormalRegions = BigMapInstanceTrans.Find("NormalRegion");
            BlockRegions = BigMapInstanceTrans.Find("BlockRegion");
            BigMapInstanceTrans.SetParent(this.cursorNavigation.Content.Find("BigMap"));
            BigMapInstanceTrans.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            referenceRectTransform = handle.Result.transform.Find("NormalRegion").transform as RectTransform;
            this.cursorNavigation.NavagationSpeed = MM.MineSystemConfig.SelectMineralSourcesSensitivity;
            this.cursorNavigation.ZoomSpeed = MM.MineSystemConfig.SelectMineralSourcesZoomSpeed;
            this.cursorNavigation.ZoomInLimit = MM.MineSystemConfig.SelectMineralSourcesZoomInLimit;
            this.cursorNavigation.ZoomOutLimit = MM.MineSystemConfig.SelectMineralSourcesZoomOutLimit;
            this.cursorNavigation.CurZoomRate = MM.MineSystemConfig.SelectMineralSourcesInitZoomRate;
            RefreshOnZoomMap();
            this.Refresh();
        };

        //初始化主岛的位置
        this.MainIsland.anchoredPosition = MM.MainIslandData.CurPos;
    }
    protected override void UnregisterInput()
    {
        ProjectOC.Input.InputManager.PlayerInput.IslandRudder.Disable();

        ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;

        // 返回
        ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        this.MapLayerUIBtnList.RemoveAllListener();
        this.MapLayerUIBtnList.DeBindInputAction();
    }

    protected override void RegisterInput()
    {
        ProjectOC.Input.InputManager.PlayerInput.IslandRudder.Enable();

        ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;

        // 返回
        ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
        this.MapLayerUIBtnList.BindNavigationInputAction(ProjectOC.Input.InputManager.PlayerInput.IslandRudder.ChangeMapLayer, UIBtnListContainer.BindType.started);
    }

    private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (curSelectRegion > 0 && MM.MineralMapData != null)                                          
        {
            GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_Mine_UIPanel/Prefab_Mine_UI_SmallMapPanel.prefab").Completed += (handle) =>
            {
                var panel = handle.Result.GetComponent<SmallMapPanel>();
                panel.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.NormalPanel, false);
                panel.SelectMineralSourcesPanel = this;
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
    private int preSelectRegion = -1;
    public int PreSelectRegion { get { return preSelectRegion; }  }
    [ShowInInspector]
    private int curSelectRegion = -1;
    public int CurSelectRegion { get { return  curSelectRegion; } }

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
        curSelectRegion = MM.BigMapTableData[gridPos.y, gridPos.x];
        MM.SmallMapCurSelectRegion = curSelectRegion;
        if (preSelectRegion != curSelectRegion)
        {
            preSelectRegion = curSelectRegion;
            MM.ChangeCurMineralMapData(curSelectRegion);
            this.Refresh();
        }
    }
    #endregion

    #region UI

    #region UI对象引用
    private MineSystemManager MM => LocalGameManager.Instance.MineSystemManager;
    [ShowInInspector]
    private GraphCursorNavigation cursorNavigation;

    private RectTransform MainIsland;
    private Slider slider;

    private Transform BigMapInstanceTrans;
    private Transform NormalRegions;
    private Transform BlockRegions;

    /// <summary>
    ///该ui对应的生产节点 id 在push该ui时赋值
    /// </summary>
    private string proNodeId = "testID" + ML.Engine.Utility.OSTime.OSCurMilliSeconedTime.ToString();
    [ShowInInspector]
    public string ProNodeId { set { proNodeId = value; } get { return proNodeId; } }

    private UIMineProNode uIMineProNode;
    public UIMineProNode UIMineProNode { get {  return uIMineProNode; } set { uIMineProNode = value; } }
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
            NormalRegions.GetChild(i).Find("Selected").gameObject.SetActive(isUnlocked && i + 1 == curSelectRegion);
        }

        for (int i = 0; i < BlockRegions.childCount; i++)
        {
            var child = BlockRegions.GetChild(i);
            string regionNumstr = child.name.Split('_')[1];
            int regionNum;
            bool isRegionNum = int.TryParse(regionNumstr, out regionNum);
            if (isRegionNum)
            {
                var isUnlocked = MM.CheckRegionIsUnlocked(regionNum);
                BlockRegions.GetChild(i).Find("Normal").gameObject.SetActive(!isUnlocked);
            }
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
            MM.ChangeCurMineralMapData(curSelectRegion);
            this.Refresh();
        };
    }
    #endregion
}
