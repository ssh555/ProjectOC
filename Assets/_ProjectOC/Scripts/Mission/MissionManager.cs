using ML.Engine.InventorySystem;
using ML.Engine.Manager.LocalManager;
using ML.Engine.Timer;
using ProjectOC.StoreNS;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace ProjectOC.MissionNS
{
    /// <summary>
    /// 任务调度管理器
    /// </summary>
    [System.Serializable]
    public sealed class MissionManager : ILocalManager
    {
        /// <summary>
        /// 搬运任务列表
        /// </summary>
        [ShowInInspector]
        public HashSet<MissionTransport> MissionTransports = new HashSet<MissionTransport>();

        private CounterDownTimer timer;
        /// <summary>
        /// 代替Tick的更新任务的计时器
        /// </summary>
        public CounterDownTimer Timer
        {
            get 
            {
                if (this.timer == null)
                {
                    this.timer = new CounterDownTimer(1f, true, false);
                    this.timer.OnEndEvent += Timer_OnEndEvent;
                }
                return this.timer;
            }
        }

        public void Init()
        {
            this.Timer.Start();
        }

        /// <summary>
        /// 创建搬运任务
        /// </summary>
        public MissionTransport CreateTransportMission(MissionTransportType transportType, string itemID, int missionNum, IMissionObj initiator)
        {
            if (!string.IsNullOrEmpty(itemID) && missionNum > 0 && initiator != null)
            {
                MissionTransport mission = new MissionTransport(transportType, itemID, missionNum, initiator);
                this.MissionTransports.Add(mission);
                return mission;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 发起者到仓库，存入仓库
        /// </summary>
        private int InitiatorToStore(MissionTransport mission)
        {
            int missionNum = mission.NeedAssignNum;
            if (missionNum > 0)
            {
                Worker worker = ManagerNS.LocalGameManager.Instance.WorkerManager.GetCanTransportWorker();
                if (worker != null)
                {
                    int maxBurNum = (int)(worker.BURMax / ItemManager.Instance.GetWeight(mission.ItemID));
                    missionNum = missionNum <= maxBurNum ? missionNum : maxBurNum;
                }
                Store store = ManagerNS.LocalGameManager.Instance.StoreManager.GetCanPutInStore(mission.ItemID, missionNum, 1);
                if (worker != null && store != null && missionNum > 0)
                {
                    Transport transport = new Transport(mission, mission.ItemID, missionNum, mission.Initiator, store, worker);
                    store.ReserveEmptyToWorker(mission.ItemID, missionNum);
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
                Dictionary<Store, int> result = ManagerNS.LocalGameManager.Instance.StoreManager.GetCanPutOutStore(mission.ItemID, mission.NeedAssignNum, 1);
                foreach (var kv in result)
                {
                    Store store = kv.Key;
                    int missionNum = kv.Value;
                    Worker worker = ManagerNS.LocalGameManager.Instance.WorkerManager.GetCanTransportWorker();
                    if (worker != null)
                    {
                        int maxBurNum = (int)(worker.BURMax / ItemManager.Instance.GetWeight(mission.ItemID));
                        missionNum = missionNum <= maxBurNum ? missionNum : maxBurNum;
                    }
                    if (worker != null && store != null && missionNum > 0)
                    {
                        Transport transport = new Transport(mission, mission.ItemID, missionNum, store, mission.Initiator, worker);
                        store.ReserveStorageToWorker(mission.ItemID, missionNum);
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
            List<MissionTransport> missionList = this.MissionTransports.ToList();
            missionList.Sort(new MissionTransport.Sort());
            foreach (MissionTransport mission in missionList)
            {
                if (mission.Type == MissionTransportType.ProNode_Store || mission.Type == MissionTransportType.Outside_Store)
                {
                    if (InitiatorToStore(mission) == -1)
                    {
                        return;
                    }
                }
                else if(mission.Type == MissionTransportType.Store_ProNode)
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

