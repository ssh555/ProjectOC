using ML.Engine.Manager;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.MissionNS
{
    /// <summary>
    /// ��������
    /// </summary>
    [System.Serializable]
    public class MissionTransport
    {
        [LabelText("��������"), ReadOnly]
        public MissionTransportType Type;
        [LabelText("������ƷID"), ReadOnly]
        public string ItemID = "";
        [LabelText("�Ѿ����������"), ShowInInspector, ReadOnly]
        public int AssignNum;
        [LabelText("��Ҫ���������"), ShowInInspector, ReadOnly]
        public int NeedAssignNum{ get{ return MissionNum - FinishNum - AssignNum; } }

        private int finishNum = 0;
        [LabelText("��ɵ�����"), ShowInInspector, ReadOnly]
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

        [LabelText("��Ҫ���˵�����"), ShowInInspector, ReadOnly]
        public int MissionNum;

        [LabelText("��������"), ShowInInspector, ReadOnly]
        public IMissionObj Initiator;

        [LabelText("����İ���"), ShowInInspector, ReadOnly, System.NonSerialized]
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
        /// ��ȡUID
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
        /// ��ֹ����
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
        /// ����
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

