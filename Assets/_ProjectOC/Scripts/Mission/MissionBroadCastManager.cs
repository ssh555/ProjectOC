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
        private List<MissionTransport> MissionWaitAssign = new List<MissionTransport>();
        /// <summary>
        /// �Ѿ����䵫��δ��ɵ������б�
        /// </summary>
        private List<MissionTransport> MissionHasAssign = new List<MissionTransport>();

        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }
        public MissionBroadCastManager()
        {
            GameManager.Instance.TickManager.RegisterTick(tickPriority, this);
        }

        /// <summary>
        /// �����������ֿ�����񣬷���ֵΪnull�򴴽�ʧ��
        /// </summary>
        /// <param name="itemID">������ƷID</param>
        /// <param name="missionNum">��������</param>
        /// <param name="sourceID">������</param>
        /// <returns></returns>
        public MissionTransport CreateStoreageMission(string itemID, int missionNum, Transform source, string sourceUID)
        {
            // ����ֿ⣬�ӷ����߰������ֿ�
            WorldStore worldStore = StoreManager.Instance.GetWorldStoreForStorage(itemID, missionNum);
            Store store = worldStore.Store;
            // �ҵ����ʵĵ���
            Worker worker = WorkerManager.Instance.GetCanTransportWorker();
            if (worker != null && store != null)
            {
                MissionTransport mission = null;// new MissionTransport("", itemID, missionNum, sourceUID, store.UID, source, worldStore.gameObject.transform, worker, true);
                // �� Source ����ʵ��������ΪԤ����, �������ڵ㴦�Ѿ�����ˣ��������޸Ŀ������������Ԥ��ֵ
                // �� Destination ����ʵ�ʿ���������ΪԤ��������
                store.ReserveEmptyCapacityToWorker(itemID, missionNum);
                return mission;
            }
            return null;
        }

        /// <summary>
        /// �����Ӳֿ�ȡ����Ʒ�����񣬷���ֵΪ���б��򴴽�ʧ��
        /// </summary>
        /// <param name="itemID">������ƷID</param>
        /// <param name="missionNum">��������</param>
        /// <param name="sourceID">������</param>
        /// <returns></returns>
        public List<MissionTransport> CreateRetrievalMission(string itemID, int missionNum, Transform target, string targetUID)
        {
            List<MissionTransport> results = new List<MissionTransport>();
            // ȡ���ֿ⣬�Ӳֿ���˵�������
            Tuple<int, List<WorldStore>> stores = StoreManager.Instance.GetWorldStoreForRetrieve(itemID, missionNum);
            // �ҵ�һ��������һ������ֱ�����ɵ���������Ĳ����嵥
            // ��һ����û�ҵ�������������ʧ�ܣ��������ڵ��Լ������һ��ʱ���ٴε���
            // ���ҵ�����δȫ�����㣬Ҳ���سɹ��������������ڵ��Լ��ٴε���
            foreach (WorldStore worldStore in stores.Item2)
            {
                Store store = worldStore.Store;
                Worker worker = WorkerManager.Instance.GetCanTransportWorker();
                if (worker != null && store != null)
                {
                    MissionTransport mission = null;// new MissionTransport("", itemID, missionNum, store.UID, targetUID, worldStore.gameObject.transform, target, worker, true);
                    // �� Destination ����ʵ�ʴ��������ΪԤ�������
                    store.ReserveStorageCapacityToWorker(itemID, missionNum);
                    results.Add(mission);
                }
            }
            return new List<MissionTransport>(results);
        }

        // ÿִ֡��һ�η�������
        public void Tick(float deltatime)
        {
            // ���񷢲�����ʱ���п��Է����Worker��ֱ�ӷ��䣬û�о�������֡��ֱ��û��Worker���Է���
            // ÿ֡һֱ���䵽��ִ�а��������Worker����Ϊ0
            // ��MissionWaitAssign��������
            // ֱ��������һ��Ϊ�գ���һ֡����
        }

        public bool UpdateMission(MissionTransport mission, int carryNum)
        {
            // ����������worker���һ�ΰ���ʱ����
            // �����������
            // ����workerһ�ΰ��˵Ľ���������
            // �����,���MissionHasAssignȡ������
            // ����ֵΪ�����Ƿ����
            return false;
        }
    }
}

