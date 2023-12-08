using ML.Engine.Manager.LocalManager;
using ML.Engine.Manager;
using ProjectOC.StoreNS;
using ProjectOC.ProductionNodeNS;
using ProjectOC.ItemNS;
using ProjectOC.WorkerNS;
using ProjectOC.MissionNS;
using UnityEngine;


namespace ProjectOC.ManagerNS
{
    [System.Serializable]
    public sealed class LocalGameManager : MonoBehaviour, ILocalManager
    {
        public DispatchTimeManager DispatchTimeManager { get; private set; }

        public MissionBroadCastManager MissionBroadCastManager { get; private set; }

        public ProductionNodeManager ProductionNodeManager { get; private set; }

        public StoreManager StoreManager { get; private set; }
        public RecipeManager RecipeManager { get; private set; }
        public WorkerManager WorkerManager { get; private set; }
        public FeatureManager FeatureManager { get; private set; }


        void Start()
        {
            GameManager.Instance.RegisterLocalManager(this);

            DispatchTimeManager = GameManager.Instance.RegisterLocalManager<DispatchTimeManager>();

            MissionBroadCastManager = GameManager.Instance.RegisterLocalManager<MissionBroadCastManager>();

            ProductionNodeManager = GameManager.Instance.RegisterLocalManager<ProductionNodeManager>();

            StoreManager = GameManager.Instance.RegisterLocalManager<StoreManager>();

            RecipeManager = GameManager.Instance.RegisterLocalManager<RecipeManager>();

            WorkerManager = GameManager.Instance.RegisterLocalManager<WorkerManager>();
            
            FeatureManager = GameManager.Instance.RegisterLocalManager<FeatureManager>();
        }
    }
}


