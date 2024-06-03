using ML.Engine.Timer;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ML.Engine.UI
{
    [Serializable]
    public class GraphCursorNavigation : UIBehaviour, ITickComponent
    {
        private ScrollRect ScrollRect;
        private Scrollbar VerticalScrollbar, HorizontalScrollbar;
        private bool VerticalScrollbarActive { get { return VerticalScrollbar.gameObject.activeInHierarchy; } }
        private bool HorizontalScrollbarActive { get { return HorizontalScrollbar.gameObject.activeInHierarchy; } }

        private RectTransform center;
        public Transform Center { get { return center.transform; } }
        private RectTransform RaycastCenter;
        /// <summary>
        /// Center ��Content ����ϵ�µ�����
        /// </summary>
        [ShowInInspector]
        public Vector3 CenterPos { get {

                Vector2 localPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(this.content as RectTransform, this.center.position, null, out localPosition);
                return localPosition;
            } 
        }

        private Vector2 LimitBound;
        private Vector2 minBounds;
        private Vector2 maxBounds;

        [ShowInInspector]
        private InputAction BindNavigationInputAction = null;
        [ShowInInspector]
        private InputAction BindZoomInInputAction = null;
        [ShowInInspector]
        private InputAction BindZoomOutInputAction = null;

        private Transform content;
        public Transform Content { get { return content; } }
        [ShowInInspector]
        private UIBtnList uiBtnList;
        public UIBtnList UIBtnList { get { return uiBtnList; } }

        private bool isEnable = false;
        [ShowInInspector]
        public bool IsEnale { get { return isEnable; } set { isEnable = value; } }

        #region ���������
        [LabelText("�߾����ã����Ҽ�࣬���¼�ࣩ")]
        public Vector2 Margin = new Vector2();
        [LabelText("�����ٶ�")]
        public float navagationSpeed = 1;
        public float NavagationSpeed { get { return navagationSpeed * curZoomscale; } set { navagationSpeed = value; } }
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
        public float CurZoomRate { get { return (CurZoomscale - ZoomOutLimit)/(ZoomInLimit - ZoomOutLimit); }  set { curZoomRate = value;  CurZoomscale = (ZoomInLimit - ZoomOutLimit) * value + ZoomOutLimit; } }
        [ShowIf("IsZoomEnabled", true), LabelText("��ǰ������ֵ"), ShowInInspector]
        public float CurZoomscale 
        { 
            set {
                this.content.localScale = new Vector3(value, value, value);
                this.center.localScale = new Vector3(value, value, value);
                curZoomscale = value;
                }
            get
            {
                return curZoomscale;
            }
        }
        public float valueToPosX { get { return ((this.content as RectTransform).rect.width - (this.transform as RectTransform).rect.width) * curZoomscale; } }
        public float valueToPosY { get { return ((this.content as RectTransform).rect.height - (this.transform as RectTransform).rect.height) * curZoomscale; } }

        public event Action OnScaleChanged;
        public event Action OnCenterPosChanged;

        protected override void Awake()
        {
            this.ScrollRect = this.transform.Find("Scroll View").GetComponent<ScrollRect>();
            this.VerticalScrollbar = ScrollRect.verticalScrollbar;
            this.HorizontalScrollbar = ScrollRect.horizontalScrollbar;
            this.center = this.transform.Find("Center").GetComponent<RectTransform>();
            this.RaycastCenter = this.center;
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
            if (NavagateMapTimer == null)
            {
                NavagateMapTimer = new CounterDownTimer(NavagateMapTimeInterval, true, true, 1, 2);
                NavagateMapTimer.OnEndEvent += () =>
                {
                    //Debug.Log("NavagateMap_started ");
                    if(isEnable == false) { return; }

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
                    
                    if (HorizontalScrollbarActive == false)
                    { 
                        canControlCenterxLeft = true;
                        canControlCenterxRight = true;
                    }

                    if (VerticalScrollbarActive == false)
                    {
                        canControlCenteryUP = true;
                        canControlCenteryDown = false;
                    }

                    //�Ҳഥ��
                    if (canControlCenterxRight)
                    {
                        Vector2 clampedPosition = center.anchoredPosition + vector2 * 5 * NavagationSpeed;
                        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
                        clampedPosition.y = center.anchoredPosition.y;
                        center.anchoredPosition = clampedPosition;

                        //�˳�����Center
                        if (center.anchoredPosition.x < 0)
                        {
                            HorizontalScrollbar.value += (vector2.x / 400) * NavagationSpeed;
                        }
                    }

                    //��ഥ��
                    if (canControlCenterxLeft)
                    {
                        Vector2 clampedPosition = center.anchoredPosition + vector2 * 5 * NavagationSpeed;
                        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
                        clampedPosition.y = center.anchoredPosition.y;
                        center.anchoredPosition = clampedPosition;

                        //�˳�����Center
                        if (center.anchoredPosition.x > 0)
                        {
                            HorizontalScrollbar.value += (vector2.x / 400) * NavagationSpeed;
                        }
                    }

                    //�ϲഥ��
                    if (canControlCenteryUP)
                    {
                        Vector2 clampedPosition = center.anchoredPosition + vector2 * 5 * NavagationSpeed;
                        clampedPosition.x = center.anchoredPosition.x;
                        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);
                        center.anchoredPosition = clampedPosition;

                        //�˳�����Center
                        if (center.anchoredPosition.y < 0)
                        {
                            VerticalScrollbar.value += (vector2.y / 400) * NavagationSpeed;
                        }
                    }

                    //�²ഥ��
                    if (canControlCenteryDown)
                    {
                        Vector2 clampedPosition = center.anchoredPosition + vector2 * 5 * NavagationSpeed;
                        clampedPosition.x = center.anchoredPosition.x;
                        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);
                        center.anchoredPosition = clampedPosition;

                        //�˳�����Center
                        if (center.anchoredPosition.y > 0)
                        {
                            VerticalScrollbar.value += (vector2.y / 400) * NavagationSpeed;
                        }
                    }

                    //���ҿ��ƶ�����
                    if (!canControlCenterxRight && !canControlCenterxLeft)
                    {
                        //HorizontalScrollbar.value += (vector2.x / 400) * NavagationSpeed;
                        HorizontalScrollbar.value += vector2.x * 5 * NavagationSpeed / valueToPosX;
                    }

                    //���¿��ƶ�����
                    if (!canControlCenteryUP && !canControlCenteryDown)
                    {
                        //VerticalScrollbar.value += (vector2.y / 400) * NavagationSpeed;
                        VerticalScrollbar.value += vector2.y * 5 * NavagationSpeed / valueToPosY;
                    }
                    OnCenterPosChanged?.Invoke();
                };
            }

        }

        private void NavagateMap_canceled(InputAction.CallbackContext obj)
        {
            ML.Engine.Manager.GameManager.Instance.CounterDownTimerManager.RemoveTimer(NavagateMapTimer);
            NavagateMapTimer = null;
        }


        public float CalculateContentPosChangePerValueChange(RectTransform content, Scrollbar scrollbar)
        {
            float totalHeight = content.rect.height; // Content ���ܸ߶�
            float viewportHeight = content.parent.GetComponent<RectTransform>().rect.height; // Viewport �ĸ߶�
            float scrollbarHeight = scrollbar.size * viewportHeight; // �������Ļ���߶�

            // ���� value �� 0 �仯�� 1 ʱ Content �� PosY �ı仯ֵ
            float contentPosChangePerValueChange = (totalHeight - viewportHeight) / (1 - scrollbarHeight / (totalHeight - viewportHeight));
            return contentPosChangePerValueChange;
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
                    if (isEnable == false) { return; }
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
                    if (isEnable == false) { return; }
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
            Vector3 Offset = this.ScrollRect.content.position - this.center.position;
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
            this.ScrollRect.content.position = this.center.position + Offset;
            OnScaleChanged?.Invoke();
        }

        /// <summary>
        /// ��С��ͼ
        /// </summary>
        public void ZoomOut()
        {
            Vector3 Offset = this.ScrollRect.content.position - this.center.position;
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
            this.ScrollRect.content.position = this.center.position + Offset;
            OnScaleChanged?.Invoke();
        }

        /// <summary>
        /// �ƶ���ָ��λ�� ����ָ��λ����Content����ϵ�µ�anchoredPosition
        /// </summary>
        public void MoveCenterToPos(Vector2 targetPos)
        {
            //�ж��Ƿ����ڿ�
            //RectTransform ViewPort = this.content.parent as RectTransform;
            RectTransform Content = this.content as RectTransform;
            //Vector2 ViewPortSize = ViewPort.rect.size;
            //Vector2 ContentSize = Content.rect.size;

            /*            bool isTouchDown = Vector2.Distance(targetPos, new Vector2(0, -(ContentSize.y - ViewPortSize.y) / 2)) < 0.1;
                        bool isTouchUP = Vector2.Distance(targetPos, new Vector2(0, (ContentSize.y - ViewPortSize.y) / 2)) < 0.1;
                        bool isTouchLeft = Vector2.Distance(targetPos, new Vector2(-(ContentSize.x - ViewPortSize.x) / 2,0)) < 0.1;
                        bool isTouchRight = Vector2.Distance(targetPos, new Vector2((ContentSize.x - ViewPortSize.x) / 2,0)) < 0.1;*/

/*            bool isTouchDown = targetPos.y < -(ContentSize.y - ViewPortSize.y) / 2;
            bool isTouchUP = targetPos.y > (ContentSize.y - ViewPortSize.y) / 2;
            bool isTouchLeft = targetPos.x < -(ContentSize.x - ViewPortSize.x) / 2;
            bool isTouchRight = targetPos.x > (ContentSize.x - ViewPortSize.x) / 2;*/

/*            Debug.Log(isTouchUP + " " + isTouchDown + " " + isTouchLeft + " " + isTouchRight);
            if (isTouchDown || isTouchUP || isTouchLeft|| isTouchRight) 
            {

                center.anchoredPosition = Vector2.zero;
                Content.position = Content.TransformPoint(targetPos);
                Content.anchoredPosition *= -1;
                //����Center��pos ʹ Center��Content����ϵ�µ�����ΪTargetPos
                Vector3 worldPoint;
                RectTransformUtility.ScreenPointToWorldPointInRectangle(Content, Content.TransformPoint(targetPos), null, out worldPoint);
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(center as RectTransform, worldPoint, null, out localPoint);
                center.anchoredPosition = localPoint;

            }
            else
            {
                //���ڿ� ֻ��ı�Content��λ����Center�غ�

                center.anchoredPosition = Vector2.zero;
                Debug.Log("Content.position " + Content.TransformPoint(targetPos));
                Content.position = Content.TransformPoint(targetPos);
                Content.anchoredPosition *= -1;
            }*/

            center.anchoredPosition = Vector2.zero;
            Content.position = Content.TransformPoint(targetPos);
            Content.anchoredPosition *= -1;
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
            this.isEnable = true;
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
            this.isEnable = false;
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


