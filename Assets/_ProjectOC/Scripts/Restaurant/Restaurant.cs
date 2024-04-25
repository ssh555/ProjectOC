using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace ProjectOC.RestaurantNS
{
    [LabelText("餐厅"), Serializable]
    public class Restaurant : ML.Engine.InventorySystem.IInventory, MissionNS.IMissionObj
    {
        #region WorldRestaurant
        [LabelText("世界餐厅"), ReadOnly, NonSerialized]
        public WorldRestaurant WorldRestaurant;
        [ShowInInspector, ReadOnly]
        public string UID { get { return WorldRestaurant?.InstanceID ?? ""; } }
        #endregion

        #region Data
        [LabelText("座位"), ShowInInspector, ReadOnly]
        private RestaurantSeat[] Seats;
        [LabelText("存储数据"), ShowInInspector, ReadOnly]
        private RestaurantData[] Datas;
        [LabelText("对应的搬运"), ReadOnly]
        public List<MissionNS.Transport> Transports = new List<MissionNS.Transport>();
        public event Action OnDataChangeEvent;
        #endregion

        #region Property
        [LabelText("是否有座位"), ShowInInspector, ReadOnly]
        public bool HaveSeat
        {
            get
            {
                if (Seats != null)
                {
                    foreach (RestaurantSeat seat in Seats)
                    {
                        if (!seat.HaveWorker)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        [LabelText("是否有食物"), ShowInInspector, ReadOnly]
        public bool HaveFood
        {
            get
            {
                if (Datas != null)
                {
                    foreach (RestaurantData data in Datas)
                    {
                        if (data.HaveFood)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        #endregion

        public void Init()
        {
            Seats = new RestaurantSeat[ManagerNS.LocalGameManager.Instance.RestaurantManager.SeatNum];
            for (int i = 0; i < Seats.Length; i++)
            {
                Seats[i].Init(this, WorldRestaurant.transform.Find($"seat{i + 1}"));
            }
            Datas = new RestaurantData[ManagerNS.LocalGameManager.Instance.RestaurantManager.DataNum];
            Datas[0].Priority = FoodPriority.No1;
            Datas[1].Priority = FoodPriority.No2;
        }

        public void Destroy()
        {
            foreach (var seat in Seats)
            {
                (seat as WorkerNS.IWorkerContainer).RemoveWorker();
            }
            foreach (MissionNS.Transport transport in Transports.ToArray())
            {
                transport?.End();
            }
            Transports.Clear();

            List<ML.Engine.InventorySystem.Item> items = new List<ML.Engine.InventorySystem.Item>();
            foreach (var data in Datas)
            {
                if (data.HaveFood)
                {
                    items.AddRange(ML.Engine.InventorySystem.ItemManager.Instance.SpawnItems(data.ItemID, data.Amount));
                }
            }
            (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).OCState.Inventory.AddItem(items);
        }

        public void OnPositionChange(Vector3 differ)
        {
            foreach (var transport in Transports)
            {
                transport?.UpdateDestination();
            }
            foreach (var seat in Seats)
            {
                (seat as WorkerNS.IWorkerContainer).OnPositionChange(differ);
            }
        }

        #region 方法
        /// <summary>
        /// 先查找空座位，找不到空座位返回false，更新该座位的信息，让刁民寻路到该座位。
        /// </summary>
        public bool AddWorker(WorkerNS.Worker worker)
        {
            if (worker != null)
            {
                for (int i = 0; i < Seats.Length; i++)
                {
                    if (!Seats[i].HaveWorker)
                    {
                        (Seats[i] as WorkerNS.IWorkerContainer).SetWorker(worker);
                        worker.SetDestination(Seats[i].Socket.position, Seats[i].OnArriveEvent);
                        ManagerNS.LocalGameManager.Instance.RestaurantManager.RemoveWorker(worker);
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
        public int FindFood(WorkerNS.Worker worker)
        {
            int result = -1;
            if (worker != null)
            {
                List<Tuple<RestaurantData, int>> tuples = Datas.Select((data, index) => Tuple.Create(data, index)).ToList();
                tuples = tuples.OrderBy(t => t.Item1, new RestaurantData.Sort()).ToList();
                foreach (var tuple in tuples)
                {
                    RestaurantData data = tuple.Item1;
                    if (data.HaveFood)
                    {
                        result = tuple.Item2;
                        if (data.Priority != FoodPriority.None || worker.APCurrent + data.AlterAP >= worker.APMax)
                        {
                            break;
                        }
                    }
                }
            }
            return result;
        }

        public string EatFood(WorkerNS.Worker worker)
        {
            int index = FindFood(worker);
            if (0 <= index && index < Datas.Length && Change(index, -1) == 1)
            {
                return Datas[index].ID;
            }
            return null;
        }
        #endregion

        #region Getter
        public bool IsValidDataIndex(int index)
        {
            return 0 <= index && index < Datas.Length;
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
                int maxCapacity = ManagerNS.LocalGameManager.Instance.RestaurantManager.MaxCapacity;
                foreach (var data in Datas)
                {
                    if (data.HaveSetFood && id == (isFoodID ? data.ID : data.ItemID))
                    {
                        result += isOut ? data.Amount : maxCapacity - data.Amount;
                    }
                }
            }
            return result;
        }
        public RestaurantData GetRestaurantData(int index)
        {
            if (0 <= index && index < Datas.Length)
            {
                return Datas[index];
            }
            return default(RestaurantData);
        }
        #endregion

        #region 数据方法
        private bool ChangeFood(int index, string id)
        {
            lock (this)
            {
                if (0 <= index && index < Datas.Length)
                {
                    string itemID = Datas[index].ItemID;
                    int amount = Datas[index].Amount;
                    Datas[index].ID = "";
                    Datas[index].Amount = 0;
                    bool haveSetFood = HaveSetFood(itemID, false);

                    foreach (MissionNS.Transport transport in Transports)
                    {
                        if (transport != null && transport.ItemID == itemID && !haveSetFood)
                        {
                            transport.End();
                        }
                    }

                    if (amount > 0)
                    {
                        (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).OCState.Inventory.AddItem(ML.Engine.InventorySystem.ItemManager.Instance.SpawnItems(itemID, amount));
                    }

                    Datas[index].ID = id ?? "";
                    OnDataChangeEvent?.Invoke();
                    return true;
                }
                return false;
            }
        }

        private int Change(int index, int amount, bool exceed = false, bool complete = true)
        {
            lock (this)
            {
                if (0 <= index && index < Datas.Length && Datas[index].HaveSetFood && amount != 0)
                {
                    if ((!exceed && amount > 0 && Datas[index].Amount >= Datas[index].MaxCapacity) || (complete && amount + Datas[index].Amount < 0))
                    {
                        return 0;
                    }
                    amount = !complete && amount + Datas[index].Amount < 0 ? -1 * Datas[index].Amount : amount;
                    Datas[index].Amount += amount;
                    OnDataChangeEvent?.Invoke();
                    return amount < 0 ? -1 * amount : amount;
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
                    int maxCapacity = ManagerNS.LocalGameManager.Instance.RestaurantManager.MaxCapacity;

                    for(int i = 0; i < Datas.Length; i++)
                    {
                        if (Datas[i].HaveSetFood && id == (isFoodID ? Datas[i].ID : Datas[i].ItemID))
                        {
                            firstIndex = firstIndex == -1 ? i : firstIndex;
                            int num = isOut ? Datas[i].Amount : maxCapacity - Datas[i].Amount;
                            if (amount > num)
                            {
                                amount -= num;
                                Datas[i].Amount = isOut ? 0 : maxCapacity;
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
                    OnDataChangeEvent?.Invoke();
                    return total - amount;
                }
                return 0;
            }
        }
        #endregion

        #region UI接口
        public void UIRemove(int index, int amount)
        {
            lock (this)
            {
                if (IsValidDataIndex(index) && amount > 0)
                {
                    string itemID = Datas[index].ItemID;
                    if (Datas[index].Amount >= amount)
                    {
                        Change(index, -amount);
                        (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).OCState.Inventory.AddItem(ML.Engine.InventorySystem.ItemManager.Instance.SpawnItems(itemID, amount));
                    }
                }
            }
        }
        public void UIFastAdd(int index)
        {
            lock (this)
            {
                if (IsValidDataIndex(index))
                {
                    string itemID = Datas[index].ItemID;
                    if (ML.Engine.InventorySystem.ItemManager.Instance.IsValidItemID(itemID))
                    {
                        var inventory = (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).OCState.Inventory;
                        int amount = inventory.GetItemAllNum(itemID);
                        int empty = Datas[index].MaxCapacity - Datas[index].Amount;
                        amount = amount <= empty  ? amount : empty;
                        if (amount > 0 && inventory.RemoveItem(itemID, amount))
                        {
                            Change(index, amount);
                        }
                    }
                }
            }
        }
        public bool UIChangeFood(int index, string itemID)
        {
            return ChangeFood(index, ManagerNS.LocalGameManager.Instance.RestaurantManager.ItemIDToFoodID(itemID));
        }
        #endregion

        #region IInventory
        public bool AddItem(ML.Engine.InventorySystem.Item item)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID) && item.Amount > 0)
            {
                return Change(item.ID, item.Amount, false) == item.Amount;
            }
            return false;
        }
        public bool RemoveItem(ML.Engine.InventorySystem.Item item)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID) && item.Amount > 0)
            {
                return Change(item.ID, -item.Amount, false) == item.Amount;
            }
            return false;
        }
        public ML.Engine.InventorySystem.Item RemoveItem(ML.Engine.InventorySystem.Item item, int amount)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID) && item.Amount > 0)
            {
                ML.Engine.InventorySystem.Item result = ML.Engine.InventorySystem.ItemManager.Instance.SpawnItem(item.ID);
                result.Amount = Change(item.ID, -item.Amount, false, false, false);
                return result;
            }
            return null;
        }
        public bool RemoveItem(string itemID, int amount)
        {
            if (!string.IsNullOrEmpty(itemID) && amount > 0)
            {
                return Change(itemID, -amount, false) == amount;
            }
            return false;
        }
        public int GetItemAllNum(string id)
        {
            return GetAmount(id, false);
        }
        public ML.Engine.InventorySystem.Item[] GetItemList()
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region IMissionObj
        public Transform GetTransform() {  return WorldRestaurant.transform; }
        public MissionNS.TransportPriority GetTransportPriority() { return MissionNS.TransportPriority.Normal; }
        public string GetUID() { return UID; }
        public void AddTransport(MissionNS.Transport transport) { Transports.Add(transport); }
        public void RemoveTranport(MissionNS.Transport transport) { Transports.Remove(transport); }
        public bool PutIn(string itemID, int amount)
        {
            return Change(itemID, amount) == amount;
        }
        public int PutOut(string itemID, int amount)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}
