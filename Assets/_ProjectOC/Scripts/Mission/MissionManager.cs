using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;

namespace ProjectOC.MissionNS
{
    [LabelText("任务调度管理器"), System.Serializable]
    public sealed class MissionManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        [LabelText("搬运任务列表"), ShowInInspector, ReadOnly]
        private HashSet<MissionTransport> MissionTransports = new HashSet<MissionTransport>();
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
            foreach (MissionTransport mission in MissionTransports.ToArray())
            {
                mission?.End(false);
            }
            MissionTransports.Clear();
        }

        /// <summary>
        /// 创建搬运任务
        /// </summary>
        public MissionTransport CreateTransportMission(MissionTransportType transportType, string itemID, int missionNum, IMissionObj initiator, MissionInitiatorType initiatorType)
        {
            if (!string.IsNullOrEmpty(itemID) && missionNum > 0 && initiator != null)
            {
                MissionTransport mission = new MissionTransport(transportType, itemID, missionNum, initiator, initiatorType);
                if (!string.IsNullOrEmpty(mission.ID))
                {
                    MissionTransports.Add(mission);
                    return mission;
                }
            }
            return null;
        }
        public MissionTransport CreateTransportMission(MissionTransportType transportType, ML.Engine.InventorySystem.Item item, IMissionObj initiator, MissionInitiatorType initiatorType)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID) && item.Amount > 0 && initiator != null)
            {
                MissionTransport mission = new MissionTransport(transportType, item, initiator, initiatorType);
                if (!string.IsNullOrEmpty(mission.ID))
                {
                    MissionTransports.Add(mission);
                    return mission;
                }
            }
            return null;
        }

        public bool RemoveMissionTransport(MissionTransport mission)
        {
            if (mission != null)
            {
                return MissionTransports.Remove(mission);
            }
            return false;
        }

        /// <summary>
        /// 发起者到目标，存入目标
        /// </summary>
        private int InitiatorToTarget(MissionObjType targetType, MissionTransport mission)
        {
            int missionNum = mission.NeedAssignNum;
            bool flag = mission.Item != null;
            if (missionNum > 0)
            {
                WorkerNS.Worker worker = ManagerNS.LocalGameManager.Instance.WorkerManager.GetCanTransportWorker();
                if (worker != null)
                {
                    int weight = flag ? mission.Item.Weight : ML.Engine.InventorySystem.ItemManager.Instance.GetWeight(mission.ID);
                    int maxBurNum = weight != 0 ? (worker.RealBURMax - worker.WeightCurrent) / weight : 0;
                    missionNum = missionNum <= maxBurNum ? missionNum : maxBurNum;
                }
                IMissionObj target = null;
                if (missionNum > 0)
                {
                    switch (targetType)
                    {
                        case MissionObjType.Store:
                            if (flag)
                            {
                                target = ManagerNS.LocalGameManager.Instance.StoreManager.GetPutInStore(mission.Item, 1, true, true);
                            }
                            else
                            {
                                target = ManagerNS.LocalGameManager.Instance.StoreManager.GetPutInStore(mission.ID, missionNum, 1, true, true);
                            }
                            break;
                        case MissionObjType.Restaurant:
                            target = ManagerNS.LocalGameManager.Instance.RestaurantManager.GetPutInRestaurant(mission.ID, missionNum);
                            break;
                    }
                }
                if (worker != null && target != null && missionNum > 0)
                {
                    if (flag) { new Transport(mission, mission.Initiator, target, worker); }
                    else { new Transport(mission, missionNum, mission.Initiator, target, worker); }
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
        private int StoreToInitiator(MissionTransport mission)
        {
            if (mission.NeedAssignNum > 0)
            {
                if (mission.Item != null)
                {
                    StoreNS.Store store = ManagerNS.LocalGameManager.Instance.StoreManager.GetPutOutStore(mission.Item, 1, true, true);
                    if (store != null)
                    {
                        WorkerNS.Worker worker = ManagerNS.LocalGameManager.Instance.WorkerManager.GetCanTransportWorker();
                        if (worker != null)
                        {
                            if (worker.RealBURMax - worker.WeightCurrent >= mission.Item.Weight)
                            {
                                new Transport(mission, store, mission.Initiator, worker);
                            }
                        }
                        else { return -2; }
                    }
                    else { return -1; }
                }
                else
                {
                    Dictionary<StoreNS.Store, int> result = ManagerNS.LocalGameManager.Instance.StoreManager.GetPutOutStore(mission.ID, mission.NeedAssignNum, 1, true, true);
                    foreach (var kv in result)
                    {
                        StoreNS.Store store = kv.Key;
                        int missionNum = kv.Value;
                        WorkerNS.Worker worker = ManagerNS.LocalGameManager.Instance.WorkerManager.GetCanTransportWorker();
                        if (worker != null)
                        {
                            int weight = ML.Engine.InventorySystem.ItemManager.Instance.GetWeight(mission.ID);
                            int maxBurNum = weight != 0 ? (worker.RealBURMax - worker.WeightCurrent) / weight : 0;
                            missionNum = missionNum <= maxBurNum ? missionNum : maxBurNum;
                        }
                        if (worker != null && store != null && missionNum > 0)
                        {
                            Transport transport = new Transport(mission, missionNum, store, mission.Initiator, worker);
                        }
                        else
                        {
                            return worker == null ? -1 : -2;
                        }
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
            List<MissionTransport> missionList = MissionTransports.ToList();
            missionList.Sort(new MissionTransport.Sort());
            foreach (MissionTransport mission in missionList)
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

