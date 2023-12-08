using ML.Engine.Manager.LocalManager;
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
    public sealed class StoreManager : ILocalManager
    {
        /// <summary>
        /// 表里所有仓库的ID
        /// </summary>
        private HashSet<string> StoreIDs = new HashSet<string>();
        /// <summary>
        /// 实例化生成的仓库
        /// </summary>
        private Dictionary<string, Store> StoreDict = new Dictionary<string, Store>();

        public StoreManager()
        {
            // TODO:读表初始化StoreIDs
        }
        /// <summary>
        /// 是否是有效的仓库ID
        /// </summary>
        /// <param name="ID">仓库ID</param>
        /// <returns></returns>
        public bool IsValidID(string ID)
        {
            return StoreIDs.Contains(ID);
        }
        /// <summary>
        /// TODO: UID判断
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        public bool IsValidUID(string UID)
        {
            return StoreDict.ContainsKey(UID);
        }
        /// <summary>
        /// 创建仓库
        /// </summary>
        public Store CreateStore(string ID)
        {
            if (this.IsValidID(ID))
            {
                Store store = new Store(ID);
                StoreDict.Add(store.UID, store);
                return store;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 根据UID获取已经实例化的仓库
        /// </summary>
        /// <param name="UID">仓库UID</param>
        /// <returns></returns>
        public Store GetStoreByUID(string UID)
        {
            if (StoreDict.ContainsKey(UID))
            {
                return StoreDict[UID];
            }
            return null;
        }

        /// <summary>
        /// 获取满足存入条件的仓库
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <param name="amount">数量</param>
        /// <returns></returns>
        public Store GetStoreForStorageMission(string itemID, int amount)
        {
            Store result = null;
            // 从头到尾遍历仓库(跳过玩家正在交互的仓库)
            foreach (Store store in this.StoreDict.Values)
            {
                if (store != null && !store.IsInteracting && store.IsStoreHaveItem(itemID))
                {
                    // 优先寻找第一个可以一次性存完的仓库
                    // 若没有，则寻找第一个可以存入的，可溢出存入
                    if (result == null)
                    {
                        result = store;
                    }
                    if (store.IsStoreHaveItemEmpty(itemID, amount))
                    {
                        result = store;
                        break;
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
        public Tuple<int, List<Store>> GetStoreForRetrieveMission(string itemID, int amount)
        {
            List<Store> result = new List<Store>();
            // 取出来的数量
            int resultAmount = 0;
            // 从头到尾遍历仓库(跳过玩家正在交互的仓库)
            foreach (Store store in this.StoreDict.Values)
            {
                if (store != null && !store.IsInteracting && store.IsStoreHaveItem(itemID))
                {
                    int storeAmount = store.GetItemStorageCapacity(itemID);
                    if (resultAmount + storeAmount >= amount)
                    {
                        result.Add(store);
                        resultAmount = amount;
                        break;
                    }
                    else
                    {
                        result.Add(store);
                        resultAmount += storeAmount;
                    }
                }
            }
            return new Tuple<int, List<Store>>(resultAmount, result);
        }
    }
}

