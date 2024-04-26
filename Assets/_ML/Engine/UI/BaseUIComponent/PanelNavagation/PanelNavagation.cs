using Cysharp.Threading.Tasks;
using ML.Engine.Manager;
using ML.Engine.SaveSystem;
using ML.Engine.TextContent;
using ML.Engine.Timer;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ML.Engine.UI
{
    public class PanelNavagation : ML.Engine.UI.UIBasePanel
    {

        private ScrollRect ScrollRect;
        private Scrollbar VerticalScrollbar, HorizontalScrollbar;
        private RectTransform Center;
        private Vector2 minBounds = new Vector2(-850,-400);
        private Vector2 maxBounds = new Vector2(850, 400);
        public float NavagationSpeed = 1;
        #region Override
        protected override void Awake()
        {
            base.Awake();
            this.ScrollRect = this.transform.Find("Scroll View").GetComponent<ScrollRect>();
            this.VerticalScrollbar = ScrollRect.verticalScrollbar;
            this.HorizontalScrollbar = ScrollRect.horizontalScrollbar;
            this.Center = this.transform.Find("Center").GetComponent<RectTransform>();
        }

        #endregion

        #region Internal
        protected override void UnregisterInput()
        {
            // ·µ»Ø
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;

            ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn.started -= NavagateMap_started;

            ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn.canceled -= NavagateMap_canceled;
        }

        protected override void RegisterInput()
        {
            
            // ·µ»Ø
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

            ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn.started += NavagateMap_started;

            ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn.canceled += NavagateMap_canceled;
        }

        #region NavagateMap_performed
        private float TimeInterval = 0.01f;
        CounterDownTimer timer = null;

        #endregion

        private bool canControlCenterxLeft;
        private bool canControlCenterxRight;
        private bool canControlCenteryUP;
        private bool canControlCenteryDown;
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
                    
                    //ÓÒ²à´¥µ×
                    if (canControlCenterxRight)
                    {
                        Vector2 clampedPosition = Center.anchoredPosition + vector2 * 5 * NavagationSpeed;
                        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
                        clampedPosition.y = Center.anchoredPosition.y;
                        Center.anchoredPosition = clampedPosition;

                        //ÍË³ö¿ØÖÆCenter
                        if (Center.anchoredPosition.x < 0)
                        {
                            HorizontalScrollbar.value += (vector2.x / 400) * NavagationSpeed;
                        }
                    }

                    //×ó²à´¥µ×
                    if (canControlCenterxLeft)
                    {
                        Vector2 clampedPosition = Center.anchoredPosition + vector2 * 5 * NavagationSpeed;
                        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
                        clampedPosition.y = Center.anchoredPosition.y;
                        Center.anchoredPosition = clampedPosition;

                        //ÍË³ö¿ØÖÆCenter
                        if (Center.anchoredPosition.x > 0)
                        {
                            HorizontalScrollbar.value += (vector2.x / 400) * NavagationSpeed;
                        }
                    }

                    //ÉÏ²à´¥µ×
                    if (canControlCenteryUP)
                    {
                        Vector2 clampedPosition = Center.anchoredPosition + vector2 * 5 * NavagationSpeed;
                        clampedPosition.x = Center.anchoredPosition.x;
                        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);
                        Center.anchoredPosition = clampedPosition;

                        //ÍË³ö¿ØÖÆCenter
                        if (Center.anchoredPosition.y < 0)
                        {
                            VerticalScrollbar.value += (vector2.y / 400) * NavagationSpeed;
                        }
                    }

                    //ÏÂ²à´¥µ×
                    if (canControlCenteryDown)
                    {
                        Vector2 clampedPosition = Center.anchoredPosition + vector2 * 5 * NavagationSpeed;
                        clampedPosition.x = Center.anchoredPosition.x;
                        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);
                        Center.anchoredPosition = clampedPosition;

                        //ÍË³ö¿ØÖÆCenter
                        if (Center.anchoredPosition.y > 0)
                        {
                            VerticalScrollbar.value += (vector2.y / 400) * NavagationSpeed;
                        }
                    }

                    //×óÓÒ¿ÉÒÆ¶¯±³¾°
                    if (!canControlCenterxRight && !canControlCenterxLeft)
                    {
                        HorizontalScrollbar.value += (vector2.x / 400) * NavagationSpeed;
                    }

                    //ÉÏÏÂ¿ÉÒÆ¶¯±³¾°
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

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            GameManager.Instance.UIManager.PopPanel();
        }

        #endregion


    }
}