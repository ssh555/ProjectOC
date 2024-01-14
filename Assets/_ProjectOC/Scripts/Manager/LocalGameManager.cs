using ML.Engine.Manager.LocalManager;
using ML.Engine.Manager;
using ProjectOC.MissionNS;
using UnityEngine;
using ProjectOC.WorkerNS;
using ProjectOC.StoreNS;
using ProjectOC.WorkerEchoNS;

namespace ProjectOC.ManagerNS
{
    [System.Serializable]
    public sealed class LocalGameManager : MonoBehaviour, ILocalManager
    {
        public DispatchTimeManager DispatchTimeManager { get; private set; }
        public MissionManager MissionBroadCastManager { get; private set; }
        public WorkerManager WorkerManager { get; private set; }
        public StoreManager StoreManager { get; private set; }
        public WorkerEcho WorkerEcho { get; private set; }
        void Start()
        {
            GameManager.Instance.RegisterLocalManager(this);
            DispatchTimeManager = GameManager.Instance.RegisterLocalManager<DispatchTimeManager>();
            MissionBroadCastManager = GameManager.Instance.RegisterLocalManager<MissionManager>();
            WorkerManager = GameManager.Instance.RegisterLocalManager<WorkerManager>();
            StoreManager = GameManager.Instance.RegisterLocalManager<StoreManager>();
            WorkerEcho = GameManager.Instance.RegisterLocalManager<WorkerEcho>();

            this.enabled = false;
        }
    }
}


