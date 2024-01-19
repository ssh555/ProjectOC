using ProjectOC.MissionNS;
using UnityEngine;
using ProjectOC.WorkerNS;
using ProjectOC.StoreNS;
using ProjectOC.WorkerEchoNS;
using ProjectOC.ProNodeNS;
using ML.Engine.InventorySystem;

namespace ProjectOC.ManagerNS
{
    [System.Serializable]
    public sealed class LocalGameManager : MonoBehaviour, ML.Engine.Manager.LocalManager.ILocalManager
    {
        public static LocalGameManager Instance;
        public ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;
        public DispatchTimeManager DispatchTimeManager { get; private set; }
        public MissionManager MissionManager { get; private set; }
        public ProNodeManager ProNodeManager { get; private set; }
        public RecipeManager RecipeManager { get; private set; }
        public StoreManager StoreManager { get; private set; }
        public WorkerManager WorkerManager { get; private set; }
        public EffectManager EffectManager { get; private set; }
        public FeatureManager FeatureManager { get; private set; }
        public SkillManager SkillManager { get; private set; }
        public WorkerEcho WorkerEcho { get; private set; }

        /// <summary>
        /// 单例管理
        /// </summary>
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this.gameObject);
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
            MissionManager = GM.RegisterLocalManager<MissionManager>();
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
            WorkerEcho = GM.RegisterLocalManager<WorkerEcho>();
            this.enabled = false;
        }
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}


