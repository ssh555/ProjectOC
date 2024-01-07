using ML.Engine.Manager.LocalManager;
using ML.Engine.Manager;
using ProjectOC.MissionNS;
using UnityEngine;


namespace ProjectOC.ManagerNS
{
    [System.Serializable]
    public sealed class LocalGameManager : MonoBehaviour, ILocalManager
    {
        public DispatchTimeManager DispatchTimeManager { get; private set; }
        public MissionBroadCastManager MissionBroadCastManager { get; private set; }

        void Start()
        {
            GameManager.Instance.RegisterLocalManager(this);
            DispatchTimeManager = GameManager.Instance.RegisterLocalManager<DispatchTimeManager>();
            MissionBroadCastManager = GameManager.Instance.RegisterLocalManager<MissionBroadCastManager>();
        }
    }
}


