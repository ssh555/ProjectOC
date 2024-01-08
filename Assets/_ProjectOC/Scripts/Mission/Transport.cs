using ML.Engine.InventorySystem;
using ProjectOC.WorkerNS;
using System.Collections.Generic;
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
                Debug.LogError("Source is not Enough");
            }
        }

        /// <summary>
        /// �����ͻ���
        /// </summary>
        public void PutInTarget()
        {
            int num = this.CurNum;
            bool flagTarget = this.Target.PutIn(this.ItemID, num);
            if (flagTarget)
            {
                this.FinishNum += num;
                this.Mission.FinishNum += num;
                this.Worker.TransportItems.Clear();
                // �������
                if (this.FinishNum == this.MissionNum)
                {
                    this.Worker.SettleTransport();
                    this.End();
                }
                else if (this.FinishNum > this.MissionNum)
                {
                    Debug.LogError($"FinishNum > MissionNum");
                }
            }
            else
            {
                Debug.LogError($"Target Cannot Put In {ItemID} {num}");
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
                ItemManager.Instance.SpawnWorldItem(item, Worker.transform.position, Worker.transform.rotation);
            }
            Worker.TransportItems.Clear();
            Worker.Transport = null;
            Mission.Transports.Remove(this);
            Source.RemoveTranport(this);
            Target.RemoveTranport(this);
        }
    }
}

