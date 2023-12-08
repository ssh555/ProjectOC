using System;
using System.Collections;
using System.Collections.Generic;
using ProjectOC.MissionNS;
using UnityEngine;

namespace ProjectOC.StoreNS
{
    /// <summary>
    /// 仓库
    /// </summary>
    [System.Serializable]
    public class Store
    {
        /// <summary>
        /// 统一使用建筑物实例编号, -1 为 invalid value
        /// </summary>
        public string UID;
        /// <summary>
        /// ID
        /// </summary>
        public string ID;
        /// <summary>
        /// 仓库类型
        /// </summary>
        public StoreType Type;
        /// <summary>
        /// 仓库存储数据
        /// </summary>
        public List<StoreData> StoreDatas = new List<StoreData>();
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
        public int Level = 1;
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
        public PriorityTransport PriorityTransport;
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
        public event Action<string, List<StoreData>> OnStoreCapacityChanged;

        public Store()
        {
            for (int i = 0; i < this.StoreCapacity; i++)
            {
                this.StoreDatas.Add(new StoreData("-1", this.StoreDataCapacity));
            }
        }
        public Store(string id)
        {
            this.ID = id;
            // TODO:读表
            for (int i = 0; i < this.StoreCapacity; i++)
            {
                this.StoreDatas.Add(new StoreData("-1", this.StoreDataCapacity));
            }
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
                if (data.StorageCapacity == 0 && data.StorageCapacityReserved == 0)
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
                    if (data.EmptyCapacity >= item.Amount)
                    {
                        data.EmptyCapacity -= item.Amount;
                        data.StorageCapacity += item.Amount;
                        item.Amount = 0;
                        break;
                    }
                    else
                    {
                        data.StorageCapacity += data.EmptyCapacity;
                        item.Amount -= data.EmptyCapacity;
                        data.EmptyCapacity = 0;
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
            int removeAmount = 0;
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID != "" && data.ItemID == itemID)
                {
                    if (data.StorageCapacity >= amount)
                    {
                        data.EmptyCapacity += amount;
                        data.StorageCapacity -= amount;
                        removeAmount += amount;
                        amount = 0;
                        break;
                    }
                    else
                    {
                        data.EmptyCapacity += data.StorageCapacity;
                        removeAmount += data.StorageCapacity;
                        amount -= data.StorageCapacity;
                        data.StorageCapacity = 0;
                    }
                }
            }
            return new StoreItem(itemID, removeAmount);
        }

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

        /// <summary>
        /// 存入来自Worker搬运任务的Item
        /// </summary>
        /// <param name="item"></param>
        public StoreItem AddItemFromWorker(StoreItem item)
        {
            // TODO: 刁民存入物品时，允许超出仓库容量上限。
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID != "" && data.ItemID == item.ItemID)
                {
                    if (data.EmptyCapacityReserved >= item.Amount)
                    {
                        data.EmptyCapacityReserved -= item.Amount;
                        data.StorageCapacity += item.Amount;
                        item.Amount = 0;
                        break;
                    }
                    else
                    {
                        data.StorageCapacity += data.EmptyCapacityReserved;
                        item.Amount -= data.EmptyCapacityReserved;
                        data.EmptyCapacityReserved = 0;
                    }
                }
            }
            // 超额存入
            if (item.Amount > 0)
            {
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID != "" && data.ItemID == item.ItemID)
                    {
                        data.EmptyCapacityReserved -= item.Amount;
                        data.StorageCapacity += item.Amount;
                        item.Amount = 0;
                        break;
                    }
                }
            }
            return item;
        }

        /// <summary>
        /// 取出来自Worker搬运任务的Item
        /// 返回值为移出的Item
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public StoreItem RemoveItemToWorker(string itemID, int amount)
        {
            int removeAmount = 0;
            foreach (StoreData data in this.StoreDatas)
            {
                if (data.ItemID != "" && data.ItemID == itemID)
                {
                    if (data.StorageCapacityReserved >= amount)
                    {
                        data.StorageCapacityReserved -= amount;
                        data.EmptyCapacity += amount;
                        removeAmount += amount;
                        amount = 0;
                        break;
                    }
                    else
                    {
                        data.EmptyCapacity += data.StorageCapacityReserved;
                        removeAmount += data.StorageCapacityReserved;
                        amount -= data.StorageCapacityReserved;
                        data.StorageCapacityReserved = 0;
                    }
                }
            }
            return new StoreItem(itemID, removeAmount);
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
                    if (data.EmptyCapacity >= amount)
                    {
                        data.EmptyCapacity -= amount;
                        data.EmptyCapacityReserved += amount;
                        amount = 0;
                        break;
                    }
                    else
                    {
                        data.EmptyCapacityReserved += data.EmptyCapacity;
                        amount -= data.EmptyCapacity;
                        data.EmptyCapacity = 0;
                    }
                }
            }
            if (amount > 0)
            {
                foreach (StoreData data in this.StoreDatas)
                {
                    if (data.ItemID != "" && data.ItemID == itemID)
                    {
                        data.EmptyCapacity -= amount;
                        data.EmptyCapacityReserved += amount;
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
                    if (data.StorageCapacity >= amount)
                    {
                        data.StorageCapacity -= amount;
                        data.StorageCapacityReserved += amount;
                        amount = 0;
                        break;
                    }
                    else
                    {
                        data.StorageCapacityReserved += data.StorageCapacity;
                        amount -= data.StorageCapacity;
                        data.StorageCapacity = 0;
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
                    result += data.StorageCapacity;
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
                    result += data.EmptyCapacity;
                }
            }
            return result;
        }

        // TODO: 拆除。拆除仓库时，将所有物品传给玩家背包。
    }
}