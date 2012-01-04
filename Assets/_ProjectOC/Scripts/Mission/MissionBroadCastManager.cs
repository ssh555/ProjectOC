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
    /// 调度的时间管理器
    /// </summary>
    [System.Serializable]
    public sealed class MissionBroadCastManager : ILocalManager, ITickComponent
    {
        /// <summary>
        /// 每次搬运的经验值
        /// </summary>
        public const int ExpTransport = 10;
        /// <summary>
        /// 等待分配的任务队列
        /// </summary>
        private List<MissionTransport> MissionWaitAssign = new List<MissionTransport>();
        /// <summary>
        /// 已经分配但尚未完成的任务列表
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
        /// 创建搬运至仓库的任务，返回值为null则创建失败
        /// </summary>
        /// <param name="itemID">搬运物品ID</param>
        /// <param name="missionNum">搬运数量</param>
        /// <param name="sourceID">发起者</param>
        /// <returns></returns>
        public MissionTransport CreateStoreageMission(string itemID, int missionNum, Transform source, string sourceUID)
        {
            // 存入仓库，从发起者搬运至仓库
            WorldStore worldStore = StoreManager.Instance.GetWorldStoreForStorage(itemID, missionNum);
            Store store = worldStore.Store;
            // 找到合适的刁民
            Worker worker = WorkerManager.Instance.GetCanTransportWorker();
            if (worker != null && store != null)
            {
                MissionTransport mission = null;// new MissionTransport("", itemID, missionNum, sourceUID, store.UID, source, worldStore.gameObject.transform, worker, true);
                // 把 Source 处的实际量更改为预留量, 在生产节点处已经完成了，后续有修改可以在这里更改预留值
                // 把 Destination 处的实际空余量更改为预留存入量
                store.ReserveEmptyCapacityToWorker(itemID, missionNum);
                return mission;
            }
            return null;
        }

        /// <summary>
        /// 创建从仓库取出物品的任务，返回值为空列表则创建失败
        /// </summary>
        /// <param name="itemID">搬运物品ID</param>
        /// <param name="missionNum">搬运数量</param>
        /// <param name="sourceID">发起者</param>
        /// <returns></returns>
        public List<MissionTransport> CreateRetrievalMission(string itemID, int missionNum, Transform target, string targetUID)
        {
            List<MissionTransport> results = new List<MissionTransport>();
            // 取出仓库，从仓库搬运到发起者
            Tuple<int, List<WorldStore>> stores = StoreManager.Instance.GetWorldStoreForRetrieve(itemID, missionNum);
            // 找到一个就生成一个任务，直到生成的任务包含的材料清单
            // 若一个都没找到，则生成任务失败，由生产节点自己负责过一段时间再次调用
            // 若找到，但未全部满足，也返回成功，后续由生产节点自己再次调用
            foreach (WorldStore worldStore in stores.Item2)
            {
                Store store = worldStore.Store;
                Worker worker = WorkerManager.Instance.GetCanTransportWorker();
                if (worker != null && store != null)
                {
                    MissionTransport mission = null;// new MissionTransport("", itemID, missionNum, store.UID, targetUID, worldStore.gameObject.transform, target, worker, true);
                    // 把 Destination 处的实际存放量更改为预留存放量
                    store.ReserveStorageCapacityToWorker(itemID, missionNum);
                    results.Add(mission);
                }
            }
            return new List<MissionTransport>(results);
        }

        // 每帧执行一次分配任务
        public void Tick(float deltatime)
        {
            // 任务发布分配时，有可以分配的Worker就直接分配，没有就跳过此帧，直到没有Worker可以分配
            // 每帧一直分配到可执行搬运任务的Worker数量为0
            // 从MissionWaitAssign分配任务
            // 直到二者有一个为空，这一帧结束
        }

        public bool UpdateMission(MissionTransport mission, int carryNum)
        {
            // 更新任务，由worker完成一次搬运时调用
            // 更新任务进度
            // 结算worker一次搬运的奖励和消耗
            // 若完成,则从MissionHasAssign取出任务
            // 返回值为任务是否完成
            return false;
        }
    }
}

