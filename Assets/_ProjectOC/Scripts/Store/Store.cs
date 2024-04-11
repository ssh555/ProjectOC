using System;
using System.Collections.Generic;
using ML.Engine.InventorySystem;
using ProjectOC.MissionNS;
using Sirenix.OdinInspector;
using UnityEngine;


namespace ProjectOC.StoreNS
{
    /// <summary>
    /// 仓库
    /// </summary>
    [System.Serializable]
    public class Store: IMissionObj, IInventory
    {
        public WorldStore WorldStore;

        public string UID { get { return WorldStore?.InstanceID ?? ""; } }

        [LabelText("仓库名字")]
        public string Name = "";

        [LabelText("仓库类型")]
        public StoreType StoreType;

        [LabelText("仓库存储数据")]
        public List<StoreData> StoreDatas = new List<StoreData>();

        [LabelText("仓库对应的搬运")]
        public List<Transport> Transports = new List<Transport>();

        /// <summary>
        /// 仓库容量，仓库能放多少种物品
        /// </summary>
        public int StoreCapacity
        {
            get
            {
                return this.LevelStoreCapacity[this.Level];
            }
        }
        
        /// <summary>
        /// 仓库数据的容量，单种物品的最大存储数量
        /// </summary>
        public int StoreDataCapacity
        {
            get
            {
                return this.LevelStoreDataCapacity[this.Level];
            }
        }

        /// <summary>
        /// 仓库等级
        /// </summary>
        public int Level;
        
        /// <summary>
        /// 仓库最大等级
        /// </summary>
        public int LevelMax = 2;

        /// <summary>
        /// 每个级别仓库的存储格子数量
        /// </summary>
        public List<int> LevelStoreCapacity = new List<int>() { 2, 4, 8 };

        /// <summary>
        /// 每个级别仓库单个格子的容量上限
        /// </summary>
        public List<int> LevelStoreDataCapacity = new List<int>() { 50, 100, 200 };

        /// <summary>
        /// 搬运优先级
        /// </summary>
        public TransportPriority TransportPriority = TransportPriority.Normal;
        /// <summary>
        /// 玩家是否正在与此仓库交互
        /// 只要玩家正在与某一个仓库进行交互，就将此项设为true,生成任务时不能考虑此项为true的仓库
        /// </summary>
        public bool IsInteracting;
        public event Action OnStoreDataChange;
        /// <summary>
        /// 场景Icon对应的Item
        /// </summary>
        public string WorldIconItemID;

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

                int storageReserve = GetStoreStorageReserve(oldItemID);
                int emptyReserve = GetStoreEmptyReserve(oldItemID);
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

        #region 给刁民的接口
        /// <summary>
        /// 给刁民预留存入的量
        /// </summary>
        public int ReserveEmptyToWorker(string itemID, int amount, Transport transport)
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
        public int ReserveStorageToWorker(string itemID, int amount, Transport transport)
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
        public int RemoveReserveEmpty(string itemID, int amount)
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
        public int RemoveReserveStorage(string itemID, int amount)
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


        /// <summary>
        /// 仓库中有多少数量的刁民能够取出的该物品
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <returns></returns>
        public int GetCanOutStoreStorage(string itemID)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID == itemID && data.CanOut)
                    {
                        result += data.Storage;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 仓库中有多少数量的刁民能够存入的该物品
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <returns></returns>
        public int GetCanInStoreEmpty(string itemID)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID == itemID && data.CanIn)
                    {
                        result += data.Empty;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 仓库是否有该物品
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <returns></returns>
        public bool IsStoreHaveItem(string itemID)
        {
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID == itemID)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool IsStoreCanInItem(string itemID)
        {
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID == itemID && data.CanIn)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool IsStoreCanOutItem(string itemID)
        {
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID == itemID && data.CanOut)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Getter
        public int GetStoreStorageAll(string itemID)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID == itemID)
                    {
                        result += data.StorageAll;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 仓库中有多少数量的该物品
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <returns></returns>
        public int GetStoreStorage(string itemID)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID == itemID)
                    {
                        result += data.Storage;
                    }
                }
            }
            return result;
        }

        public int GetStoreStorageReserve(string itemID)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID == itemID)
                    {
                        result += data.StorageReserve;
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
        public int GetStoreEmpty(string itemID)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID == itemID)
                    {
                        result += data.Empty;
                    }
                }
            }
            return result;
        }

        public int GetStoreEmptyReserve(string itemID)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID == itemID)
                    {
                        result += data.EmptyReserve;
                    }
                }
            }
            return result;
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
        public void UIAdd(Player.PlayerCharacter player, StoreData storeData, int amount)
        {
            if (player != null && storeData != null && amount > 0)
            {
                if (ItemManager.Instance.IsValidItemID(storeData.ItemID) && storeData.Empty >= amount)
                {
                    if (player.Inventory.GetItemAllNum(storeData.ItemID) >= amount)
                    {
                        if (player.Inventory.RemoveItem(storeData.ItemID, amount))
                        {
                            storeData.Storage += amount;
                        }
                        else
                        {
                            //Debug.LogError("UIAdd Error");
                        }
                    }
                }
            }
        }
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
                            //Debug.LogError("UIRemove Error");
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
                    else
                    {
                        //Debug.LogError("UIFastAdd Error");
                    }
                }
            }
        }
        public void UIFastRemove(Player.PlayerCharacter player, StoreData storeData)
        {
            if (player != null && storeData != null)
            {
                if (ItemManager.Instance.IsValidItemID(storeData.ItemID))
                {
                    int amount = storeData.Storage;
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
                            //Debug.LogError("UIFastRemove Error");
                            break;
                        }
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
        public void AddMissionTranport(MissionTransport mission) {}
        public void RemoveMissionTranport(MissionTransport mission) {}
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
        #endregion

        #region IInventory接口
        public bool AddItem(Item item)
        {
            int amount = item.Amount;
            if (GetStoreEmpty(item.ID) < amount || amount < 0)
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

        public bool RemoveItem(Item item)
        {
            int amount = item.Amount;
            if (GetStoreStorage(item.ID) < amount || amount < 0)
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

        public Item RemoveItem(Item item, int amount)
        {
            int oldAmount = amount;
            if (amount > 0)
            {
                if (GetStoreStorage(item.ID) >= amount)
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

        public bool RemoveItem(string itemID, int amount)
        {
            if (!string.IsNullOrEmpty(itemID) && GetStoreStorage(itemID) < amount || amount < 0)
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

        public int GetItemAllNum(string id)
        {
            return GetStoreStorage(id);
        }

        public Item[] GetItemList()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}