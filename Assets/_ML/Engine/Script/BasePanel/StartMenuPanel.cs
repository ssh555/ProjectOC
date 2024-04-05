using ML.Engine.Manager;
using ML.Engine.TextContent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using static ML.Engine.UI.StartMenuPanel;
using static ProjectOC.Player.UI.PlayerUIPanel;
using System;
using Sirenix.OdinInspector;
using ML.Engine.SaveSystem;
using static UnityEngine.Rendering.DebugUI;



namespace ML.Engine.UI
{
    public class StartMenuPanel : ML.Engine.UI.UIBasePanel<StartMenuPanelStruct>
    {
        #region Unity
        protected override void Awake()
        {
            base.Awake();
            btnList = this.transform.Find("ButtonList");
        }
        private ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;
        private List<AsyncOperationHandle<GameObject>> goHandle = new List<AsyncOperationHandle<GameObject>>();

        protected override void OnDestroy()
        {
            foreach (var handle in goHandle)
            {
                GM.ABResourceManager.ReleaseInstance(handle);
            }
        }
        #endregion

        #region override
        public override void OnEnter()
        {
            UIBtnList = new UIBtnList(parent: btnList);
            base.OnEnter();
        }

        protected override void Enter()
        {
            this.UIBtnList.EnableBtnList();
            base.Enter();
        }

        protected override void Exit()
        {
            this.UIBtnList.DisableBtnList();
            base.Exit();
        }

        #endregion

        #region Internal
        protected override void UnregisterInput()
        {
            this.UIBtnList.RemoveAllListener();
            this.UIBtnList.DeBindInputAction();
            ML.Engine.Input.InputManager.Instance.Common.StartMenu.Disable();


        }
        protected override void RegisterInput()
        {

            //NewGameBtn
            this.UIBtnList.SetBtnAction("NewGameBtn",
            () =>
            {
                UIBasePanel panel = null;
                System.Action<string, string> preCallback = async (string s1, string s2) =>
                {
                    //开始新游戏时，删除之前的newgame存档，从0开始新的newgame存档
                    if (SC.GetSaveDataFolder(0) != null)
                    {
                        await SC.DeleteSaveDataFolderAsync(0, null);
                    }
#pragma warning disable CS4014
                    SC.CreateSaveDataFolderAsync(0, "newgame", async () =>
                    {
                        Debug.Log("存入newgame！");

                        await SC.SelectSaveDataFolderAsync(0, null);
                        await SC.LoadSaveDataAsync(null);
                    });
#pragma warning restore CS4014

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
            async () =>
            {

                await SC.SelectSaveDataFolderAsync(0, null);
                await SC.LoadSaveDataAsync(null);
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
            #if UNITY_EDITOR
                            UnityEditor.EditorApplication.isPlaying = false;//如果是在unity编译器中
            #else
                                                    Application.Quit();//否则在打包文件中
            #endif
            }
            );
            ML.Engine.Input.InputManager.Instance.Common.StartMenu.Enable();

            this.UIBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.StartMenu.SwichBtn, UIBtnListContainer.BindType.started);
            this.UIBtnList.BindButtonInteractInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.Confirm, UIBtnListContainer.BindType.started);


        }

        public void SwichBtn_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
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
                this.UIBtnList.MoveUPIUISelected();
            }
            else if (angle > 45 && angle < 135)
            {
                this.UIBtnList.MoveRightIUISelected();
            }
            else if (angle > 135 && angle < 225)
            {
                this.UIBtnList.MoveDownIUISelected();
            }
            else if (angle > 225 && angle < 315)
            {
                this.UIBtnList.MoveLeftIUISelected();
            }
        }

        public void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.UIBtnList.GetCurSelected().Interact();
        }
        #endregion


        #region SaveSystem
        SaveController SC => GameManager.Instance.SaveManager.SaveController;
        //下标0 为newgame 1为savegame
        private void InitSaveSystemData()
        {
            
            if (SC.GetSaveDataFolder(1) == null)
            {
                this.UIBtnList.GetBtn("ContinueGameBtn").Interactable = false;
            }
            else
            {
                this.UIBtnList.GetBtn("ContinueGameBtn").Interactable = true;
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
        protected override void InitTextContentPathData()
        {
            this.abpath = "ML/Json/TextContent";
            this.abname = "StartMenuPanel";
            this.description = "StartMenuPanel数据加载完成";
        }
        private Transform btnList;
        [ShowInInspector]
        private UIBtnList UIBtnList;
        private void InitBtnData(StartMenuPanelStruct datas)
        {    
            foreach (var tt in datas.Btns)
            {
                this.UIBtnList.SetBtnText(tt.name, tt.description.GetText());
            }
        }

        #endregion
    }

}
