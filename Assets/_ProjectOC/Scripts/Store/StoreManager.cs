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
        public void OnRegister()
        {
            LoadTableData();
        }

        #region 配置
        [LabelText("仓库最大等级"), FoldoutGroup("配置")]
        public int LevelMax = 2;
        [LabelText("每个级别的仓库容量"), FoldoutGroup("配置")]
        public List<int> LevelCapacity = new List<int>() { 2, 4, 8 };
        [LabelText("每个级别的仓库数据容量"), FoldoutGroup("配置")]
        public List<int> LevelDataCapacity = new List<int>() { 50, 100, 200 };
        #endregion

        #region Load And Data
        private Dictionary<string, StoreIconTableData> StoreIconTableDict = new Dictionary<string, StoreIconTableData>();
        public ML.Engine.ABResources.ABJsonAssetProcessor<StoreIconTableData[]> ABJAProcessor;
        public void LoadTableData()
        {
            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<StoreIconTableData[]>("OCTableData", "StoreIcon", (datas) =>
            {
                foreach (var data in datas)
                {
                    StoreIconTableDict.Add(data.ID, data);
                }
            }, "仓库图标表数据");
            ABJAProcessor.StartLoadJsonAssetData();
        }
        #endregion

        private Dictionary<string, WorldStore> WorldStoreDict = new Dictionary<string, WorldStore>();

        /// <param name="priorityType">是否按照优先级获取 0表示不需要，1表示优先级从高到低，-1表示优先级从低到高</param>
        public List<Store> GetStores(int priorityType = 0)
        {
            List<Store> stores = new List<Store>();
            foreach (WorldStore worldStore in this.WorldStoreDict.Values)
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
        public Dictionary<Store, int> GetPutOutStore(string itemID, int amount, int priorityType = 0, bool judgeInteracting = false, bool judgeCanOut = false)
        {
            Dictionary<Store, int> result = new Dictionary<Store, int>();
            if (!string.IsNullOrEmpty(itemID) && amount > 0)
            {
                int resultAmount = 0;
                List<Store> stores = GetStores(priorityType);
                foreach (Store store in stores)
                {
                    if ((!judgeInteracting || !store.IsInteracting) && (!judgeCanOut || store.HaveSetItem(itemID, false, judgeCanOut)))
                    {
                        int storeAmount = store.GetAmount(itemID, DataNS.DataOpType.Storage, false, judgeCanOut);
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
        public Store GetPutInStore(string itemID, int amount, int priorityType = 0, bool judgeInteracting = false, bool judgeCanIn = false)
        {
            List<Store> stores = GetStores(priorityType);
            Store result = null;
            foreach (Store store in stores)
            {
                if ((!judgeInteracting || !store.IsInteracting) && (!judgeCanIn || store.HaveSetItem(itemID, judgeCanIn)))
                {
                    // 优先寻找第一个可以一次性存完的仓库
                    // 若没有，则寻找第一个可以存入的，可溢出存入
                    int empty = store.GetAmount(itemID, DataNS.DataOpType.Empty, judgeCanIn);
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
            return result;
        }

        public List<string> GetStoreIconItems()
        {
            List<string> result = new List<string>();
            foreach (var data in this.StoreIconTableDict.Values)
            {
                result.Add(data.ID);
            }
            return ManagerNS.LocalGameManager.Instance.ItemManager.SortItemIDs(result);
        }

        public Store SpawnStore(ML.Engine.BuildingSystem.BuildingPart.BuildingCategory2 storeType)
        {
            Store store = new Store(storeType);
            return store;
        }

        public void WorldStoreSetData(WorldStore worldStore, ML.Engine.BuildingSystem.BuildingPart.BuildingCategory2 storeType, int level)
        {
            if (worldStore != null && level >= 0)
            {
                Store store = SpawnStore(storeType);
                WorldStoreSetData(worldStore, store);
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
                if (worldStore.Store != null)
                {
                    worldStore.Store.WorldStore = null;
                }
                worldStore.Store = store;
                store.WorldStore = worldStore;
            }
        }
    }
}
