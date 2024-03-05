using ML.Engine.Manager;
using ML.Engine.TextContent;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;



namespace ML.Engine.UI
{
    public class StartMenuPanel : ML.Engine.UI.UIBasePanel
    {
        #region Unity
        public bool IsInit = false;
        private void Start()
        {

            InitUITextContents();

            var btnList = this.transform.Find("ButtonList");
            this.NewGameBtn = btnList.Find("NewGameBtn").GetComponent<SelectedButton>();
            this.NewGameBtnText = NewGameBtn.transform.Find("BtnText").GetComponent<TextMeshProUGUI>();
            this.NewGameBtn.OnInteract += () =>
            {

                // 实例化
                var panel = GameObject.Instantiate(GameManager.Instance.EnterPoint.LoadingScenePanelPrefab).GetComponent<LoadingScenePanel>();

                panel.transform.SetParent(GameObject.Find("Canvas").transform, false);

                // Push
                GameManager.Instance.UIManager.PushPanel(panel);
                GameManager.Instance.StartCoroutine(GameManager.Instance.LevelSwitchManager.LoadSceneAsync("GameScene",null,
                (string s1, string s2) =>
                {
                    // Pop
                    GameManager.Instance.UIManager.PopPanel();
                }));



            };

            this.ContinueGameBtn = btnList.Find("ContinueGameBtn").GetComponent<SelectedButton>();
            this.ContinueGameBtnText = ContinueGameBtn.transform.Find("BtnText").GetComponent<TextMeshProUGUI>();
            this.ContinueGameBtn.OnInteract += () =>
            {

            };

            this.OptionBtn = btnList.Find("OptionBtn").GetComponent<SelectedButton>();
            this.OptionBtnText = OptionBtn.transform.Find("BtnText").GetComponent<TextMeshProUGUI>();
            this.OptionBtn.OnInteract += () =>
            {
 
            };

            this.QuitGameBtn = btnList.Find("QuitGameBtn").GetComponent<SelectedButton>();
            this.QuitGameBtnText = QuitGameBtn.transform.Find("BtnText").GetComponent<TextMeshProUGUI>();
            this.QuitGameBtn.OnInteract += () =>
            {

            };

            var btn = btnList.GetComponentsInChildren<SelectedButton>();

            for (int i = 0; i < btn.Length; ++i)
            {
                int last = (i - 1 + btn.Length) % btn.Length;
                int next = (i + 1 + btn.Length) % btn.Length;

                btn[i].UpUI = btn[last];
                btn[i].DownUI = btn[next];
            }

            this.CurSelected = NewGameBtn;
            this.CurSelected.OnSelectedEnter();
            IsInit = true;



            Refresh();
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
            ML.Engine.Input.InputManager.Instance.Common.StartMenu.Enable();
            this.Refresh();
        }

        private void Exit()
        {
            ML.Engine.Input.InputManager.Instance.Common.StartMenu.Disable();
            this.UnregisterInput();

        }

        private void UnregisterInput()
        {


            //切换按钮
            ML.Engine.Input.InputManager.Instance.Common.StartMenu.SwichBtn.started -= SwichBtn_started;

            //确认
            ML.Engine.Input.InputManager.Instance.Common.Common.Comfirm.performed -= Confirm_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;



        }

        private void RegisterInput()
        {

            //切换按钮
            ML.Engine.Input.InputManager.Instance.Common.StartMenu.SwichBtn.started += SwichBtn_started;

            //确认
            ML.Engine.Input.InputManager.Instance.Common.Common.Comfirm.performed += Confirm_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

        }



        private void SwichBtn_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var vec2 = obj.ReadValue<Vector2>();
            if (vec2.y > 0.1f)
            {
                this.CurSelected.OnSelectedExit();
                this.CurSelected = this.CurSelected.UpUI;
                this.CurSelected.OnSelectedEnter();
            }
            else if (vec2.y < -0.1f)
            {
                this.CurSelected.OnSelectedExit();
                this.CurSelected = this.CurSelected.DownUI;
                this.CurSelected.OnSelectedEnter();
            }
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            
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

        private IUISelected CurSelected;

        private SelectedButton NewGameBtn;
        private SelectedButton ContinueGameBtn;
        private SelectedButton OptionBtn;
        private SelectedButton QuitGameBtn;

        private TMPro.TextMeshProUGUI NewGameBtnText;
        private TMPro.TextMeshProUGUI ContinueGameBtnText;
        private TMPro.TextMeshProUGUI OptionBtnText;
        private TMPro.TextMeshProUGUI QuitGameBtnText;

        #endregion

        public override void Refresh()
        {
            if (ABJAProcessorJson_StartMenuPanel == null || !ABJAProcessorJson_StartMenuPanel.IsLoaded || !IsInit)
            {
                Debug.Log("ABJAProcessorJson is null");
                return;
            }

            NewGameBtnText.text = PanelTextContent_StartMenuPanel.NewGameBtn;
            ContinueGameBtnText.text = PanelTextContent_StartMenuPanel.ContinueGameBtn;
            OptionBtnText.text = PanelTextContent_StartMenuPanel.OptionBtn;
            QuitGameBtnText.text = PanelTextContent_StartMenuPanel.QuitGameBtn;

        }
        #endregion



        #region TextContent
        [System.Serializable]
        public struct StartMenuPanelStruct
        {
            public ML.Engine.TextContent.TextContent NewGameBtn;
            public ML.Engine.TextContent.TextContent ContinueGameBtn;
            public ML.Engine.TextContent.TextContent OptionBtn;
            public ML.Engine.TextContent.TextContent QuitGameBtn;
        }

        public static StartMenuPanelStruct PanelTextContent_StartMenuPanel => ABJAProcessorJson_StartMenuPanel.Datas;
        public static ML.Engine.ABResources.ABJsonAssetProcessor<StartMenuPanelStruct> ABJAProcessorJson_StartMenuPanel;
        private void InitUITextContents()
        {
            if (ABJAProcessorJson_StartMenuPanel == null)
            {
                ABJAProcessorJson_StartMenuPanel = new ML.Engine.ABResources.ABJsonAssetProcessor<StartMenuPanelStruct>("Json/TextContent/StartMenuPanel", "StartMenuPanel", (datas) =>
                {
                    Refresh();
                    this.enabled = false;
                }, null, "StartMenuPanel数据");
                ABJAProcessorJson_StartMenuPanel.StartLoadJsonAssetData();
            }

        }
        #endregion



    }

}
