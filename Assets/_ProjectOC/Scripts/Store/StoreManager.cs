using ML.Engine.Manager;
using ML.Engine.TextContent;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProjectOC.WorkerNS.EffectManager;

namespace ProjectOC.StoreNS
{
    /// <summary>
    /// 仓库管理器
    /// </summary>
    [System.Serializable]
    public sealed class StoreManager : ML.Engine.Manager.GlobalManager.IGlobalManager
    {
        #region Instance
        private StoreManager() { }

        private static StoreManager instance;

        public static StoreManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new StoreManager();
                    GameManager.Instance.RegisterGlobalManager(instance);
                    instance.LoadTableData();
                }
                return instance;
            }
        }
        #endregion

        /// <summary>
        /// 生成的仓库，键为ID
        /// </summary>
        private Dictionary<string, List<Store>> StoreDict = new Dictionary<string, List<Store>>();
        /// <summary>
        /// 实例化生成的仓库，键为UID
        /// </summary>
        private Dictionary<string, WorldStore> WorldStoreDict = new Dictionary<string, WorldStore>();
        /// <summary>
        /// 基础Store数据表
        /// </summary>
        private Dictionary<string, StoreTableJsonData> StoreTableDict = new Dictionary<string, StoreTableJsonData>();

        /// <summary>
        /// 是否是有效的仓库ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsValidID(string id)
        {
            return this.StoreTableDict.ContainsKey(id);
        }
        /// <summary>
        /// 是否是有效的仓库UID
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public bool IsValidUID(string uid)
        {
            return WorldStoreDict.ContainsKey(uid);
        }
        public List<Store> GetStore(string id)
        {
            if (this.StoreDict.ContainsKey(id))
            {
                return this.StoreDict[id];
            }
            return new List<Store>();
        }
        public WorldStore GetWorldStore(string uid)
        {
            if (this.WorldStoreDict.ContainsKey(uid))
            {
                return this.WorldStoreDict[uid];
            }
            return null;
        }
        /// <summary>
        /// 获取满足存入条件的仓库
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <param name="amount">数量</param>
        /// <returns></returns>
        public Store GetStoreForStorage(string itemID, int amount)
        {
            Store result = null;
            // 从头到尾遍历仓库(跳过玩家正在交互的仓库)
            foreach (WorldStore worldStore in this.WorldStoreDict.Values)
            {
                Store store = worldStore.Store;
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
        public List<Tuple<int, Store>> GetStoreForRetrieve(string itemID, int amount)
        {
            List<Tuple<int, Store>> result = new List<Tuple<int, Store>>();
            int resultAmount = 0;
            // 从头到尾遍历仓库(跳过玩家正在交互的仓库)
            foreach (WorldStore worldStore in this.WorldStoreDict.Values)
            {
                Store store = worldStore.Store;
                if (store != null && !store.IsInteracting && store.IsStoreHaveItem(itemID))
                {
                    int storeAmount = store.GetItemStorageCapacity(itemID);
                    if (resultAmount + storeAmount >= amount)
                    {
                        result.Add(new Tuple<int, Store>(amount - resultAmount, store));
                        resultAmount = amount;
                        break;
                    }
                    else
                    {
                        result.Add(new Tuple<int, Store>(storeAmount, store));
                        resultAmount += storeAmount;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 根据id创建新的仓库
        /// </summary>
        /// <param name="id">仓库id</param>
        /// <returns></returns>
        public Store SpawnStore(string id)
        {
            if (StoreTableDict.ContainsKey(id))
            {
                StoreTableJsonData row = this.StoreTableDict[id];
                Store store = new Store(row);
                if (!StoreDict.ContainsKey(store.ID))
                {
                    StoreDict.Add(store.ID, new List<Store>());
                }
                StoreDict[id].Add(store);
                return store;
            }
            Debug.LogError("没有对应ID为 " + id + " 的仓库");
            return null;
        }
        public WorldStore SpawnWorldStore(Store store, Vector3 pos, Quaternion rot)
        {
            if (store == null)
            {
                return null;
            }
            // to-do : 可采用对象池形式
            GameObject obj = GameObject.Instantiate(GameManager.Instance.ABResourceManager.LoadLocalAB(WorldObjPath).LoadAsset<GameObject>(this.StoreTableDict[store.ID].worldobject), pos, rot);
            WorldStore worldstore = obj.GetComponent<WorldStore>();
            if (worldstore == null)
            {
                worldstore = obj.AddComponent<WorldStore>();
            }
            worldstore.SetStore(store);
            WorldStoreDict.Add(store.UID, worldstore);
            return worldstore;
        }
        public static WorldStore SpawnWorldStoreInEditor(GameObject obj, Store store, Vector3 pos, Quaternion rot)
        {
#if !UNITY_EDITOR
            return null;
#else
            if (store == null || UnityEditor.EditorApplication.isPlaying)
            {
                return null;
            }

            obj = UnityEditor.PrefabUtility.InstantiatePrefab(obj) as GameObject;
            obj.transform.position = pos;
            obj.transform.rotation = rot;

            WorldStore worldstore = obj.GetComponent<WorldStore>();
            if (worldstore == null)
            {
                worldstore = obj.AddComponent<WorldStore>();
            }
            worldstore.SetStore(store);
            Instance.WorldStoreDict.Add(store.UID, worldstore);
            return worldstore;
#endif
        }

        public Texture2D GetTexture2D(string id)
        {
            if (!this.StoreTableDict.ContainsKey(id))
            {
                return null;
            }

            return GameManager.Instance.ABResourceManager.LoadLocalAB(Texture2DPath).LoadAsset<Texture2D>(this.StoreTableDict[id].texture2d);
        }
        public Sprite GetSprite(string id)
        {
            var tex = this.GetTexture2D(id);
            if (tex == null)
            {
                return null;
            }
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        public GameObject GetObject(string id)
        {
            if (!this.StoreTableDict.ContainsKey(id))
            {
                return null;
            }

            return GameManager.Instance.ABResourceManager.LoadLocalAB(WorldObjPath).LoadAsset<GameObject>(this.StoreTableDict[id].worldobject);
        }

        #region to-do : 需读表导入所有所需的 Store 数据
        public const string Texture2DPath = "ui/Store/texture2d";
        public const string WorldObjPath = "prefabs/Store/WorldStore";

        [System.Serializable]
        public struct StoreTableJsonData
        {
            public string id;
            public TextContent name;
            public StoreType type;
            public string texture2d;
            public string worldobject;
        }
        public static ML.Engine.ABResources.ABJsonAssetProcessor<StoreTableJsonData[]> ABJAProcessor;

        public void LoadTableData()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<StoreTableJsonData[]>("Json/TableData", "StoreTableData", (datas) =>
                {
                    foreach (var data in datas)
                    {
                        this.StoreTableDict.Add(data.id, data);
                    }
                }, null, "仓库表数据");
                ABJAProcessor.StartLoadJsonAssetData();
            }
        }

        #endregion
    }
}

