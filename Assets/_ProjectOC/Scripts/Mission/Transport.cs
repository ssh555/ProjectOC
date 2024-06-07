using Sirenix.OdinInspector;
using ProjectOC.DataNS;
using System;

namespace ProjectOC.MissionNS
{
    [LabelText("搬运"), System.Serializable]
    public class Transport
    {
        #region Data
        [LabelText("搬运的数据"), ReadOnly, ShowInInspector]
        public IDataObj Data;
        [LabelText("搬运所属的任务"), ShowInInspector, NonSerialized]
        public MissionTransport Mission;
        [LabelText("取货地"), ShowInInspector, NonSerialized]
        public IMissionObj Source;
        [LabelText("送货地"), ShowInInspector, NonSerialized]
        public IMissionObj Target;
        #endregion

        #region Property
        [LabelText("负责该搬运的刁民"), ReadOnly, ShowInInspector, NonSerialized]
        public WorkerNS.Worker Worker;
        [LabelText("需要搬运的数量"), ReadOnly]
        public int MissionNum;
        [LabelText("当前拿到的数量"), ReadOnly]
        public int CurNum;
        [LabelText("完成的数量"), ReadOnly]
        public int FinishNum;
        [LabelText("取货地预留的数量"), ReadOnly]
        public int SoureceReserveNum;
        [LabelText("送货地预留的数量"), ReadOnly]
        public int TargetReserveNum;
        [LabelText("是否到达取货地"), ReadOnly]
        public bool ArriveSource;
        [LabelText("是否到达目的地"), ReadOnly]
        public bool ArriveTarget;
        [LabelText("是否是有效的"), ReadOnly]
        public bool IsValid => Data != null;
        public int Weight => Data != null ? Data.GetDataWeight() * CurNum : 0;
        #endregion

        #region Method
        public Transport(MissionTransport mission, int missionNum, IMissionObj source, IMissionObj target, WorkerNS.Worker worker)
        {
            if (mission.Data == null) { return; }
            Data = mission.Data;
            Mission = mission;
            Source = source;
            Target = target;
            MissionNum = missionNum;
            Mission.AddTransport(this);
            Source.AddTransport(this);
            Target.AddTransport(this);
            bool isReplaceData = mission.ReplaceIndex >= 0;
            if (isReplaceData)
            {
                SoureceReserveNum = Source.ReservePutOut(mission.Data, missionNum, isReplaceData, this);
            }
            else
            {
                SoureceReserveNum = Source.ReservePutOut(mission.Data, missionNum);
                TargetReserveNum = Target.ReservePutIn(mission.Data, missionNum, Mission.ReserveEmpty);
            }
            Worker = worker;
            if (SoureceReserveNum == 0 || (!isReplaceData && TargetReserveNum == 0)) { End(); }
            else
            {
                Worker.Transport = this;
                Worker.SetDestination(Source.GetTransform().position, OnSourceArriveEvent);
            }
        }
        public void UpdateDestination()
        {
            if (!ArriveSource)
            {
                if (Source.GetTransform().position != Worker.Target)
                {
                    Worker.SetDestination(Source.GetTransform().position, OnSourceArriveEvent);
                }
            }
            else
            {
                if (Target.GetTransform().position != Worker.Target)
                {
                    Worker.SetDestination(Target.GetTransform().position, OnTargetArriveEvent);
                }
            }
        }
        private void OnSourceArriveEvent(WorkerNS.Worker worker)
        {
            worker.Transport.ArriveSource = true;
            worker.Transport.PutOutSource();
            worker.SetDestination(worker.Transport.Target.GetTransform().position, OnTargetArriveEvent);
        }
        private void OnTargetArriveEvent(WorkerNS.Worker worker)
        {
            worker.Transport.ArriveTarget = true;
            worker.Transport.PutInTarget();
        }
        public void PutOutSource()
        {
            int weight = Data.GetDataWeight();
            int burMaxNum = weight != 0 ? (Worker.RealBURMax - Worker.WeightCurrent) / weight : SoureceReserveNum;
            int num = SoureceReserveNum <= burMaxNum ? SoureceReserveNum : burMaxNum;
            num = Source.PutOut(Data, num, Mission.ReserveEmpty);
            if (num > 0)
            {
                SoureceReserveNum -= num;
                CurNum += num;
                if (SoureceReserveNum > 0)
                {
                    SoureceReserveNum -= Source.RemoveReservePutOut(Data, SoureceReserveNum);
                }
            }
            else { End(); }
        }
        public void PutInTarget()
        {
            if (CurNum > 0)
            {
                FinishNum += CurNum;
                TargetReserveNum -= CurNum;
                CurNum = 0;
                if (Mission.ReplaceIndex >= 0) { Target.PutIn(Mission.ReplaceIndex, Data, FinishNum); }
                else { Target.PutIn(Data, FinishNum); }
                Worker.SettleTransport();
                End();
                Mission.FinishNum += FinishNum;
            }
            else { End(); }
        }
        public void End()
        {
            if (CurNum > 0)
            {
                Data.ConvertToWorldObj(CurNum, Worker.transform);
            }
            if (Worker != null)
            {
                Worker.Transport = null;
                Worker.ClearDestination();
            }
            if (SoureceReserveNum > 0)
            {
                Source?.RemoveReservePutOut(Data, SoureceReserveNum);
            }
            if (TargetReserveNum > 0)
            {
                Target?.RemoveReservePutIn(Data, TargetReserveNum, Mission.ReserveEmpty);
            }
            Mission?.RemoveTransport(this);
            Source?.RemoveTranport(this);
            Target?.RemoveTranport(this);
            Data = null;
        }
        #endregion
    }
}