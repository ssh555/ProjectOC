using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.StoreNS
{
    /// <summary>
    /// �ֿ������
    /// </summary>
    [System.Serializable]
    public sealed class StoreManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        /// <summary>
        /// ʵ�������ɵĲֿ⣬��ΪUID
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
        /// ��ȡ������������Ĳֿ�
        /// </summary>
        /// <param name="itemID">��ƷID</param>
        /// <param name="amount">����</param>
        /// <returns></returns>
        public Store GetCanPutInStore(string itemID, int amount)
        {
            Store result = null;
            // ��ͷ��β�����ֿ�(����������ڽ����Ĳֿ�)
            foreach (WorldStore worldStore in this.WorldStoreDict.Values)
            {
                if (worldStore != null)
                {
                    Store store = worldStore.Store;
                    if (!store.IsInteracting && store.IsStoreHaveItem(itemID))
                    {
                        // ����Ѱ�ҵ�һ������һ���Դ���Ĳֿ�
                        // ��û�У���Ѱ�ҵ�һ�����Դ���ģ����������
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
        /// ��ȡ����ȡ�������Ĳֿ�
        /// </summary>
        /// <param name="itemID">��ƷID</param>
        /// <param name="amount">����</param>
        /// <returns>ȡ�������Ͷ�Ӧ�ֿ��б�</returns>
        public Dictionary<Store, int> GetCanPutOutStore(string itemID, int amount)
        {
            Dictionary<Store, int> result = new Dictionary<Store, int>();
            int resultAmount = 0;
            // ��ͷ��β�����ֿ�(����������ڽ����Ĳֿ�)
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
        /// �����µĲֿ�
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

