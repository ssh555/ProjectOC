using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.LandMassExpand;
using ProjectOC.ManagerNS;
using ProjectOC.MineSystem;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ak.wwise;
using static UIIslandRudderPanel;
public class UIIslandRudderPanel : UIBasePanel<IslandRudderPanelStruct>
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
        this.DotLine.gameObject.SetActive(false);
    }
    #endregion

    #region Override

    public override void OnEnter()
    {
        base.OnEnter();
        MM.RefreshUI += this.RefreshWhileIslandMoving;
        this.InitData();
        this.cursorNavigation.OnScaleChanged += RefreshOnZoomMap;
        MM.MainIslandData.OnisMovingChanged += RefreshOnChangeMovingState;
        this.cursorNavigation.ChangeRaycastCenter(this.MainIsland);
        this.uIIslandUpdatePanel.ChangeToIslandRudderPanel();
    }
    public override void OnExit()
    {
        this.UnregisterInput();
        this.objectPool.OnDestroy();
        this.gameObject.SetActive(false);
        this.Exit();
        MM.RefreshUI -= this.RefreshWhileIslandMoving;
        this.cursorNavigation.OnScaleChanged -= RefreshOnZoomMap;
        MM.MainIslandData.OnisMovingChanged -= RefreshOnChangeMovingState;
        this.uIIslandUpdatePanel.IslandRudderPanelChangeTo();
        (this.cursorNavigation.Content as RectTransform).anchoredPosition = Vector2.zero;
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
        this.cursorNavigation.EnableGraphCursorNavigation(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, ProjectOC.Input.InputManager.PlayerInput.IslandRudder.RT, ProjectOC.Input.InputManager.PlayerInput.IslandRudder.LT);
    }
    #endregion

    #region Internal
    private Dictionary<SelectedButton,string> BtnToMapRegionIdDic = new Dictionary<SelectedButton,string>();
    private void InitData()
    {
        BigMapInstanceTrans = MM.BigMapInstance.transform;
        NormalRegions = BigMapInstanceTrans.Find("NormalRegion");
        BlockRegions = BigMapInstanceTrans.Find("BlockRegion");
        BigMapInstanceTrans.SetParent(this.cursorNavigation.Content.Find("BigMap"));
        BigMapInstanceTrans.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        this.cursorNavigation.CurZoomscale = MM.GridScale;
        this.cursorNavigation.NavagationSpeed = MM.MineSystemConfig.IslandRudderSensitivity;
        this.cursorNavigation.ZoomSpeed = MM.MineSystemConfig.IslandRudderZoomSpeed;
        this.cursorNavigation.ZoomInLimit = MM.MineSystemConfig.IslandRudderZoomInLimit;
        this.cursorNavigation.ZoomOutLimit = MM.MineSystemConfig.IslandRudderZoomOutLimit;
        RefreshOnZoomMap();
        this.Refresh();

        //初始化主岛的位置
        this.MainIsland.anchoredPosition = MM.MainIslandData.CurPos;
        this.MainIsland.ForceUpdateRectTransforms();
        //初始化光标位置
        //计算主岛位置在Conten坐标系下的位置
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(MM.IslandRudderPanelInstance.transform as RectTransform);
        // 将世界坐标转换为屏幕坐标
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, this.MainIsland.position);
        Vector2 targetPostion;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(cursorNavigation.Content as RectTransform, screenPoint, null, out targetPostion);
        UnityEngine.Debug.Log("targetPostion " + targetPostion);
        cursorNavigation.MoveCenterToPos(targetPostion);
    }
    protected override void UnregisterInput()
    {
        ProjectOC.Input.InputManager.PlayerInput.IslandRudder.Disable();

        ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;

/*        // 返回
        ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;*/

        this.MapLayerUIBtnList.RemoveAllListener();
        this.MapLayerUIBtnList.DeBindInputAction();
    }

    protected override void RegisterInput()
    {
        ProjectOC.Input.InputManager.PlayerInput.IslandRudder.Enable();

        ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;
/*
        // 返回
        ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;*/
        this.MapLayerUIBtnList.BindNavigationInputAction(ProjectOC.Input.InputManager.PlayerInput.IslandRudder.ChangeMapLayer, UIBtnListContainer.BindType.started);
    }

    private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        //先检测是否碰撞
        Vector3 MovingDir = (this.cursorNavigation.CenterPos - (Vector3)MM.MainIslandData.CurPos).normalized;
        if (MM.DetectRegion(MainIsland.transform.position + MovingDir * ColliderRadiu) == 0)
        {
            return;
        }

        if (!MM.SetNewNavagatePoint(this.cursorNavigation.CenterPos))
        {
            MM.MainIslandData.IsPause = true;
            GameManager.Instance.StartCoroutine(PushNoticeUIInstance1());
        }

    }
    private IEnumerator PushNoticeUIInstance1()
    {
        uIIslandUpdatePanel.DisableBack_performed();
        yield return new WaitForSeconds(0.1f);
        GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.PopUpUI, new UIManager.PopUpUIData("已有导航点，是否重新设置？", null, null, () => {
            MM.SetNewNavagatePoint(this.cursorNavigation.CenterPos, true);
            MM.MainIslandData.IsPause = false;
            uIIslandUpdatePanel.EnableBack_performed();
        }, () => { MM.MainIslandData.IsPause = false; uIIslandUpdatePanel.EnableBack_performed(); }));
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
    private Transform BlockRegions;

    private Image DotLine;
    private RectTransform DotLineRectTransform;

    private UIIslandUpdatePanel uIIslandUpdatePanel;
    public UIIslandUpdatePanel UIIslandUpdatePanel { set { uIIslandUpdatePanel = value; } }

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

        for (int i = 0; i < BlockRegions.childCount; i++)
        {
            var child = BlockRegions.GetChild(i);
            string regionNumstr = child.name.Split('_')[1];
            int regionNum;
            bool isRegionNum = int.TryParse(regionNumstr, out regionNum);
            if(isRegionNum)
            {
                var isUnlocked = MM.CheckRegionIsUnlocked(regionNum);
                BlockRegions.GetChild(i).Find("Normal").gameObject.SetActive(!isUnlocked);
            }
        }
        #endregion
    }

    private void RefreshWhileIslandMoving()
    {
        #region 虚线
        float angle = Vector3.Angle(MM.MainIslandData.MovingDir, Vector2.right);
        angle = MM.MainIslandData.TargetPos.y < MM.MainIslandData.CurPos.y ? -angle : angle;
        var dis = Vector2.Distance(MM.MainIslandData.CurPos, MM.MainIslandData.TargetPos);
        this.DotLineRectTransform.sizeDelta = new Vector2(dis, DotLineRectTransform.sizeDelta.y);
        this.DotLineRectTransform.rotation = Quaternion.Euler(0, 0, angle);
        this.DotLineRectTransform.anchoredPosition = (MM.MainIslandData.CurPos + MM.MainIslandData.TargetPos) / 2;
        this.DotLine.material.SetFloat("_Scale", dis / 30);
        #endregion
    }
    private void RefreshOnZoomMap()
    {
        this.slider.value = this.cursorNavigation.CurZoomRate;
        MM.GridScale = this.cursorNavigation.CurZoomscale;
    }
    private void RefreshOnChangeMovingState(bool isMoving)
    {
        this.Target.anchoredPosition = MM.MainIslandData.TargetPos;
        this.Target.gameObject.SetActive(isMoving);
        this.DotLine.gameObject.SetActive(isMoving);
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

    private UIBtnList MapLayerUIBtnList = null;
    protected override void InitBtnInfo()
    {
        this.cursorNavigation.InitUIBtnList();
        MapLayerUIBtnList = new UIBtnList(this.transform.Find("MapLayer").Find("ButtonList").GetComponent<UIBtnListInitor>());
        MapLayerUIBtnList.OnSelectButtonChanged += () =>
        {
            if (MM.MainIslandData.IsMoving)
            {
                MM.MainIslandData.IsPause = true;
                uIIslandUpdatePanel.DisableBack_performed();
                GameManager.Instance.StartCoroutine(PushNoticeUIInstance2());
            }
            else
            {
                MM.CurMapLayerIndex = MapLayerUIBtnList.GetCurSelectedPos1();
                //进入新区域
                MM.UnlockMapRegion(MM.CurRegionNum);
                this.Refresh();
            }
        };
    }

    private IEnumerator PushNoticeUIInstance2()
    {
        uIIslandUpdatePanel.DisableBack_performed();
        yield return new WaitForSeconds(0.1f);
        GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.PopUpUI, new UIManager.PopUpUIData("取消已有的导航点并切换地图层？", null, null, () =>
        {
            MM.MainIslandData.IsPause = false;
            MM.MainIslandData.Reset();
            MM.CurMapLayerIndex = MapLayerUIBtnList.GetCurSelectedPos1();
            //进入新区域
            MM.UnlockMapRegion(MM.CurRegionNum);
            this.Refresh();
            uIIslandUpdatePanel.EnableBack_performed();
        }, () => { MM.MainIslandData.IsPause = false; uIIslandUpdatePanel.EnableBack_performed(); }));
    }
    #endregion
}
