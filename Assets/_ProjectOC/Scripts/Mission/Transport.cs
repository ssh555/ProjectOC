using ML.Engine.InventorySystem;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.MissionNS
{
    [LabelText("搬运"), System.Serializable]
    public class Transport
    {
        [LabelText("搬运物品ID"), ReadOnly]
        public string ItemID = "";
        [LabelText("搬运所属的任务"), ReadOnly]
        public MissionTransport Mission;
        [LabelText("取货地"), ReadOnly]
        public IMissionObj Source;
        [LabelText("送货地"), ReadOnly]
        public IMissionObj Target;
        [LabelText("负责该搬运的刁民"), ReadOnly]
        public Worker Worker;
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


        public Transport(MissionTransport mission, string itemID, int missionNum, IMissionObj source, IMissionObj destination, Worker worker)
        {
            this.ItemID = itemID;
            this.Mission = mission;
            this.Source = source;
            this.Target = destination;
            this.Worker = worker;
            this.MissionNum = missionNum;

            this.Mission.AddTransport(this);
            this.Source.AddTransport(this);
            this.Source.ReservePutOut(mission.ItemID, missionNum, this);
            this.Target.ReservePutIn(mission.ItemID, missionNum, this);

            this.Target.AddTransport(this);
            this.Worker.Transport = this;
            this.Worker.SetDestination(this.Source.GetTransform().position, Transport_Source_Action);
        }

        public void UpdateDestination()
        {
            if (!ArriveSource)
            {
                if (this.Source.GetTransform().position != this.Worker.Target)
                {
                    this.Worker.SetDestination(this.Source.GetTransform().position, Transport_Source_Action);
                }
            }
            else
            {
                if (this.Target.GetTransform().position != this.Worker.Target)
                {
                    this.Worker.SetDestination(this.Target.GetTransform().position, Transport_Target_Action);
                }
            }
        }

        private void Transport_Source_Action(Worker worker)
        {
            worker.Transport.ArriveSource = true;
            worker.Transport.PutOutSource();
            worker.SetDestination(worker.Transport.Target.GetTransform().position, Transport_Target_Action);
        }

        private void Transport_Target_Action(Worker worker)
        {
            worker.Transport.ArriveTarget = true;
            worker.Transport.PutInTarget();
        }

        /// <summary>
        /// 从取货地拿出
        /// </summary>
        public void PutOutSource()
        {
            bool flagBurden = false;
            bool flagSource = false;
            List<Item> items = ItemManager.Instance.SpawnItems(ItemID, this.MissionNum);
            foreach (Item item in items)
            {
                if (item.Weight + Worker.BURCurrent >= Worker.BURMax)
                {
                    int num = (Worker.BURMax - Worker.BURCurrent) / ItemManager.Instance.GetWeight(item.ID);
                    item.Amount = num;
                    flagBurden = true;
                }
                int sourceNum = Source.PutOut(item.ID, item.Amount);
                if (sourceNum < item.Amount)
                {
                    item.Amount = sourceNum;
                    flagSource = true;
                }
                Worker.TransportItems.Add(item);
                SoureceReserveNum -= item.Amount;
                CurNum += item.Amount;
                if (flagBurden || flagSource)
                {
                    break;
                }
            }
            if (SoureceReserveNum > 0)
            {
                SoureceReserveNum -= Source.RemoveReservePutOut(ItemID, SoureceReserveNum);
            }
        }

        /// <summary>
        /// 放入送货地
        /// </summary>
        public void PutInTarget()
        {
            foreach (Item item in Worker.TransportItems)
            {
                if (item.ID == ItemID)
                {
                    if (item.Amount >= CurNum)
                    {
                        item.Amount -= CurNum;
                        FinishNum += CurNum;
                        CurNum = 0;
                        break;
                    }
                    else
                    {
                        CurNum -= item.Amount;
                        FinishNum += item.Amount;
                        item.Amount = 0;
                    }
                }
            }
            this.Worker.TransportItems.RemoveAll(item => item.Amount == 0);
            this.Target.PutIn(this.ItemID, this.FinishNum);
            this.Worker.SettleTransport();
            this.End();
            this.Mission.FinishNum += this.FinishNum;
        }

        /// <summary>
        /// 强制结束搬运
        /// </summary>
        public void End(bool removeMission=true)
        {
            List<Item> items = ItemManager.Instance.SpawnItems(ItemID, CurNum);
            foreach (Item item in items)
            {
                #pragma warning disable CS4014
                ItemManager.Instance.SpawnWorldItem(item, Worker.transform.position, Worker.transform.rotation);
                #pragma warning restore CS4014
            }
            foreach (Item item in Worker.TransportItems)
            {
                if (item.ID == ItemID)
                {
                    if (item.Amount >= CurNum)
                    {
                        item.Amount -= CurNum;
                        CurNum = 0;
                        break;
                    }
                    else
                    {
                        CurNum -= item.Amount;
                        item.Amount = 0;
                    }
                }
            }
            Worker.TransportItems.RemoveAll(item => item.Amount == 0);
            Worker.Transport = null;
            Worker.ClearDestination();
            if (!ArriveSource || SoureceReserveNum > 0)
            {
                Source?.RemoveReservePutOut(ItemID, SoureceReserveNum);
            }
            if (!ArriveTarget)
            {
                Target?.RemoveReservePutIn(ItemID, TargetReserveNum);
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

