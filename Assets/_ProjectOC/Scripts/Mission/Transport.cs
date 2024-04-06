using ML.Engine.InventorySystem;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System;
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
        [LabelText("������ƷID")]
        public string ItemID = "";
        [LabelText("��������������"), NonSerialized]
        public MissionTransport Mission;
        [LabelText("ȡ����")]
        public IMissionObj Source;
        [LabelText("�ͻ���")]
        public IMissionObj Target;
        [LabelText("����ð��˵ĵ���")]
        public Worker Worker;

        [LabelText("��Ҫ���˵�����")]
        public int MissionNum;
        [LabelText("��ǰ�õ�������")]
        public int CurNum;
        [LabelText("��ɵ�����")]
        public int FinishNum;
        [LabelText("ȡ����Ԥ��������")]
        public int SoureceReserveNum;
        [LabelText("�ͻ���Ԥ��������")]
        public int TargetReserveNum;

        [LabelText("�Ƿ񵽴�ȡ����")]
        public bool ArriveSource;


        public Transport(MissionTransport mission, string itemID, int missionNum, IMissionObj source, IMissionObj destination, Worker worker)
        {
            this.ItemID = itemID;
            this.Mission = mission;
            this.Source = source;
            this.Target = destination;
            this.Worker = worker;
            this.MissionNum = missionNum;

            this.Mission.Transports.Add(this);
            this.Source.AddTransport(this);
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
            worker.Transport.PutInTarget();
        }

        /// <summary>
        /// ��ȡ�����ó�
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
                CurNum += item.Amount;
                if (flagBurden || flagSource)
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
        /// ǿ�ƽ�������
        /// </summary>
        public void End(bool remove=true)
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
            if (Source != null && Source is StoreNS.Store source)
            {
                source.RemoveReserveStorage(ItemID, SoureceReserveNum);
            }
            if (Target != null && Target is StoreNS.Store target)
            {
                target.RemoveReserveEmpty(ItemID, TargetReserveNum);
            }

            if (remove)
            {
                Mission.Transports.Remove(this);
            }
            Source?.RemoveTranport(this);
            Target?.RemoveTranport(this);
        }
    }
}

