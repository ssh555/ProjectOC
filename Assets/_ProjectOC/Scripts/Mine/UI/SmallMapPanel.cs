using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ML.Engine.Utility;
using ProjectOC.ManagerNS;
using ProjectOC.MineSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using UnityEngine.UI;
using static ProjectOC.MineSystem.MineSystemData;
using static SmallMapPanel;
public class SmallMapPanel : UIBasePanel<SmallMapPanelStruct>
{
    #region Unity
    protected override void Awake()
    {
        base.Awake();
        this.cursorNavigation = this.transform.Find("GraphCursorNavigation").GetComponent<GraphCursorNavigation>();
        this.PlacedCircle = this.transform.Find("GraphCursorNavigation").Find("Scroll View").Find("Viewport").Find("Content").Find("PlacedCircle") as RectTransform;
        this.PlacedCircle.gameObject.SetActive(false);
        this.SmallMapBackground = this.transform.Find("GraphCursorNavigation").Find("Scroll View").Find("Viewport").Find("Content").Find("SmallMapBackground") as RectTransform;
        this.slider = this.cursorNavigation.transform.Find("Slider").GetComponent<Slider>();
        this.slider.onValueChanged.AddListener((value) => { this.cursorNavigation.CurZoomRate = value; });

        this.MineInfoContent = this.transform.Find("MineInfo").Find("Content");
    }
    #endregion

    #region Override
    public override void OnEnter()
    {
        base.OnEnter();
        this.InitData();
        this.cursorNavigation.OnCenterPosChanged += ChangeMiningData;
        this.cursorNavigation.OnScaleChanged += RefreshOnZoomMap;
    }
    public override void OnExit()
    {
        base.OnExit();
        this.cursorNavigation.OnCenterPosChanged -= ChangeMiningData;
        this.cursorNavigation.OnScaleChanged -= RefreshOnZoomMap;
    }

    protected override void Exit()
    {
        base.Exit();
        this.cursorNavigation.DisableGraphCursorNavigation();
    }

    protected override void Enter()
    {
        base.Enter();
        this.cursorNavigation.EnableGraphCursorNavigation(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, ML.Engine.Input.InputManager.Instance.Common.Common.NextTerm, ML.Engine.Input.InputManager.Instance.Common.Common.LastTerm);
    }
    #endregion

    #region Internal
    private Dictionary<SelectedButton, MineData> BtnToMineDataDic = new Dictionary<SelectedButton, MineData>();
    private void InitData()
    {
        if (MM.MineralMapData == null) return;
        this.CheckRange = MM.MineSystemConfig.MiningCircleRadius * this.EnlargeRate;
        (this.cursorNavigation.Center as RectTransform).sizeDelta = new Vector2(2 * CheckRange, 2 * CheckRange);
        this.PlacedCircle.sizeDelta = new Vector2(2 * CheckRange, 2 * CheckRange);
        Synchronizer synchronizer = new Synchronizer(MM.MineralMapData.MineDatas.Count, () => { ChangeMiningData(); });
        foreach (var minedata in MM.MineralMapData.MineDatas)
        {
            this.cursorNavigation.UIBtnList.AddBtn("Prefab_Mine_UIPrefab/Prefab_MineSystem_UI_BaseUISelectedBtn.prefab",
            BtnSettingAction:
            (btn) =>
            {
                btn.GetComponent<RectTransform>().anchoredPosition = minedata.Position * EnlargeRate;
                btn.GetComponent<RectTransform>().localScale = new Vector3(0.3f, 0.3f, 0.3f);
                BtnToMineDataDic.Add(btn, minedata);
                btn.transform.Find("Image").GetComponent<Image>().sprite = MM.GetMineSprite(minedata.MineID);
            }, OnFinishAdd: () => { synchronizer.Check(); }
            ); ;
        }

        //生成小地图背景
        Texture2D texture2D = MM.MineralMapData.texture2D;
        this.SmallMapBackground.GetComponent<Image>().sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
        this.SmallMapBackground.sizeDelta = texture2D.Size();
        this.SmallMapBackground.localScale = new Vector2(EnlargeRate, EnlargeRate);

        //获取小地图最左下角的屏幕坐标
        Vector2 screenPosition = this.SmallMapBackground.TransformPoint(new Vector3(SmallMapBackground.rect.xMin, SmallMapBackground.rect.yMin, 0f));
        localPosition =  ScreenToRectTransformLocalPosition(this.cursorNavigation.Content as RectTransform, screenPosition);
        (this.cursorNavigation.UIBtnList.Parent as RectTransform).anchoredPosition = localPosition;

        //初始化矿圈位置信息
        PlaceCircleData placeCircleData = MM.GetMineralCircleData(selectMineralSourcesPanel.ProNodeId,selectMineralSourcesPanel.CurSelectRegion);
        if(placeCircleData != null)
        {
            this.PlacedCircle.gameObject.SetActive(placeCircleData.isPlacedCircle);
            this.PlacedCircle.anchoredPosition = placeCircleData.PlaceCirclePosition;
        }
        else
        {
            this.PlacedCircle.gameObject.SetActive(false);
        }
        //初始化slider
        RefreshOnZoomMap();
    }
    private Vector2 localPosition;

    // 将屏幕坐标转换为指定 RectTransform 的局部坐标
    public static Vector2 ScreenToRectTransformLocalPosition(RectTransform rectTransform, Vector2 screenPosition)
    {
        Vector2 localPosition = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPosition, null, out localPosition);
        return localPosition;
    }
    private void ChangeMiningData()
    {
        tmpMineInfoDic.Clear();
        foreach (var (btn,minedata) in BtnToMineDataDic)
        {
            bool isInCircle = Vector2.Distance(minedata.Position * EnlargeRate + localPosition, (Vector2)this.cursorNavigation.CenterPos) <= CheckRange;
            btn.Selected.gameObject.SetActive(isInCircle);

            if(isInCircle)
            {
                if (!tmpMineInfoDic.ContainsKey(minedata.GainItems.id))
                {
                    tmpMineInfoDic.Add(minedata.GainItems.id, minedata.GainItems.num);
                }
                else
                {
                    tmpMineInfoDic[minedata.GainItems.id] += minedata.GainItems.num;
                }
            }
        }
        UpdateMineInfo();
    }

    #region 矿物信息更新
    [ShowInInspector]
    private Dictionary<string, int> tmpMineInfoDic = new Dictionary<string, int>();
    private void UpdateMineInfo()
    {
        if (!isInitObjectPool) return;

        this.objectPool.ResetPool("MineInfoPool");
        foreach (var (itemid,num) in tmpMineInfoDic)
        {
            var tPrefab = this.objectPool.GetNextObject("MineInfoPool", this.MineInfoContent);
            tPrefab.transform.Find("Image").GetComponent<Image>().sprite = ItemManager.Instance.GetItemSprite(itemid);
            tPrefab.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "X <color=#49D237>" + num.ToString() + "</color>/每次";
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.MineInfoContent as RectTransform);
    }

    #endregion

    private void RefreshOnZoomMap()
    {
        this.slider.value = this.cursorNavigation.CurZoomRate;
        MM.GridScale = this.cursorNavigation.CurZoomscale;
    }

    protected override void UnregisterInput()
    {
        ProjectOC.Input.InputManager.PlayerInput.SmallMap.Disable();
        ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;
        // 返回
        ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
    }

    protected override void RegisterInput()
    {
        ProjectOC.Input.InputManager.PlayerInput.SmallMap.Enable();
        ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;
        // 返回
        ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
    }

    private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        //生成矿圈
        this.PlacedCircle.anchoredPosition = this.cursorNavigation.CenterPos;
        MM.AddMineralCircleData(this.cursorNavigation.CenterPos, selectMineralSourcesPanel.ProNodeId, selectMineralSourcesPanel.CurSelectRegion);
        if(hasPlacedCircle == false)
        {
            this.PlacedCircle.gameObject.SetActive(true);
        }
        selectMineralSourcesPanel.CurMiningData.Clear();
        foreach (var (btn, minedata) in BtnToMineDataDic)
        {
            bool isInCircle = Vector2.Distance(minedata.Position * EnlargeRate + localPosition, (Vector2)this.cursorNavigation.CenterPos) <= CheckRange;
            btn.Selected.gameObject.SetActive(isInCircle);
            if(isInCircle)
            {
                selectMineralSourcesPanel.CurMiningData.Add(minedata);
            }
        }
    }

    private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        UIMgr.PopPanel();
    }
    #endregion

    #region UI
    #region UI对象引用
    private MineSystemManager MM => LocalGameManager.Instance.MineSystemManager;
    [ShowInInspector]
    private GraphCursorNavigation cursorNavigation;
    private Slider slider;
    private float CheckRange;
    private RectTransform PlacedCircle;
    private RectTransform SmallMapBackground;
    private bool hasPlacedCircle = false;

    private Transform MineInfoContent;

    /// <summary>
    ///当前矿物坐标放大系数
    /// </summary>
    private int EnlargeRate = 50;

    private SelectMineralSourcesPanel selectMineralSourcesPanel;
    public SelectMineralSourcesPanel SelectMineralSourcesPanel { set { selectMineralSourcesPanel = value; } }
    #endregion

    public override void Refresh()
    {
        if (!this.objectPool.IsLoadFinish())
        {
            return;
        }
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

    bool isInitObjectPool = false;
    protected override void InitObjectPool()
    {
        this.objectPool.RegisterPool(UIObjectPool.HandleType.Prefab, "MineInfoPool", 5, "Prefab_Mine_UIPrefab/Prefab_MineSystem_UI_MineInfo.prefab", (handle) => { isInitObjectPool = true; });
        base.InitObjectPool();
    }
    #endregion
}
