using ML.Engine.InventorySystem;
using ML.Engine.Manager.LocalManager;
using ML.Engine.Timer;
using ProjectOC.RestaurantNS;
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
    [LabelText("������ȹ�����"), System.Serializable]
    public sealed class MissionManager : ILocalManager
    {
        [LabelText("���������б�"), ShowInInspector, ReadOnly]
        private HashSet<MissionTransport> MissionTransports = new HashSet<MissionTransport>();
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

        public void OnRegister()
        {
            this.Timer.Start();
        }

        public void OnUnregister()
        {
            if (timer != null)
            {
                timer.End();
            }
            foreach (MissionTransport mission in MissionTransports)
            {
                mission?.End(false);
            }
            MissionTransports.Clear();
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
                return null;
            }
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
        /// �����ߵ��ֿ⣬����ֿ�
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
                Store store = ManagerNS.LocalGameManager.Instance.StoreManager.GetPutInStore(mission.ItemID, missionNum, 1, true, true);
                if (worker != null && store != null && missionNum > 0)
                {
                    Transport transport = new Transport(mission, mission.ItemID, missionNum, mission.Initiator, store, worker);
                }
                else
                {
                    return worker == null ? -1 : -2;
                }
            }
            return 1;
        }

        /// <summary>
        /// �ֿ⵽�����ߣ�ȡ���ֿ�
        /// </summary>
        private int StoreToInitiator(MissionTransport mission)
        {
            if (mission.NeedAssignNum > 0)
            {
                Dictionary<Store, int> result = ManagerNS.LocalGameManager.Instance.StoreManager.GetPutOutStore(mission.ItemID, mission.NeedAssignNum, 1, true, true);
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
        /// �����ߵ��������������
        /// </summary>
        private int InitiatorToRestaurant(MissionTransport mission)
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
                Restaurant restaurant = ManagerNS.LocalGameManager.Instance.RestaurantManager.GetPutInRestaurant(mission.ItemID, missionNum);
                if (worker != null && restaurant != null && missionNum > 0)
                {
                    Transport transport = new Transport(mission, mission.ItemID, missionNum, mission.Initiator, restaurant, worker);
                }
                else
                {
                    return worker == null ? -1 : -2;
                }
            }
            return 1;
        }

        /// <summary>
        /// ��ʱ������ʱִ��һ�η�������
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
                else if (mission.Type == MissionTransportType.ProNode_Restaurant)
                {
                    if (InitiatorToRestaurant(mission) == -1)
                    {
                        return;
                    }
                }
            }
        }
    }
}

