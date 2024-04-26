using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;


namespace ProjectOC.ManagerNS
{
    [System.Serializable]
    public sealed class LocalGameManager : SerializedMonoBehaviour, ML.Engine.Manager.LocalManager.ILocalManager
    {
        public static LocalGameManager Instance;
        public ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;
        public DispatchTimeManager DispatchTimeManager;
        public MissionNS.MissionManager MissionManager;
        public ProNodeNS.ProNodeManager ProNodeManager;
        public ML.Engine.InventorySystem.RecipeManager RecipeManager;
        public StoreNS.StoreManager StoreManager;
        public WorkerNS.WorkerManager WorkerManager;
        public WorkerNS.EffectManager EffectManager;
        public WorkerNS.FeatureManager FeatureManager;
        public WorkerNS.SkillManager SkillManager;
        public WorkerEchoNS.WorkerEchoManager WorkerEchoManager;
        public ClanNS.ClanManager ClanManager;
        public ML.Engine.BuildingSystem.MonoBuildingManager MonoBuildingManager;
        public TechTree.TechTreeManager TechTreeManager;
        public ML.Engine.InventorySystem.ItemManager ItemManager;
        public ML.Engine.InventorySystem.CompositeSystem.CompositeManager CompositeManager;
        public LandMassExpand.IslandModelManager IslandManager;
        public LandMassExpand.BuildPowerIslandManager BuildPowerIslandManager;
        public RestaurantNS.RestaurantManager RestaurantManager;

        public IslandAreaManager IslandAreaManager;
        public Order.OrderManager OrderManager;
        public PinchFace.PinchFaceManager PinchFaceManager;
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
            GM.RegisterLocalManager(MissionManager);
            GM.RegisterLocalManager(ProNodeManager);
            GM.RegisterLocalManager(RecipeManager);
            GM.RegisterLocalManager(StoreManager);
            GM.RegisterLocalManager(WorkerManager);
            GM.RegisterLocalManager(EffectManager);
            GM.RegisterLocalManager(FeatureManager);
            GM.RegisterLocalManager(SkillManager);
            GM.RegisterLocalManager(WorkerEchoManager);
            GM.RegisterLocalManager(ClanManager);
            GM.RegisterLocalManager(MonoBuildingManager);
            GM.RegisterLocalManager(TechTreeManager);
            GM.RegisterLocalManager(ItemManager);
            GM.RegisterLocalManager(CompositeManager);
            GM.RegisterLocalManager(IslandManager);
            GM.RegisterLocalManager(BuildPowerIslandManager);
            GM.RegisterLocalManager(RestaurantManager);
            //生成Character
            ML.Engine.Manager.GameManager.Instance.CharacterManager.SceneInit();
            StartCoroutine(AfterPlayerCharacter());
        }

        private void Start()
        {

            //为了Editor渲染Gizoms
#if !UNITY_EDITOR
            this.enabled = false;
#endif
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                GM?.UnregisterLocalManager<DispatchTimeManager>();
                GM?.UnregisterLocalManager<MissionNS.MissionManager>();
                GM?.UnregisterLocalManager<ProNodeNS.ProNodeManager>();
                GM?.UnregisterLocalManager<ML.Engine.InventorySystem.RecipeManager>();
                GM?.UnregisterLocalManager<StoreNS.StoreManager>();
                GM?.UnregisterLocalManager<WorkerNS.WorkerManager>();
                GM?.UnregisterLocalManager<WorkerNS.EffectManager>();
                GM?.UnregisterLocalManager<WorkerNS.FeatureManager>();
                GM?.UnregisterLocalManager<WorkerNS.SkillManager>();
                GM?.UnregisterLocalManager<WorkerEchoNS.WorkerEchoManager>();
                GM?.UnregisterLocalManager<ClanNS.ClanManager>();
                GM?.UnregisterLocalManager<ML.Engine.BuildingSystem.MonoBuildingManager>();
                GM?.UnregisterLocalManager<TechTree.TechTreeManager>();
                GM?.UnregisterLocalManager<ML.Engine.InventorySystem.ItemManager>();
                GM?.UnregisterLocalManager<ML.Engine.InventorySystem.CompositeSystem.CompositeManager>();
                GM?.UnregisterLocalManager<LandMassExpand.IslandModelManager>();
                GM?.UnregisterLocalManager<LandMassExpand.BuildPowerIslandManager>();
                GM?.UnregisterLocalManager<RestaurantNS.RestaurantManager>();
                GM?.UnregisterLocalManager<LocalGameManager>();

                GM?.UnregisterLocalManager<IslandAreaManager>();
                GM?.UnregisterLocalManager<Order.OrderManager>();
                //可能会提前注销，关闭捏脸面板的时候
                GM?.UnregisterLocalManager<PinchFace.PinchFaceManager>();
                Instance = null;
            }
        }
        
        //在PlayerCharacter生成之后调用
        IEnumerator AfterPlayerCharacter()
        {
            // Debug.Log(GameManager.Instance.CharacterManager.GetLocalController().GetType() );
            Player.OCPlayerController playerController = ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController;
            
            while (playerController.currentCharacter == null || !playerController.currentCharacter.LoadOver)
            {
                yield return null;
            }

            //要获取玩家模型，放在后面
            GM.RegisterLocalManager(IslandAreaManager);
            GM.RegisterLocalManager(PinchFaceManager);
            GM.RegisterLocalManager(OrderManager);
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
        [Space(20)]
        public GizmosEnableControl gizmosEnableControl = GizmosEnableControl.IslandManager; 
        private void OnDrawGizmos()
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
