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
        /// ��ȡUID
        /// </summary>
        public string GetUID()
        {
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

