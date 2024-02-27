using ML.Engine.InventorySystem;
using ProjectOC.WorkerNS;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.MissionNS
{
    /// <summary>
    /// 搬运
    /// </summary>
    [System.Serializable]
    public class Transport
    {
        /// <summary>
        /// 搬运所属的任务
        /// </summary>
        [System.NonSerialized]
        public MissionTransport Mission;

        /// <summary>
        /// 搬运物品ID
        /// </summary>
        public string ItemID = "";

        /// <summary>
        /// 需要搬运的数量
        /// </summary>
        public int MissionNum;

        /// <summary>
        /// 取货地
        /// </summary>
        public IMissionObj Source;

        /// <summary>
        /// 送货地
        /// </summary>
        public IMissionObj Target;

        /// <summary>
        /// 该任务的刁民
        /// </summary>
        public Worker Worker;

        /// <summary>
        /// 当前拿到的数量
        /// </summary>
        public int CurNum
        {
            get
            {
                int result = 0;
                if (this.Worker != null)
                {
                    foreach (Item item in Worker.TransportItems)
                    {
                        if (item.ID == ItemID)
                        {
                            result += item.Amount;
                        }
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 还需要搬运的数量
        /// </summary>
        public int NeedTransportNum
        {
            get
            {
                return MissionNum - FinishNum - CurNum;
            }
        }

        /// <summary>
        /// 完成的数量
        /// </summary>
        public int FinishNum;

        /// <summary>
        /// 刁民搬运状态
        /// </summary>
        public TransportState TransportState
        {
            get
            {
                return CurNum > 0 ? TransportState.HoldingObjects : TransportState.EmptyHanded;
            }
        }

        public Transport(MissionTransport mission, string itemID, int missionNum, IMissionObj source, IMissionObj destination, Worker worker)
        {
            this.Mission = mission;
            this.ItemID = itemID;
            this.MissionNum = missionNum;
            this.Source = source;
            this.Target = destination;
            this.Worker = worker;
            this.Mission.Transports.Add(this);
            this.Source.AddTransport(this);
            this.Target.AddTransport(this);
            this.Worker.Transport = this;
            this.Worker.SetTimeStatusAll(TimeStatus.Work_Transport);
            this.Worker.SetDestination(this.Worker.Transport.Source.GetTransform(), Transport_Source_Action);
        }
        private void Transport_Source_Action(Worker worker)
        {
            worker.Transport.PutOutSource();
            worker.SetDestination(worker.Transport.Target.GetTransform(), Transport_Target_Action);
        }

        private void Transport_Target_Action(Worker worker)
        {
            worker.Transport.PutInTarget();
            worker.ClearDestination();
        }

        /// <summary>
        /// 从取货地拿出
        /// </summary>
        public void PutOutSource()
        {
            bool flagBurden = false;
            bool flagSource = false;
            List<Item> items = ItemManager.Instance.SpawnItems(ItemID, this.NeedTransportNum);
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
                if (flagBurden || flagSource)
                {
                    break;
                }
            }
            if (flagSource)
            {
                //Debug.LogError("Source is not Enough");
            }
        }

        /// <summary>
        /// 放入送货地
        /// </summary>
        public void PutInTarget()
        {
            int num = this.CurNum;
            bool flagTarget = this.Target.PutIn(this.ItemID, num);
            if (flagTarget)
            {
                this.Worker.TransportItems.Clear();
                this.FinishNum += num;
                // 任务结束
                if (this.FinishNum == this.MissionNum)
                {
                    this.Worker.SettleTransport();
                    this.End();
                }
                else if (this.FinishNum > this.MissionNum)
                {
                    //Debug.LogError($"FinishNum > MissionNum");
                }
                this.Mission.FinishNum += num;
            }
            else
            {
                //Debug.LogError($"Target Cannot Put In {ItemID} {num}");
                this.End();
            }
        }

        /// <summary>
        /// 强制结束搬运
        /// </summary>
        public void End(bool remove=true)
        {
            foreach (Item item in Worker.TransportItems)
            {
                ItemManager.Instance.SpawnWorldItem(item, Worker.transform.position, Worker.transform.rotation);
            }
            Worker.SetTimeStatusAll(TimeStatus.Relax);
            Worker.TransportItems.Clear();
            Worker.Transport = null;
            Worker.ClearDestination();
            if (remove)
            {
                Mission.Transports.Remove(this);
            }
            Source.RemoveTranport(this);
            Target.RemoveTranport(this);
        }
    }
}

