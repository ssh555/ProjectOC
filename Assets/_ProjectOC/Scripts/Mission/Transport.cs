using ML.Engine.InventorySystem;
using ProjectOC.WorkerNS;
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
        public IMission Source;
        /// <summary>
        /// 送货地
        /// </summary>
        public IMission Target;
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
        public bool IsFinish
        {
            get 
            {
                return this.FinishNum >= this.MissionNum;
            }
        }
        public Transport(MissionTransport mission, string itemID, int missionNum, IMission source, IMission destination, Worker worker)
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
        }
        /// <summary>
        /// 从取货地拿出
        /// </summary>
        public void PutOutSource()
        {
            bool flagBurden = false;
            bool flagSource = false;
            while (true)
            {
                Item item = ItemSpawner.Instance.SpawnItem(ItemID, this.NeedTransportNum);
                if (item.Weight + Worker.BURCurrent >= Worker.BURMax)
                {
                    int num = (Worker.BURMax - Worker.BURCurrent) / item.SingleItemWeight;
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
                if (this.NeedTransportNum == 0 || flagBurden || flagSource)
                {
                    break;
                }
            }
        }
        /// <summary>
        /// 放入送货地
        /// </summary>
        public void PutInTarget()
        {
            bool flagTarget = this.Target.PutIn(this.ItemID, this.CurNum);
            if (flagTarget)
            {
                this.FinishNum += this.CurNum;
                this.Mission.FinishNum += this.CurNum;
                this.Worker.TransportItems.Clear();
                // 任务结束
                if (this.FinishNum == this.MissionNum)
                {
                    this.Worker.SettleTransport();
                    this.End();
                }
            }
            else
            {
                this.End();
            }
        }
        /// <summary>
        /// 强制结束搬运
        /// </summary>
        public void End()
        {
            foreach (Item item in Worker.TransportItems)
            {
                ItemSpawner.Instance.SpawnWorldItem(item, Worker.transform.position, Worker.transform.rotation);
            }
            Worker.TransportItems.Clear();
            Worker.Transport = null;
            Mission.Transports.Remove(this);
            Source.RemoveTranport(this);
            Target.RemoveTranport(this);
        }
    }
}

