using ML.Engine.Manager;
using ML.Engine.Timer;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ML.Engine.UI.LoadingScenePanel;



namespace ML.Engine.UI
{
    public class LoadingScenePanel : ML.Engine.UI.UIBasePanel<LoadingScenePanelStruct>, ITickComponent
    {
        #region Unity
        public bool IsInit = false;

        protected override void Awake()
        {
            slider = this.transform.Find("Slider").GetComponent<Slider>();

            LoadText = this.transform.Find("LoadText").GetComponent<TextMeshProUGUI>();
            ProgressText = this.transform.Find("ProgressText").GetComponent<TextMeshProUGUI>();
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
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
            base.Enter();
            
        }

        protected override void Exit()
        {
            ML.Engine.Manager.GameManager.Instance.TickManager.UnregisterTick(this);
            base.Exit();
            
        }
        #endregion

        #region Tick
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }

        public void Tick(float deltatime)
        {
            if (GameManager.Instance.LevelSwitchManager.SceneHandle.IsValid())
            {
                float progress = Mathf.Clamp01(GameManager.Instance.LevelSwitchManager.SceneHandle.PercentComplete);
                ProgressText.text = ((int)progress * 100).ToString() + "%";
                slider.value = progress;
            }

        }
        #endregion

        #region UI

        #region UI对象引用
        private Slider slider;
        private TMPro.TextMeshProUGUI LoadText;
        private TMPro.TextMeshProUGUI ProgressText;
        #endregion

        public override void Refresh()
        {
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit)
            {
                return;
            }
            LoadText.text = PanelTextContent.LoadText;
        }
        #endregion

        #region TextContent
        [System.Serializable]
        public struct LoadingScenePanelStruct
        {
            public ML.Engine.TextContent.TextContent LoadText;
        }

        protected override void InitTextContentPathData()
        {
            this.abpath = "TextContent";
            this.abname = "LoadingScenePanel";
            this.description = "LoadingScenePanel数据加载完成";
        }
        #endregion

    }

}
