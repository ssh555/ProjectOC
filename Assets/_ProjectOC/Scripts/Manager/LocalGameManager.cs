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
        public WorkerNS.WorkerEchoManager WorkerEchoManager;
        public ClanNS.ClanManager ClanManager;
        public ML.Engine.BuildingSystem.MonoBuildingManager MonoBuildingManager;
        public TechTree.TechTreeManager TechTreeManager;
        public ML.Engine.InventorySystem.ItemManager ItemManager;
        public LandMassExpand.IslandModelManager IslandManager;
        public LandMassExpand.BuildPowerIslandManager BuildPowerIslandManager;
        public RestaurantNS.RestaurantManager RestaurantManager;

        public IslandAreaManager IslandAreaManager;
        public Order.OrderManager OrderManager;
        public PinchFace.PinchFaceManager PinchFaceManager;
        public Dialog.DialogManager DialogManager;
        public Player.OCPlayerController Player => (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController);


        private void Awake()
        {
            if (Instance != null)
            {
                ML.Engine.Manager.GameManager.DestroyObj(this.gameObject);
                return;
            }
            Instance = this;
            //// TODO : �˳�LocalGameManager��ʹ�ó���֮��Ҫ�ֶ����ٵ�
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
            GM.RegisterLocalManager(WorkerEchoManager);
            GM.RegisterLocalManager(ClanManager);
            GM.RegisterLocalManager(MonoBuildingManager);
            GM.RegisterLocalManager(TechTreeManager);
            GM.RegisterLocalManager(ItemManager);
            GM.RegisterLocalManager(IslandManager);
            GM.RegisterLocalManager(BuildPowerIslandManager);
            GM.RegisterLocalManager(RestaurantManager);
            GM.RegisterLocalManager(DialogManager);
            ML.Engine.Manager.GameManager.Instance.CharacterManager.SceneInit();
            StartCoroutine(AfterPlayerCharacter());
        }

        private void Start()
        {

            //Ϊ��Editor��ȾGizoms
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
                GM?.UnregisterLocalManager<WorkerNS.WorkerEchoManager>();
                GM?.UnregisterLocalManager<ClanNS.ClanManager>();
                GM?.UnregisterLocalManager<ML.Engine.BuildingSystem.MonoBuildingManager>();
                GM?.UnregisterLocalManager<TechTree.TechTreeManager>();
                GM?.UnregisterLocalManager<ML.Engine.InventorySystem.ItemManager>();
                GM?.UnregisterLocalManager<LandMassExpand.IslandModelManager>();
                GM?.UnregisterLocalManager<LandMassExpand.BuildPowerIslandManager>();
                GM?.UnregisterLocalManager<RestaurantNS.RestaurantManager>();
                GM?.UnregisterLocalManager<LocalGameManager>();

                GM?.UnregisterLocalManager<IslandAreaManager>();
                GM?.UnregisterLocalManager<Order.OrderManager>();
                //���ܻ���ǰע�����ر���������ʱ��
                GM?.UnregisterLocalManager<PinchFace.PinchFaceManager>();
                GM?.UnregisterLocalManager<Dialog.DialogManager>();
                Instance = null;
            }
        }
        
        //��PlayerCharacter����֮�����
        IEnumerator AfterPlayerCharacter()
        {
            // Debug.Log(GameManager.Instance.CharacterManager.GetLocalController().GetType() );
            Player.OCPlayerController playerController = ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController;
            
            while (playerController.currentCharacter == null || !playerController.currentCharacter.LoadOver)
            {
                yield return null;
            }

            //Ҫ��ȡ���ģ�ͣ����ں���
            GM.RegisterLocalManager(IslandAreaManager);
            GM.RegisterLocalManager(PinchFaceManager);
            GM.RegisterLocalManager(OrderManager);
        }
    
        #region Gizmos����
#if UNITY_EDITOR
        [System.Flags]
        public enum GizmosEnableControl
        {
            [LabelText("All")]
            All = int.MaxValue,
            [LabelText("None")]
            None = 0,
            [LabelText("���췶Χ")]
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
