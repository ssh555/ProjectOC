using ML.Engine.InventorySystem;
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

        private CounterDownTimer timer;
        /// <summary>
        /// ����Tick�ĸ�������ļ�ʱ��
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
        /// ������������
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
                Debug.LogError($"{itemID} {missionNum} {initiator}");
                return null;
            }
        }

        /// <summary>
        /// �����ߵ��ֿ⣬����ֿ�
        /// </summary>
        private bool InitiatorToStore(MissionTransport mission)
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
                Store store = ManagerNS.LocalGameManager.Instance.StoreManager.GetCanPutInStore(mission.ItemID, missionNum);
                if (worker != null && store != null)
                {
                    Transport transport = new Transport(mission, mission.ItemID, missionNum, mission.Initiator, store, worker);
                    store.ReserveEmptyToWorker(mission.ItemID, missionNum);
                }
                return worker != null;
            }
            return true;
        }

        /// <summary>
        /// �ֿ⵽�����ߣ�ȡ���ֿ�
        /// </summary>
        private bool StoreToInitiator(MissionTransport mission)
        {
            if (mission.NeedAssignNum > 0)
            {
                Dictionary<Store, int> result = ManagerNS.LocalGameManager.Instance.StoreManager.GetCanPutOutStore(mission.ItemID, mission.NeedAssignNum);
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
                    if (worker != null && store != null)
                    {
                        Transport transport = new Transport(mission, mission.ItemID, missionNum, store, mission.Initiator, worker);
                        store.ReserveStorageToWorker(mission.ItemID, missionNum);
                    }
                    // û�п��е�Worker
                    if (worker == null)
                    {
                        return false;
                    }
                }
            }
            return true;
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
                        if (!InitiatorToStore(mission))
                        {
                            return;
                        }
                        break;
                    // ȡ���ֿ�
                    case MissionTransportType.Store_ProNode:
                        if (!StoreToInitiator(mission))
                        {
                            return;
                        }
                        break;
                }
            }
        }
    }
}

