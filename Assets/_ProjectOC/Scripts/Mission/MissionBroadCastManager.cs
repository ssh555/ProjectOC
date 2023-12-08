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
    /// ���ȵ�ʱ�������
    /// </summary>
    [System.Serializable]
    public sealed class MissionBroadCastManager : ILocalManager, ITickComponent
    {
        /// <summary>
        /// ÿ�ΰ��˵ľ���ֵ
        /// </summary>
        public const int ExpTransport = 10;
        /// <summary>
        /// �ȴ�������������
        /// </summary>
        private List<MissionTransport> MissionQueueToBeAssigned = new List<MissionTransport>();
        /// <summary>
        /// �Ѿ����䵫��δ��ɵ������б�
        /// </summary>
        private List<MissionTransport> MissionListHasBeAssigned = new List<MissionTransport>();

        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }
        public MissionBroadCastManager()
        {
            GameManager.Instance.TickManager.RegisterTick(tickPriority, this);
            GameManager.Instance.TickManager.RegisterFixedTick(fixedTickPriority, this);
            GameManager.Instance.TickManager.RegisterLateTick(lateTickPriority, this);
        }

        /// <summary>
        /// �����������ֿ�����񣬷���ֵΪnull�򴴽�ʧ��
        /// </summary>
        /// <param name="itemID">������ƷID</param>
        /// <param name="missionNum">��������</param>
        /// <param name="sourceID">������</param>
        /// <returns></returns>
        public MissionTransport CreateStoreageMission(string itemID, int missionNum, string sourceID)
        {
            // ����ֿ⣬�ӷ����߰������ֿ�
            Store store = GameManager.Instance.GetLocalManager<StoreManager>().GetStoreForStorageMission(itemID, missionNum);
            // �ҵ����ʵĵ���
            Worker worker = GameManager.Instance.GetLocalManager<WorkerManager>().GetCanTransportWorker();
            MissionTransport mission = null;
            if (worker != null && store != null)
            {
                mission = new MissionTransport("", itemID, 0, missionNum, sourceID, store.UID, worker, true);
                // �� Source ����ʵ��������ΪԤ����, �������ڵ㴦�Ѿ�����ˣ��������޸Ŀ������������Ԥ��ֵ
                // �� Destination ����ʵ�ʿ���������ΪԤ��������
                store.ReserveEmptyCapacityToWorker(itemID, missionNum);
            }
            return mission;
        }

        /// <summary>
        /// �����Ӳֿ�ȡ����Ʒ�����񣬷���ֵΪ���б��򴴽�ʧ��
        /// </summary>
        /// <param name="itemID">������ƷID</param>
        /// <param name="missionNum">��������</param>
        /// <param name="sourceID">������</param>
        /// <returns></returns>
        public List<MissionTransport> CreateRetrievalMission(string itemID, int missionNum, string sourceID)
        {
            List<MissionTransport> results = new List<MissionTransport>();
            // ȡ���ֿ⣬�Ӳֿ���˵�������
            Tuple<int, List<Store>> stores = GameManager.Instance.GetLocalManager<StoreManager>().GetStoreForRetrieveMission(itemID, missionNum);
            // �ҵ�һ��������һ������ֱ�����ɵ���������Ĳ����嵥
            // ��һ����û�ҵ�������������ʧ�ܣ��������ڵ��Լ������һ��ʱ���ٴε���
            // ���ҵ�����δȫ�����㣬Ҳ���سɹ��������������ڵ��Լ��ٴε���
            foreach (Store store in stores.Item2)
            {
                Worker worker = GameManager.Instance.GetLocalManager<WorkerManager>().GetCanTransportWorker();
                if (worker != null && store != null)
                {
                    MissionTransport mission = new MissionTransport("", itemID, 0, missionNum, sourceID, store.UID, worker, true);
                    // �� Destination ����ʵ�ʴ��������ΪԤ�������
                    store.ReserveStorageCapacityToWorker(itemID, missionNum);
                    results.Add(mission);
                }
            }
            return results;
        }

        // ÿִ֡��һ�η�������
        public void Tick(float deltatime)
        {
            //���񷢲�����ʱ���п��Է����Worker��ֱ�ӷ��䣬û�о�������֡��ֱ��û��Worker���Է���
            //ÿ֡һֱ���䵽��ִ�а��������Worker����Ϊ0
            //�ɰ��� => û�з���ֵ���Ҵ�������״̬
            // ����WorkerDict��������״̬��Worker
            // ��MissionQueueToBeAssigned��������
            // ֱ��������һ��Ϊ�գ���һ֡����
        }

        public bool UpdateMission(MissionTransport mission, int carryNum)
        {
            // ����������worker���һ�ΰ���ʱ����
            // �����������
            // ����workerһ�ΰ��˵Ľ���������
            // �����,���MissionListHasBeAssignedȡ������
            // ����ֵΪ�����Ƿ����
            return false;
        }

        public void UpdateWorkerStatusList(Worker worker, Status preStatus, Status newStatus)
        {

        }
    }
}

