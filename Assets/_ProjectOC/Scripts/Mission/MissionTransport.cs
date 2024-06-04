using Sirenix.OdinInspector;
using System.Collections.Generic;
using ProjectOC.DataNS;
using System;

namespace ProjectOC.MissionNS
{
    [LabelText("搬运任务"), Serializable]
    public class MissionTransport
    {
        #region Data
        [LabelText("搬运数据"), ReadOnly, ShowInInspector]
        public IDataObj Data { get; private set; }
        [LabelText("任务发起者"), ReadOnly, ShowInInspector, NonSerialized]
        public IMissionObj Initiator;
        [LabelText("分配的搬运"), ReadOnly, ShowInInspector, NonSerialized]
        private List<Transport> Transports = new List<Transport>();
        [LabelText("搬运类型"), ReadOnly]
        public MissionTransportType Type;
        [LabelText("搬运发起者类型"), ReadOnly]
        public MissionInitiatorType MissionInitiatorType;
        public int ReplaceIndex;
        public bool ReserveEmpty;
        [LabelText("已经分配的数量"), ReadOnly]
        public int AssignNum;
        [LabelText("需要分配的数量"), ReadOnly, ShowInInspector]
        public int NeedAssignNum => MissionNum - finishNum - AssignNum;
        private int finishNum = 0;
        [LabelText("完成的数量"), ReadOnly, ShowInInspector]
        public int FinishNum 
        {
            get { return finishNum; }
            set 
            {
                finishNum = value;
                if (finishNum >= MissionNum) { End(); }
            }
        }
        [LabelText("需要搬运的数量"), ReadOnly]
        public int MissionNum;
        #endregion

        #region Method
        public MissionTransport(MissionTransportType type, IDataObj data, int missionNum, IMissionObj imission, 
            MissionInitiatorType initiatorType, int replaceIndex = -1, bool reserveEmpty = false)
        {
            if (data == null) { return; }
            Type = type;
            Data = data;
            MissionNum = missionNum;
            Initiator = imission;
            Initiator.AddMissionTranport(this);
            MissionInitiatorType = initiatorType;
            ReplaceIndex = replaceIndex;
            ReserveEmpty = reserveEmpty;
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
        public void ChangeMissionNum(int num)
        {
            MissionNum = num;
            if (MissionNum <= 0 || FinishNum >= MissionNum) { End(); }
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
                ManagerNS.LocalGameManager.Instance.MissionManager.RemoveMissionTransport(this);
            }
        }
        #endregion

        #region Sort
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
        #endregion
    }
}