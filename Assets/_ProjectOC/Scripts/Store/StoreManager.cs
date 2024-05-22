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

    [LabelText("仓库管理器"), System.Serializable]
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
            }, "仓库图标表数据");
            ABJAProcessor.StartLoadJsonAssetData();
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<StoreConfigAsset>("Config_Store").Completed += (handle) =>
            {
                StoreConfigAsset data = handle.Result;
                Config = new StoreConfig(data.Config);
            };
        }
        #endregion

        private Dictionary<string, WorldStore> WorldStoreDict = new Dictionary<string, WorldStore>();

        /// <param name="priorityType">是否按照优先级获取 0表示不需要，1表示优先级从高到低，-1表示优先级从低到高</param>
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
        /// 获取满足取出条件的仓库
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <param name="amount">数量</param>
        /// <param name="priorityType">是否按照优先级获取 0表示不需要，1表示优先级从高到低，-1表示优先级从低到高</param>
        /// <returns>取出数量和对应仓库列表</returns>
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
        /// 获取满足存入条件的仓库
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <param name="amount">数量</param>
        /// <param name="priorityType">是否按照优先级获取 0表示不需要，1表示优先级从高到低，-1表示优先级从低到高</param>
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
                        // 优先寻找第一个可以一次性存完的仓库
                        // 若没有，则寻找第一个可以存入的，可溢出存入
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
