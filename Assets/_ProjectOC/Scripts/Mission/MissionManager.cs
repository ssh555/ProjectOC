using ML.Engine.Manager.LocalManager;
using ML.Engine.Timer;
using ProjectOC.StoreNS;
using ProjectOC.WorkerNS;
using System;
using System.Collections;
using System.Collections.Generic;
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
        public SortedSet<MissionTransport> MissionTransports = new SortedSet<MissionTransport>(new MissionTransport.Sort());
        public CounterDownTimer timer;
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
        public MissionManager()
        {
            this.Timer.Start();
        }
        /// <summary>
        /// 创建搬运任务
        /// </summary>
        public MissionTransport CreateTransportMission(MissionTransportType transportType, string itemID, int missionNum, IMission Initiator)
        {
            MissionTransport mission = new MissionTransport(transportType, itemID, missionNum, Initiator);
            this.MissionTransports.Add(mission);
            return mission;
        }

        /// <summary>
        /// 发起者到仓库，存入仓库
        /// </summary>
        private void InitiatorToStore(MissionTransport mission)
        {
            int missionNum = mission.NeedAssignNum;
            Store store = StoreManager.Instance.GetStoreForStorage(mission.ItemID, missionNum);
            Worker worker = WorkerManager.Instance.GetCanTransportWorker();
            if (worker != null && store != null)
            {
                Transport transport = new Transport(mission, mission.ItemID, missionNum, mission.Initiator, store, worker);
                store.ReserveEmptyCapacityToWorker(mission.ItemID, missionNum);
            }
        }
        /// <summary>
        /// 仓库到发起者，取出仓库
        /// </summary>
        private void StoreToInitiator(MissionTransport mission)
        {
            List<Tuple<int, Store>> result = StoreManager.Instance.GetStoreForRetrieve(mission.ItemID, mission.NeedAssignNum);
            foreach (var res in result)
            {
                int missionNum = res.Item1;
                Store store = res.Item2;
                Worker worker = WorkerManager.Instance.GetCanTransportWorker();
                if (worker != null && store != null)
                { 
                    Transport transport = new Transport(mission, mission.ItemID, missionNum, mission.Initiator, store, worker);
                    store.ReserveStorageCapacityToWorker(mission.ItemID, missionNum);
                }
                if (worker == null)
                {
                    break;
                }
            }
        }
        /// <summary>
        /// 计时器结束时执行一次分配任务
        /// </summary>
        public void Timer_OnEndEvent()
        {
            foreach (MissionTransport mission in this.MissionTransports)
            {
                switch (mission.Type)
                {
                    // 存入仓库
                    case MissionTransportType.ProductionNode_Store:
                    case MissionTransportType.Outside_Store:
                        InitiatorToStore(mission);
                        break;
                    // 取出仓库
                    case MissionTransportType.Store_ProductionNode:
                        StoreToInitiator(mission);
                        break;
                }
            }
        }
    }
}

