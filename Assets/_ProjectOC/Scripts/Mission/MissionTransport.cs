using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.MissionNS
{
    [LabelText("搬运任务"), System.Serializable]
    public class MissionTransport
    {
        #region Data
        [LabelText("搬运类型"), ReadOnly]
        public MissionTransportType Type;
        [LabelText("搬运发起者类型"), ReadOnly]
        public MissionInitiatorType MissionInitiatorType;
        [LabelText("搬运东西的ID"), ReadOnly]
        public string ID = "";
        public ML.Engine.InventorySystem.Item Item;
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
                    End();
                }
            }
        }
        [LabelText("需要搬运的数量"), ShowInInspector, ReadOnly]
        public int MissionNum;
        [LabelText("任务发起者"), ShowInInspector, ReadOnly]
        public IMissionObj Initiator;
        [LabelText("分配的搬运"), ShowInInspector, ReadOnly, System.NonSerialized]
        private List<Transport> Transports = new List<Transport>();
        #endregion

        public void Init(MissionTransportType type, string itemID, int missionNum, IMissionObj imission, MissionInitiatorType initiatorType)
        {
            Type = type;
            ID = itemID;
            MissionNum = missionNum;
            Initiator = imission;
            MissionInitiatorType = initiatorType;
            Initiator.AddMissionTranport(this);
        }
        public MissionTransport(MissionTransportType type, string itemID, int missionNum, IMissionObj imission, MissionInitiatorType initiatorType)
        {
            Init(type, itemID, missionNum, imission, initiatorType);
        }
        public MissionTransport(MissionTransportType type, ML.Engine.InventorySystem.Item item, IMissionObj imission, MissionInitiatorType initiatorType)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID))
            {
                if (!ManagerNS.LocalGameManager.Instance.ItemManager.GetCanStack(item.ID) && item.Amount == 1)
                {
                    Item = item;
                }
                Init(type, item.ID, item.Amount, imission, initiatorType);
            }
        }

        public void ChangeMissionNum(int num)
        {
            MissionNum = num;
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

        public void End(bool removeManager = true, bool needJudge = false)
        {
            foreach (Transport transport in Transports.ToArray())
            {
                if (!needJudge || MissionInitiatorType == MissionInitiatorType.PutIn_Initiator 
                    || (MissionInitiatorType == MissionInitiatorType.PutOut_Initiator && !transport.ArriveSource))
                {
                    transport?.End();
                }
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
