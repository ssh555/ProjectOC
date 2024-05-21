using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;

namespace ProjectOC.MissionNS
{
    [LabelText("任务调度管理器"), System.Serializable]
    public sealed class MissionManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        [LabelText("搬运任务列表"), ShowInInspector, ReadOnly]
        private HashSet<IMissionTransport> MissionTransports = new HashSet<IMissionTransport>();
        private ML.Engine.Timer.CounterDownTimer timer;
        public ML.Engine.Timer.CounterDownTimer Timer
        {
            get 
            {
                if (timer == null)
                {
                    timer = new ML.Engine.Timer.CounterDownTimer(1f, true, false);
                    timer.OnEndEvent += Timer_OnEndEvent;
                }
                return timer;
            }
        }

        public void OnRegister()
        {
            Timer.Start();
        }

        public void OnUnregister()
        {
            timer?.End();
            foreach (IMissionTransport mission in MissionTransports.ToArray())
            {
                mission?.End(false);
            }
            MissionTransports.Clear();
        }

        public MissionTransport<T> CreateTransportMission<T>(MissionTransportType transportType, T data, int missionNum, IMissionObj<T> initiator, MissionInitiatorType initiatorType)
        {
            if (data != null && missionNum > 0 && initiator != null)
            {
                IMissionTransport mission = null;
                if (data is string str)
                {
                    mission = new ItemIDMissionTransport(transportType, str, missionNum, initiator as IMissionObj<string>, initiatorType);
                }
                else if(data is ML.Engine.InventorySystem.Item item)
                {
                    mission = new ItemMissionTransport(transportType, item, missionNum, initiator as IMissionObj<ML.Engine.InventorySystem.Item>, initiatorType);
                }
                if (mission != null)
                {
                    MissionTransports.Add(mission);
                }
                return mission as MissionTransport<T>;
            }
            return null;
        }

        public bool RemoveMissionTransport(IMissionTransport mission)
        {
            if (mission != null)
            {
                return MissionTransports.Remove(mission);
            }
            return false;
        }

        private int InitiatorToTarget(MissionObjType targetType, IMissionTransport mission)
        {
            int missionNum = mission.NeedAssignNum;
            if (missionNum > 0)
            {
                WorkerNS.Worker worker = ManagerNS.LocalGameManager.Instance.WorkerManager.GetCanTransportWorker();
                if (worker != null)
                {
                    int weight = mission.GetWeight();
                    int maxBurNum = weight != 0 ? (worker.RealBURMax - worker.WeightCurrent) / weight : 0;
                    missionNum = missionNum <= maxBurNum ? missionNum : maxBurNum;
                }

                MissionObjType sourceType = mission.GetInitiatorMissionObjType();
                IMissionObj target = null;
                if (missionNum > 0)
                {
                    //switch (targetType)
                    //{
                    //    case MissionObjType.Store:
                    //        target = ManagerNS.LocalGameManager.Instance.StoreManager.GetPutInStore(mission.ID, missionNum, 1, true, true);
                    //        break;
                    //    case MissionObjType.Restaurant:
                    //        target = ManagerNS.LocalGameManager.Instance.RestaurantManager.GetPutInRestaurant(mission.ID, missionNum);
                    //        break;
                    //}
                }
                if (worker != null && target != null && missionNum > 0)
                {
                    if (sourceType == MissionObjType.CreatureStore || targetType == MissionObjType.CreatureStore)
                    {
                        new ItemTransport(mission as ItemMissionTransport, missionNum, 
                            mission.GetInitiator() as IMissionObj<ML.Engine.InventorySystem.Item>, 
                            target as IMissionObj<ML.Engine.InventorySystem.Item>, 
                            worker);
                    }
                    else
                    {
                        new ItemIDTransport(mission as ItemIDMissionTransport, missionNum, 
                            mission.GetInitiator() as IMissionObj<string>,
                            target as IMissionObj<string>, worker);
                    }
                }
                else
                {
                    return worker == null ? -1 : -2;
                }
            }
            return 1;
        }

        /// <summary>
        /// 仓库到发起者，取出仓库
        /// </summary>
        private int StoreToInitiator(IMissionTransport mission)
        {
            if (mission.NeedAssignNum > 0)
            {
                Dictionary<StoreNS.Store, int> result = new Dictionary<StoreNS.Store, int>();
                //Dictionary<StoreNS.Store, int> result = ManagerNS.LocalGameManager.Instance.StoreManager.GetPutOutStore(mission.ID, mission.NeedAssignNum, 1, true, true);
                foreach (var kv in result)
                {
                    StoreNS.Store store = kv.Key;
                    int missionNum = kv.Value;
                    WorkerNS.Worker worker = ManagerNS.LocalGameManager.Instance.WorkerManager.GetCanTransportWorker();
                    if (worker != null)
                    {
                        int weight = mission.GetWeight();
                        int maxBurNum = weight != 0 ? (worker.RealBURMax - worker.WeightCurrent) / weight : 0;
                        missionNum = missionNum <= maxBurNum ? missionNum : maxBurNum;
                    }
                    if (worker != null && store != null && missionNum > 0)
                    {
                        new ItemIDTransport(mission as ItemIDMissionTransport, missionNum, store, 
                            mission.GetInitiator() as IMissionObj<string>, worker);
                    }
                    else
                    {
                        return worker == null ? -1 : -2;
                    }
                }
            }
            return 1;
        }

        /// <summary>
        /// 计时器结束时执行一次分配任务
        /// </summary>
        public void Timer_OnEndEvent()
        {
            List<IMissionTransport> missionList = MissionTransports.ToList();
            missionList.Sort(new IMissionTransport.Sort());
            foreach (var mission in missionList)
            {
                if (mission.Type == MissionTransportType.ProNode_Store || mission.Type == MissionTransportType.Outside_Store)
                {
                    if (InitiatorToTarget(MissionObjType.Store, mission) == -1)
                    {
                        return;
                    }
                }
                else if (mission.Type == MissionTransportType.ProNode_Restaurant)
                {
                    if (InitiatorToTarget(MissionObjType.Restaurant, mission) == -1)
                    {
                        return;
                    }
                }
                else if (mission.Type == MissionTransportType.Store_ProNode)
                {
                    if (StoreToInitiator(mission) == -1)
                    {
                        return;
                    }
                }
            }
        }
    }
}

