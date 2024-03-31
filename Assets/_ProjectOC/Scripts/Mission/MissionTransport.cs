using ML.Engine.Manager;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.MissionNS
{
    /// <summary>
    /// 搬运任务
    /// </summary>
    [System.Serializable]
    public class MissionTransport
    {
        /// <summary>
        /// 搬运类型
        /// </summary>
        public MissionTransportType Type;

        /// <summary>
        /// 搬运物品ID
        /// </summary>
        public string ItemID = "";

        /// <summary>
        /// 已经分配的数量
        /// </summary>
        public int AssignNum
        {
            get
            {
                int result = 0;
                foreach (Transport transport in this.Transports)
                {
                    result += transport.MissionNum;
                }
                return result;
            }
        }

        /// <summary>
        /// 需要分配的数量
        /// </summary>
        public int NeedAssignNum{ get{ return MissionNum - FinishNum - AssignNum; } }

        /// <summary>
        /// 完成的数量
        /// </summary>
        private int finishNum = 0;
        /// <summary>
        /// 完成的数量
        /// </summary>
        public int FinishNum 
        {
            get { return finishNum; }
            set 
            {
                finishNum = value;
                if (finishNum >= MissionNum)
                {
                    this.End();
                }
            }
        }

        /// <summary>
        /// 需要搬运的数量
        /// </summary>
        public int MissionNum;

        /// <summary>
        /// 任务发起者
        /// </summary>
        public IMissionObj Initiator;

        /// <summary>
        /// 分配的搬运
        /// </summary>
        public List<Transport> Transports = new List<Transport>();

        public MissionTransport(MissionTransportType type, string itemID, int missionNum, IMissionObj imission)
        {
            this.Type = type;
            this.ItemID = itemID;
            this.MissionNum = missionNum;
            this.Initiator = imission;
            this.Initiator.AddMissionTranport(this);
        }

        /// <summary>
        /// 获取UID
        /// </summary>
        public string GetUID()
        {
            return this.Initiator?.GetUID() ?? "";
        }

        /// <summary>
        /// 终止任务
        /// </summary>
        public void End(bool remove=true)
        {
            foreach (Transport transport in this.Transports)
            {
                transport?.End(false);
            }
            if (remove)
            {
                this.Initiator.RemoveMissionTranport(this);
            }
            MissionManager missionManager = GameManager.Instance.GetLocalManager<MissionManager>();
            missionManager?.MissionTransports?.Remove(this);
        }

        /// <summary>
        /// 排序
        /// </summary>
        public class Sort : IComparer<MissionTransport>
        {
            public int Compare(MissionTransport x, MissionTransport y)
            {
                if (x == null)
                {
                    if (y == null)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
                if (y == null)
                {
                    return -1;
                }
                if (x.Type != y.Type)
                {
                    return x.Type.CompareTo(y.Type);
                }
                int priorityX = (int)x.Initiator.GetTransportPriority();
                int priorityY = (int)y.Initiator.GetTransportPriority();
                if (priorityX != priorityY)
                {
                    return priorityX.CompareTo(priorityY);
                }
                return x.GetUID().CompareTo(y.GetUID());
            }
        }
    }
}

