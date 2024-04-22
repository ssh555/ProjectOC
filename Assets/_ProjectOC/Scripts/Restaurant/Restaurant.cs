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

        [LabelText("名字"), ReadOnly]
        public string Name = "";
        [LabelText("搬运优先级"), ReadOnly]
        public TransportPriority TransportPriority = TransportPriority.Normal;
        [LabelText("对应的搬运"), ReadOnly]
        public List<Transport> Transports = new List<Transport>();
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
            if (Datas.Length >= 1)
            {
                Datas[0].Priority = FoodPriority.No1;
            }
            if (Datas.Length >= 2)
            {
                Datas[1].Priority = FoodPriority.No2;
            }
        }

        public void Destroy()
        {
            foreach (var seat in Seats)
            {
                seat.ClearData();
            }

            List<Transport> transports = new List<Transport>();
            transports.AddRange(Transports);
            foreach (Transport transport in transports)
            {
                if (transport.Target == this || !transport.ArriveSource)
                {
                    transport?.End();
                }
            }
            Transports.Clear();

            List<Item> items = new List<Item>();
            foreach (var data in Datas)
            {
                if (ItemManager.Instance.IsValidItemID(data.ItemID) && data.Amount > 0)
                {
                    items.AddRange(ItemManager.Instance.SpawnItems(data.ItemID, data.Amount));
                }
            }
            items = (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).OCState.Inventory.AddItem(items);
            foreach (Item item in items)
            {
#pragma warning disable CS4014
                ItemManager.Instance.SpawnWorldItem(item, WorldRestaurant.transform.position, WorldRestaurant.transform.rotation);
#pragma warning restore CS4014
            }
        }

        public void OnPositionChange()
        {
            foreach (var transport in Transports)
            {
                transport?.UpdateDestination();
            }
        }

        #region 方法
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
                    if (data.HaveFood && (data.Priority != FoodPriority.None || worker.APCurrent + data.AlterAP >= worker.APMax))
                    {
                        return tuple.Item2;
                    }
                }
                return tuples.Count > 0 ? tuples.Count - 1 : -1;
            }
            return -1;
        }
        public string EatFood(Worker worker)
        {
            int index = FindFood(worker);
            if (0 <= index && index < Datas.Length && Change(index, -1) == 1)
            {
                return Datas[index].ID;
            }
            return null;
        }
        #endregion

        #region Action + Event
        /// <summary>
        /// 刁民到达座位后调用，如果餐厅的HasFood为false，则清空刁民的座位数据，将刁民加入Manager的队列中；
        /// 反之调用FindFood给刁民分配食物，更新座位的食物ID，然后调用EatFood。
        /// </summary>
        private void OnArriveEvent(Worker worker)
        {
            int seatIndex = GetWorkerSeatIndex(worker);
            if (worker != null && seatIndex > 0)
            {
                Seats[seatIndex].HasArrive = true;
                if (HasFood)
                {
                    int index = FindFood(worker);
                    if (Change(index, -1) == 1)
                    {
                        worker.Agent.enabled = false;
                        worker.LastPosition = worker.transform.position;
                        worker.transform.position = Seats[seatIndex].Socket.position + new Vector3(0, 2f, 0);
                        Seats[seatIndex].SetFood(Datas[index].ID);
                        return;
                    }
                }
                Seats[seatIndex].ClearData();
            }
            LocalGameManager.Instance.RestaurantManager.AddWorker(worker);
        }
        #endregion

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
        public bool HaveSetFood(string id, bool isFoodID = true)
        {
            if (!string.IsNullOrEmpty(id))
            {
                foreach (var data in Datas)
                {
                    if (data.HaveSetFood && id == (isFoodID ? data.ID : data.ItemID))
                    {
                        return true;
                    }
                }
            }
            return false;
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
                    if (data.HaveSetFood && id == (isFoodID ? data.ID : data.ItemID))
                    {
                        result += isOut ? data.Amount : data.MaxCapacity - data.Amount;
                    }
                }
            }
            return result;
        }
        #endregion

        #region 数据方法
        private bool ChangeFood(int index, string id)
        {
            if (0 <= index && index < Datas.Length)
            {
                string itemID = Datas[index].ItemID;
                int amount = Datas[index].Amount;
                Datas[index].ID = "";
                Datas[index].Amount = 0;

                foreach (Transport transport in Transports)
                {
                    if (transport != null && transport.ItemID == itemID 
                        && transport.Target == this && !HaveSetFood(itemID, false))
                    {
                        transport.End();
                    }
                }

                List<Item> items = new List<Item>();
                if (ItemManager.Instance.IsValidItemID(itemID) && amount > 0)
                {
                    items.AddRange(ItemManager.Instance.SpawnItems(itemID, amount));
                }
                items = (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).OCState.Inventory.AddItem(items);
                if (items != null)
                {
                    foreach (Item item in items)
                    {
#pragma warning disable CS4014
                        ItemManager.Instance.SpawnWorldItem(item, WorldRestaurant.transform.position, WorldRestaurant.transform.rotation);
#pragma warning restore CS4014
                    }
                }
                Datas[index].ID = id ?? "";
                return true;
            }
            return false;
        }

        private int Change(int index, int amount, bool exceed = false, bool complete = true)
        {
            lock (this)
            {
                if (0 <= index && index < Datas.Length && Datas[index].HaveSetFood && amount != 0)
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

        private int Change(string id, int amount, bool isFoodID = true, bool exceed = false, bool complete = true)
        {
            lock (this)
            {
                if (!string.IsNullOrEmpty(id) && amount != 0)
                {
                    bool isOut = amount < 0;
                    int amountAll = GetAmount(id, isFoodID, isOut);
                    if ((!exceed && amount >= amountAll) || (complete && amount + amountAll < 0))
                    {
                        return 0;
                    }
                    amount = amount < 0 ? -amount : amount;
                    int total = amount;
                    int firstIndex = -1;

                    for(int i = 0; i < Datas.Length; i++)
                    {
                        if (Datas[i].HaveSetFood && id == (isFoodID ? Datas[i].ID : Datas[i].ItemID))
                        {
                            firstIndex = firstIndex == -1 ? i : firstIndex;
                            int num = isOut ? Datas[i].Amount : Datas[i].MaxCapacity - Datas[i].Amount;
                            if (amount > num)
                            {
                                amount -= num;
                                Datas[i].Amount = isOut ? 0 : Datas[i].MaxCapacity;
                            }
                            else
                            {
                                Datas[i].Amount += isOut ? -amount : amount;
                                amount = 0;
                                break;
                            }
                        }
                    }

                    if (exceed && !isOut && amount > 0 && 0 <= firstIndex && firstIndex < Datas.Length)
                    {
                        Datas[firstIndex].Amount += amount;
                        amount = 0;
                    }
                    return total - amount;
                }
                return 0;
            }
        }
        #endregion

        #region UI接口
        #endregion

        #region IInventory
        public bool AddItem(Item item)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID))
            {
                return Change(item.ID, item.Amount, false) >= item.Amount;
            }
            return false;
        }
        public bool RemoveItem(Item item)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID))
            {
                return Change(item.ID, -item.Amount, false) >= item.Amount;
            }
            return false;
        }
        public Item RemoveItem(Item item, int amount)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID))
            {
                Item result = ItemManager.Instance.SpawnItem(item.ID);
                result.Amount = Change(item.ID, -item.Amount, false, false, false);
                return result;
            }
            return null;
        }
        public bool RemoveItem(string itemID, int amount)
        {
            if (!string.IsNullOrEmpty(itemID))
            {
                return Change(itemID, -amount, false) >= amount;
            }
            return false;
        }
        public int GetItemAllNum(string id)
        {
            return GetAmount(id, false);
        }
        public Item[] GetItemList()
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region IMissionObj
        public Transform GetTransform() {  return WorldRestaurant.transform; }
        public TransportPriority GetTransportPriority() { return TransportPriority; }
        public string GetUID() { return UID; }
        public void AddTransport(Transport transport) { Transports.Add(transport); }
        public void RemoveTranport(Transport transport) { Transports.Remove(transport); }
        public bool PutIn(string itemID, int amount)
        {
            return Change(itemID, amount) >= amount;
        }
        public int PutOut(string itemID, int amount)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}
