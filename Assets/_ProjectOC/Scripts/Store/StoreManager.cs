using ML.Engine.Manager.LocalManager;
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
    public sealed class StoreManager : ILocalManager
    {
        /// <summary>
        /// �������вֿ��ID
        /// </summary>
        private HashSet<string> StoreIDs = new HashSet<string>();
        /// <summary>
        /// ʵ�������ɵĲֿ�
        /// </summary>
        private Dictionary<string, Store> StoreDict = new Dictionary<string, Store>();

        public StoreManager()
        {
            // TODO:�����ʼ��StoreIDs
        }
        /// <summary>
        /// �Ƿ�����Ч�Ĳֿ�ID
        /// </summary>
        /// <param name="ID">�ֿ�ID</param>
        /// <returns></returns>
        public bool IsValidID(string ID)
        {
            return StoreIDs.Contains(ID);
        }
        /// <summary>
        /// TODO: UID�ж�
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        public bool IsValidUID(string UID)
        {
            return StoreDict.ContainsKey(UID);
        }
        /// <summary>
        /// �����ֿ�
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
        /// ����UID��ȡ�Ѿ�ʵ�����Ĳֿ�
        /// </summary>
        /// <param name="UID">�ֿ�UID</param>
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
        /// ��ȡ������������Ĳֿ�
        /// </summary>
        /// <param name="itemID">��ƷID</param>
        /// <param name="amount">����</param>
        /// <returns></returns>
        public Store GetStoreForStorageMission(string itemID, int amount)
        {
            Store result = null;
            // ��ͷ��β�����ֿ�(����������ڽ����Ĳֿ�)
            foreach (Store store in this.StoreDict.Values)
            {
                if (store != null && !store.IsInteracting && store.IsStoreHaveItem(itemID))
                {
                    // ����Ѱ�ҵ�һ������һ���Դ���Ĳֿ�
                    // ��û�У���Ѱ�ҵ�һ�����Դ���ģ����������
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
        /// ��ȡ����ȡ�������Ĳֿ�
        /// </summary>
        /// <param name="itemID">��ƷID</param>
        /// <param name="amount">����</param>
        /// <returns>ȡ�������Ͷ�Ӧ�ֿ��б�</returns>
        public Tuple<int, List<Store>> GetStoreForRetrieveMission(string itemID, int amount)
        {
            List<Store> result = new List<Store>();
            // ȡ����������
            int resultAmount = 0;
            // ��ͷ��β�����ֿ�(����������ڽ����Ĳֿ�)
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

