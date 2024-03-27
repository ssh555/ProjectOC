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


        #region Internal
        protected override void UnregisterInput()
        {
            ML.Engine.Input.InputManager.Instance.Common.StartMenu.Disable();

            //�л���ť
            ML.Engine.Input.InputManager.Instance.Common.StartMenu.SwichBtn.started -= this.UIBtnList.SwichBtn_started;

            //ȷ��
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= this.UIBtnList.Confirm_performed;

        }
        protected override void RegisterInput()
        {
            ML.Engine.Input.InputManager.Instance.Common.StartMenu.Enable();

            
        }
        #endregion


        #region SaveSystem
        SaveController SC => GameManager.Instance.SaveManager.SaveController;
        //�±�0 Ϊnewgame 1Ϊsavegame
        private void InitSaveSystemData()
        {
            
            if (SC.SaveDataFolders[1] == null)
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
            //�л���ť
            ML.Engine.Input.InputManager.Instance.Common.StartMenu.SwichBtn.started += this.UIBtnList.SwichBtn_started;

            //ȷ��
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += this.UIBtnList.Confirm_performed;
        }
        protected override void InitTextContentPathData()
        {
            this.abpath = "ML/Json/TextContent";
            this.abname = "StartMenuPanel";
            this.description = "StartMenuPanel���ݼ������";
        }
        private Transform btnList;
        [ShowInInspector]
        private UIBtnList UIBtnList;
        private void InitBtnData(StartMenuPanelStruct datas)
        {

            Action OnslectedEnter = () => { };
            Action OnslectedExit = () => { };

            UIBtnList = new UIBtnList(parent: btnList);
            foreach (var tt in datas.Btns)
            {
                this.UIBtnList.SetBtnText(tt.name, tt.description.GetText());
            }

            //NewGameBtn
            this.UIBtnList.SetBtnAction("NewGameBtn",
            () =>
            {
                UIBasePanel panel = null;
                System.Action<string, string> preCallback = async (string s1, string s2) =>
                {
                    //��ʼ����Ϸʱ��ɾ��֮ǰ��newgame�浵����0��ʼ�µ�newgame�浵
                    if (SC.SaveDataFolders[0] != null)
                    {
                        await SC.DeleteSaveDataFolderAsync(0);
                    }
                    SC.CreateSaveDataFolder(0, "newgame", async () => { 
                        Debug.Log("����newgame��");

                        await SC.SelectSaveDataFolderAsync(0, null);

                    });

                    GameManager.Instance.EnterPoint.GetLoadingScenePanelInstance().Completed += (handle) =>
                    {
                        // ʵ����
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

                await SC.SelectSaveDataFolderAsync(1, null);
                UIBasePanel panel = null;
                System.Action<string, string> preCallback = async (string s1, string s2) =>
                {
                    
                    GameManager.Instance.EnterPoint.GetLoadingScenePanelInstance().Completed += (handle) =>
                    {
                        // ʵ����
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
                    // ʵ����
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
                                UnityEditor.EditorApplication.isPlaying = false;//�������unity��������
                #else
                                        Application.Quit();//�����ڴ���ļ���
                #endif
            }
            );

            //InitSaveSystemData();

        }

        #endregion
    }

}
