using ML.Engine.Manager;
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
        /// <summary>
        /// ��������
        /// </summary>
        public MissionTransportType Type;

        /// <summary>
        /// ������ƷID
        /// </summary>
        public string ItemID = "";

        /// <summary>
        /// �Ѿ����������
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
        /// ��Ҫ���������
        /// </summary>
        public int NeedAssignNum{ get{ return MissionNum - FinishNum - AssignNum; } }

        /// <summary>
        /// ��ɵ�����
        /// </summary>
        private int finishNum = 0;
        /// <summary>
        /// ��ɵ�����
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
        /// ��Ҫ���˵�����
        /// </summary>
        public int MissionNum;

        /// <summary>
        /// ��������
        /// </summary>
        public IMissionObj Initiator;

        /// <summary>
        /// ����İ���
        /// </summary>
        public HashSet<Transport> Transports = new HashSet<Transport>();

        public MissionTransport(MissionTransportType type, string itemID, int missionNum, IMissionObj imission)
        {
            this.Type = type;
            this.ItemID = itemID;
            this.MissionNum = missionNum;
            this.Initiator = imission;
            this.Initiator.AddMissionTranport(this);
        }

        /// <summary>
        /// ��ȡ�������ȼ������ȼ�Խ������ԽС
        /// </summary>
        /// <returns></returns>
        public int GetTransportPriority()
        {
            int priority = 3;
            if (this.Initiator != null)
            {
                switch (this.Initiator.GetTransportPriority())
                {
                    case TransportPriority.Urgency:
                        priority = 0;
                        break;
                    case TransportPriority.Normal:
                        priority = 1;
                        break;
                    case TransportPriority.Alternative:
                        priority = 2;
                        break;
                }
            }
            return priority;
        }

        /// <summary>
        /// ��ȡUID
        /// </summary>
        /// <returns></returns>
        public string GetUID()
        {
            // Ϊnull�򷵻� ""
            return this.Initiator?.GetUID() ?? "";
        }

        /// <summary>
        /// ��ֹ����
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
                    x.Type.CompareTo(y.Type);
                }
                int priorityX = x.GetTransportPriority();
                int priorityY = y.GetTransportPriority();
                if (priorityX != priorityY)
                {
                    priorityX.CompareTo(priorityY);
                }
                return x.GetUID().CompareTo(y.GetUID());
            }
        }
    }
}

