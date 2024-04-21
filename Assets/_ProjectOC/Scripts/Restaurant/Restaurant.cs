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
    [LabelText("餐厅"), Serializable]
    public class Restaurant : IInventory, IMissionObj
    {
        #region WorldRestaurant
        [LabelText("世界餐厅"), ReadOnly]
        public WorldRestaurant WorldRestaurant;
        [ShowInInspector, ReadOnly]
        public string UID { get { return WorldRestaurant?.InstanceID ?? ""; } }
        #endregion

        #region 当前数据
        [LabelText("座位"), ShowInInspector, ReadOnly]
        private RestaurantSeat[] Seats;
        [LabelText("存储数据"), ShowInInspector, ReadOnly]
        private RestaurantData[] Datas;
        #endregion

        #region 属性
        [LabelText("是否有座位"), ShowInInspector, ReadOnly]
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
        [LabelText("是否有食物"), ShowInInspector, ReadOnly]
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
        /// 先查找空座位，找不到空座位返回false，更新该座位的信息，让刁民寻路到该座位。
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
        /// 刁民到达座位后调用，如果餐厅的HasFood为false，则清空刁民的座位数据，将刁民加入Manager的队列中；
        /// 反之调用FindFood给刁民分配食物，更新座位的食物ID，然后调用EatFood。
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
        /// 给玩家分配食物
        /// 将Datas转为列表并排序，遍历该列表，如果有No1和No2的食物，则返回No1或No2，
        /// 否则继续遍历该列表，返回第一个能让刁民体力值溢出的食物，找不到的话就返回体力值最大的食物。
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
        /// 获取id对应的数量
        /// </summary>
        /// <param name="id">食物id或者物品id</param>
        /// <param name="isFoodID">true表示食物id，false表示物品id</param>
        /// <param name="isOut">true表示能取出的数量，false表示能存入的数量</param>
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

        #region 数据方法
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
