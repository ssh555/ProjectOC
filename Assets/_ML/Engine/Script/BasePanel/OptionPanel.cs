using Cysharp.Threading.Tasks;
using ML.Engine.Manager;
using ML.Engine.SaveSystem;
using ML.Engine.TextContent;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ML.Engine.UI.OptionPanel;
using static ML.Engine.UI.UIBtnListContainer;

namespace ML.Engine.UI
{
    public class OptionPanel : ML.Engine.UI.UIBasePanel<OptionPanelStruct>
    {
        #region Unity
        public bool IsInit = false;

        protected override void Awake()
        {
            base.Awake();
            ToptitleText = this.transform.Find("TopTitle").Find("Text").GetComponent<TextMeshProUGUI>();
        }

        protected override void Start()
        {
            IsInit = true;
            Refresh();
            base.Start();
        }

        #endregion

        #region Override
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
            //解绑按钮导航和按钮确认InputAction的回调函数
            this.UIBtnList.DeBindInputAction();

            ML.Engine.Input.InputManager.Instance.Common.Option.Disable();

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }

        protected override void RegisterInput()
        {
            //GraphicBtn
            this.UIBtnList.SetBtnAction("GraphicBtn",
            () =>
            {
                Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/MainInteract/GraphPanel.prefab").Completed += (handle) =>
                {
                    // 实例化
                    var panel = handle.Result.GetComponent<ControllerPanel>();

                    panel.transform.SetParent(GameManager.Instance.UIManager.GetCanvas.transform, false);

                    GameManager.Instance.UIManager.PushPanel(panel);
                };
            }
            );
            //AudioBtn
            this.UIBtnList.SetBtnAction("AudioBtn",
            () =>
            {
                //GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.PopUpUI, new UIManager.PopUpUIData("确定取消该订单吗？", "您将面临违约惩罚", null, () => { Debug.Log("确定响应！"); }));
 /*               Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("ML/BaseUIPanel/GridNavagation/GridTestPanelA.prefab").Completed += (handle) =>
                {
                    // 实例化
                    var panel = handle.Result.GetComponent<TestPanelA>();

                    panel.transform.SetParent(GameManager.Instance.UIManager.GetCanvas.transform, false);

                    GameManager.Instance.UIManager.PushPanel(panel);
                };*/
                Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/MainInteract/AudioPanel.prefab").Completed += (handle) =>
                {
                    // 实例化
                    var panel = handle.Result.GetComponent<ControllerPanel>();

                    panel.transform.SetParent(GameManager.Instance.UIManager.GetCanvas.transform, false);

                    GameManager.Instance.UIManager.PushPanel(panel);
                };
            }
            );
            //ControllerBtn
            this.UIBtnList.SetBtnAction("ControllerBtn",
            () =>
            {

                Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/MainInteract/ControllerPanel.prefab").Completed += (handle) =>
                {
                    // 实例化
                    var panel = handle.Result.GetComponent<ControllerPanel>();

                    panel.transform.SetParent(GameManager.Instance.UIManager.GetCanvas.transform, false);

                    GameManager.Instance.UIManager.PushPanel(panel);
                };
            }
            );
            //TutorialBtn
            this.UIBtnList.SetBtnAction("TutorialBtn",
            () =>
            {
                GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.FloatTextUI, new UIManager.FloatTextUIData("message1"));

                /*                Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("ML/BaseUIPanel/GridNavagation/GridTestPanelB.prefab").Completed += (handle) =>
                                {
                                    // 实例化
                                    var panel = handle.Result.GetComponent<TestPanelB>();

                                    panel.transform.SetParent(GameManager.Instance.UIManager.GetCanvas.transform, false);

                                    GameManager.Instance.UIManager.PushPanel(panel);
                                };*/
                Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/MainInteract/TutorialPanel.prefab").Completed += (handle) =>
                {
                    // 实例化
                    var panel = handle.Result.GetComponent<ControllerPanel>();

                    panel.transform.SetParent(GameManager.Instance.UIManager.GetCanvas.transform, false);

                    GameManager.Instance.UIManager.PushPanel(panel);
                };
            }
            );
            //BackBtn
            this.UIBtnList.SetBtnAction("BackBtn",
            () =>
            {
                if (GameManager.Instance.LevelSwitchManager.CurSceneName == "EnterPointScene")
                {
                    GameManager.Instance.UIManager.PopPanel();
                }
                else
                {
                    UIBasePanel loadingpanel = null;
                    System.Action<string, string> preCallback = (string s1, string s2) =>
                    {
                        //新游戏的临时存档，保存当前新游戏存档时，覆盖之前的savegame，并开始使用savegame存档，即当前使用存档为savegame
#pragma warning disable CS4014
                        SC.CreateSaveDataFolderAsync(1, "savegame", () => { Debug.Log("存入savegame！"); });
#pragma warning restore CS4014

                        GameManager.Instance.EnterPoint.GetLoadingScenePanelInstance().Completed += (handle) =>
                        {
                            // 实例化
                            loadingpanel = handle.Result.GetComponent<LoadingScenePanel>();

                            loadingpanel.transform.SetParent(GameManager.Instance.UIManager.GetCanvas.transform, false);

                            loadingpanel.OnEnter();


                        };

                    };
                    System.Action<string, string> postCallback = async (string s1, string s2) =>
                    {
                        await UniTask.RunOnThreadPool(() =>
                        {
                            while (loadingpanel == null) ;
                        });
                        loadingpanel.OnExit();

                        GameManager.Instance.EnterPoint.GetStartMenuPanelInstance().Completed += (handle) =>
                        {
                            // 实例化
                            var panel = handle.Result.GetComponent<StartMenuPanel>();

                            panel.transform.SetParent(GameManager.Instance.UIManager.GetCanvas.transform, false);

                            GameManager.Instance.UIManager.ChangeBotUIPanel(panel);

                            //panel.OnEnter();
                        };
                    };
                    GameManager.Instance.UIManager.PopPanel();//Pop OptionPanel
                    GameManager.Instance.StartCoroutine(GameManager.Instance.LevelSwitchManager.LoadSceneAsync("EnterPointScene", preCallback, postCallback,isDelay: true));

                }
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

            ML.Engine.Input.InputManager.Instance.Common.Option.Enable();

            //绑定按钮导航和按钮确认InputAction的回调函数

            this.UIBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Option.SwichBtn, BindType.started);
            this.UIBtnList.BindButtonInteractInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.Confirm, BindType.started);
            
            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            GameManager.Instance.UIManager.PopPanel();
        }

        #endregion

        #region UI

        #region UI对象引用

        private TMPro.TextMeshProUGUI ToptitleText;



        #endregion

        public override void Refresh()
        {
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit)
            {
                return;
            }
        }
        #endregion

        #region SaveSystem
        SaveController SC => GameManager.Instance.SaveManager.SaveController;
        //下标0 为newgame 1为savegame

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

        protected override void OnLoadJsonAssetComplete(OptionPanelStruct datas)
        {
            base.OnLoadJsonAssetComplete(datas);
            InitBtnData(datas);
        }
        protected override void InitTextContentPathData()
        {
            this.abpath = "ML/Json/TextContent";
            this.abname = "OptionPanel";
            this.description = "OptionPanel数据加载完成";
        }
        private UIBtnList UIBtnList;

        protected override void InitBtnInfo()
        {
            UIBtnListInitor uIBtnListInitor = this.transform.GetComponentInChildren<UIBtnListInitor>(true);
            this.UIBtnList = new UIBtnList(uIBtnListInitor);
        }
        private void InitBtnData(OptionPanelStruct datas)
        {
            foreach (var tt in datas.Btns)
            {
                this.UIBtnList.SetBtnText(tt.name, tt.description.GetText());
            }
        }
        #endregion



    }

}
