using ProjectOC.WorkerNS;
namespace ProjectOC.MissionNS
{
    /// <summary>
    /// 搬运任务清单
    /// </summary>
    [System.Serializable]
    public class MissionTransport
    {
        /// <summary>
        /// ID
        /// </summary>
        public string ID = "";
        /// <summary>
        /// 搬运物品ID
        /// </summary>
        public string ItemID = "";
        /// <summary>
        /// 当前拿到的数量
        /// </summary>
        public int CurNum;
        /// <summary>
        /// 需要搬运的数量
        /// </summary>
        public int MissionNum;
        /// <summary>
        /// 取货地
        /// </summary>
        public string SourceUID = "";
        /// <summary>
        /// 送货地
        /// </summary>
        public string DestinationUID = "";
        /// <summary>
        /// 该任务的刁民
        /// </summary>
        public Worker Worker;
        /// <summary>
        /// 搬运完成是否奖励
        /// </summary>
        public bool IsSettleBonus;
        public MissionTransport(string id, string itemID, int curNum, int missionNum, string sourceUID, string destinationUID, Worker worker, bool isSettleBonus)
        {
            this.ID = id;
            this.ItemID = itemID;
            this.CurNum = curNum;
            this.MissionNum = missionNum;
            this.SourceUID = sourceUID;
            this.DestinationUID = destinationUID;
            this.Worker = worker;
            this.IsSettleBonus = isSettleBonus;
        }
    }
}

