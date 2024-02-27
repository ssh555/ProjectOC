using System;
using System.Collections;
using System.Collections.Generic;
using ML.Engine.BuildingSystem;
using ML.Engine.InventorySystem;
using ML.Engine.InventorySystem.CompositeSystem;
using ProjectOC.MissionNS;
using Sirenix.OdinInspector;
using UnityEngine;
using static ML.Engine.InventorySystem.CompositeSystem.CompositeManager;

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
        /// 修改等级
        /// </summary>
        public bool SetLevel(int newLevel)
        {
            if (WorldStore.transform != null && newLevel >= 0 && newLevel <= LevelMax)
            {
                int newStoreCapacity = LevelStoreCapacity[newLevel];
                int newStoreDataCapacity = LevelStoreDataCapacity[newLevel];
                Dictionary<string, int> temp = new Dictionary<string, int>();
                if (newStoreCapacity >= StoreCapacity)
                {
                    for (int i = 0; i < newStoreCapacity - StoreCapacity; i++)
                    {
                        this.StoreDatas.Add(new StoreData("", newStoreDataCapacity));
                    }
                }
                else
                {
                    for (int i = 0; i < StoreCapacity - newStoreCapacity; i++)
                    {
                        StoreData storeData = StoreDatas[StoreDatas.Count - 1];
                        if (ItemManager.Instance.IsValidItemID(storeData.ItemID) && storeData.StorageAll > 0)
                        {
                            temp.Add(storeData.ItemID, storeData.StorageAll);
                        }
                        StoreDatas.RemoveAt(StoreDatas.Count - 1);
                    }
                }
                if (newStoreDataCapacity >= StoreDataCapacity)
                {
                    foreach (StoreData storeData in StoreDatas)
                    {
                        storeData.MaxCapacity = newStoreDataCapacity;
                    }
                }
                else
                {
                    foreach (StoreData storeData in StoreDatas)
                    {
                        int removeAmount = storeData.MaxCapacity - newStoreDataCapacity;
                        if (storeData.Storage > removeAmount)
                        {
                            storeData.Storage -= removeAmount;
                            temp.Add(storeData.ItemID, removeAmount);
                        }
                        else
                        {
                            temp.Add(storeData.ItemID, storeData.Storage);
                            storeData.Storage = 0;
                        }
                        storeData.MaxCapacity = newStoreDataCapacity;
                    }
                }
                // 根据temp生成场景物体
                foreach (var kv in temp)
                {
                    List<Item> items = ItemManager.Instance.SpawnItems(kv.Key, kv.Value);
                    foreach (Item item in items)
                    {
                        ItemManager.Instance.SpawnWorldItem(item, WorldStore.transform.position, WorldStore.transform.rotation);
                    }
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
        public bool ChangeStoreData(int index, string itemID)
        {
            if (itemID == null)
            {
                itemID = "";
            }
            if (0 <= index && index < this.StoreCapacity)
            {
                StoreData data = this.StoreDatas[index];
                if (data.Storage == 0 && data.StorageReserve == 0 && data.EmptyReserve == 0)
                {
                    this.StoreDatas[index] = new StoreData(itemID, this.StoreDataCapacity);
                    OnStoreDataChange?.Invoke();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 给刁民预留存入的量
        /// </summary>
        public int ReserveEmptyToWorker(string itemID, int amount)
        {
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID != "" && data.ItemID == itemID)
                    {
                        if (data.Empty >= amount)
                        {
                            data.EmptyReserve += amount;
                            amount = 0;
                            break;
                        }
                        else
                        {
                            amount -= data.Empty;
                            data.EmptyReserve += data.Empty;
                        }
                    }
                }
                if (amount > 0)
                {
                    foreach (StoreData data in this.StoreDatas)
                    {
                        if (data.ItemID != "" && data.ItemID == itemID)
                        {
                            data.EmptyReserve += amount;
                            amount = 0;
                            break;
                        }
                    }
                }
            }
            return amount;
        }

        /// <summary>
        /// 给刁民预留取出的量
        /// </summary>
        public int ReserveStorageToWorker(string itemID, int amount)
        {
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID != "" && data.ItemID == itemID)
                    {
                        if (data.Storage >= amount)
                        {
                            data.Storage -= amount;
                            data.StorageReserve += amount;
                            amount = 0;
                            break;
                        }
                        else
                        {
                            data.StorageReserve += data.Storage;
                            amount -= data.Storage;
                            data.Storage = 0;
                        }
                    }
                }
            }
            return amount;
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

        /// <summary>
        /// 仓库是否有指定数量的该物品
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <param name="amount">数量</param>
        /// <returns></returns>
        public bool IsStoreHaveStorage(string itemID, int amount)
        {
            return GetStoreStorage(itemID) > amount;
        }

        /// <summary>
        /// 仓库是否能存入指定数量的该物品
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <param name="amount">数量</param>
        /// <returns></returns>
        public bool IsStoreHaveEmpty(string itemID, int amount)
        {
            return GetStoreEmpty(itemID) > amount;
        }

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
        public List<Formula> GetUpgradeRaw()
        {
            List<Formula> result = new List<Formula>();
            if (this.WorldStore != null)
            {
                List<Formula> formulas = BuildingManager.Instance.GetUpgradeRaw(this.WorldStore.Classification.ToString().Replace('-', '_'));
                if (formulas != null)
                {
                    result.AddRange(formulas);
                }
            }
            return result;
        }
        
        public List<Formula> GetUpgradeRawCurrent(Player.PlayerCharacter player)
        {
            List<Formula> result = new List<Formula>();
            if (this.WorldStore != null)
            {
                List<Formula> formulas = BuildingManager.Instance.GetUpgradeRaw(this.WorldStore.Classification.ToString().Replace('-', '_'));
                if (formulas != null)
                {
                    foreach (Formula formula in formulas)
                    {
                        int num = player.Inventory.GetItemAllNum(formula.id);
                        Formula newFormula = new Formula();
                        newFormula.id = formula.id;
                        newFormula.num = num;
                        result.Add(newFormula);
                    }
                }
            }
            return result;
        }

        public void Upgrade(Player.PlayerCharacter player)
        {
            if (this.WorldStore != null)
            {
                string ID = BuildingManager.Instance.GetActorID(this.WorldStore.Classification.ToString().Replace('-', '_'));
                string upgradeID = BuildingManager.Instance.GetUpgradeID(this.WorldStore.Classification.ToString().Replace('-', '_'));
                string upgradeCID = BuildingManager.Instance.GetUpgradeCID(this.WorldStore.Classification.ToString().Replace('-', '_'));

                if (!string.IsNullOrEmpty(upgradeID) 
                    && !string.IsNullOrEmpty(upgradeCID)
                    && BuildingManager.Instance.IsValidBPartID(upgradeCID) 
                    && CompositeManager.Instance.OnlyCostResource(player.Inventory, $"{ID}_{upgradeID}"))
                {
                    if (BuildingManager.Instance.GetOneBPartInstance(upgradeCID) is WorldStore upgrade)
                    {
                        upgrade.InstanceID = this.WorldStore.InstanceID;
                        upgrade.transform.position = this.WorldStore.transform.position;
                        upgrade.transform.rotation = this.WorldStore.transform.rotation;
                        UnityEngine.Object.Destroy(this.WorldStore.gameObject);
                        this.WorldStore = upgrade;
                        upgrade.Store = this;
                        this.SetLevel(upgrade.Classification.Category4 - 1);
                    }
                }
            }
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
            if (item.Amount != newAmount)
            {
                //Debug.LogError($"Item Amount Error ItemAmount: {result.Amount} Amount: {newAmount}");
            }
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
            return GetStoreStorageAll(id);
        }

        public Item[] GetItemList()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}