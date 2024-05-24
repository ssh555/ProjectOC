using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ML.Engine.Utility;
using ProjectOC.ManagerNS;
using ProjectOC.MineSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
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
        this.CheckRange = 300;
    }
    #endregion

    #region Override
    public override void OnEnter()
    {
        base.OnEnter();
        this.InitData();
        this.cursorNavigation.OnCenterPosChanged += ChangeMiningData;
    }
    public override void OnExit()
    {
        base.OnExit();
        this.cursorNavigation.OnCenterPosChanged -= ChangeMiningData;
        ClearTemp();
    }

    protected override void Exit()
    {
        base.Exit();
        this.cursorNavigation.DisableGraphCursorNavigation();
    }

    protected override void Enter()
    {
        base.Enter();
        this.cursorNavigation.EnableGraphCursorNavigation(ProjectOC.Input.InputManager.PlayerInput.SmallMap.SwichBtn, ML.Engine.Input.InputManager.Instance.Common.Common.LastTerm, ML.Engine.Input.InputManager.Instance.Common.Common.NextTerm);
    }


    #endregion

    #region Internal
    private Dictionary<SelectedButton, MineData> BtnToMineDataDic = new Dictionary<SelectedButton, MineData>();
    private void InitData()
    {
        if (MM.MineralMapData == null) return;

        Synchronizer synchronizer = new Synchronizer(MM.MineralMapData.MineDatas.Count, () => { ChangeMiningData(); });

        foreach (var minedata in MM.MineralMapData.MineDatas)
        {
            this.cursorNavigation.UIBtnList.AddBtn("Prefab_Mine_UIPrefab/Prefab_MineSystem_UI_BaseUISelectedBtn.prefab",
            BtnSettingAction:
            (btn) =>
            {
                btn.GetComponent<RectTransform>().anchoredPosition = minedata.Position * EnlargeRate;
                BtnToMineDataDic.Add(btn, minedata);
                //btn.transform.Find("Image").GetComponent<Image>().sprite = 
            }, OnFinishAdd: () => { synchronizer.Check(); }
            ); ;
        }

        //生成小地图背景
        Texture2D texture2D = MM.MineralMapData.texture2D;
        this.SmallMapBackground.GetComponent<Image>().sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
        this.SmallMapBackground.sizeDelta = texture2D.Size();
        this.SmallMapBackground.localScale = new Vector2(100, 100);

        //获取小地图最左下角的屏幕坐标
        Vector2 screenPosition = this.SmallMapBackground.TransformPoint(new Vector3(SmallMapBackground.rect.xMin, SmallMapBackground.rect.yMin, 0f));
        localPosition =  ScreenToRectTransformLocalPosition(this.cursorNavigation.Content as RectTransform, screenPosition);
        (this.cursorNavigation.UIBtnList.Parent as RectTransform).anchoredPosition = localPosition;
    }
    private Vector2 localPosition;

    // 将屏幕坐标转换为指定 RectTransform 的局部坐标
    public static Vector2 ScreenToRectTransformLocalPosition(RectTransform rectTransform, Vector2 screenPosition)
    {
        Vector2 localPosition = Vector2.zero;

        // 检查 RectTransform 是否存在
        if (rectTransform != null)
        {
            // 使用 RectTransformUtility.ScreenPointToLocalPointInRectangle 方法将屏幕坐标转换为局部坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPosition, null, out localPosition);
        }
        else
        {
            Debug.LogWarning("RectTransform is null!");
        }

        return localPosition;
    }

    private void ChangeMiningData()
    {
        foreach (var (btn,minedata) in BtnToMineDataDic)
        {
            bool isInCircle = Vector2.Distance(minedata.Position * EnlargeRate + localPosition, (Vector2)this.cursorNavigation.CenterPos) <= CheckRange;
            btn.Selected.gameObject.SetActive(isInCircle);
        }
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

        if(hasPlacedCircle == false)
        {
            this.PlacedCircle.gameObject.SetActive(true);
        }
        this.curMiningData.Clear();
        foreach (var (btn, minedata) in BtnToMineDataDic)
        {
            bool isInCircle = Vector2.Distance(minedata.Position * EnlargeRate + localPosition, (Vector2)this.cursorNavigation.CenterPos) <= CheckRange;
            btn.Selected.gameObject.SetActive(isInCircle);
            if(isInCircle )
            {
                this.curMiningData.Add(minedata);
            }
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
    private float CheckRange;
    private RectTransform PlacedCircle;
    private RectTransform SmallMapBackground;
    private bool hasPlacedCircle = false;
    /// <summary>
    ///当前矿物坐标放大系数
    /// </summary>
    private int EnlargeRate = 100;

    /// <summary>
    ///当前矿圈所圈住的矿物集合
    /// </summary>
    private List<MineData> curMiningData = new List<MineData>();
    [ShowInInspector]
    public List<MineData> CurMiningData { get { return curMiningData; } }

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
    #endregion
}
