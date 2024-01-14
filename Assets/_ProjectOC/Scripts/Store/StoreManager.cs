using ML.Engine.Manager;
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

        /// <summary>
        /// 是否是有效的仓库UID
        /// </summary>
        public bool IsValidUID(string uid)
        {
            return WorldStoreDict.ContainsKey(uid);
        }

        /// <summary>
        /// 根据UID获取WorldStore
        /// </summary>
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
        public Store GetCanPutInStore(string itemID, int amount)
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
                    if (store.IsStoreHaveEmpty(itemID, amount))
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
        public Dictionary<Store, int> GetCanPutOutStore(string itemID, int amount)
        {
            Dictionary<Store, int> result = new Dictionary<Store, int>();
            int resultAmount = 0;
            // 从头到尾遍历仓库(跳过玩家正在交互的仓库)
            foreach (WorldStore worldStore in this.WorldStoreDict.Values)
            {
                Store store = worldStore.Store;
                if (store != null && !store.IsInteracting && store.IsStoreHaveItem(itemID))
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

        public WorldStore SpawnWorldStore(Store store, Vector3 pos, Quaternion rot)
        {
            if (store == null)
            {
                return null;
            }
            GameObject obj = GameObject.Instantiate(GetObject(), pos, rot);
            WorldStore worldstore = obj.AddComponent<WorldStore>();
            worldstore.SetStore(store);
            WorldStoreDict.Add(store.UID, worldstore);
            return worldstore;
        }

        public const string Texture2DPath = "ui/Store/texture2d";
        public const string WorldObjPath = "prefabs/Store/WorldStore";
        public Texture2D GetTexture2D()
        {
            return GameManager.Instance.ABResourceManager.LoadLocalAB(Texture2DPath).LoadAsset<Texture2D>("Store");
        }
        public Sprite GetSprite()
        {
            var tex = GetTexture2D();
            if (tex == null)
            {
                return null;
            }
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        public GameObject GetObject()
        {
            return GameManager.Instance.ABResourceManager.LoadLocalAB(WorldObjPath).LoadAsset<GameObject>("Store");
        }
    }
}

