using ML.Engine.Manager;
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

        /// <summary>
        /// �Ƿ�����Ч�Ĳֿ�UID
        /// </summary>
        public bool IsValidUID(string uid)
        {
            return WorldStoreDict.ContainsKey(uid);
        }

        /// <summary>
        /// ����UID��ȡWorldStore
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
                Store store = worldStore.Store;
                if (store != null && !store.IsInteracting && store.IsStoreHaveItem(itemID))
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
        /// �����µĲֿ�
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

