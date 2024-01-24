using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.StoreNS
{
    /// <summary>
    /// 仓库管理器
    /// </summary>
    [System.Serializable]
    public sealed class StoreManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        /// <summary>
        /// 实例化生成的仓库，键为UID
        /// </summary>
        private Dictionary<string, WorldStore> WorldStoreDict = new Dictionary<string, WorldStore>();

        public bool IsValidUID(string uid)
        {
            if (!string.IsNullOrEmpty(uid))
            {
                return WorldStoreDict.ContainsKey(uid);
            }
            return false;
        }

        /// <summary>
        /// 获取满足存入条件的仓库
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <param name="amount">数量</param>
        /// <returns></returns>
        public Store GetCanPutInStore(string itemID, int amount)
        {
            Store result = null;
            // 从头到尾遍历仓库(跳过玩家正在交互的仓库)
            foreach (WorldStore worldStore in this.WorldStoreDict.Values)
            {
                if (worldStore != null)
                {
                    Store store = worldStore.Store;
                    if (!store.IsInteracting && store.IsStoreHaveItem(itemID))
                    {
                        // 优先寻找第一个可以一次性存完的仓库
                        // 若没有，则寻找第一个可以存入的，可溢出存入
                        if (result == null)
                        {
                            result = store;
                        }
                        if (store.IsStoreHaveEmpty(itemID, amount))
                        {
                            result = store;
                            break;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取满足取出条件的仓库
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <param name="amount">数量</param>
        /// <returns>取出数量和对应仓库列表</returns>
        public Dictionary<Store, int> GetCanPutOutStore(string itemID, int amount)
        {
            Dictionary<Store, int> result = new Dictionary<Store, int>();
            int resultAmount = 0;
            // 从头到尾遍历仓库(跳过玩家正在交互的仓库)
            foreach (WorldStore worldStore in this.WorldStoreDict.Values)
            {
                if (worldStore != null)
                { 
                    Store store = worldStore.Store;
                    if (!store.IsInteracting && store.IsStoreHaveItem(itemID))
                    {
                        int storeAmount = store.GetStoreStorage(itemID);
                        if (resultAmount + storeAmount >= amount)
                        {
                            result.Add(store, amount - resultAmount);
                            resultAmount = amount;
                            break;
                        }
                        else
                        {
                            result.Add(store, storeAmount);
                            resultAmount += storeAmount;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 创建新的仓库
        /// </summary>
        public Store SpawnStore(StoreType storeType)
        {
            Store store = new Store(storeType);
            return store;
        }
        public void WorldStoreSetData(WorldStore worldStore, StoreType storeType, int level)
        {
            if (worldStore != null && level >= 0)
            {
                if (!WorldStoreDict.ContainsKey(worldStore.InstanceID))
                {
                    WorldStoreDict.Add(worldStore.InstanceID, worldStore);
                }
                else
                {
                    WorldStoreDict[worldStore.InstanceID] = worldStore;
                }
                Store store = SpawnStore(storeType);
                if (store != null)
                {
                    if (worldStore.Store != null)
                    {
                        worldStore.Store.WorldStore = null;
                    }
                    worldStore.Store = store;
                    store.WorldStore = worldStore;
                    store.SetLevel(level);
                }
                else
                {
                    Debug.LogError($"StoreType {storeType} cannot create store");
                }
            }
        }
    }
}

