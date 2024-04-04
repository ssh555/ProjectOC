using System;
using System.Collections;
using System.Collections.Generic;
using ML.Engine.BuildingSystem;
using ProjectOC.MissionNS;
using UnityEngine;
using ProjectOC.WorkerNS;
using ProjectOC.StoreNS;
using ProjectOC.WorkerEchoNS;
using ProjectOC.ProNodeNS;
using ML.Engine.InventorySystem;
using ProjectOC.LandMassExpand;
using Sirenix.OdinInspector;
using ML.Engine.UI;
using ProjectOC.TechTree;
using ML.Engine.Manager;
using ProjectOC.ClanNS;

namespace ProjectOC.ManagerNS
{
    [System.Serializable]
    public sealed class LocalGameManager : SerializedMonoBehaviour, ML.Engine.Manager.LocalManager.ILocalManager
    {
        public static LocalGameManager Instance;
        public ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;
        public DispatchTimeManager DispatchTimeManager;
        public MissionManager MissionManager;
        public ProNodeManager ProNodeManager;
        public RecipeManager RecipeManager;
        public StoreManager StoreManager;
        public WorkerManager WorkerManager;
        public EffectManager EffectManager;
        public FeatureManager FeatureManager;
        public SkillManager SkillManager;
        public WorkerEchoManager WorkerEchoManager;
        public ClanManager ClanManager;
        public IslandAreaManager IslandAreaManager;
        public BuildPowerIslandManager BuildPowerIslandManager;
        public IslandModelManager IslandManager;
        public MonoBuildingManager MonoBuildingManager;
        public TechTreeManager TechTreeManager;
       
        /// <summary>
        /// 单例管理
        /// </summary>
        private void Awake()
        {
            if (Instance != null)
            {
                ML.Engine.Manager.GameManager.DestroyObj(this.gameObject);
                return;
            }
            Instance = this;
            //// TODO : 退出LocalGameManager的使用场景之后，要手动销毁掉
            //DontDestroyOnLoad(this);
            GM.RegisterLocalManager(this);
            GM.RegisterLocalManager(DispatchTimeManager);
            DispatchTimeManager.Init();
            GM.RegisterLocalManager(MissionManager);
            MissionManager.Init();
            GM.RegisterLocalManager(ProNodeManager);
            ProNodeManager.LoadTableData();
            GM.RegisterLocalManager(RecipeManager);
            RecipeManager.LoadTableData();
            GM.RegisterLocalManager(StoreManager);
            GM.RegisterLocalManager(WorkerManager);
            GM.RegisterLocalManager(EffectManager);
            EffectManager.LoadTableData();
            GM.RegisterLocalManager(FeatureManager);
            FeatureManager.LoadTableData();
            GM.RegisterLocalManager(SkillManager);
            SkillManager.LoadTableData();
            GM.RegisterLocalManager(WorkerEchoManager);
            WorkerEchoManager.LoadTableData();
            GM.RegisterLocalManager(ClanManager);
            GM.RegisterLocalManager(MonoBuildingManager);
            MonoBuildingManager.Init();
            GM.RegisterLocalManager(TechTreeManager);
            TechTreeManager.Init();
            GM.RegisterLocalManager(IslandManager);
            IslandManager.Init();
            GM.RegisterLocalManager(BuildPowerIslandManager);
            GM.RegisterLocalManager(IslandAreaManager);
        }

        private void Start()
        {
            GameManager.Instance.CharacterManager.Scene1Init();

#if !UNITY_EDITOR
            this.enabled = false;
#endif
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                GM?.UnregisterLocalManager<DispatchTimeManager>();
                GM?.UnregisterLocalManager<MissionManager>();
                GM?.UnregisterLocalManager<ProNodeManager>();
                GM?.UnregisterLocalManager<RecipeManager>();
                GM?.UnregisterLocalManager<StoreManager>();
                WorkerManager?.DeleteAllWorker();
                GM?.UnregisterLocalManager<WorkerManager>();
                GM?.UnregisterLocalManager<EffectManager>();
                GM?.UnregisterLocalManager<FeatureManager>();
                GM?.UnregisterLocalManager<SkillManager>();
                GM?.UnregisterLocalManager<WorkerEchoManager>();
                GM?.UnregisterLocalManager<ClanManager>();
                GM?.UnregisterLocalManager<LocalGameManager>();
                GM?.UnregisterLocalManager<IslandAreaManager>();
                GM?.UnregisterLocalManager<IslandModelManager>();
                GM?.UnregisterLocalManager<BuildPowerIslandManager>();
                Instance = null;
            }
        }


        #region Gizmos管理
#if UNITY_EDITOR
        [System.Flags]
        public enum GizmosEnableControl
        {
            [LabelText("All")]
            All = int.MaxValue,
            [LabelText("None")]
            None = 0,
            [LabelText("岛屿范围")]
            IslandManager = 1 << 0,
            [LabelText("Test2")]
            Test2 = 1 << 1
        }

        public GizmosEnableControl gizmosEnableControl; 
        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying && (GizmosEnableControl.IslandManager & gizmosEnableControl) != 0)
            {
                IslandManager.OnDrawGizmosSelected();       
            }
            if((GizmosEnableControl.Test2 & gizmosEnableControl) != 0)
            {
                Debug.Log("gizmosEnableControl 2");
            }
        }
#endif
        #endregion

    }
}


