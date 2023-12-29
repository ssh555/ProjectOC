using ML.Engine.InventorySystem;
using ProjectOC.WorkerNS;
using UnityEngine;

namespace ProjectOC.MissionNS
{
    /// <summary>
    /// ����
    /// </summary>
    [System.Serializable]
    public class Transport
    {
        /// <summary>
        /// ��������������
        /// </summary>
        public MissionTransport Mission;
        /// <summary>
        /// ������ƷID
        /// </summary>
        public string ItemID = "";
        /// <summary>
        /// ��Ҫ���˵�����
        /// </summary>
        public int MissionNum;
        /// <summary>
        /// ȡ����
        /// </summary>
        public IMission Source;
        /// <summary>
        /// �ͻ���
        /// </summary>
        public IMission Target;
        /// <summary>
        /// ������ĵ���
        /// </summary>
        public Worker Worker;
        /// <summary>
        /// ��ǰ�õ�������
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
        /// ����Ҫ���˵�����
        /// </summary>
        public int NeedTransportNum
        {
            get
            {
                return MissionNum - FinishNum - CurNum;
            }
        }
        /// <summary>
        /// ��ɵ�����
        /// </summary>
        public int FinishNum;
        /// <summary>
        /// �������״̬
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
        /// ��ȡ�����ó�
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
        /// �����ͻ���
        /// </summary>
        public void PutInTarget()
        {
            bool flagTarget = this.Target.PutIn(this.ItemID, this.CurNum);
            if (flagTarget)
            {
                this.FinishNum += this.CurNum;
                this.Mission.FinishNum += this.CurNum;
                this.Worker.TransportItems.Clear();
                // �������
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
        /// ǿ�ƽ�������
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

