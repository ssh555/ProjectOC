using ML.Engine.Manager;
using ML.Engine.TextContent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using static ML.Engine.UI.StartMenuPanel;
using static ProjectOC.Player.UI.PlayerUIPanel;



namespace ML.Engine.UI
{
    public class StartMenuPanel : ML.Engine.UI.UIBasePanel<StartMenuPanelStruct>
    {
        #region Unity
        public bool IsInit = false;

        protected override void Awake()
        {
            base.Awake();
            this.InitTextContentPathData();

            /*            this.functionExecutor.AddFunction(new List<Func<AsyncOperationHandle>> {
                            this.InitDescriptionPrefab,
                            this.InitBeastBioPrefab,
                            this.InitUITexture2D});*/
            this.functionExecutor.SetOnAllFunctionsCompleted(() =>
            {
                this.Refresh();
            });

            StartCoroutine(functionExecutor.Execute());


            btnList = this.transform.Find("ButtonList");
            
        }
        protected override void Start()
        {
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

        protected override void Enter()
        {
            this.RegisterInput();
            ML.Engine.Input.InputManager.Instance.Common.StartMenu.Enable();
            base.Enter();   
        }

        protected override void Exit()
        {
            ML.Engine.Input.InputManager.Instance.Common.StartMenu.Disable();
            this.UnregisterInput();
            base.Exit();
        }
        #endregion







        #region Internal



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
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit)
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

        protected override void OnLoadJsonAssetComplete(StartMenuPanelStruct datas)
        {
            InitBtnData(datas);
        }

        private void InitTextContentPathData()
        {
            this.abpath = "ML/Json/TextContent";
            this.abname = "StartMenuPanel";
            this.description = "StartMenuPanel数据加载完成";
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
