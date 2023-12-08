using ProjectOC.WorkerNS;
namespace ProjectOC.MissionNS
{
    /// <summary>
    /// ���������嵥
    /// </summary>
    [System.Serializable]
    public class MissionTransport
    {
        /// <summary>
        /// ID
        /// </summary>
        public string ID = "";
        /// <summary>
        /// ������ƷID
        /// </summary>
        public string ItemID = "";
        /// <summary>
        /// ��ǰ�õ�������
        /// </summary>
        public int CurNum;
        /// <summary>
        /// ��Ҫ���˵�����
        /// </summary>
        public int MissionNum;
        /// <summary>
        /// ȡ����
        /// </summary>
        public string SourceUID = "";
        /// <summary>
        /// �ͻ���
        /// </summary>
        public string DestinationUID = "";
        /// <summary>
        /// ������ĵ���
        /// </summary>
        public Worker Worker;
        /// <summary>
        /// ��������Ƿ���
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

