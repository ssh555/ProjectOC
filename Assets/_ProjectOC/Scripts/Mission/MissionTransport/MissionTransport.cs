using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.MissionNS
{
    [LabelText("��������"), System.Serializable]
    public abstract class MissionTransport<T> : IMissionTransport
    {
        #region Data
        [LabelText("��������"), ReadOnly]
        public T Data;
        [LabelText("��������"), HideInInspector]
        public IMissionObj<T> Initiator;
        [LabelText("����İ���"), ReadOnly, ShowInInspector]
        private List<Transport<T>> Transports = new List<Transport<T>>();
        #endregion

        #region Property
        [LabelText("��������"), ReadOnly, ShowInInspector]
        public MissionTransportType Type { get; set; }
        [LabelText("���˷���������"), ReadOnly, ShowInInspector]
        public MissionInitiatorType MissionInitiatorType { get; set; }
        [LabelText("�Ѿ����������"), ReadOnly, ShowInInspector]
        public int AssignNum { get; set; }
        [LabelText("��Ҫ���������"), ShowInInspector, ReadOnly]
        public int NeedAssignNum => MissionNum - FinishNum - AssignNum;
        private int finishNum = 0;
        [LabelText("��ɵ�����"), ReadOnly, ShowInInspector]
        public int FinishNum 
        {
            get { return finishNum; }
            set 
            {
                finishNum = value;
                if (finishNum >= MissionNum) { End(); }
            }
        }
        [LabelText("��Ҫ���˵�����"), ReadOnly, ShowInInspector]
        public int MissionNum { get; set; }
        #endregion

        #region Method
        public abstract int GetWeight();
        public TransportPriority GetInitiatorTransportPriority() { return Initiator.GetTransportPriority(); }
        public string GetInitiatorUID() { return Initiator.GetUID(); }
        public IMissionObj GetInitiator() { return Initiator; }
        public MissionObjType GetInitiatorMissionObjType() { return Initiator.GetMissionObjType(); }
        public void ChangeMissionNum(int num)
        {
            MissionNum = num;
            if (MissionNum <= 0 || FinishNum >= MissionNum)
            {
                End();
            }
        }
        public void UpdateDestionation()
        {
            foreach (Transport<T> transport in Transports.ToArray())
            {
                transport?.UpdateDestination();
            }
        }
        public void End(bool removeManager = true, bool needJudge = false)
        {
            foreach (Transport<T> transport in Transports.ToArray())
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
        #endregion

        #region T Method
        public MissionTransport(MissionTransportType type, T data, int missionNum, IMissionObj<T> imission, MissionInitiatorType initiatorType)
        {
            Data = data;
            Initiator = imission;
            Initiator.AddMissionTranport(this);
            Type = type;
            MissionInitiatorType = initiatorType;
            MissionNum = missionNum;
        }
        public bool AddTransport(Transport<T> transport)
        {
            if (transport != null)
            {
                Transports.Add(transport);
                AssignNum += transport.MissionNum;
                return true;
            }
            return false;
        }
        public bool RemoveTransport(Transport<T> transport)
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
        #endregion
    }
}