using ML.Engine.Timer;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Animancer.Easing;

namespace ML.Engine.UI
{
    [Serializable]
    public class GraphCursorNavigation : UIBehaviour, ITickComponent
    {
        private ScrollRect ScrollRect;
        private Scrollbar VerticalScrollbar, HorizontalScrollbar;
        private RectTransform Center;
        private RectTransform RaycastCenter;
        /// <summary>
        /// Center ��Content ����ϵ�µ�����
        /// </summary>
        [ShowInInspector]
        public Vector3 CenterPos { get { return Center.anchoredPosition3D - Content.GetComponent<RectTransform>().anchoredPosition3D / curZoomscale; } }

        private Vector2 LimitBound;
        private Vector2 minBounds;
        private Vector2 maxBounds;

        private InputAction BindNavigationInputAction = null;
        private InputAction BindZoomInInputAction = null;
        private InputAction BindZoomOutInputAction = null;

        private Transform content;
        public Transform Content { get { return content; } }
        [ShowInInspector]
        private UIBtnList uiBtnList;
        public UIBtnList UIBtnList { get { return uiBtnList; } }

        #region ���������
        [LabelText("�߾����ã����Ҽ�࣬���¼�ࣩ")]
        public Vector2 Margin = new Vector2();
        [LabelText("�����ٶ�")]
        public float NavagationSpeed = 1;
        [LabelText("�Ƿ��������Ź���")]
        public bool IsZoomEnabled = false;
        [ShowIf("IsZoomEnabled", true)]
        [LabelText("�����ٶ�")]
        public float ZoomSpeed = 1;
        [ShowIf("IsZoomEnabled", true)]
        [LabelText("���Ŵ��� ")]
        public float ZoomInLimit = 2;
        [ShowIf("IsZoomEnabled", true)]
        [LabelText("��С��С����")]
        public float ZoomOutLimit = 0.5f;
        #endregion


        //��ǰ�����scale 
        private float curZoomscale;
        [ShowIf("IsZoomEnabled", true), LabelText("��ǰ��������"), ShowInInspector]
        private float curZoomRate;
        public float CurZoomRate { get { return (curZoomscale - ZoomOutLimit)/(ZoomInLimit - ZoomOutLimit); }  set { curZoomRate = value; CurZoomscale = (ZoomInLimit - ZoomOutLimit) * value + ZoomOutLimit; } }
        [ShowIf("IsZoomEnabled", true), LabelText("��ǰ������ֵ"), ShowInInspector]
        public float CurZoomscale 
        { 
            set {
                this.content.localScale = new Vector3(value, value, value);
                this.Center.localScale = new Vector3(value, value, value);
                curZoomscale = value;
                }
            get
            {
                return curZoomscale;
            }
        }

        public event Action OnScaleChanged;

        protected override void Awake()
        {
            this.ScrollRect = this.transform.Find("Scroll View").GetComponent<ScrollRect>();
            this.VerticalScrollbar = ScrollRect.verticalScrollbar;
            this.HorizontalScrollbar = ScrollRect.horizontalScrollbar;
            this.Center = this.transform.Find("Center").GetComponent<RectTransform>();
            this.RaycastCenter = this.Center;
            var rec = this.GetComponent<RectTransform>();
            this.LimitBound = new Vector2(rec.rect.width / 2, rec.rect.height / 2);
            this.minBounds = -LimitBound + Margin;
            this.maxBounds = LimitBound - Margin;
            this.content = this.ScrollRect.content;
            CurZoomscale = 1;
        }

        #region Tick
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }
        [ShowInInspector]
        private GameObject lastSelected = null;
        public void Tick(float deltatime)
        {
            // ��ȡ UI Ԫ�ص���Ļ���� ScreenOverLay��Ϊ��������
            Vector3 worldPosition = this.RaycastCenter.position;

            // ��������
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = worldPosition;

            // ��������Ͷ��
            List<RaycastResult> results = new List<RaycastResult>();// ���Ը�����Ҫ�������߼����������
            EventSystem.current.RaycastAll(pointerEventData, results);

            // ������е� UI Ԫ��
            if (results.Count > 0)
            {
                if (lastSelected != results[0].gameObject)
                {
                    this.uiBtnList.RefreshSelected(results[0].gameObject.transform.GetComponent<SelectedButton>());
                    lastSelected = results[0].gameObject;
                }
            }
            else
            {
                this.uiBtnList.SetCurSelectedNull();
                lastSelected = null;
            }
        }

        #endregion

        public void BindNavigationInput(InputAction inputAction)
        {
            this.BindNavigationInputAction = inputAction;
            this.BindNavigationInputAction.started += NavagateMap_started;
            this.BindNavigationInputAction.canceled += NavagateMap_canceled;
        }

        public void DeBindNavigationInput()
        {
            this.BindNavigationInputAction.started -= NavagateMap_started;
            this.BindNavigationInputAction.canceled -= NavagateMap_canceled;
        }

        public void BindZoomInInput(InputAction inputAction)
        {
            this.BindZoomInInputAction = inputAction;
            this.BindZoomInInputAction.started += ZoomIn_started;
            this.BindZoomInInputAction.canceled += ZoomIn_canceled;
        }

        public void DeBindZoomInInput()
        {
            this.BindZoomInInputAction.started -= ZoomIn_started;
            this.BindZoomInInputAction.canceled -= ZoomIn_canceled;
        }

        public void BindZoomOutInput(InputAction inputAction)
        {
            this.BindZoomOutInputAction = inputAction;
            this.BindZoomOutInputAction.started += ZoomOut_started;
            this.BindZoomOutInputAction.canceled += ZoomOut_canceled;
        }

        public void DeBindZoomOutInput()
        {
            this.BindZoomOutInputAction.started -= ZoomOut_started;
            this.BindZoomOutInputAction.canceled -= ZoomOut_canceled;
        }

        #region NavagateMap_performed
        private float NavagateMapTimeInterval = 0.01f;
        CounterDownTimer NavagateMapTimer = null;
        private bool canControlCenterxLeft;
        private bool canControlCenterxRight;
        private bool canControlCenteryUP;
        private bool canControlCenteryDown;
        #endregion
        private void NavagateMap_started(InputAction.CallbackContext obj)
        {
            if (NavagateMapTimer == null)
            {
                NavagateMapTimer = new CounterDownTimer(NavagateMapTimeInterval, true, true, 1, 2);
                NavagateMapTimer.OnEndEvent += () =>
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

                    //�Ҳഥ��
                    if (canControlCenterxRight)
                    {
                        Vector2 clampedPosition = Center.anchoredPosition + vector2 * 5 * NavagationSpeed;
                        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
                        clampedPosition.y = Center.anchoredPosition.y;
                        Center.anchoredPosition = clampedPosition;

                        //�˳�����Center
                        if (Center.anchoredPosition.x < 0)
                        {
                            HorizontalScrollbar.value += (vector2.x / 400) * NavagationSpeed;
                        }
                    }

                    //��ഥ��
                    if (canControlCenterxLeft)
                    {
                        Vector2 clampedPosition = Center.anchoredPosition + vector2 * 5 * NavagationSpeed;
                        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
                        clampedPosition.y = Center.anchoredPosition.y;
                        Center.anchoredPosition = clampedPosition;

                        //�˳�����Center
                        if (Center.anchoredPosition.x > 0)
                        {
                            HorizontalScrollbar.value += (vector2.x / 400) * NavagationSpeed;
                        }
                    }

                    //�ϲഥ��
                    if (canControlCenteryUP)
                    {
                        Vector2 clampedPosition = Center.anchoredPosition + vector2 * 5 * NavagationSpeed;
                        clampedPosition.x = Center.anchoredPosition.x;
                        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);
                        Center.anchoredPosition = clampedPosition;

                        //�˳�����Center
                        if (Center.anchoredPosition.y < 0)
                        {
                            VerticalScrollbar.value += (vector2.y / 400) * NavagationSpeed;
                        }
                    }

                    //�²ഥ��
                    if (canControlCenteryDown)
                    {
                        Vector2 clampedPosition = Center.anchoredPosition + vector2 * 5 * NavagationSpeed;
                        clampedPosition.x = Center.anchoredPosition.x;
                        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);
                        Center.anchoredPosition = clampedPosition;

                        //�˳�����Center
                        if (Center.anchoredPosition.y > 0)
                        {
                            VerticalScrollbar.value += (vector2.y / 400) * NavagationSpeed;
                        }
                    }

                    //���ҿ��ƶ�����
                    if (!canControlCenterxRight && !canControlCenterxLeft)
                    {
                        HorizontalScrollbar.value += (vector2.x / 400) * NavagationSpeed;
                    }

                    //���¿��ƶ�����
                    if (!canControlCenteryUP && !canControlCenteryDown)
                    {
                        VerticalScrollbar.value += (vector2.y / 400) * NavagationSpeed;
                    }
                };
            }

        }

        private void NavagateMap_canceled(InputAction.CallbackContext obj)
        {
            ML.Engine.Manager.GameManager.Instance.CounterDownTimerManager.RemoveTimer(NavagateMapTimer);
            NavagateMapTimer = null;
        }

        #region ZoomOut_performed
        private float ZoomOutTimeInterval = 0.01f;
        private CounterDownTimer ZoomOutTimer = null;
        #endregion
        private void ZoomOut_started(InputAction.CallbackContext obj)
        {
            if (ZoomOutTimer == null)
            {
                ZoomOutTimer = new CounterDownTimer(ZoomOutTimeInterval, true, true, 1, 2);
                ZoomOutTimer.OnEndEvent += () =>
                {
                    ZoomOut();
                };
            }
        }

        private void ZoomOut_canceled(InputAction.CallbackContext obj)
        {
            ML.Engine.Manager.GameManager.Instance.CounterDownTimerManager.RemoveTimer(ZoomOutTimer);
            ZoomOutTimer = null;
        }


        #region ZoomIn_performed
        private float ZoomInTimeInterval = 0.01f;
        private CounterDownTimer ZoomInTimer = null;
        #endregion
        private void ZoomIn_started(InputAction.CallbackContext obj)
        {
            if (ZoomInTimer == null)
            {
                ZoomInTimer = new CounterDownTimer(ZoomInTimeInterval, true, true, 1, 2);
                ZoomInTimer.OnEndEvent += () =>
                {
                    ZoomIn();
                };
            }
        }

        private void ZoomIn_canceled(InputAction.CallbackContext obj)
        {
            ML.Engine.Manager.GameManager.Instance.CounterDownTimerManager.RemoveTimer(ZoomInTimer);
            ZoomInTimer = null;
        }

        /// <summary>
        /// �Ŵ��ͼ
        /// </summary>
        public void ZoomIn()
        {
            Vector3 Offset = this.ScrollRect.content.position - this.Center.position;
            float PreZoomscale = CurZoomscale;
            if (CurZoomscale + ZoomSpeed*0.01f > ZoomInLimit) 
            {
                CurZoomscale = ZoomInLimit;
            }
            else
            {
                CurZoomscale += ZoomSpeed * 0.01f;
            }
            Offset *= CurZoomscale / PreZoomscale;
            this.ScrollRect.content.position = this.Center.position + Offset;
            OnScaleChanged?.Invoke();
        }

        /// <summary>
        /// ��С��ͼ
        /// </summary>
        public void ZoomOut()
        {
            Vector3 Offset = this.ScrollRect.content.position - this.Center.position;
            float PreZoomscale = CurZoomscale;

            if (CurZoomscale - ZoomSpeed * 0.01f < ZoomOutLimit)
            {
                CurZoomscale = ZoomOutLimit;
            }
            else
            {
                CurZoomscale -= ZoomSpeed * 0.01f;
            }

            Offset *= CurZoomscale / PreZoomscale;
            this.ScrollRect.content.position = this.Center.position + Offset;
            OnScaleChanged?.Invoke();
        }
        public void EnableGraphCursorNavigation(InputAction NavigationInputAction, InputAction ZoomInInputAction = null, InputAction ZoomOutInputAction = null)
        {
            this.BindNavigationInput(NavigationInputAction);
            if(this.IsZoomEnabled)
            {
                this.BindZoomInInput(ZoomInInputAction);
                this.BindZoomOutInput(ZoomOutInputAction);
            }
            this.uiBtnList.EnableBtnList();
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
        }

        public void DisableGraphCursorNavigation()
        {
            this.DeBindNavigationInput();
            if (this.IsZoomEnabled)
            {
                this.DeBindZoomInInput();
                this.DeBindZoomOutInput();
            }
            this.uiBtnList.DisableBtnList();
            ML.Engine.Manager.GameManager.Instance.TickManager.UnregisterTick(this);
        }

        public void InitUIBtnList()
        {
            this.uiBtnList = new UIBtnList(this.content.GetComponentInChildren<UIBtnListInitor>());
        }

        public void ChangeRaycastCenter(RectTransform rectTransform)
        {
            this.RaycastCenter = rectTransform;
        }
    }
}


