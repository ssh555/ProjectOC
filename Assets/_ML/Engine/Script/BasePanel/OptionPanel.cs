using ML.Engine.Manager;
using ML.Engine.TextContent;
using ProjectOC.ResonanceWheelSystem.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static ML.Engine.UI.StartMenuPanel;



namespace ML.Engine.UI
{
    public class OptionPanel : ML.Engine.UI.UIBasePanel
    {
        #region Unity
        public bool IsInit = false;

        private void Awake()
        {
            

            ToptitleText = this.transform.Find("TopTitle").Find("Text").GetComponent<TextMeshProUGUI>();
            btnList = this.transform.Find("ButtonList");
            gridLayout = btnList.GetComponent<GridLayoutGroup>();
        }

        protected override void Start()
        {
            InitUITextContents();


            IsInit = true;



            Refresh();

            base.Start();
        }

        #endregion

        #region Override
        public override void OnEnter()
        {
            base.OnEnter();
            this.Enter();
        }

        public override void OnExit()
        {
            base.OnExit();
            this.Exit();
            ClearTemp();
        }

        public override void OnPause()
        {
            base.OnPause();
            this.Exit();
        }

        public override void OnRecovery()
        {
            base.OnRecovery();
            this.Enter();
        }

        #endregion

        #region Internal

        private void Enter()
        {
            this.RegisterInput();
            ML.Engine.Input.InputManager.Instance.Common.Option.Enable();
            this.Refresh();
        }

        private void Exit()
        {
            ML.Engine.Input.InputManager.Instance.Common.Option.Disable();
            this.UnregisterInput();

        }

        private void UnregisterInput()
        {


            //切换按钮
            ML.Engine.Input.InputManager.Instance.Common.Option.SwichBtn.started -= SwichBtn_started;

            //确认
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;



        }

        private void RegisterInput()
        {

            //切换按钮
            ML.Engine.Input.InputManager.Instance.Common.Option.SwichBtn.started += SwichBtn_started;

            //确认
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

        }



        private void SwichBtn_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            string actionName = obj.action.name;

            // 使用 ReadValue<T>() 方法获取附加数据
            string actionMapName = obj.action.actionMap.name;

            var vector2 = obj.ReadValue<UnityEngine.Vector2>();
            float angle = Mathf.Atan2(vector2.x, vector2.y);

            angle = angle * 180 / Mathf.PI;
            if (angle < 0)
            {
                angle = angle + 360;
            }

            if (angle < 45 || angle > 315)
            {
                this.CurSelected.SelectedExit();
                this.CurSelected = this.CurSelected.UpUI;
                this.CurSelected.SelectedEnter();
            }  
            else if (angle > 45 && angle < 135)
            {
                this.CurSelected.SelectedExit();
                this.CurSelected = this.CurSelected.RightUI;
                this.CurSelected.SelectedEnter();
            }
            else if (angle > 135 && angle < 225)
            {
                this.CurSelected.SelectedExit();
                this.CurSelected = this.CurSelected.DownUI;
                this.CurSelected.SelectedEnter();
            }
            else if (angle > 225 && angle < 315)
            {
                this.CurSelected.SelectedExit();
                this.CurSelected = this.CurSelected.LeftUI;
                this.CurSelected.SelectedEnter();
            }
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            GameManager.Instance.UIManager.PopPanel();
        }

        private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.CurSelected.Interact();
        }
        #endregion

        #region UI
        #region Temp
        private List<Sprite> tempSprite = new List<Sprite>();
        private Dictionary<ML.Engine.InventorySystem.ItemType, GameObject> tempItemType = new Dictionary<ML.Engine.InventorySystem.ItemType, GameObject>();
        private List<GameObject> tempUIItems = new List<GameObject>();


        private void ClearTemp()
        {
            foreach (var s in tempSprite)
            {
                Destroy(s);
            }
            foreach (var s in tempItemType.Values)
            {
                Destroy(s);
            }
            foreach (var s in tempUIItems)
            {
                Destroy(s);
            }
        }

        #endregion

        #region UI对象引用

        private GridLayoutGroup gridLayout;

        private TMPro.TextMeshProUGUI ToptitleText;



        #endregion

        public override void Refresh()
        {
            if (ABJAProcessorJson_OptionPanel == null || !ABJAProcessorJson_OptionPanel.IsLoaded || !IsInit)
            {
                return;
            }
        }
        #endregion



        #region TextContent
        [System.Serializable]
        public struct OptionPanelStruct
        {
            public ML.Engine.TextContent.TextContent TopTitle;

            public TextTip[] Btns;

            //BotKeyTips
            public KeyTip Confirm;
            public KeyTip Back;
        }

        public OptionPanelStruct PanelTextContent_OptionPanel => ABJAProcessorJson_OptionPanel.Datas;
        public ML.Engine.ABResources.ABJsonAssetProcessor<OptionPanelStruct> ABJAProcessorJson_OptionPanel;
        private void InitUITextContents()
        {

            ABJAProcessorJson_OptionPanel = new ML.Engine.ABResources.ABJsonAssetProcessor<OptionPanelStruct>("ML/Json/TextContent", "OptionPanel", (datas) =>
            {
                InitBtnData(datas);
                Refresh();
                this.enabled = false;
            }, "OptionPanel数据");
            ABJAProcessorJson_OptionPanel.StartLoadJsonAssetData();
            

        }


        private Transform btnList;
        private List<UISelectedButtonComponent> BtnComponents = new List<UISelectedButtonComponent>();
        private IUISelected CurSelected;
        private void InitBtnData(OptionPanelStruct datas)
        {
            foreach (var tt in datas.Btns)
            {
                var btn = new UISelectedButtonComponent(btnList, tt.name);
                btn.textMeshProUGUI.text = tt.description.GetText();
                this.BtnComponents.Add(btn);
            }

            //GraphicBtn
            this.BtnComponents[0].selectedButton.OnInteract += () =>
            {
                
            };
            //AudioBtn
            this.BtnComponents[1].selectedButton.OnInteract += () =>
            {

            };
            //ControllerBtn
            this.BtnComponents[2].selectedButton.OnInteract += () =>
            {

            };
            //TutorialBtn
            this.BtnComponents[3].selectedButton.OnInteract += () =>
            {

            };
            //BackBtn
            this.BtnComponents[3].selectedButton.OnInteract += () =>
            {

            };
            //QuitGameBtn
            this.BtnComponents[3].selectedButton.OnInteract += () =>
            {

            };

            var btns = btnList.GetComponentsInChildren<SelectedButton>();
            int ConstraintCount = gridLayout.constraintCount;
            for (int i = 0; i < btns.Length; ++i)
            {
                int last = (i - 1 + btns.Length) % btns.Length;
                int next = (i + 1 + btns.Length) % btns.Length;

                btns[i].UpUI = i - ConstraintCount >= 0 ? btns[i - ConstraintCount] : btns[i - ConstraintCount + btns.Length];
                btns[i].DownUI = i + ConstraintCount < btns.Length ? btns[i + ConstraintCount] : btns[i + ConstraintCount - btns.Length];
                btns[i].RightUI = i % ConstraintCount + 1 < ConstraintCount ? btns[i + 1] : btns[i / ConstraintCount * ConstraintCount];
                btns[i].LeftUI = i % ConstraintCount - 1 >= 0 ? btns[i - 1] : btns[i / ConstraintCount * ConstraintCount + ConstraintCount - 1];
            }

            foreach (var btn in btns)
            {
                btn.OnSelectedEnter += () =>
                {
                    btn.transform.Find("Selected").gameObject.SetActive(true);
                };
                btn.OnSelectedExit += () =>
                {
                    btn.transform.Find("Selected").gameObject.SetActive(false);
                };
            }

            this.CurSelected = this.BtnComponents[0].selectedButton;
            this.CurSelected.SelectedEnter();
        }



        #endregion



    }

}
