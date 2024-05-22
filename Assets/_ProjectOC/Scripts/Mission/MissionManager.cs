using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using ProjectOC.DataNS;

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

        public MissionTransport CreateTransportMission(MissionTransportType transportType, IDataObj data, int missionNum, IMissionObj initiator, MissionInitiatorType initiatorType)
        {
            if (data != null && missionNum > 0 && initiator != null)
            {
                MissionTransport mission = new MissionTransport(transportType, data, missionNum, initiator, initiatorType);
                if (mission.Data != null)
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

        private int InitiatorToTarget(MissionObjType targetType, MissionTransport mission)
        {
            int missionNum = mission.NeedAssignNum;
            if (missionNum > 0)
            {
                WorkerNS.Worker worker = ManagerNS.LocalGameManager.Instance.WorkerManager.GetCanTransportWorker();
                if (worker != null)
                {
                    int weight = mission.Data.GetDataWeight();
                    int maxBurNum = weight != 0 ? (worker.RealBURMax - worker.WeightCurrent) / weight : 0;
                    missionNum = missionNum <= maxBurNum ? missionNum : maxBurNum;
                }

                IMissionObj target = null;
                if (missionNum > 0)
                {
                    switch (targetType)
                    {
                        case MissionObjType.Store:
                            target = ManagerNS.LocalGameManager.Instance.StoreManager.GetPutInStore(mission.Data, missionNum, 1, true, true);
                            break;
                        case MissionObjType.Restaurant:
                            target = ManagerNS.LocalGameManager.Instance.RestaurantManager.GetPutInRestaurant(mission.Data, missionNum);
                            break;
                    }
                }
                if (worker != null && target != null)
                {
                    new Transport(mission, missionNum, mission.Initiator, target, worker);
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
                Dictionary<StoreNS.IStore, int> result = mission.Data is ItemIDDataObj ? 
                    ManagerNS.LocalGameManager.Instance.StoreManager.GetPutOutStore(mission.Data, mission.NeedAssignNum, 1, true, true) :
                    ManagerNS.LocalGameManager.Instance.StoreManager.GetPutOutStore(mission.Data.GetDataID(), mission.NeedAssignNum, 1, true, true);
                foreach (var kv in result)
                {
                    StoreNS.IStore store = kv.Key;
                    int missionNum = kv.Value;
                    WorkerNS.Worker worker = ManagerNS.LocalGameManager.Instance.WorkerManager.GetCanTransportWorker();
                    if (worker != null)
                    {
                        int weight = mission.Data.GetDataWeight();
                        int maxBurNum = weight != 0 ? (worker.RealBURMax - worker.WeightCurrent) / weight : 0;
                        missionNum = missionNum <= maxBurNum ? missionNum : maxBurNum;
                    }
                    if (worker != null && store != null && missionNum > 0)
                    {
                        new Transport(mission, missionNum, store, mission.Initiator, worker);
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
            List<MissionTransport> missionList = MissionTransports.ToList();
            missionList.Sort(new MissionTransport.Sort());
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

