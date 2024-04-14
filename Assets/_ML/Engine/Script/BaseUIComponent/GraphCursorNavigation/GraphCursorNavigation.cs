using ML.Engine.Timer;
using ML.Engine.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[Serializable]
public class GraphCursorNavigation : UIBehaviour ,ITickComponent
{
    private ScrollRect ScrollRect;
    private Scrollbar VerticalScrollbar, HorizontalScrollbar;
    private RectTransform Center;


    private Vector2 LimitBound;
    private Vector2 minBounds;
    private Vector2 maxBounds;

    private InputAction BindInputAction = null;

    private Transform content;
    public Transform Content { get { return content; } }
    [ShowInInspector]
    private UIBtnList uiBtnList;
    public UIBtnList UIBtnList { get { return uiBtnList; } }    

    [LabelText("边距配置（左右间距，上下间距）")]
    public Vector2 Margin = new Vector2();
    [LabelText("导航速度")]
    public float NavagationSpeed = 1;
    protected override void Awake()
    {
        this.ScrollRect = this.transform.Find("Scroll View").GetComponent<ScrollRect>();
        this.VerticalScrollbar = ScrollRect.verticalScrollbar;
        this.HorizontalScrollbar = ScrollRect.horizontalScrollbar;
        this.Center = this.transform.Find("Center").GetComponent<RectTransform>();
        var rec = this.GetComponent<RectTransform>();
        this.LimitBound = new Vector2(rec.rect.width / 2, rec.rect.height / 2);
        this.minBounds = -LimitBound + Margin;
        this.maxBounds = LimitBound - Margin;
        this.content = this.ScrollRect.content;
    }

    #region Tick
    public int tickPriority { get; set; }
    public int fixedTickPriority { get; set; }
    public int lateTickPriority { get; set; }

    private GameObject lastSelected = null;
    public void Tick(float deltatime)
    {
        // 获取 UI 元素的屏幕坐标 ScreenOverLay即为世界坐标
        Vector3 worldPosition = this.Center.position;

        // 构造射线
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = worldPosition;

/*        foreach (var result in LastFrameResults)
        {
            result.gameObject.transform.Find("Selected")?.gameObject.SetActive(false);
        }*/

        // 进行射线投射
        List<RaycastResult> results = new List<RaycastResult>();// 可以根据需要设置射线检测的最大数量
        EventSystem.current.RaycastAll(pointerEventData, results);

        // 处理击中的 UI 元素
        if (results.Count > 0)
        {
            if(lastSelected!= results[0].gameObject)
            {
                this.uiBtnList.RefreshSelected(results[0].gameObject.transform.GetComponent<SelectedButton>());
                lastSelected = results[0].gameObject;
            }
        }
        else
        {
            this.uiBtnList.SetCurSelectedNull();
        }
    }

    #endregion

    public void BindNavigationInput(InputAction inputAction)
    {
        this.BindInputAction = inputAction;
        this.BindInputAction.started += NavagateMap_started;
        this.BindInputAction.canceled += NavagateMap_canceled;
    }

    public void DeBindNavigationInput()
    {
        this.BindInputAction.started -= NavagateMap_started;
        this.BindInputAction.canceled -= NavagateMap_canceled;
    }

    #region NavagateMap_performed
    private float TimeInterval = 0.01f;
    CounterDownTimer timer = null;

    [ShowInInspector]
    private bool canControlCenterxLeft;
    [ShowInInspector]
    private bool canControlCenterxRight;
    [ShowInInspector]
    private bool canControlCenteryUP;
    [ShowInInspector]
    private bool canControlCenteryDown;
    #endregion


    private void NavagateMap_started(InputAction.CallbackContext obj)
    {
        if (timer == null)
        {
            timer = new CounterDownTimer(TimeInterval, true, true, 1, 2);
            timer.OnEndEvent += () =>
            {
                canControlCenterxLeft = false;
                canControlCenterxRight = false;
                canControlCenteryUP = false;
                canControlCenteryDown = false;
                var vector2 = obj.ReadValue<UnityEngine.Vector2>();

                if (Mathf.Abs(HorizontalScrollbar.value - 1) < 0.0025)
                {
                    canControlCenterxRight = true;
                }


                if (Mathf.Abs(HorizontalScrollbar.value) < 0.0025)
                {
                    canControlCenterxLeft = true;
                }


                if (Mathf.Abs(VerticalScrollbar.value - 1) < 0.0025)
                {
                    canControlCenteryUP = true;
                }


                if (Mathf.Abs(VerticalScrollbar.value) < 0.0025)
                {
                    canControlCenteryDown = true;
                }

                //右侧触底
                if (canControlCenterxRight)
                {
                    Vector2 clampedPosition = Center.anchoredPosition + vector2 * 5 * NavagationSpeed;
                    clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
                    clampedPosition.y = Center.anchoredPosition.y;
                    Center.anchoredPosition = clampedPosition;

                    //退出控制Center
                    if (Center.anchoredPosition.x < 0)
                    {
                        HorizontalScrollbar.value += (vector2.x / 400) * NavagationSpeed;
                    }
                }

                //左侧触底
                if (canControlCenterxLeft)
                {
                    Vector2 clampedPosition = Center.anchoredPosition + vector2 * 5 * NavagationSpeed;
                    clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
                    clampedPosition.y = Center.anchoredPosition.y;
                    Center.anchoredPosition = clampedPosition;

                    //退出控制Center
                    if (Center.anchoredPosition.x > 0)
                    {
                        HorizontalScrollbar.value += (vector2.x / 400) * NavagationSpeed;
                    }
                }

                //上侧触底
                if (canControlCenteryUP)
                {
                    Vector2 clampedPosition = Center.anchoredPosition + vector2 * 5 * NavagationSpeed;
                    clampedPosition.x = Center.anchoredPosition.x;
                    clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);
                    Center.anchoredPosition = clampedPosition;

                    //退出控制Center
                    if (Center.anchoredPosition.y < 0)
                    {
                        VerticalScrollbar.value += (vector2.y / 400) * NavagationSpeed;
                    }
                }

                //下侧触底
                if (canControlCenteryDown)
                {
                    Vector2 clampedPosition = Center.anchoredPosition + vector2 * 5 * NavagationSpeed;
                    clampedPosition.x = Center.anchoredPosition.x;
                    clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);
                    Center.anchoredPosition = clampedPosition;

                    //退出控制Center
                    if (Center.anchoredPosition.y > 0)
                    {
                        VerticalScrollbar.value += (vector2.y / 400) * NavagationSpeed;
                    }
                }

                //左右可移动背景
                if (!canControlCenterxRight && !canControlCenterxLeft)
                {
                    HorizontalScrollbar.value += (vector2.x / 400) * NavagationSpeed;
                }

                //上下可移动背景
                if (!canControlCenteryUP && !canControlCenteryDown)
                {
                    VerticalScrollbar.value += (vector2.y / 400) * NavagationSpeed;
                }
            };
        }

    }

    private void NavagateMap_canceled(InputAction.CallbackContext obj)
    {
        ML.Engine.Manager.GameManager.Instance.CounterDownTimerManager.RemoveTimer(timer);
        timer = null;
    }

    public void EnableGraphCursorNavigation(InputAction inputAction)
    {
        this.BindNavigationInput(inputAction);
        this.uiBtnList.EnableBtnList();
        ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
    }

    public void DisableGraphCursorNavigation()
    {
        this.DeBindNavigationInput();
        this.uiBtnList.DisableBtnList();
        ML.Engine.Manager.GameManager.Instance.TickManager.UnregisterTick(this);
    }

    public void InitUIBtnList()
    {
        this.uiBtnList = new UIBtnList(this.content.GetComponentInChildren<UIBtnListInitor>());
    }
}
