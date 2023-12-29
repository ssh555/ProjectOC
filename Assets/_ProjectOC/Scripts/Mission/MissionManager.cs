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
    /// ������ȹ�����
    /// </summary>
    [System.Serializable]
    public sealed class MissionManager : ILocalManager
    {
        /// <summary>
        /// ���������б�
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
        /// ������������
        /// </summary>
        public MissionTransport CreateTransportMission(MissionTransportType transportType, string itemID, int missionNum, IMission Initiator)
        {
            MissionTransport mission = new MissionTransport(transportType, itemID, missionNum, Initiator);
            this.MissionTransports.Add(mission);
            return mission;
        }

        /// <summary>
        /// �����ߵ��ֿ⣬����ֿ�
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
        /// �ֿ⵽�����ߣ�ȡ���ֿ�
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
        /// ��ʱ������ʱִ��һ�η�������
        /// </summary>
        public void Timer_OnEndEvent()
        {
            foreach (MissionTransport mission in this.MissionTransports)
            {
                switch (mission.Type)
                {
                    // ����ֿ�
                    case MissionTransportType.ProductionNode_Store:
                    case MissionTransportType.Outside_Store:
                        InitiatorToStore(mission);
                        break;
                    // ȡ���ֿ�
                    case MissionTransportType.Store_ProductionNode:
                        StoreToInitiator(mission);
                        break;
                }
            }
        }
    }
}

