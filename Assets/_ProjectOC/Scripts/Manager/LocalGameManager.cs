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
using ProjectOC.TechTree;
using ML.Engine.Manager;
using ProjectOC.Order;
using ProjectOC.ClanNS;
using ML.Engine.InventorySystem.CompositeSystem;
using ProjectOC.PinchFace;
using ProjectOC.Player;


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
        public OrderManager OrderManager;
        public ItemManager ItemManager;
        public CompositeManager CompositeManager;
        public PinchFace.PinchFaceManager PinchFaceManager;
        public RestaurantNS.RestaurantManager RestaurantManager;

        /// <summary>
        /// ��������
        /// </summary>
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
            //����Character
            GameManager.Instance.CharacterManager.SceneInit();

            
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
                GM?.UnregisterLocalManager<MissionManager>();
                GM?.UnregisterLocalManager<ProNodeManager>();
                GM?.UnregisterLocalManager<RecipeManager>();
                GM?.UnregisterLocalManager<StoreManager>();
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
                GM?.UnregisterLocalManager<IslandAreaManager>();
                GM?.UnregisterLocalManager<OrderManager>();
                GM?.UnregisterLocalManager<ItemManager>();
                GM?.UnregisterLocalManager<CompositeManager>();
                GM?.UnregisterLocalManager<PinchFaceManager>(); //���ܻ���ǰע�����ر���������ʱ��
                GM?.UnregisterLocalManager<RestaurantNS.RestaurantManager>();
                Instance = null;
            }
        }
        
        //��PlayerCharacter����֮�����
        IEnumerator AfterPlayerCharacter()
        {
            // Debug.Log(GameManager.Instance.CharacterManager.GetLocalController().GetType() );
            ProjectOC.Player.OCPlayerController playerController = GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController;
            
            while (playerController.currentCharacter == null || !playerController.currentCharacter.LoadOver)
            {
                yield return null;
            }

            //Ҫ��ȡ���ģ�ͣ����ں���
            GM.RegisterLocalManager(IslandAreaManager);
            GM.RegisterLocalManager(PinchFaceManager);
/*            GM.RegisterLocalManager(OrderManager);*/
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
