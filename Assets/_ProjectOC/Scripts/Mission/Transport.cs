using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.MissionNS
{
    [LabelText("����"), System.Serializable]
    public abstract class Transport<T> : ITransport
    {
        #region Data
        [LabelText("��������������"), ReadOnly, ShowInInspector]
        public T Data;
        [LabelText("��������������"), HideInInspector]
        public MissionTransport<T> Mission;
        [LabelText("ȡ����"), HideInInspector]
        public IMissionObj<T> Source;
        [LabelText("�ͻ���"), HideInInspector]
        public IMissionObj<T> Target;
        #endregion

        #region Property
        [LabelText("����ð��˵ĵ���"), ReadOnly, HideInInspector]
        public WorkerNS.Worker Worker { get; set; }
        [LabelText("��Ҫ���˵�����"), ReadOnly, ShowInInspector]
        public int MissionNum { get; set; }
        [LabelText("��ǰ�õ�������"), ReadOnly, ShowInInspector]
        public int CurNum { get; set; }
        [LabelText("��ɵ�����"), ReadOnly, ShowInInspector]
        public int FinishNum { get; set; }
        [LabelText("ȡ����Ԥ��������"), ReadOnly, ShowInInspector]
        public int SoureceReserveNum { get; set; }
        [LabelText("�ͻ���Ԥ��������"), ReadOnly, ShowInInspector]
        public int TargetReserveNum { get; set; }
        [LabelText("�Ƿ񵽴�ȡ����"), ReadOnly, ShowInInspector]
        public bool ArriveSource { get; set; }
        [LabelText("�Ƿ񵽴�Ŀ�ĵ�"), ReadOnly, ShowInInspector]
        public bool ArriveTarget { get; set; }
        [LabelText("�Ƿ�����Ч��"), ReadOnly, ShowInInspector]
        public bool IsValid => Data != null;
        #endregion

        #region Abstract
        protected abstract int GetWeight();
        protected abstract bool WorkerAddData(int num);
        protected abstract int WorkerRemoveData(int num);
        protected abstract void DataToWorldItem(int num);
        #endregion

        #region Method
        public Transport(MissionTransport<T> mission, int missionNum, IMissionObj<T> source, IMissionObj<T> target, WorkerNS.Worker worker)
        {
            Data = mission.Data;
            Mission = mission;
            Source = source;
            Target = target;
            Mission.AddTransport(this);
            Source.AddTransport(this);
            Target.AddTransport(this);

            MissionNum = missionNum;
            SoureceReserveNum = Source.ReservePutOut(mission.Data, missionNum);
            TargetReserveNum = Target.ReservePutIn(mission.Data, missionNum);

            Worker = worker;
            if (Data == null || SoureceReserveNum == 0 || TargetReserveNum == 0)
            {
                End();
            }
            else
            {
                Worker.Transport = this;
                Worker.SetDestination(Source.GetTransform().position, OnSourceArriveEvent);
            }
        }
        public Transform GetSourceTransform() { return Source.GetTransform(); }
        public Transform GetTargetTransform() { return Target.GetTransform(); }
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
            worker.SetDestination(worker.Transport.GetTargetTransform().position, OnTargetArriveEvent);
        }
        private void OnTargetArriveEvent(WorkerNS.Worker worker)
        {
            worker.Transport.ArriveTarget = true;
            worker.Transport.PutInTarget();
        }
        public void PutOutSource()
        {
            int weight = GetWeight();
            int burMaxNum = weight != 0 ? (Worker.RealBURMax - Worker.WeightCurrent) / weight : SoureceReserveNum;
            int num = SoureceReserveNum <= burMaxNum ? SoureceReserveNum : burMaxNum;
            num = Source.PutOut(Data, num);
            if (num > 0 && WorkerAddData(num))
            {
                SoureceReserveNum -= num;
                CurNum += num;
                if (SoureceReserveNum > 0)
                {
                    SoureceReserveNum -= Source.RemoveReservePutOut(Data, SoureceReserveNum);
                }
            }
            else
            {
                Source.PutIn(Data, num);
                End();
            }
        }
        public void PutInTarget()
        {
            int num = WorkerRemoveData(CurNum);
            if (num > 0)
            {
                CurNum -= num;
                FinishNum += num;
                TargetReserveNum -= num;
                Target.PutIn(Data, FinishNum);
                Worker.SettleTransport();
                End();
                Mission.FinishNum += FinishNum;
            }
            else
            {
                End();
            }
        }
        public void End()
        {
            if (CurNum > 0)
            {
                int remove = WorkerRemoveData(CurNum);
                DataToWorldItem(remove);
            }
            Worker.Transport = null;
            Worker.ClearDestination();
            if (SoureceReserveNum > 0)
            {
                Source?.RemoveReservePutOut(Data, SoureceReserveNum);
            }
            if (TargetReserveNum > 0)
            {
                Target?.RemoveReservePutIn(Data, TargetReserveNum);
            }
            Mission?.RemoveTransport(this);
            Source?.RemoveTranport(this);
            Target?.RemoveTranport(this);
            Data = default(T);
            Mission = null;
            Source = null;
            Target = null;
            Worker = null;
        }
        #endregion
    }
}