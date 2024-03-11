using ML.Engine.Manager;
using ML.Engine.TextContent;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;



namespace ML.Engine.UI
{
    public class StartMenuPanel : ML.Engine.UI.UIBasePanel
    {
        #region Unity
        public bool IsInit = false;

        private void Awake()
        {
            
            //InitPrefabs();
            btnList = this.transform.Find("ButtonList");
            
        }
        protected override void Start()
        {
            InitUITextContents();

            IsInit = true;
            Refresh();
            base.Start();
        }
        private ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;
        private List<AsyncOperationHandle<GameObject>> goHandle = new List<AsyncOperationHandle<GameObject>>();
        private AsyncOperationHandle spriteAtlasHandle;
        private void OnDestroy()
        {
            GM.ABResourceManager.Release(spriteAtlasHandle);
            foreach (var handle in goHandle)
            {
                GM.ABResourceManager.ReleaseInstance(handle);
            }
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
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;



        }

        private void RegisterInput()
        {

            //切换按钮
            ML.Engine.Input.InputManager.Instance.Common.StartMenu.SwichBtn.started += SwichBtn_started;

            //确认
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

        }



        private void SwichBtn_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var vec2 = obj.ReadValue<Vector2>();
            if (vec2.y > 0.1f)
            {
                this.CurSelected.SelectedExit();
                this.CurSelected = this.CurSelected.UpUI;
                this.CurSelected.SelectedEnter();
            }
            else if (vec2.y < -0.1f)
            {
                this.CurSelected.SelectedExit();
                this.CurSelected = this.CurSelected.DownUI;
                this.CurSelected.SelectedEnter();
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

        private void ClearTemp()
        {
            
        }

        #endregion

        #region UI对象引用

        #endregion

        public override void Refresh()
        {
            if (ABJAProcessorJson_StartMenuPanel == null || !ABJAProcessorJson_StartMenuPanel.IsLoaded || !IsInit)
            {
                return;
            }

        }
        #endregion



        #region TextContent
        [System.Serializable]
        public struct StartMenuPanelStruct
        {
            public TextTip[] Btns;
        }

        public StartMenuPanelStruct PanelTextContent_StartMenuPanel => ABJAProcessorJson_StartMenuPanel.Datas;
        public ML.Engine.ABResources.ABJsonAssetProcessor<StartMenuPanelStruct> ABJAProcessorJson_StartMenuPanel;
        private void InitUITextContents()
        {
            ABJAProcessorJson_StartMenuPanel = new ML.Engine.ABResources.ABJsonAssetProcessor<StartMenuPanelStruct>("ML/Json/TextContent", "StartMenuPanel", (datas) =>
            {
                InitBtnData(datas);
                Refresh();
                this.enabled = false;
            }, "StartMenuPanel数据");
            ABJAProcessorJson_StartMenuPanel.StartLoadJsonAssetData();

        }

        private Transform btnList;
        private List<UISelectedButtonComponent> BtnComponents = new List<UISelectedButtonComponent>();
        private IUISelected CurSelected;
        private void InitBtnData(StartMenuPanelStruct datas)
        {
            foreach (var tt in datas.Btns)
            {
                var btn = new UISelectedButtonComponent(btnList, tt.name);
                btn.textMeshProUGUI.text = tt.description.GetText();
                this.BtnComponents.Add(btn);
            }

            //NewGameBtn
            this.BtnComponents[0].selectedButton.OnInteract += () =>
            {                               
                UIBasePanel panel = null;
                System.Action<string, string> preCallback = (string s1, string s2) =>
                {
                    GameManager.Instance.EnterPoint.GetLoadingScenePanelInstance().Completed += (handle) =>
                    {
                        // 实例化
                        panel = handle.Result.GetComponent<LoadingScenePanel>();

                        panel.transform.SetParent(GameManager.Instance.UIManager.GetCanvas.transform, false);

                        panel.OnEnter();


                    };

                };
                System.Action<string, string> postCallback = async (string s1, string s2) =>
                {
                    await System.Threading.Tasks.Task.Run(() =>
                    {
                        while (panel == null) ;
                    });
                    panel.OnExit();
                };
                GameManager.Instance.StartCoroutine(GameManager.Instance.LevelSwitchManager.LoadSceneAsync("GameScene", preCallback, postCallback, true));
                this.OnExit();
            };
            //ContinueGameBtn
            this.BtnComponents[1].selectedButton.OnInteract += () =>
            {

            };
            //OptionBtn
            this.BtnComponents[2].selectedButton.OnInteract += () =>
            {

                GameManager.Instance.EnterPoint.GetOptionPanelInstance().Completed += (handle) =>
                {
                    // 实例化
                    var panel = handle.Result.GetComponent<OptionPanel>();

                    panel.transform.SetParent(GameManager.Instance.UIManager.GetCanvas.transform, false);

                    GameManager.Instance.UIManager.PushPanel(panel);
                };
            };
            //QuitGameBtn
            this.BtnComponents[3].selectedButton.OnInteract += () =>
            {

            };


            //SelectedButton初始化在Awake
            var btns = btnList.GetComponentsInChildren<SelectedButton>();

            for (int i = 0; i < btns.Length; ++i)
            {
                int last = (i - 1 + btns.Length) % btns.Length;
                int next = (i + 1 + btns.Length) % btns.Length;

                btns[i].UpUI = btns[last];
                btns[i].DownUI = btns[next];
            }

            foreach (var btn in btns)
            {
                btn.OnSelectedEnter += () => { btn.image.color = Color.red; };
                btn.OnSelectedExit += () => { btn.image.color = Color.white; };
            }


            this.CurSelected = this.BtnComponents[0].selectedButton;
            this.CurSelected.SelectedEnter();
        }

        #endregion

        #region Prefab
        /*        private GameObject LoadingScenePanelPrefab;
                private void InitPrefabs()
                {
                    GM.EnterPoint.GetLoadingScenePanelInstance().Completed += (handle) =>
                    {
                        this.goHandle.Add(handle);
                        this.LoadingScenePanelPrefab = handle.Result;

                    };
                }*/

        #endregion

    }

}
