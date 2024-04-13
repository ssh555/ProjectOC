using System;
using System.Collections.Generic;
using ML.Engine.InventorySystem;
using ProjectOC.MissionNS;
using Sirenix.OdinInspector;
using UnityEngine;


namespace ProjectOC.StoreNS
{
    [LabelText("仓库"), Serializable]
    public class Store: IMissionObj, IInventory
    {
        #region 当前数据
        [LabelText("世界仓库"), ReadOnly]
        public WorldStore WorldStore;
        [ShowInInspector, ReadOnly]
        public string UID { get { return WorldStore?.InstanceID ?? ""; } }
        [LabelText("仓库名字"), ReadOnly]
        public string Name = "";
        [LabelText("仓库类型"), ReadOnly]
        public StoreType StoreType;
        [LabelText("仓库存储数据"), ReadOnly]
        public List<StoreData> StoreDatas = new List<StoreData>();
        [LabelText("仓库对应的搬运"), ReadOnly]
        public List<Transport> Transports = new List<Transport>();
        [LabelText("仓库等级"), ReadOnly]
        public int Level;
        [LabelText("搬运优先级"), ReadOnly]
        public TransportPriority TransportPriority = TransportPriority.Normal;
        [LabelText("玩家是否正在与此仓库交互"), ReadOnly]
        public bool IsInteracting;
        [LabelText("场景Icon对应的Item"), ReadOnly]
        public string WorldIconItemID;
        #endregion

        #region 配置
        [LabelText("仓库最大等级"), FoldoutGroup("配置")]
        public int LevelMax = 2;
        [LabelText("每个级别的仓库容量"), FoldoutGroup("配置")]
        public List<int> LevelStoreCapacity = new List<int>() { 2, 4, 8 };
        [LabelText("每个级别的仓库数据容量"), FoldoutGroup("配置")]
        public List<int> LevelStoreDataCapacity = new List<int>() { 50, 100, 200 };
        #endregion

        #region Property
        [LabelText("仓库容量"), ShowInInspector, ReadOnly]
        public int StoreCapacity { get { return this.LevelStoreCapacity[this.Level]; } }
        [LabelText("仓库数据的容量"), ShowInInspector, ReadOnly]
        public int StoreDataCapacity { get { return this.LevelStoreDataCapacity[this.Level]; } }
        #endregion

        public event Action OnStoreDataChange;

        public Store(StoreType storeType)
        {
            for (int i = 0; i < this.StoreCapacity; i++)
            {
                this.StoreDatas.Add(new StoreData("", this.StoreDataCapacity));
            }
            this.StoreType = storeType;
            this.Name = storeType.ToString();
        }

        /// <summary>
        /// 销毁时调用
        /// </summary>
        public void Destroy(Player.PlayerCharacter player = null)
        {
            List<Transport> transports = new List<Transport>();
            transports.AddRange(Transports);
            foreach (Transport transport in transports)
            {
                if (transport.Target == this || !transport.ArriveSource)
                {
                    transport?.End();
                }
            }
            this.Transports.Clear();
            // 将堆放的成品，素材，全部返还至玩家背包
            bool flag = false;
            List<Item> resItems = new List<Item>();
            foreach (StoreData data in StoreDatas)
            {
                if (ItemManager.Instance.IsValidItemID(data.ItemID) && data.Storage > 0)
                {
                    List<Item> items = ItemManager.Instance.SpawnItems(data.ItemID, data.Storage);
                    foreach (var item in items)
                    {
                        if (flag)
                        {
                            resItems.Add(item);
                        }
                        else
                        {
                            if (player == null || !player.Inventory.AddItem(item))
                            {
                                flag = true;
                            }
                        }
                    }
                }
            }
            // 没有加到玩家背包的都变成WorldItem
            foreach (Item item in resItems)
            {
#pragma warning disable CS4014
                ItemManager.Instance.SpawnWorldItem(item, WorldStore.transform.position, WorldStore.transform.rotation);
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

        /// <summary>
        /// 修改等级
        /// </summary>
        public bool SetLevel(int newLevel)
        {
            if (WorldStore.transform != null && newLevel > this.Level && newLevel <= LevelMax)
            {
                int newStoreCapacity = LevelStoreCapacity[newLevel];
                int newStoreDataCapacity = LevelStoreDataCapacity[newLevel];
                for (int i = 0; i < newStoreCapacity - StoreCapacity; i++)
                {
                    this.StoreDatas.Add(new StoreData("", newStoreDataCapacity));
                }
                foreach (StoreData storeData in StoreDatas)
                {
                    storeData.MaxCapacity = newStoreDataCapacity;
                }
                Level = newLevel;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 修改仓库存储的物品
        /// </summary>
        /// <param name="index">第几个存储格子</param>
        /// <param name="itemID">新的物品ID</param>
        /// <returns></returns>
        public bool ChangeStoreData(int index, string itemID, IInventory inventory)
        {
            if (itemID == null)
            {
                itemID = "";
            }
            if (0 <= index && index < this.StoreCapacity)
            {
                StoreData data = this.StoreDatas[index];
                string oldItemID = data.ItemID;
                // 将堆放的物品，全部返还至玩家背包
                bool flag = false;
                List<Item> resItems = new List<Item>();
                if (ItemManager.Instance.IsValidItemID(data.ItemID) && data.Storage > 0)
                {
                    List<Item> items = ItemManager.Instance.SpawnItems(data.ItemID, data.Storage);
                    foreach (var item in items)
                    {
                        if (flag)
                        {
                            resItems.Add(item);
                        }
                        else
                        {
                            if (inventory != null || !inventory.AddItem(item))
                            {
                                flag = true;
                            }
                        }
                    }
                }
                // 没有加到玩家背包的都变成WorldItem
                foreach (Item item in resItems)
                {
#pragma warning disable CS4014
                    ItemManager.Instance.SpawnWorldItem(item, WorldStore.transform.position, WorldStore.transform.rotation);
#pragma warning restore CS4014
                }
                data.Storage = 0;
                data.StorageReserve = 0;
                data.EmptyReserve = 0;
                data.ItemID = itemID;

                int storageReserve = GetStorage(oldItemID, false, true);
                int emptyReserve = GetEmpty(oldItemID, false, true);
                foreach (Transport transport in Transports)
                {
                    if (transport!=null && transport.ItemID == oldItemID)
                    {
                        if (transport.Source == this && !transport.ArriveSource)
                        {
                            if (storageReserve <= 0)
                            {
                                transport.End();
                            }
                            else
                            {
                                storageReserve -= transport.MissionNum;
                            }
                        }
                        else if(transport.Target == this && emptyReserve == 0)
                        {
                            transport.End();
                        }
                    }
                }
                OnStoreDataChange?.Invoke();
                return true;
            }
            return false;
        }


        #region 数据方法
        /// <summary>
        /// 仓库是否有该物品
        /// </summary>
        public bool IsStoreHaveItem(string itemID, bool needCanIn = false, bool needCanOut = false)
        {
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (StoreData data in StoreDatas)
                {
                    if (data.ItemID == itemID && (!needCanIn || data.CanIn) && (!needCanOut || data.CanOut))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 仓库中有多少数量的该物品
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <returns></returns>
        public int GetStorage(string itemID, bool needCanOut=false, bool isReserve=false)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (StoreData data in StoreDatas)
                {
                    if (data.ItemID == itemID && (!needCanOut || data.CanOut))
                    {
                        result += isReserve ? data.StorageReserve : data.Storage;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 仓库中能存放多少数量的该物品
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <returns></returns>
        public int GetEmpty(string itemID, bool needCanIn=false, bool isReserve=false)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (StoreData data in StoreDatas)
                {
                    if (data.ItemID == itemID && (!needCanIn || data.CanIn))
                    {
                        result += isReserve ? data.EmptyReserve : data.Empty;
                    }
                }
            }
            return result;
        }

        public int AddItem()
        {

        }
        public int RemoveItem()
        {

        }

        #endregion

        /// <summary>
        /// 排序
        /// </summary>
        public class Sort : IComparer<Store>
        {
            public int Compare(Store x, Store y)
            {
                if (x == null)
                {
                    if (y == null)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
                if (y == null)
                {
                    return -1;
                }
                int priorityX = (int)x.TransportPriority;
                int priorityY = (int)y.TransportPriority;
                if (priorityX != priorityY)
                {
                    return priorityX.CompareTo(priorityY);
                }
                return x.UID.CompareTo(y.UID);
            }
        }

        #region UI接口
        public void UIRemove(Player.PlayerCharacter player, StoreData storeData, int amount)
        {
            if (player != null && storeData != null && amount > 0)
            {
                if (ItemManager.Instance.IsValidItemID(storeData.ItemID) && storeData.Storage >= amount)
                {
                    List<Item> items = ItemManager.Instance.SpawnItems(storeData.ItemID, amount);
                    foreach (Item item in items)
                    {
                        int itemAmount = item.Amount;
                        if (player.Inventory.AddItem(item))
                        {
                            storeData.Storage -= itemAmount;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
        public void UIFastAdd(Player.PlayerCharacter player, StoreData storeData)
        {
            if (player != null && storeData != null)
            {
                if (ItemManager.Instance.IsValidItemID(storeData.ItemID))
                {
                    int amount = player.Inventory.GetItemAllNum(storeData.ItemID);
                    amount = amount >= storeData.Empty ? storeData.Empty : amount;
                    if (player.Inventory.RemoveItem(storeData.ItemID, amount))
                    {
                        storeData.Storage += amount;
                    }
                }
            }
        }
        public bool UIChangeStoreData(Player.PlayerCharacter player, int index, string itemID)
        {
            if (player != null)
            {
                return ChangeStoreData(index, itemID, player.Inventory);
            }
            return false;
        }
        #endregion

        #region IMission接口
        public Transform GetTransform()
        {
            return WorldStore?.transform;
        }
        public TransportPriority GetTransportPriority()
        {
            return this.TransportPriority;
        }
        public string GetUID()
        {
            return this.UID;
        }
        public void AddTransport(Transport transport)
        {
            this.Transports.Add(transport);
        }
        public void RemoveTranport(Transport transport)
        {
            this.Transports.Remove(transport);
        }
        public bool PutIn(string itemID, int amount)
        {
            if (!string.IsNullOrEmpty(itemID) && amount >= 0)
            {
                StoreData temp = null;
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID != "" && data.ItemID == itemID)
                    {
                        if (temp == null)
                        {
                            temp = data;
                        }
                        if (data.EmptyReserve >= amount)
                        {
                            data.EmptyReserve -= amount;
                            data.Storage += amount;
                            amount = 0;
                            break;
                        }
                        else
                        {
                            data.Storage += data.EmptyReserve;
                            amount -= data.EmptyReserve;
                            data.EmptyReserve = 0;
                        }
                    }
                }
                if (amount > 0 && temp != null)
                {
                    temp.Storage += amount;
                    temp.EmptyReserve = 0;
                    amount = 0;
                }
                OnStoreDataChange?.Invoke();
                return amount == 0;
            }
            return false;
        }
        /// <summary>
        /// 返回取出的数量
        /// </summary>
        public int PutOut(string itemID, int amount)
        {
            if (!string.IsNullOrEmpty(itemID) && amount > 0)
            {
                int result = amount;
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID != "" && data.ItemID == itemID)
                    {
                        if (data.StorageReserve >= result)
                        {
                            data.StorageReserve -= result;
                            result = 0;
                            break;
                        }
                        else
                        {
                            result -= data.StorageReserve;
                            data.StorageReserve = 0;
                        }
                    }
                }
                OnStoreDataChange?.Invoke();
                return amount - result;
            }
            return 0;
        }
        /// <summary>
        /// 给刁民预留存入的量
        /// </summary>
        public int ReservePutIn(string itemID, int amount, Transport transport)
        {
            if (!string.IsNullOrEmpty(itemID))
            {
                int reserveNum = 0;
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID != "" && data.CanIn && data.ItemID == itemID)
                    {
                        if (data.Empty >= amount)
                        {
                            data.EmptyReserve += amount;
                            reserveNum += amount;
                            amount = 0;
                            break;
                        }
                        else
                        {
                            amount -= data.Empty;
                            data.EmptyReserve += data.Empty;
                            reserveNum += data.Empty;
                        }
                    }
                }
                if (amount > 0)
                {
                    foreach (StoreData data in this.StoreDatas)
                    {
                        if (data.ItemID != "" && data.CanIn && data.ItemID == itemID)
                        {
                            data.EmptyReserve += amount;
                            reserveNum += amount;
                            amount = 0;
                            break;
                        }
                    }
                }
                transport.TargetReserveNum = reserveNum;
            }
            return amount;
        }
        /// <summary>
        /// 给刁民预留取出的量
        /// </summary>
        public int ReservePutOut(string itemID, int amount, Transport transport)
        {
            if (!string.IsNullOrEmpty(itemID))
            {
                int reserveNum = 0;
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID != "" && data.CanOut && data.ItemID == itemID)
                    {
                        if (data.Storage >= amount)
                        {
                            data.Storage -= amount;
                            data.StorageReserve += amount;
                            reserveNum += amount;
                            amount = 0;
                            break;
                        }
                        else
                        {
                            data.StorageReserve += data.Storage;
                            reserveNum += data.Storage;
                            amount -= data.Storage;
                            data.Storage = 0;
                        }
                    }
                }
                transport.SoureceReserveNum = reserveNum;
            }
            return amount;
        }
        /// <summary>
        /// 移除预留存入量
        /// </summary>
        public int RemoveReservePutIn(string itemID, int amount)
        {
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID != "" && data.ItemID == itemID && data.EmptyReserve > 0)
                    {
                        if (data.EmptyReserve >= amount)
                        {
                            data.EmptyReserve -= amount;
                            amount = 0;
                            break;
                        }
                        else
                        {
                            amount -= data.EmptyReserve;
                            data.EmptyReserve = 0;
                        }
                    }
                }
            }
            return amount;
        }
        /// <summary>
        /// 移除预留取出量
        /// </summary>
        public int RemoveReservePutOut(string itemID, int amount)
        {
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID != "" && data.ItemID == itemID && data.StorageReserve > 0)
                    {
                        if (data.StorageReserve >= amount)
                        {
                            data.Storage += amount;
                            data.StorageReserve -= amount;
                            amount = 0;
                            break;
                        }
                        else
                        {
                            data.Storage += data.StorageReserve;
                            amount -= data.StorageReserve;
                            data.StorageReserve = 0;
                        }
                    }
                }
            }
            return amount;
        }
        #endregion

        #region IInventory接口
        public bool AddItem(Item item)
        {
            lock (this)
            {
                int amount = item.Amount;
                if (GetEmpty(item.ID) < amount || amount < 0)
                {
                    return false;
                }
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID != "" && data.ItemID == item.ID)
                    {
                        if (data.Empty >= amount)
                        {
                            data.Storage += amount;
                            amount = 0;
                            break;
                        }
                        else
                        {
                            amount -= data.Empty;
                            data.Storage += data.Empty;
                        }
                    }
                }
                OnStoreDataChange?.Invoke();
                return true;
            }
        }

        public bool RemoveItem(Item item)
        {
            lock (this)
            {
                int amount = item.Amount;
                if (GetStorage(item.ID) < amount || amount < 0)
                {
                    return false;
                }
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID != "" && data.ItemID == item.ID)
                    {
                        if (data.Storage >= amount)
                        {
                            data.Storage -= amount;
                            amount = 0;
                            break;
                        }
                        else
                        {
                            amount -= data.Storage;
                            data.Storage = 0;
                        }
                    }
                }
                OnStoreDataChange?.Invoke();
                return true;
            }
        }

        public Item RemoveItem(Item item, int amount)
        {
            lock (this)
            {
                int oldAmount = amount;
                if (amount > 0)
                {
                    if (GetStorage(item.ID) >= amount)
                    {
                        foreach (StoreData data in this.StoreDatas)
                        {
                            if (data.ItemID != "" && data.ItemID == item.ID)
                            {
                                if (data.Storage >= amount)
                                {
                                    data.Storage -= amount;
                                    amount = 0;
                                    break;
                                }
                                else
                                {
                                    amount -= data.Storage;
                                    data.Storage = 0;
                                }
                            }
                        }
                    }
                }
                Item result = ItemManager.Instance.SpawnItem(item.ID);
                int newAmount = oldAmount - amount;
                result.Amount = newAmount;
                OnStoreDataChange?.Invoke();
                return result;
            }
        }

        public bool RemoveItem(string itemID, int amount)
        {
            lock(this)
            {
                if (!string.IsNullOrEmpty(itemID) && GetStorage(itemID) < amount || amount < 0)
                {
                    return false;
                }
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID != "" && data.ItemID == itemID)
                    {
                        if (data.Storage >= amount)
                        {
                            data.Storage -= amount;
                            amount = 0;
                            break;
                        }
                        else
                        {
                            amount -= data.Storage;
                            data.Storage = 0;
                        }
                    }
                }
                OnStoreDataChange?.Invoke();
                return true;
            }
        }
            

        public int GetItemAllNum(string id)
        {
            return GetStorage(id);
        }

        public Item[] GetItemList()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}