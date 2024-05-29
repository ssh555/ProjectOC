using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using ProjectOC.MineSystem;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ak.wwise;
using static IslandRudderPanel;
public class IslandRudderPanel : UIBasePanel<IslandRudderPanelStruct>
{
    #region Unity
    protected override void Awake()
    {
        base.Awake();
        this.cursorNavigation = this.transform.Find("GraphCursorNavigation").GetComponent<GraphCursorNavigation>();
        var Content = this.cursorNavigation.transform.Find("Scroll View").Find("Viewport").Find("Content");
        this.MainIsland = Content.Find("MainIsland").GetComponent<RectTransform>();
        this.ColliderRadiu = MainIsland.GetComponent<CircleCollider2D>().radius;
        this.Target = Content.Find("Target").GetComponent<RectTransform>();
        this.Target.gameObject.SetActive(false);
        this.slider = this.transform.Find("MapLayer").Find("Slider").GetComponent<Slider>();
        this.slider.onValueChanged.AddListener((value) => { this.cursorNavigation.CurZoomRate = value; });
        this.DotLine = Content.Find("DotLine").GetComponent<Image>();
        this.DotLineRectTransform = DotLine.GetComponent<RectTransform>();
    }
    #endregion

    #region Override

    public override void OnEnter()
    {
        base.OnEnter();
        MM.RefreshUI += this.RefreshMainIsland;
        this.InitData();
        this.cursorNavigation.OnScaleChanged += RefreshOnZoomMap;
        MM.MainIslandData.OnisMovingChanged += RefreshTarget;
        this.cursorNavigation.ChangeRaycastCenter(this.MainIsland);
    }
    public override void OnExit()
    {
        base.OnExit();
        MM.RefreshUI -= this.RefreshMainIsland;
        this.cursorNavigation.OnScaleChanged -= RefreshOnZoomMap;
        MM.MainIslandData.OnisMovingChanged -= RefreshTarget;
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
        this.cursorNavigation.EnableGraphCursorNavigation(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, ML.Engine.Input.InputManager.Instance.Common.Common.NextTerm, ML.Engine.Input.InputManager.Instance.Common.Common.LastTerm);
    }

    #endregion

    #region Internal
    private Dictionary<SelectedButton,string> BtnToMapRegionIdDic = new Dictionary<SelectedButton,string>();
    private bool isInit = false;
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
            this.cursorNavigation.NavagationSpeed = MM.MineSystemConfig.IslandRudderSensitivity;
            this.cursorNavigation.ZoomSpeed = MM.MineSystemConfig.ZoomSpeed;
            this.cursorNavigation.ZoomInLimit = MM.MineSystemConfig.ZoomInLimit;
            this.cursorNavigation.ZoomOutLimit = MM.MineSystemConfig.ZoomOutLimit;
            RefreshOnZoomMap();
            DetectMainIslandCurRegion();
            isInit = true;
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
        //先检测是否碰撞
        Vector3 MovingDir = (this.cursorNavigation.CenterPos - (Vector3)MM.MainIslandData.CurPos).normalized;
        if (DetectRegion(MainIsland.transform.position + MovingDir * ColliderRadiu) == 0)
        {
            return;
        }

        if (!MM.SetNewNavagatePoint(this.cursorNavigation.CenterPos))
        {
            MM.MainIslandData.IsPause = true;
            GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.PopUpUI, new UIManager.PopUpUIData("已有导航点，是否重新设置？", null, null, () => {
                MM.SetNewNavagatePoint(this.cursorNavigation.CenterPos, true);
                MM.MainIslandData.IsPause = false;
            }, () => { MM.MainIslandData.IsPause = false; }));
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
    private int CurColliderPointRegion = -1;
    private int PreColliderPointRegion = -1;
    private void DetectMainIslandCurRegion()
    {
        
        PreColliderPointRegion = CurColliderPointRegion;
        CurColliderPointRegion = DetectRegion(MainIsland.transform.position + (Vector3)MM.MainIslandData.MovingDir * ColliderRadiu);
        if (PreColliderPointRegion != CurColliderPointRegion && CurColliderPointRegion == 0)
        {
            //碰撞
            MM.UnlockMapRegion(0);
            return;
        }
        PreRegion = CurRegion;
        CurRegion = DetectRegion(MainIsland.transform.position);
        if (PreRegion != CurRegion)
        {
            //进入新区域
            MM.UnlockMapRegion(CurRegion);
            PreRegion = CurRegion;
            this.Refresh();
        }
    }

    private int DetectRegion(Vector3 pos)
    {
        Vector3 worldPosition = pos;
        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(referenceRectTransform, worldPosition, null, out localPosition);
        Vector2 referenceSize = referenceRectTransform.rect.size;
        Vector2 anchorPosition = new Vector2(localPosition.x / referenceSize.x + 0.5f, localPosition.y / referenceSize.y + 0.5f);
        anchorPosition = new Vector2(anchorPosition.x, 1 - anchorPosition.y);
        int width = MM.BigMapTableData.GetLength(0);
        Vector2Int gridPos = new Vector2Int(
        Mathf.Clamp((int)(anchorPosition.x * (width)), 0, width - 1),
        Mathf.Clamp((int)(anchorPosition.y * (width)), 0, width - 1));
        return MM.BigMapTableData[gridPos.y, gridPos.x];
    }

    #endregion

    #region UI

    #region UI对象引用
    private MineSystemManager MM => LocalGameManager.Instance.MineSystemManager;
    [ShowInInspector]
    private GraphCursorNavigation cursorNavigation;

    private RectTransform MainIsland;
    private float ColliderRadiu;

    private RectTransform Target;

    private Slider slider;

    private Transform BigMapInstanceTrans;
    private Transform NormalRegions;

    private Image DotLine;
    private RectTransform DotLineRectTransform;
    private Material DotLineMaterial;
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
        }
        #endregion

    }

    private void RefreshMainIsland()
    {
        if (!isInit) return;
        this.MainIsland.anchoredPosition = MM.MainIslandData.CurPos;
        this.Target.anchoredPosition = MM.MainIslandData.TargetPos;
        DetectMainIslandCurRegion();

        float angle = Vector3.Angle(MM.MainIslandData.MovingDir, Vector2.right);
        angle = MM.MainIslandData.TargetPos.y < MM.MainIslandData.CurPos.y ? -angle : angle;
        var dis = Vector2.Distance(MM.MainIslandData.CurPos, MM.MainIslandData.TargetPos);
        this.DotLineRectTransform.sizeDelta = new Vector2(dis, DotLineRectTransform.sizeDelta.y);
        this.DotLineRectTransform.rotation = Quaternion.Euler(0, 0, angle);
        this.DotLineRectTransform.anchoredPosition = (MM.MainIslandData.CurPos + MM.MainIslandData.TargetPos) / 2;
        this.DotLine.material.SetFloat("_Scale", dis / 30);
    }

    private void RefreshOnZoomMap()
    {
        this.slider.value = this.cursorNavigation.CurZoomRate;
        MM.GridScale = this.cursorNavigation.CurZoomscale;
    }

    private void RefreshTarget(bool isMoving)
    {
        this.Target.gameObject.SetActive(isMoving);
        this.DotLine.gameObject.SetActive(isMoving);
    }

    #region 虚线
    


    #endregion


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

    private UIBtnList MapLayerUIBtnList = null;
    protected override void InitBtnInfo()
    {
        this.cursorNavigation.InitUIBtnList();
        MapLayerUIBtnList = new UIBtnList(this.transform.Find("MapLayer").Find("ButtonList").GetComponent<UIBtnListInitor>());
        MapLayerUIBtnList.OnSelectButtonChanged += () =>
        {
            MM.CurMapLayerIndex = MapLayerUIBtnList.GetCurSelectedPos1();
            //进入新区域
            MM.UnlockMapRegion(CurRegion);
            this.Refresh();
        };
    }
    #endregion
}
