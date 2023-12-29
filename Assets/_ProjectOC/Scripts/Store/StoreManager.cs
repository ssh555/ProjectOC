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
    /// �ֿ������
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
        /// ���ɵĲֿ⣬��ΪID
        /// </summary>
        private Dictionary<string, List<Store>> StoreDict = new Dictionary<string, List<Store>>();
        /// <summary>
        /// ʵ�������ɵĲֿ⣬��ΪUID
        /// </summary>
        private Dictionary<string, WorldStore> WorldStoreDict = new Dictionary<string, WorldStore>();
        /// <summary>
        /// ����Store���ݱ�
        /// </summary>
        private Dictionary<string, StoreTableJsonData> StoreTableDict = new Dictionary<string, StoreTableJsonData>();

        /// <summary>
        /// �Ƿ�����Ч�Ĳֿ�ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsValidID(string id)
        {
            return this.StoreTableDict.ContainsKey(id);
        }
        /// <summary>
        /// �Ƿ�����Ч�Ĳֿ�UID
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
        /// ��ȡ������������Ĳֿ�
        /// </summary>
        /// <param name="itemID">��ƷID</param>
        /// <param name="amount">����</param>
        /// <returns></returns>
        public Store GetStoreForStorage(string itemID, int amount)
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
        public List<Tuple<int, Store>> GetStoreForRetrieve(string itemID, int amount)
        {
            List<Tuple<int, Store>> result = new List<Tuple<int, Store>>();
            int resultAmount = 0;
            // ��ͷ��β�����ֿ�(����������ڽ����Ĳֿ�)
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
        /// ����id�����µĲֿ�
        /// </summary>
        /// <param name="id">�ֿ�id</param>
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
            Debug.LogError("û�ж�ӦIDΪ " + id + " �Ĳֿ�");
            return null;
        }
        public WorldStore SpawnWorldStore(Store store, Vector3 pos, Quaternion rot)
        {
            if (store == null)
            {
                return null;
            }
            // to-do : �ɲ��ö������ʽ
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

        #region to-do : ���������������� Store ����
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
                }, null, "�ֿ������");
                ABJAProcessor.StartLoadJsonAssetData();
            }
        }

        #endregion
    }
}

