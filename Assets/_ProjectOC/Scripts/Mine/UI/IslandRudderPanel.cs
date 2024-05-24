using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using ProjectOC.MineSystem;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static IslandRudderPanel;
using static ML.Engine.UI.UIBtnList;


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
        var MapRegionDatas = MM.MapRegionDatas;
        UIBtnList.Synchronizer synchronizer = new Synchronizer(MapRegionDatas.Count, () =>
        {
            this.cursorNavigation.CurZoomscale = MM.GridScale;
            RefreshOnZoomMap();
            this.Refresh();
        });

        foreach (var mapRegion in MapRegionDatas)
        {
            this.cursorNavigation.UIBtnList.AddBtn("Prefab_Mine_UIPrefab/Prefab_MineSystem_UI_MapRegion.prefab",
                        OnSelectEnter: () =>
                        {
                            
                        },

                        OnSelectExit: () =>
                        {
                            
                        },
                        BtnSettingAction:
                        (btn) =>
                        {
                            var rect = btn.transform.GetComponent<RectTransform>();
                            rect.anchoredPosition = mapRegion.position;
                            btn.transform.Find("Normal").gameObject.SetActive(false);
                            btn.transform.Find("Locked").gameObject.SetActive(true);
                            btn.transform.Find("Selected").gameObject.SetActive(false);
                            btn.name = mapRegion.MapRegionID;

                            BtnToMapRegionIdDic.Add(btn, mapRegion.MapRegionID);
                        },
                        OnFinishAdd: () =>
                        {
                            synchronizer.Check();
                        }
                        );
        }
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

    #endregion

    public override void Refresh()
    {
        if (!this.objectPool.IsLoadFinish())
        {
            return;
        }
    }

    private void RefreshMainIsland()
    {
        this.MainIsland.anchoredPosition = MM.MainIslandData.CurPos;
        this.Target.anchoredPosition = MM.MainIslandData.TargetPos;


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
