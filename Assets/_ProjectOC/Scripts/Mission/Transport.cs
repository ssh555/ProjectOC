using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.MissionNS
{
    [LabelText("����"), System.Serializable]
    public class Transport
    {
        #region Data
        [LabelText("���˶�����ID"), ReadOnly]
        public string ID = "";
        [LabelText("��������������"), ReadOnly]
        public MissionTransport Mission;
        [LabelText("ȡ����"), ReadOnly]
        public IMissionObj Source;
        [LabelText("�ͻ���"), ReadOnly]
        public IMissionObj Target;
        [LabelText("����ð��˵ĵ���"), ReadOnly]
        public WorkerNS.Worker Worker;
        [LabelText("��Ҫ���˵�����"), ReadOnly]
        public int MissionNum;

        [LabelText("��ǰ�õ�������"), ReadOnly]
        public int CurNum;
        [LabelText("��ɵ�����"), ReadOnly]
        public int FinishNum;
        [LabelText("ȡ����Ԥ��������"), ReadOnly]
        public int SoureceReserveNum;
        [LabelText("�ͻ���Ԥ��������"), ReadOnly]
        public int TargetReserveNum;
        [LabelText("�Ƿ񵽴�ȡ����"), ReadOnly]
        public bool ArriveSource;
        [LabelText("�Ƿ񵽴�Ŀ�ĵ�"), ReadOnly]
        public bool ArriveTarget;
        #endregion

        public Transport(MissionTransport mission, string id, int missionNum, IMissionObj source, IMissionObj target, WorkerNS.Worker worker)
        {
            Mission = mission;
            ID = id;
            MissionNum = missionNum;
            Source = source;
            Target = target;
            Worker = worker;

            Mission.AddTransport(this);
            Source.AddTransport(this);
            Target.AddTransport(this);

            SoureceReserveNum = Source.ReservePutOut(mission.ID, missionNum);
            TargetReserveNum = Target.ReservePutIn(mission.ID, missionNum);

            Worker.Transport = this;
            Worker.SetDestination(Source.GetTransform().position, OnSourceArriveEvent);
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

        /// <summary>
        /// ��ȡ�����ó�
        /// </summary>
        public void PutOutSource()
        {
            int putOutNum = SoureceReserveNum > 0 ? SoureceReserveNum : MissionNum;
            int weight = ML.Engine.InventorySystem.ItemManager.Instance.GetWeight(ID);
            int burMaxNum = weight != 0 ? (Worker.WeightMax - Worker.WeightCurrent) / weight : putOutNum;
            putOutNum = putOutNum <= burMaxNum ? putOutNum : burMaxNum;
            int sourceNum = Source.PutOut(ID, putOutNum);
            if (!Worker.TransportDict.ContainsKey(ID))
            {
                Worker.TransportDict[ID] = 0;
            }
            Worker.TransportDict[ID] += sourceNum;
            SoureceReserveNum -= sourceNum;
            CurNum += sourceNum;
            if (SoureceReserveNum > 0)
            {
                SoureceReserveNum -= Source.RemoveReservePutOut(ID, SoureceReserveNum);
            }
        }

        /// <summary>
        /// �����ͻ���
        /// </summary>
        public void PutInTarget()
        {
            if (Worker.TransportDict.ContainsKey(ID))
            {
                int remove = Worker.TransportDict[ID] <= CurNum ? Worker.TransportDict[ID] : CurNum;
                Worker.TransportDict[ID] -= remove;
                CurNum -= remove;
                FinishNum += remove;
                TargetReserveNum -= remove;
                if (Worker.TransportDict[ID] == 0)
                {
                    Worker.TransportDict.Remove(ID);
                }
                Target.PutIn(ID, FinishNum);
                Worker.SettleTransport();
                if (TargetReserveNum > 0)
                {
                    TargetReserveNum -= Target.RemoveReservePutIn(ID, TargetReserveNum);
                }
            }
            End();
            Mission.FinishNum += FinishNum;
        }

        /// <summary>
        /// ��������
        /// </summary>
        public void End(bool removeMission=true)
        {
            if (Worker.TransportDict.ContainsKey(ID) &&  CurNum > 0)
            {
                CurNum = CurNum <= Worker.TransportDict[ID] ? CurNum : Worker.TransportDict[ID];
                Worker.TransportDict[ID] -= CurNum;
                if (Worker.TransportDict[ID] == 0)
                {
                    Worker.TransportDict.Remove(ID);
                }
            }
            if (ML.Engine.InventorySystem.ItemManager.Instance != null)
            {
                List<ML.Engine.InventorySystem.Item> items = ML.Engine.InventorySystem.ItemManager.Instance.SpawnItems(ID, CurNum);
                foreach (var item in items)
                {
#pragma warning disable CS4014
                    ML.Engine.InventorySystem.ItemManager.Instance.SpawnWorldItem(item, Worker.transform.position, Worker.transform.rotation);
#pragma warning restore CS4014
                }
            }
            Worker.Transport = null;
            Worker.ClearDestination();
            if (SoureceReserveNum > 0)
            {
                Source?.RemoveReservePutOut(ID, SoureceReserveNum);
            }
            if (TargetReserveNum > 0)
            {
                Target?.RemoveReservePutIn(ID, TargetReserveNum);
            }
            if (removeMission)
            {
                Mission.RemoveTransport(this);
            }
            Source?.RemoveTranport(this);
            Target?.RemoveTranport(this);
        }
    }
}
