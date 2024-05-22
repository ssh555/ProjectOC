using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.StoreNS
{
    [System.Serializable]
    public struct StoreIconTableData
    {
        public string ID;
        public string Icon;
    }

    [LabelText("�ֿ������"), System.Serializable]
    public sealed class StoreManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        #region ILocalManager
        private Dictionary<string, StoreIconTableData> StoreIconTableDict = new Dictionary<string, StoreIconTableData>();
        public ML.Engine.ABResources.ABJsonAssetProcessor<StoreIconTableData[]> ABJAProcessor;
        public StoreConfig Config;
        public void OnRegister()
        {
            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<StoreIconTableData[]>("OCTableData", "StoreIcon", (datas) =>
            {
                foreach (var data in datas)
                {
                    StoreIconTableDict.Add(data.ID, data);
                }
            }, "�ֿ�ͼ�������");
            ABJAProcessor.StartLoadJsonAssetData();
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<StoreConfigAsset>("Config_Store").Completed += (handle) =>
            {
                StoreConfigAsset data = handle.Result;
                Config = new StoreConfig(data.Config);
            };
        }
        #endregion

        private Dictionary<string, WorldStore> WorldStoreDict = new Dictionary<string, WorldStore>();

        /// <param name="priorityType">�Ƿ������ȼ���ȡ 0��ʾ����Ҫ��1��ʾ���ȼ��Ӹߵ��ͣ�-1��ʾ���ȼ��ӵ͵���</param>
        public List<Store> GetStores(int priorityType = 0)
        {
            List<Store> stores = new List<Store>();
            foreach (WorldStore worldStore in WorldStoreDict.Values)
            {
                if (worldStore != null)
                {
                    stores.Add(worldStore.Store);
                }
            }
            if (priorityType == 1)
            {
                stores.Sort(new Store.Sort());
            }
            else if (priorityType == -1)
            {
                stores.Sort(new Store.Sort());
                stores.Reverse();
            }
            return stores;
        }

        /// <summary>
        /// ��ȡ����ȡ�������Ĳֿ�
        /// </summary>
        /// <param name="itemID">��ƷID</param>
        /// <param name="amount">����</param>
        /// <param name="priorityType">�Ƿ������ȼ���ȡ 0��ʾ����Ҫ��1��ʾ���ȼ��Ӹߵ��ͣ�-1��ʾ���ȼ��ӵ͵���</param>
        /// <returns>ȡ�������Ͷ�Ӧ�ֿ��б�</returns>
        public Dictionary<Store, int> GetPutOutStore(DataNS.IDataObj data, int amount, int priorityType = 0, bool judgeInteracting = false, bool judgeCanOut = false)
        {
            Dictionary<Store, int> result = new Dictionary<Store, int>();
            if (!string.IsNullOrEmpty(itemID) && amount > 0)
            {
                int resultAmount = 0;
                List<Store> stores = GetStores(priorityType);
                foreach (Store store in stores)
                {
                    if (!judgeInteracting || !store.IsInteracting)
                    {
                        int storeAmount = store.DataContainer.GetAmount(itemID, DataNS.DataOpType.Storage, false, judgeCanOut);
                        if (storeAmount > 0)
                        {
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
            }
            return result;
        }
        /// <summary>
        /// ��ȡ������������Ĳֿ�
        /// </summary>
        /// <param name="itemID">��ƷID</param>
        /// <param name="amount">����</param>
        /// <param name="priorityType">�Ƿ������ȼ���ȡ 0��ʾ����Ҫ��1��ʾ���ȼ��Ӹߵ��ͣ�-1��ʾ���ȼ��ӵ͵���</param>
        /// <returns></returns>
        public Store GetPutInStore(DataNS.IDataObj data, int amount, int priorityType = 0, bool judgeInteracting = false, bool judgeCanIn = false)
        {
            Store result = null;
            if (!string.IsNullOrEmpty(itemID) && amount > 0)
            {
                List<Store> stores = GetStores(priorityType);
                foreach (Store store in stores)
                {
                    if (!judgeInteracting || !store.IsInteracting)
                    {
                        // ����Ѱ�ҵ�һ������һ���Դ���Ĳֿ�
                        // ��û�У���Ѱ�ҵ�һ�����Դ���ģ����������
                        int empty = store.DataContainer.GetAmount(itemID, DataNS.DataOpType.Empty, judgeCanIn);
                        if (result == null && empty > 0)
                        {
                            result = store;
                        }
                        if (empty >= amount)
                        {
                            result = store;
                            break;
                        }
                    }
                }
            }
            return result;
        }
        public List<string> GetStoreIconItems()
        {
            List<string> result = new List<string>();
            foreach (var data in StoreIconTableDict.Values)
            {
                result.Add(data.ID);
            }
            return ManagerNS.LocalGameManager.Instance.ItemManager.SortItemIDs(result);
        }

        public Store SpawnStore(ML.Engine.BuildingSystem.BuildingPart.BuildingCategory2 storeType) { return new Store(storeType); }

        public void WorldStoreSetData(WorldStore worldStore, ML.Engine.BuildingSystem.BuildingPart.BuildingCategory2 storeType, int level)
        {
            if (worldStore != null && level >= 0)
            {
                WorldStoreSetData(worldStore, SpawnStore(storeType));
            }
        }

        public void WorldStoreSetData(WorldStore worldStore, Store store)
        {
            if (worldStore != null && store != null)
            {
                if (!WorldStoreDict.ContainsKey(worldStore.InstanceID))
                {
                    WorldStoreDict.Add(worldStore.InstanceID, worldStore);
                }
                else
                {
                    WorldStoreDict[worldStore.InstanceID] = worldStore;
                }
                if (worldStore.Store != store)
                {
                    if (worldStore.Store != null)
                    {
                        worldStore.Store.Destroy();
                        worldStore.Store.WorldStore = null;
                    }
                    if (store.WorldStore != null)
                    {
                        store.WorldStore.Store = null;
                    }
                    worldStore.Store = store;
                    store.WorldStore = worldStore;
                }
            }
        }
    }
}
