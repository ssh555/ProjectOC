using ML.Engine.Manager;
using Sirenix.OdinInspector;
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
        [LabelText("搬运类型"), ReadOnly]
        public MissionTransportType Type;
        [LabelText("搬运物品ID"), ReadOnly]
        public string ItemID = "";
        [LabelText("已经分配的数量"), ShowInInspector, ReadOnly]
        public int AssignNum;
        [LabelText("需要分配的数量"), ShowInInspector, ReadOnly]
        public int NeedAssignNum{ get{ return MissionNum - FinishNum - AssignNum; } }

        private int finishNum = 0;
        [LabelText("完成的数量"), ShowInInspector, ReadOnly]
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

        [LabelText("需要搬运的数量"), ShowInInspector, ReadOnly]
        public int MissionNum;

        [LabelText("任务发起者"), ShowInInspector, ReadOnly]
        public IMissionObj Initiator;

        [LabelText("分配的搬运"), ShowInInspector, ReadOnly, System.NonSerialized]
        private List<Transport> Transports = new List<Transport>();

        public MissionTransport(MissionTransportType type, string itemID, int missionNum, IMissionObj imission)
        {
            this.Type = type;
            this.ItemID = itemID;
            this.MissionNum = missionNum;
            this.Initiator = imission;
            this.Initiator.AddMissionTranport(this);
        }

        public bool AddTransport(Transport transport)
        {
            if (transport != null)
            {
                this.Transports.Add(transport);
                AssignNum += transport.MissionNum;
                return true;
            }
            return false;
        }

        public bool RemoveTransport(Transport transport)
        {
            if (transport != null)
            {
                if (this.Transports.Remove(transport))
                {
                    AssignNum -= transport.MissionNum;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取UID
        /// </summary>
        public string GetUID()
        {
            return this.Initiator?.GetUID() ?? "";
        }

        public void UpdateTransportDestionation()
        {
            foreach (Transport transport in this.Transports)
            {
                transport?.UpdateDestination();
            }
        }

        /// <summary>
        /// 终止任务
        /// </summary>
        public void End(bool removeManager = true)
        {
            foreach (Transport transport in this.Transports)
            {
                transport?.End(false);
            }
            this.Initiator.RemoveMissionTranport(this);
            if (removeManager)
            {
                GameManager.Instance.GetLocalManager<MissionManager>().RemoveMissionTransport(this);
            }
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

