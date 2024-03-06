using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.Timer;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;



namespace ML.Engine.UI
{
    public class LoadingScenePanel : ML.Engine.UI.UIBasePanel, ITickComponent
    {
        #region Unity
        public bool IsInit = false;

        private void Awake()
        {
            //DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            
            InitUITextContents();


            slider = this.transform.Find("Slider").GetComponent<Slider>();

            LoadText = this.transform.Find("LoadText").GetComponent<TextMeshProUGUI>();
            ProgressText = this.transform.Find("ProgressText").GetComponent<TextMeshProUGUI>();

            IsInit = true;
            Refresh();
        }

        #endregion

        #region Override
        public override void OnEnter()
        {
            base.OnEnter();
            
            this.Enter();

            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);

        }

        public override void OnExit()
        {
            base.OnExit();

            this.Exit();
            ML.Engine.Manager.GameManager.Instance.TickManager.UnregisterTick(this);
        }

        public override void OnPause()
        {
            base.OnPause();
            this.Exit();
            ML.Engine.Manager.GameManager.Instance.TickManager.UnregisterTick(this);
        }

        public override void OnRecovery()
        {
            base.OnRecovery();
            this.Enter();
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
        }

        #endregion


        #region Tick
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }

        public void Tick(float deltatime)
        {

            if (GameManager.Instance.LevelSwitchManager.ao != null)
            {
                float progress = Mathf.Clamp01(GameManager.Instance.LevelSwitchManager.ao.progress / 0.9f);
                ProgressText.text = ((int)progress * 100).ToString() + "%";
                slider.value = progress;
            }

        }

        #endregion




        #region Internal

        private void Enter()
        {
            this.RegisterInput();
            this.Refresh();
        }

        private void Exit()
        {
            this.UnregisterInput();

        }

        private void UnregisterInput()
        {
        }

        private void RegisterInput()
        {

        }

        #endregion

        #region UI


        #region UI��������
        private Slider slider;


        private TMPro.TextMeshProUGUI LoadText;
        private TMPro.TextMeshProUGUI ProgressText;


        #endregion

        public override void Refresh()
        {
            if (ABJAProcessorJson_StartMenuPanel == null || !ABJAProcessorJson_StartMenuPanel.IsLoaded || !IsInit)
            {
                Debug.Log("ABJAProcessorJson is null");
                return;
            }

            LoadText.text = PanelTextContent_StartMenuPanel.LoadText;

        }
        #endregion



        #region TextContent
        [System.Serializable]
        public struct LoadingScenePanelStruct
        {
            public ML.Engine.TextContent.TextContent LoadText;
        }

        public static LoadingScenePanelStruct PanelTextContent_StartMenuPanel => ABJAProcessorJson_StartMenuPanel.Datas;
        public static ML.Engine.ABResources.ABJsonAssetProcessor<LoadingScenePanelStruct> ABJAProcessorJson_StartMenuPanel;
        private void InitUITextContents()
        {
            if (ABJAProcessorJson_StartMenuPanel == null)
            {
                ABJAProcessorJson_StartMenuPanel = new ML.Engine.ABResources.ABJsonAssetProcessor<LoadingScenePanelStruct>("Json/TextContent/LoadingScenePanel", "LoadingScenePanel", (datas) =>
                {
                    Refresh();
                    this.enabled = false;
                }, null, "LoadingScenePanel����");
                ABJAProcessorJson_StartMenuPanel.StartLoadJsonAssetData();
            }

        }
        #endregion



    }

}
