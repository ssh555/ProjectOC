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
                foreach (var data in datas) { StoreIconTableDict.Add(data.ID, data); }
            }, "仓库图标表数据");
            ABJAProcessor.StartLoadJsonAssetData();
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<StoreConfigAsset>("Config_Store").Completed += (handle) =>
            {
                Config = new StoreConfig(handle.Result.Config);
            };
        }
        #endregion

        private Dictionary<string, IWorldStore> WorldStoreDict = new Dictionary<string, IWorldStore>();

        /// <param name="priorityType">是否按照优先级获取 0表示不需要，1表示优先级从高到低，-1表示优先级从低到高</param>
        public List<IStore> GetStores(int priorityType = 0)
        {
            List<IStore> stores = new List<IStore>();
            foreach (var worldStore in WorldStoreDict.Values)
            {
                if (worldStore != null)
                {
                    stores.Add(worldStore.Store);
                }
            }
            if (priorityType == 1)
            {
                stores.Sort(new IStore.Sort());
            }
            else if (priorityType == -1)
            {
                stores.Sort(new IStore.Sort());
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
        public Dictionary<IStore, int> GetPutOutStore(MissionNS.MissionTransport mission, int priorityType = 0, bool judgeInteracting = false, bool judgeCanOut = false)
        {
            DataNS.IDataObj data = mission.Data;
            int amount = mission.NeedAssignNum;
            bool flag = data is ML.Engine.InventorySystem.CreatureItem;
            Dictionary<IStore, int> result = new Dictionary<IStore, int>();
            if (data != null && amount > 0)
            {
                int resultAmount = 0;
                List<IStore> stores = GetStores(priorityType);
                foreach (IStore store in stores)
                {
                    if (!judgeInteracting || !store.IsInteracting)
                    {
                        int storeAmount = store.DataContainer.GetAmount(data.GetDataID(), DataNS.DataOpType.Storage, false, judgeCanOut);
                        if (storeAmount > 0)
                        {
                            if (flag && store.DataContainer.GetData(0) is ML.Engine.InventorySystem.CreatureItem creature && creature.Output < mission.OutputThreshold)
                            {
                                continue;
                            }
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
        public Dictionary<IStore, int> GetPutOutStore(DataNS.IDataObj data, int amount, int priorityType = 0, bool judgeInteracting = false, bool judgeCanOut = false)
        {
            Dictionary<IStore, int> result = new Dictionary<IStore, int>();
            if (data != null && amount > 0)
            {
                int resultAmount = 0;
                List<IStore> stores = GetStores(priorityType);
                foreach (IStore store in stores)
                {
                    if (!judgeInteracting || !store.IsInteracting)
                    {
                        int storeAmount = store.DataContainer.GetAmount(data.GetDataID(), DataNS.DataOpType.Storage, false, judgeCanOut);
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
        public IStore GetPutInStore(DataNS.IDataObj data, int amount, int priorityType = 0, bool judgeInteracting = false, bool judgeCanIn = false)
        {
            IStore result = null;
            if (data != null && amount > 0)
            {
                bool flag = data is DataNS.ItemIDDataObj;
                List<IStore> stores = GetStores(priorityType);
                foreach (IStore store in stores)
                {
                    if (!judgeInteracting || !store.IsInteracting)
                    {
                        if (flag)
                        {
                            // 优先寻找第一个可以一次性存完的仓库
                            // 若没有，则寻找第一个可以存入的，可溢出存入
                            int empty = store.DataContainer.GetAmount(data, DataNS.DataOpType.Empty, judgeCanIn);
                            if (result == null && empty > 0) { result = store; }
                            if (empty >= amount)
                            {
                                result = store;
                                break;
                            }
                        }
                        else
                        {
                            if (store is CreatureStore creatureStore &&
                            creatureStore.CreatureItemID == data.GetDataID() &&
                            store.DataContainer.GetEmptyIndex(judgeCanIn) >= 0)
                            {
                                result = store;
                                break;
                            }
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
        public List<string> GetCreatureStoreIconItems()
        {
            string[] all = ManagerNS.LocalGameManager.Instance.ItemManager.GetAllItemID();
            List<string> result = new List<string>();
            foreach (string id in all)
            {
                if (ManagerNS.LocalGameManager.Instance.ItemManager.GetItemType(id) == ML.Engine.InventorySystem.ItemType.Creature)
                {
                    result.Add(id);
                }
            }
            return ManagerNS.LocalGameManager.Instance.ItemManager.SortItemIDs(result);
        }

        public IStore SpawnStore(IWorldStore worldStore, int level) 
        {
            if (worldStore is WorldStore)
            {
                return new Store(worldStore.Classification.Category2, level);
            }
            else if (worldStore is CreatureWorldStore)
            {
                return new CreatureStore(worldStore.Classification.Category2, level);
            }
            return null;
        }

        public void WorldStoreSetData(IWorldStore worldStore, int level)
        {
            if (worldStore != null && level >= 0)
            {
                WorldStoreSetData(worldStore, SpawnStore(worldStore, level));
            }
        }

        public void WorldStoreSetData(IWorldStore worldStore, IStore store)
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