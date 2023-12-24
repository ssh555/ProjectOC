using ProjectOC.WorkerNS;
using UnityEngine;

namespace ProjectOC.MissionNS
{
    /// <summary>
    /// 搬运任务清单
    /// </summary>
    [System.Serializable]
    public class MissionTransport
    {
        /// <summary>
        /// 搬运物品ID
        /// </summary>
        public string ItemID = "";
        /// <summary>
        /// 当前拿到的数量
        /// </summary>
        public int CurNum;
        /// <summary>
        /// 完成搬运的数量
        /// </summary>
        public int FinishNum;
        /// <summary>
        /// 需要搬运的数量
        /// </summary>
        public int MissionNum;
        /// <summary>
        /// 取货地建筑类型
        /// </summary>
        public MissionBuildingType SourceType;
        /// <summary>
        /// 送货地建筑类型
        /// </summary>
        public MissionBuildingType TargetType;
        /// <summary>
        /// 取货地
        /// </summary>
        public Transform Source;
        /// <summary>
        /// 送货地
        /// </summary>
        public Transform Target;
        /// <summary>
        /// 该任务的刁民
        /// </summary>
        public Worker Worker;
        /// <summary>
        /// 搬运完成是否奖励
        /// </summary>
        public bool IsSettleBonus;
        public MissionTransport(string itemID, int missionNum, MissionBuildingType sourceType, MissionBuildingType targetType,
            Transform source, Transform destination, Worker worker, bool isSettleBonus)
        {
            this.ItemID = itemID;
            this.MissionNum = missionNum;
            this.SourceType = sourceType;
            this.TargetType = targetType;
            this.Source = source;
            this.Target = destination;
            this.Worker = worker;
            this.IsSettleBonus = isSettleBonus;
        }
    }
}

