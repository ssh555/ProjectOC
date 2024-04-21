using ML.Engine.InventorySystem;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ProjectOC.MissionNS;
using ProjectOC.ManagerNS;
using ProjectOC.WorkerNS;
using System.Linq;


namespace ProjectOC.RestaurantNS
{
    [LabelText("����"), Serializable]
    public class Restaurant : IInventory, IMissionObj
    {
        #region WorldRestaurant
        [LabelText("�������"), ReadOnly]
        public WorldRestaurant WorldRestaurant;
        [ShowInInspector, ReadOnly]
        public string UID { get { return WorldRestaurant?.InstanceID ?? ""; } }
        #endregion

        #region ��ǰ����
        [LabelText("��λ"), ShowInInspector, ReadOnly]
        private RestaurantSeat[] Seats;
        [LabelText("�洢����"), ShowInInspector, ReadOnly]
        private RestaurantData[] Datas;
        #endregion

        #region ����
        [LabelText("�Ƿ�����λ"), ShowInInspector, ReadOnly]
        public bool HasSeat
        {
            get
            {
                foreach (RestaurantSeat seat in Seats)
                {
                    if (!seat.HasWorker)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        [LabelText("�Ƿ���ʳ��"), ShowInInspector, ReadOnly]
        public bool HasFood
        {
            get
            {
                foreach (RestaurantData data in Datas)
                {
                    if (LocalGameManager.Instance.RestaurantManager.WorkerFood_IsValidID(data.ID))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        #endregion

        public Restaurant()
        {
            Seats = new RestaurantSeat[LocalGameManager.Instance.RestaurantManager.SeatNum];
            for (int i = 0; i < Seats.Length; i++)
            {
                Seats[i].Restaurant = this;
                Seats[i].Socket = WorldRestaurant.transform;
            }
            Datas = new RestaurantData[LocalGameManager.Instance.RestaurantManager.DataNum];
        }

        /// <summary>
        /// �Ȳ��ҿ���λ���Ҳ�������λ����false�����¸���λ����Ϣ���õ���Ѱ·������λ��
        /// </summary>
        public bool AddWorker(Worker worker)
        {
            if (worker != null)
            {
                for (int i = 0; i < Seats.Length; i++)
                {
                    if (!Seats[i].HasWorker)
                    {
                        Seats[i].SetWorker(worker);
                        worker.SetDestination(Seats[i].Socket.position, OnArriveEvent);
                        LocalGameManager.Instance.RestaurantManager.RemoveWorker(worker);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// ���񵽴���λ����ã����������HasFoodΪfalse������յ������λ���ݣ����������Manager�Ķ����У�
        /// ��֮����FindFood���������ʳ�������λ��ʳ��ID��Ȼ�����EatFood��
        /// </summary>
        private void OnArriveEvent(Worker worker)
        {
            int seatIndex = GetWorkerSeatIndex(worker);
            if (worker != null && seatIndex > 0)
            {
                if (HasFood)
                {
                    int index = FindFood(worker);
                    if (0 <= index && index < Datas.Length)
                    {
                        worker.Agent.enabled = false;
                        worker.LastPosition = worker.transform.position;
                        worker.transform.position = Seats[seatIndex].Socket.position + new Vector3(0, 2f, 0);
                        if (Change(index, -1) == 1)
                        {
                            Seats[index].SetFood(Datas[index].ID);
                            return;
                        }
                    }
                }
                Seats[seatIndex].ClearData();
            }
            LocalGameManager.Instance.RestaurantManager.AddWorker(worker);
        }

        /// <summary>
        /// ����ҷ���ʳ��
        /// ��DatasתΪ�б����򣬱������б������No1��No2��ʳ��򷵻�No1��No2��
        /// ��������������б����ص�һ�����õ�������ֵ�����ʳ��Ҳ����Ļ��ͷ�������ֵ����ʳ�
        /// </summary>
        public int FindFood(Worker worker)
        {
            if (worker != null)
            {
                List<Tuple<RestaurantData, int>> tuples = Datas.Select((data, index) => Tuple.Create(data, index)).ToList();
                tuples = tuples.OrderBy(t => t.Item1, new RestaurantData.Sort()).ToList();
                foreach (var tuple in tuples)
                {
                    RestaurantData data = tuple.Item1;
                    if (data.HasFood && (data.Priority != FoodPriority.None || worker.APCurrent + data.AlterAP >= worker.APMax))
                    {
                        return tuple.Item2;
                    }
                }
                return tuples.Count > 0 ? tuples.Count - 1 : -1;
            }
            return -1;
        }

        #region Getter
        public int GetWorkerSeatIndex(Worker worker)
        {
            int seatIndex = -1;
            if (worker != null)
            {
                for (int i = 0; i < Seats.Length; i++)
                {
                    if (Seats[i].Worker == worker)
                    {
                        seatIndex = i;
                        break;
                    }
                }
            }
            return seatIndex;
        }
        /// <summary>
        /// ��ȡid��Ӧ������
        /// </summary>
        /// <param name="id">ʳ��id������Ʒid</param>
        /// <param name="isFoodID">true��ʾʳ��id��false��ʾ��Ʒid</param>
        /// <param name="isOut">true��ʾ��ȡ����������false��ʾ�ܴ��������</param>
        public int GetAmount(string id, bool isFoodID=true, bool isOut=true)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(id))
            {
                foreach (var data in Datas)
                {
                    if (data.HasSetFood && id == (isFoodID ? data.ID : data.ItemID))
                    {
                        result += isOut ? data.Amount : data.MaxCapacity - data.Amount;
                    }
                }
            }
            return result;
        }
        #endregion

        #region ���ݷ���
        public int Change(int index, int amount, bool exceed = false, bool complete = true)
        {
            lock (this)
            {
                if (0 <= index && index < Datas.Length && Datas[index].HasSetFood && amount != 0)
                {
                    if ((!exceed && Datas[index].Amount >= Datas[index].MaxCapacity) || (complete && amount + Datas[index].Amount < 0))
                    {
                        return 0;
                    }
                    amount = !complete && amount + Datas[index].Amount < 0 ? Datas[index].Amount : amount;
                    Datas[index].Amount += amount;
                    return amount;
                }
                return 0;
            }
        }
        public int Change(string id, int amount, bool isFoodID = true, bool exceed = false, bool complete = true)
        {
            lock (this)
            {
                if (!string.IsNullOrEmpty(id) && amount != 0)
                {
                    int amountAll = GetAmount(id, isFoodID, amount < 0);
                }
                return 0;
            }
        }
        #endregion

        #region IInventory
        public bool AddItem(Item item)
        {
            throw new System.NotImplementedException();
        }

        public int GetItemAllNum(string id)
        {
            throw new System.NotImplementedException();
        }

        public Item[] GetItemList()
        {
            throw new System.NotImplementedException();
        }

        public bool RemoveItem(Item item)
        {
            throw new System.NotImplementedException();
        }

        public Item RemoveItem(Item item, int amount)
        {
            throw new System.NotImplementedException();
        }

        public bool RemoveItem(string itemID, int amount)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region IMissionObj
        public Transform GetTransform()
        {
            throw new NotImplementedException();
        }

        public TransportPriority GetTransportPriority()
        {
            throw new NotImplementedException();
        }

        public string GetUID()
        {
            throw new NotImplementedException();
        }

        public bool PutIn(string itemID, int amount)
        {
            throw new NotImplementedException();
        }

        public int PutOut(string itemID, int amount)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
