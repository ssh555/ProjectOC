using Cysharp.Threading.Tasks;
using ML.Engine.Manager;
using ML.Engine.SaveSystem;
using ML.Engine.TextContent;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ML.Engine.UI.OptionPanel;

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
            btnList = this.transform.Find("ButtonList");
            gridLayout = btnList.GetComponent<GridLayoutGroup>();
        }

        protected override void Start()
        {
            IsInit = true;
            Refresh();
            base.Start();
        }

        #endregion

        #region Override
        public override void OnEnter()
        {
            UIBtnList = new UIBtnList(parent: btnList, limitNum: gridLayout.constraintCount);
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

            ML.Engine.Input.InputManager.Instance.Common.Option.Disable();

            //���ť�����Ͱ�ťȷ��InputAction�Ļص�����
            this.UIBtnList.DeBindInputAction();

            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }

        protected override void RegisterInput()
        {
            //GraphicBtn
            this.UIBtnList.SetBtnAction("GraphicBtn",
            () =>
            {
                GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.SideBarUI, new UIManager.SideBarUIData("<color=yellow>��������</color>  �����˽�������", "��������"));
            }
            );
            //AudioBtn
            this.UIBtnList.SetBtnAction("AudioBtn",
            () =>
            {
                GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.PopUpUI, new UIManager.PopUpUIData("ȷ��ȡ���ö�����", "��������ΥԼ�ͷ�", null, () => { Debug.Log("ȷ����Ӧ��"); }));
            }
            );
            //ControllerBtn
            this.UIBtnList.SetBtnAction("ControllerBtn",
            () =>
            {
                GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.BtnUI, new UIManager.BtnUIData("message1", () => { Debug.Log("��ť��Ӧ��"); }));
            }
            );
            //TutorialBtn
            this.UIBtnList.SetBtnAction("TutorialBtn",
            () =>
            {
                GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.FloatTextUI, new UIManager.FloatTextUIData("message1"));
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
                        //����Ϸ����ʱ�浵�����浱ǰ����Ϸ�浵ʱ������֮ǰ��savegame������ʼʹ��savegame�浵������ǰʹ�ô浵Ϊsavegame

                        SC.CreateSaveDataFolder(1, "savegame", () => { Debug.Log("����savegame��"); });

                        GameManager.Instance.EnterPoint.GetLoadingScenePanelInstance().Completed += (handle) =>
                        {
                            // ʵ����
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
                            // ʵ����
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
                            UnityEditor.EditorApplication.isPlaying = false;//�������unity��������
            #else
                                    Application.Quit();//�����ڴ���ļ���
            #endif
                        }
                        );

            ML.Engine.Input.InputManager.Instance.Common.Option.Enable();

            //�󶨰�ť�����Ͱ�ťȷ��InputAction�Ļص�����

            this.UIBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Option.SwichBtn, UIBtnList.BindType.started);
            this.UIBtnList.BindButtonInteractInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.Confirm, UIBtnList.BindType.started);
            
            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            GameManager.Instance.UIManager.PopPanel();
        }

        #endregion

        #region UI

        #region UI��������

        private GridLayoutGroup gridLayout;

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
        //�±�0 Ϊnewgame 1Ϊsavegame

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
            this.description = "OptionPanel���ݼ������";
        }
        private Transform btnList;
        private UIBtnList UIBtnList; 
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
