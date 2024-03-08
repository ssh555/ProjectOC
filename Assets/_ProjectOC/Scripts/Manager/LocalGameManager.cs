using System;
using System.Collections;
using System.Collections.Generic;
using ProjectOC.MissionNS;
using UnityEngine;
using ProjectOC.WorkerNS;
using ProjectOC.StoreNS;
using ProjectOC.WorkerEchoNS;
using ProjectOC.ProNodeNS;
using ML.Engine.InventorySystem;
using ProjectOC.LandMassExpand;
using Sirenix.OdinInspector;

namespace ProjectOC.ManagerNS
{
    [System.Serializable]
    public sealed class LocalGameManager : MonoBehaviour, ML.Engine.Manager.LocalManager.ILocalManager
    {
        public static LocalGameManager Instance;
        public ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;
        public DispatchTimeManager DispatchTimeManager { get; private set; }
        [ShowInInspector]
        public MissionManager MissionManager;
        public ProNodeManager ProNodeManager { get; private set; }
        public RecipeManager RecipeManager { get; private set; }
        public StoreManager StoreManager { get; private set; }
        public WorkerManager WorkerManager { get; private set; }
        public EffectManager EffectManager { get; private set; }
        public FeatureManager FeatureManager { get; private set; }
        public SkillManager SkillManager { get; private set; }
        public WorkerEchoManager WorkerEchoManager { get; private set; }
        public NavMeshManager NavMeshManager { get; private set; }
        public BuildPowerIslandManager BuildPowerIslandManager { get; private set; }
        public IslandManager IslandManager { get; private set; }
        /// <summary>
        /// 单例管理
        /// </summary>
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }
        /// <summary>
        /// 数据载入初始化
        /// </summary>
        private void Start()
        {
            GM.RegisterLocalManager(this);
            DispatchTimeManager = GM.RegisterLocalManager<DispatchTimeManager>();
            DispatchTimeManager.Init();
            MissionManager = GM.RegisterLocalManager<MissionManager>();
            MissionManager.Init();
            ProNodeManager = GM.RegisterLocalManager<ProNodeManager>();
            ProNodeManager.LoadTableData();
            RecipeManager = GM.RegisterLocalManager<RecipeManager>();
            RecipeManager.LoadTableData();
            StoreManager = GM.RegisterLocalManager<StoreManager>();
            WorkerManager = GM.RegisterLocalManager<WorkerManager>();
            EffectManager = GM.RegisterLocalManager<EffectManager>();
            EffectManager.LoadTableData();
            FeatureManager = GM.RegisterLocalManager<FeatureManager>();
            FeatureManager.LoadTableData();
            SkillManager = GM.RegisterLocalManager<SkillManager>();
            SkillManager.LoadTableData();
            WorkerEchoManager = GM.RegisterLocalManager<WorkerEchoManager>();
            WorkerEchoManager.LoadTableData();
            NavMeshManager = GM.RegisterLocalManager<NavMeshManager>();
            IslandManager = GM.RegisterLocalManager<IslandManager>();
            IslandManager.Init();
            BuildPowerIslandManager = GM.RegisterLocalManager<BuildPowerIslandManager>();
            StartCoroutine(DelayStart());
            this.enabled = false;
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
                GM?.UnregisterLocalManager<WorkerManager>();
                GM?.UnregisterLocalManager<EffectManager>();
                GM?.UnregisterLocalManager<FeatureManager>();
                GM?.UnregisterLocalManager<SkillManager>();
                GM?.UnregisterLocalManager<WorkerEchoManager>();
                GM?.UnregisterLocalManager<LocalGameManager>();
                GM?.UnregisterLocalManager<NavMeshManager>();
                GM?.UnregisterLocalManager<IslandManager>();
                GM?.UnregisterLocalManager<BuildPowerIslandManager>();
                Instance = null;
            }
        }

        IEnumerator DelayStart()
        {
            yield return null;
            NavMeshManager.DelayInit();
        }

        private void OnDrawGizmosSelected()
        {
            //IslandManager.OnDrawGizmosSelected();
        }
    }
}


