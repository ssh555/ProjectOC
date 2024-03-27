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

        #endregion

        #region Internal
        protected override void UnregisterInput()
        {
            ML.Engine.Input.InputManager.Instance.Common.Option.Disable();


            //�л���ť
            ML.Engine.Input.InputManager.Instance.Common.Option.SwichBtn.started -= SwichBtn_started;

            //ȷ��
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;

            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }

        protected override void RegisterInput()
        {
            ML.Engine.Input.InputManager.Instance.Common.Option.Enable();
            //�л���ť
            ML.Engine.Input.InputManager.Instance.Common.Option.SwichBtn.started += SwichBtn_started;

            //ȷ��
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;

            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

        }
        public void SwichBtn_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            string actionName = obj.action.name;

            // ʹ�� ReadValue<T>() ������ȡ��������
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
            UIBtnList = new UIBtnList(parent: btnList, limitNum: gridLayout.constraintCount);
            foreach (var tt in datas.Btns)
            {
                this.UIBtnList.SetBtnText(tt.name, tt.description.GetText());
            }

            //GraphicBtn
            this.UIBtnList.SetBtnAction("GraphicBtn",
            () =>
                {
                    GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.SideBarUI, "GraphicBtn");
                }
            );
            //AudioBtn
            this.UIBtnList.SetBtnAction("AudioBtn",
            () =>
            {
                GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.PopUpUI, "AudioBtn");
            }
            );
            //ControllerBtn
            this.UIBtnList.SetBtnAction("ControllerBtn",
            () =>
            {
                GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.FloatTextUI, "ControllerBtn");
            }
            );
            //TutorialBtn
            this.UIBtnList.SetBtnAction("TutorialBtn",
            () =>
            {
                GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.FloatTextUI, "TutorialBtn");
            }
            );
            //BackBtn
            this.UIBtnList.SetBtnAction("BackBtn",
            () =>
            {
                if(GameManager.Instance.LevelSwitchManager.CurSceneName == "EnterPointScene")
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
                    GameManager.Instance.StartCoroutine(GameManager.Instance.LevelSwitchManager.LoadSceneAsync("EnterPointScene", preCallback, postCallback));

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
 
        }
        #endregion



    }

}
