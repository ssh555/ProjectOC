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
        [LabelText("���˵Ķ���"), ReadOnly]
        public ML.Engine.InventorySystem.Item Item;
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

        public void Init(MissionTransport mission, int missionNum, IMissionObj source, IMissionObj target, WorkerNS.Worker worker)
        {
            Mission = mission;
            ID = mission.ID;
            MissionNum = missionNum;
            Source = source;
            Target = target;
            Worker = worker;

            Mission.AddTransport(this);
            Source.AddTransport(this);
            Target.AddTransport(this);

            if (Item != null)
            {
                SoureceReserveNum = Source.ReservePutOut(Item);
                TargetReserveNum = Target.ReservePutIn(Item);
            }
            else
            {
                SoureceReserveNum = Source.ReservePutOut(mission.ID, missionNum);
                TargetReserveNum = Target.ReservePutIn(mission.ID, missionNum);
            }

            Worker.Transport = this;
            Worker.SetDestination(Source.GetTransform().position, OnSourceArriveEvent);

            if (Item != null && (SoureceReserveNum != 1 || TargetReserveNum != 1))
            {
                End();
            }
        }
        public Transport(MissionTransport mission, int missionNum, IMissionObj source, IMissionObj target, WorkerNS.Worker worker)
        {
            Init(mission, missionNum, source, target, worker);
        }
        public Transport(MissionTransport mission, IMissionObj source, IMissionObj target, WorkerNS.Worker worker)
        {
            ML.Engine.InventorySystem.Item item = mission.Item;
            if (item != null && !string.IsNullOrEmpty(item.ID))
            {
                if (!ManagerNS.LocalGameManager.Instance.ItemManager.GetCanStack(item.ID) && item.Amount == 1)
                {
                    Item = item;
                }
                Init(mission, item.Amount, source, target, worker);
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

        /// <summary>
        /// ��ȡ�����ó�
        /// </summary>
        public void PutOutSource()
        {
            if (Item != null)
            {
                if (Worker.RealBURMax - Worker.WeightCurrent >= Item.Weight)
                {
                    int sourceNum = Source.PutOut(Item);
                    if (sourceNum == 1)
                    {
                        Worker.TransportItems.Add(Item);
                        SoureceReserveNum -= 1;
                        CurNum += 1;
                        return;
                    }
                }
                End();
            }
            else
            {
                int putOutNum = SoureceReserveNum > 0 ? SoureceReserveNum : MissionNum;
                int weight = ML.Engine.InventorySystem.ItemManager.Instance.GetWeight(ID);
                int burMaxNum = weight != 0 ? (Worker.RealBURMax - Worker.WeightCurrent) / weight : putOutNum;
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
        }

        /// <summary>
        /// �����ͻ���
        /// </summary>
        public void PutInTarget()
        {
            if (Item != null)
            {
                if (Worker.TransportItems.Contains(Item))
                {
                    Worker.TransportItems.Remove(Item);
                    CurNum -= 1;
                    TargetReserveNum -= 1;
                    Target.PutIn(Item);
                    Worker.SettleTransport();
                    End();
                    Mission.FinishNum += 1;
                }
                else
                {
                    End();
                }
            }
            else
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
                    End();
                    Mission.FinishNum += FinishNum;
                }
                else
                {
                    End();
                }
            }
        }

        /// <summary>
        /// ��������
        /// </summary>
        public void End()
        {
            if (Item != null)
            {
                if (Worker != null && Worker.TransportItems.Contains(Item) && CurNum > 0)
                {
                    Worker.TransportItems.Remove(Item);
                    ML.Engine.InventorySystem.ItemManager.Instance?.SpawnWorldItem(Item, Worker.transform.position, Worker.transform.rotation);
                }
            }
            else
            {
                if (Worker != null && Worker.TransportDict.ContainsKey(ID) && CurNum > 0)
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
            }
            
            Worker.Transport = null;
            Worker.ClearDestination();
            if (SoureceReserveNum > 0)
            {
                if (Item != null) { Source?.RemoveReservePutOut(Item); }
                else { Source?.RemoveReservePutOut(ID, SoureceReserveNum); }
            }
            if (TargetReserveNum > 0)
            {
                if (Item != null) { Target?.RemoveReservePutIn(Item); }
                else { Target?.RemoveReservePutIn(ID, TargetReserveNum); }
            }
            Mission?.RemoveTransport(this);
            Source?.RemoveTranport(this);
            Target?.RemoveTranport(this);
        }
    }
}