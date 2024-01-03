using ML.Engine.Manager;
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
            Store store = GameManager.Instance.GetLocalManager<StoreManager>()?.GetCanPutInStore(mission.ItemID, missionNum);
            Worker worker = GameManager.Instance.GetLocalManager<WorkerManager>()?.GetCanTransportWorker();
            if (worker != null && store != null)
            {
                Transport transport = new Transport(mission, mission.ItemID, missionNum, mission.Initiator, store, worker);
                store.ReserveEmptyToWorker(mission.ItemID, missionNum);
            }
        }
        /// <summary>
        /// �ֿ⵽�����ߣ�ȡ���ֿ�
        /// </summary>
        private void StoreToInitiator(MissionTransport mission)
        {
            Dictionary<Store, int> result = GameManager.Instance.GetLocalManager<StoreManager>()?.GetCanPutOutStore(mission.ItemID, mission.NeedAssignNum);
            foreach (var kv in result)
            {
                Store store = kv.Key;
                int missionNum = kv.Value;
                Worker worker = GameManager.Instance.GetLocalManager<WorkerManager>()?.GetCanTransportWorker();
                if (worker != null && store != null)
                { 
                    Transport transport = new Transport(mission, mission.ItemID, missionNum, mission.Initiator, store, worker);
                    store.ReserveStorageToWorker(mission.ItemID, missionNum);
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
                    case MissionTransportType.ProNode_Store:
                    case MissionTransportType.Outside_Store:
                        InitiatorToStore(mission);
                        break;
                    // ȡ���ֿ�
                    case MissionTransportType.Store_ProNode:
                        StoreToInitiator(mission);
                        break;
                }
            }
        }
    }
}

