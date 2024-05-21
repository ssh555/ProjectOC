using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.MissionNS
{
    public interface IMissionTransport
    {
        #region Property
        [LabelText("搬运类型"), ReadOnly, ShowInInspector]
        public MissionTransportType Type { get; set; }
        [LabelText("搬运发起者类型"), ReadOnly, ShowInInspector]
        public MissionInitiatorType MissionInitiatorType { get; set; }
        [LabelText("已经分配的数量"), ReadOnly, ShowInInspector]
        public int AssignNum { get; set; }
        [LabelText("需要分配的数量"), ReadOnly, ShowInInspector]
        public int NeedAssignNum { get; }
        [LabelText("完成的数量"), ReadOnly, ShowInInspector]
        public int FinishNum { get; set; }
        [LabelText("需要搬运的数量"), ReadOnly, ShowInInspector]
        public int MissionNum { get; set; }
        #endregion

        #region Method
        public int GetWeight();
        public TransportPriority GetInitiatorTransportPriority();
        public MissionObjType GetInitiatorMissionObjType();
        public string GetInitiatorUID();
        public IMissionObj GetInitiator();
        public void ChangeMissionNum(int num);
        public void UpdateDestionation();
        public void End(bool removeManager = true, bool needJudge = false);
        #endregion

        #region Sort
        public class Sort : IComparer<IMissionTransport>
        {
            public int Compare(IMissionTransport x, IMissionTransport y)
            {
                if (x == null || y == null)
                {
                    return (x == null).CompareTo((y == null));
                }
                if (x.Type != y.Type)
                {
                    return x.Type.CompareTo(y.Type);
                }
                int priorityX = (int)x.GetInitiatorTransportPriority();
                int priorityY = (int)y.GetInitiatorTransportPriority();
                if (priorityX != priorityY)
                {
                    return priorityX.CompareTo(priorityY);
                }
                return x.GetInitiatorUID().CompareTo(y.GetInitiatorUID());
            }
        }
        #endregion
    }
}