using System;
using System.Collections;
using System.Collections.Generic;
using ML.Engine.InventorySystem;
using ML.Engine.TextContent;
using ProjectOC.MissionNS;
using UnityEngine;

namespace ProjectOC.StoreNS
{
    /// <summary>
    /// 仓库
    /// </summary>
    [System.Serializable]
    public class Store: IMission
    {
        public WorldStore WorldStore;
        public string UID { get { return WorldStore?.InstanceID ?? ""; } }
        public string ID;
        public TextContent Name;
        /// <summary>
        /// 仓库类型
        /// </summary>
        public StoreType Type;
        /// <summary>
        /// 仓库存储数据
        /// </summary>
        public List<StoreData> StoreDatas = new List<StoreData>();
        public List<Transport> Transports = new List<Transport>();

        /// <summary>
        /// 仓库容量，仓库能放多少种物品
        /// </summary>
        public int StoreCapacity
        {
            get
            {
                return this.LevelStoreCapacity[this.Level - 1];
            }
        }
        /// <summary>
        /// 仓库数据的容量，单种物品的最大存储数量
        /// </summary>
        public int StoreDataCapacity
        {
            get
            {
                return this.LevelStoreDataCapacity[this.Level - 1];
            }
        }
        private int level = 1;
        /// <summary>
        /// 仓库等级
        /// </summary>
        public int Level 
        {
            get { return level; }
            set 
            {
                int newLevel = value;
                if (newLevel >= 1 && newLevel<=LevelMax)
                {
                    int newStoreCapacity = LevelStoreCapacity[newLevel];
                    int newStoreDataCapacity = LevelStoreDataCapacity[newLevel];
                    List<StoreItem> temp = new List<StoreItem>();
                    if (newStoreCapacity >= StoreCapacity)
                    {
                        for (int i=0; i<newStoreCapacity-StoreCapacity;i++)
                        {
                            this.StoreDatas.Add(new StoreData("", newStoreDataCapacity));
                        }
                    }
                    else
                    {
                        for (int i = 0; i <  StoreCapacity - newStoreCapacity; i++)
                        {
                            StoreData storeData = this.StoreDatas[this.StoreDatas.Count - 1];
                            if (ItemSpawner.Instance.IsValidItemID(storeData.ItemID) && storeData.StorageAll > 0)
                            {
                                temp.Add(new StoreItem(storeData.ItemID, storeData.StorageAll));
                            }
                            this.StoreDatas.RemoveAt(this.StoreDatas.Count - 1);
                        }
                    }
                    if (newStoreDataCapacity >= StoreDataCapacity)
                    {
                        foreach (StoreData storeData in this.StoreDatas)
                        {
                            storeData.MaxCapacity = newStoreDataCapacity;
                        }
                    }
                    else
                    {
                        foreach (StoreData storeData in this.StoreDatas)
                        {
                            int removeAmount = storeData.MaxCapacity - newStoreDataCapacity;
                            if (storeData.Storage > removeAmount)
                            {
                                storeData.Storage -= removeAmount;
                                temp.Add(new StoreItem(storeData.ItemID, removeAmount));
                            }
                            else
                            {
                                temp.Add(new StoreItem(storeData.ItemID, storeData.Storage));
                                storeData.Storage = 0;
                            }
                            storeData.MaxCapacity = newStoreDataCapacity;
                        }
                    }
                    // TODO:根据temp生成场景物体
                    this.level = value;
                }
            } 
        }
        /// <summary>
        /// 仓库最大等级
        /// </summary>
        public int LevelMax = 3;
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
        public TransportPriority TransportPriority;
        /// <summary>
        /// 玩家是否正在与此仓库交互
        /// 只要玩家正在与某一个仓库进行交互，就将此项设为true,生成任务时不能考虑此项为true的仓库
        /// </summary>
        public bool IsInteracting;
        /// <summary>
        /// 仓库存储发生变化时调用
        /// 仓库升级、存入、移出
        /// 参数为当前仓库的<ID, List<StoreData>>
        /// </summary>
        //public event Action<string, List<StoreData>> OnStoreCapacityChanged;

        public Store(StoreManager.StoreTableJsonData config)
        {
            for (int i = 0; i < this.StoreCapacity; i++)
            {
                this.StoreDatas.Add(new StoreData("", this.StoreDataCapacity));
            }
            this.ID = config.id;
            this.Name = config.name;
            this.Type = config.type;
        }
        /// <summary>
        /// 修改仓库存储的物品
        /// </summary>
        /// <param name="index">第几个存储格子</param>
        /// <param name="itemID">新的物品ID</param>
        /// <returns></returns>
        public bool ChangeStoreData(int index, string itemID)
        {
            if (0 <= index && index < this.StoreCapacity)
            {
                StoreData data = this.StoreDatas[index];
                if (data.Storage == 0 && data.StorageReserved == 0)
                {
                    this.StoreDatas[index] = new StoreData(itemID, this.StoreDataCapacity);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Player存入Item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public StoreItem AddItemFromPlayer(StoreItem item)
        {
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID != "" && data.ItemID == item.ItemID)
                {
                    if (data.Empty >= item.Amount)
                    {
                        data.Storage += item.Amount;
                        item.Amount = 0;
                        break;
                    }
                    else
                    {
                        item.Amount -= data.Empty;
                        data.Storage += data.Empty;
                    }
                }
            }
            return item;
        }
        /// <summary>
        /// Player取出Item
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="amount"></param>
        /// <returns>取出来的结果</returns>
        public StoreItem RemoveItemToPlayer(string itemID, int amount)
        {
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
            return new StoreItem(itemID, amount);
        }

        /// <summary>
        /// 存入来自Worker搬运任务的Item
        /// </summary>
        /// <param name="item"></param>
        public StoreItem AddItemFromWorker(StoreItem item)
        {
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID != "" && data.ItemID == item.ItemID)
                {
                    if (data.EmptyReserved >= item.Amount)
                    {
                        data.EmptyReserved -= item.Amount;
                        data.Storage += item.Amount;
                        item.Amount = 0;
                        break;
                    }
                    else
                    {
                        data.Storage += data.EmptyReserved;
                        item.Amount -= data.EmptyReserved;
                        data.EmptyReserved = 0;
                    }
                }
            }
            return item;
        }

        /// <summary>
        /// 取出来自Worker搬运任务的Item
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public StoreItem RemoveItemToWorker(string itemID, int amount)
        {
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID != "" && data.ItemID == itemID)
                {
                    if (data.StorageReserved >= amount)
                    {
                        data.StorageReserved -= amount;
                        amount = 0;
                        break;
                    }
                    else
                    {
                        amount -= data.StorageReserved;
                        data.StorageReserved = 0;
                    }
                }
            }
            return new StoreItem(itemID, amount);
        }

        /// <summary>
        /// 给刁民预留存入的量
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public StoreItem ReserveEmptyCapacityToWorker(string itemID, int amount)
        {
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID != "" && data.ItemID == itemID)
                {
                    if (data.Empty >= amount)
                    {
                        data.EmptyReserved += amount;
                        amount = 0;
                        break;
                    }
                    else
                    {
                        amount -= data.Empty;
                        data.EmptyReserved += data.Empty;
                    }
                }
            }
            if (amount > 0)
            {
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID != "" && data.ItemID == itemID)
                    {
                        data.EmptyReserved += amount;
                        amount = 0;
                        break;
                    }
                }
            }
            return new StoreItem(itemID, amount);
        }
        /// <summary>
        /// 给刁民预留取出的量
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public StoreItem ReserveStorageCapacityToWorker(string itemID, int amount)
        {
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID != "" && data.ItemID == itemID)
                {
                    if (data.Storage >= amount)
                    {
                        data.Storage -= amount;
                        data.StorageReserved += amount;
                        amount = 0;
                        break;
                    }
                    else
                    {
                        data.StorageReserved += data.Storage;
                        amount -= data.Storage;
                        data.Storage = 0;
                    }
                }
            }
            return new StoreItem(itemID, amount);
        }

        /// <summary>
        /// 仓库是否有该物品
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <returns></returns>
        public bool IsStoreHaveItem(string itemID)
        {
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID == itemID)
                {
                    return true;
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
        public bool IsStoreHaveItemStorage(string itemID, int amount)
        {
            return GetItemStorageCapacity(itemID) > amount;
        }
        /// <summary>
        /// 仓库是否能存入指定数量的该物品
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <param name="amount">数量</param>
        /// <returns></returns>
        public bool IsStoreHaveItemEmpty(string itemID, int amount)
        {
            return GetItemEmptyCapacity(itemID) > amount;
        }
        /// <summary>
        /// 仓库中有多少数量的该物品
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <returns></returns>
        public int GetItemStorageCapacity(string itemID)
        {
            int result = 0;
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID == itemID)
                {
                    result += data.Storage;
                }
            }
            return result;
        }
        /// <summary>
        /// 仓库中能存放多少数量的该物品
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <returns></returns>
        public int GetItemEmptyCapacity(string itemID)
        {
            int result = 0;
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID == itemID)
                {
                    result += data.Empty;
                }
            }
            return result;
        }

        #region TODO
        // TODO: 拆除。拆除仓库时，将所有物品传给玩家背包。
        /// <summary>
        /// 常规存，按照数量将物品在背包和仓库之间转移
        /// 存不可超出仓库空位
        /// </summary>
        public void NormalAdd(Player.PlayerCharacter player, string itemID, int amount)
        {
            // TODO: 等待背包接口
        }
        /// <summary>
        /// 常规取，按照数量将物品在背包和仓库之间转移
        /// 取不可超出仓库存货和背包空位的较小值
        /// </summary>
        public void NormalRemove(Player.PlayerCharacter player, string itemID, int amount)
        {
            // TODO: 等待背包接口
        }
        /// <summary>
        /// 快捷存放，将玩家背包中可存放在该仓库的物品全部转移至仓库中；
        /// 仓库空位不足时将其填满，剩余的留在背包中；
        /// </summary>
        public void FastAdd(Player.PlayerCharacter player)
        {
            // TODO: 等待背包接口
        }
        /// <summary>
        /// 快捷取出 背包空位不足时将其填满，剩余的留在仓库中。
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="itemID"></param>
        /// <param name="amount"></param>
        public void FastRemove(Player.PlayerCharacter player, string itemID, int amount)
        {
            // TODO: 等待背包接口
        }


        #endregion

        #region IMission接口
        Transform IMission.GetTransform()
        {
            throw new NotImplementedException();
        }

        TransportPriority IMission.GetTransportPriority()
        {
            throw new NotImplementedException();
        }

        string IMission.GetUID()
        {
            throw new NotImplementedException();
        }

        void IMission.AddTransport(Transport transport)
        {
            throw new NotImplementedException();
        }

        void IMission.RemoveTranport(Transport transport)
        {
            throw new NotImplementedException();
        }

        void IMission.AddMissionTranport(MissionTransport mission)
        {
            throw new NotImplementedException();
        }

        void IMission.RemoveMissionTranport(MissionTransport mission)
        {
            throw new NotImplementedException();
        }

        bool IMission.PutIn(string itemID, int amount)
        {
            throw new NotImplementedException();
        }

        int IMission.PutOut(string itemID, int amount)
        {
            throw new NotImplementedException();
        }

        int IMission.GetItemAmount(string itemID)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}