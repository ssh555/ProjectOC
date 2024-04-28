using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.MissionNS
{
    [LabelText("��������"), System.Serializable]
    public class MissionTransport
    {
        [LabelText("��������"), ReadOnly]
        public MissionTransportType Type;
        [LabelText("���˶�����ID"), ReadOnly]
        public string ID = "";
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
                    End();
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
            Type = type;
            ID = itemID;
            MissionNum = missionNum;
            Initiator = imission;
            Initiator.AddMissionTranport(this);
        }

        public void ChangeMissionNum(int num)
        {
            MissionNum += num;
            if (MissionNum <= 0 || FinishNum >= MissionNum)
            {
                End();
            }
        }

        public bool AddTransport(Transport transport)
        {
            if (transport != null)
            {
                Transports.Add(transport);
                AssignNum += transport.MissionNum;
                return true;
            }
            return false;
        }

        public bool RemoveTransport(Transport transport)
        {
            if (transport != null)
            {
                if (Transports.Remove(transport))
                {
                    AssignNum -= transport.MissionNum;
                    return true;
                }
            }
            return false;
        }

        public void UpdateDestionation()
        {
            foreach (Transport transport in Transports.ToArray())
            {
                transport?.UpdateDestination();
            }
        }

        public void End(bool removeManager = true)
        {
            foreach (Transport transport in Transports.ToArray())
            {
                transport?.End(false);
            }
            Initiator.RemoveMissionTranport(this);
            if (removeManager)
            {
                ML.Engine.Manager.GameManager.Instance.GetLocalManager<MissionManager>().RemoveMissionTransport(this);
            }
        }

        public class Sort : IComparer<MissionTransport>
        {
            public int Compare(MissionTransport x, MissionTransport y)
            {
                if (x == null || y == null)
                { 
                    return (x == null).CompareTo((y == null));
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
                return x.Initiator.GetUID().CompareTo(y.Initiator.GetUID());
            }
        }
    }
}
