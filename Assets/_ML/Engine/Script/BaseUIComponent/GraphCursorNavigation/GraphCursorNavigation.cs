using ML.Engine.Timer;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GraphCursorNavigation : UIBehaviour
{
    private ScrollRect ScrollRect;
    private Scrollbar VerticalScrollbar, HorizontalScrollbar;
    private RectTransform Center;


    private Vector2 LimitBound;
    private Vector2 minBounds;
    private Vector2 maxBounds;

    private InputAction BindInputAction = null;

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
    }

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

    private bool canControlCenterxLeft;
    private bool canControlCenterxRight;
    private bool canControlCenteryUP;
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



}
