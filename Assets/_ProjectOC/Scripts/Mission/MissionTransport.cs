using ProjectOC.WorkerNS;
using UnityEngine;

namespace ProjectOC.MissionNS
{
    /// <summary>
    /// ���������嵥
    /// </summary>
    [System.Serializable]
    public class MissionTransport
    {
        /// <summary>
        /// ������ƷID
        /// </summary>
        public string ItemID = "";
        /// <summary>
        /// ��ǰ�õ�������
        /// </summary>
        public int CurNum;
        /// <summary>
        /// ��ɰ��˵�����
        /// </summary>
        public int FinishNum;
        /// <summary>
        /// ��Ҫ���˵�����
        /// </summary>
        public int MissionNum;
        /// <summary>
        /// ȡ���ؽ�������
        /// </summary>
        public MissionBuildingType SourceType;
        /// <summary>
        /// �ͻ��ؽ�������
        /// </summary>
        public MissionBuildingType TargetType;
        /// <summary>
        /// ȡ����
        /// </summary>
        public Transform Source;
        /// <summary>
        /// �ͻ���
        /// </summary>
        public Transform Target;
        /// <summary>
        /// ������ĵ���
        /// </summary>
        public Worker Worker;
        /// <summary>
        /// ��������Ƿ���
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

