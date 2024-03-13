using ML.Engine.Manager;
using ML.Engine.TextContent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;



namespace ML.Engine.UI
{
    public class StartMenuPanel : ML.Engine.UI.UIBasePanel
    {
        #region Unity
        public bool IsInit = false;

        private void Awake()
        {
            Debug.Log("12423423424");
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
        //private AsyncOperationHandle spriteAtlasHandle;
        private void OnDestroy()
        {
            //GM.ABResourceManager.Release(spriteAtlasHandle);
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
                this.UIBtnList.MoveUPIUISelected();
            }
            else if (vec2.y < -0.1f)
            {
                this.UIBtnList.MoveDownIUISelected();
            }
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            
        }

        private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.UIBtnList.GetCurSelected().Interact();
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
        private UIBtnList UIBtnList;
        private void InitBtnData(StartMenuPanelStruct datas)
        {
            UIBtnList = new UIBtnList(parent: btnList, BtnType: 1);
            foreach (var tt in datas.Btns)
            {
                this.UIBtnList.SetBtnText(tt.name, tt.description.GetText());
            }

            //NewGameBtn
            this.UIBtnList.SetBtnAction("NewGameBtn",
            () =>
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
                    await UniTask.RunOnThreadPool(() =>
                    {
                        while (panel == null) ;
                    });
                    panel.OnExit();
                };
                GameManager.Instance.StartCoroutine(GameManager.Instance.LevelSwitchManager.LoadSceneAsync("GameScene", preCallback, postCallback, true));
                this.OnExit();
            }
            );

            //ContinueGameBtn
            this.UIBtnList.SetBtnAction("ContinueGameBtn",
            () =>
            {
                Debug.Log("ContinueGameBtn");
            }
            );

            //OptionBtn
            this.UIBtnList.SetBtnAction("OptionBtn",
            () =>
            {
                GameManager.Instance.EnterPoint.GetOptionPanelInstance().Completed += (handle) =>
                {
                    // 实例化
                    var panel = handle.Result.GetComponent<OptionPanel>();

                    panel.transform.SetParent(GameManager.Instance.UIManager.GetCanvas.transform, false);

                    GameManager.Instance.UIManager.PushPanel(panel);
                };
            }
            );
            //QuitGameBtn
            this.UIBtnList.SetBtnAction("QuitGameBtn",
            () =>
            {
                Debug.Log("QuitGameBtn");
            }
            );

        }

        #endregion
    }

}
