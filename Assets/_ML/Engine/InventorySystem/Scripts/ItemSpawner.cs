using ML.Engine.BuildingSystem.BuildingPart;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ML.Engine.BuildingSystem.Test_BuildingManager;

namespace ML.Engine.InventorySystem
{
    /// <summary>
    /// 没有以 Manager 为后缀，是懒得改其他地方了，太多了
    /// </summary>
    public sealed class ItemSpawner : Manager.GlobalManager.IGlobalManager
    {
        #region Instance
        private ItemSpawner() { }

        private static ItemSpawner instance;

        public static ItemSpawner Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ItemSpawner();
                    Manager.GameManager.Instance.RegisterGlobalManager(instance);
                }
                return instance;
            }
        }
        #endregion
        
        /// <summary>
        /// 根据需求自动添加
        /// </summary>
        private Dictionary<int, Type> ItemDict = new Dictionary<int, Type>();

        /// <summary>
        /// 读入的表数据
        /// to-do : 需根据实际需求更改
        /// </summary>
        public struct ItemTabelData
        {
            public string name;
            public string type;
            public bool bCanStack;
            public int maxAmount;
            public Sprite sprite;
            public GameObject worldObject;
        }
        /// <summary>
        /// to-do : 读表初始化
        /// 基础Item数据表 => 可加入联合体包含具体类型应有的数据
        /// </summary>
        private Dictionary<int, ItemTabelData> ItemTypeStrDict = new Dictionary<int, ItemTabelData>();

        /// <summary>
        /// 根据 id 生成一个崭新的Item，数量为1
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Item SpawnItem(int id)
        {
            if (this.ItemTypeStrDict.TryGetValue(id, out ItemTabelData itemRow))
            {
                Type type;
                if(!this.ItemDict.TryGetValue(id, out type))
                {
                    type = GetTypeByName(itemRow.type);
                    if (type == null)
                    {
                        Debug.LogError("没有对应ID为 " + id + " 的Item");
                        return null;
                    }
                    this.ItemDict.Add(id, type);
                }
                // to-do : 采用反射，可能会有性能问题
                Item item = System.Activator.CreateInstance(type, id) as Item;
                item.Init(itemRow);
                return item;
            }
            Debug.LogError("没有对应ID为 " + id + " 的Item");
            return null;
        }



        public WorldItem SpawnWorldItem(Item item, Vector3 pos, Quaternion rot)
        {
            if (item == null)
            {
                return null;
            }
            // to-do : 可采用对象池形式
            GameObject obj = GameObject.Instantiate(this.ItemTypeStrDict[item.ID].worldObject, pos, rot);
            WorldItem worldItem = obj.GetComponent<WorldItem>();
            if (worldItem == null)
            {
                worldItem = obj.AddComponent<WorldItem>();
            }
            worldItem.SetItem(item);

            return worldItem;
        }

        public static WorldItem SpawnWorldItemInEditor(GameObject obj, Item item, Vector3 pos, Quaternion rot)
        {
#if !UNITY_EDITOR
            return null;
#else
            if (item == null || UnityEditor.EditorApplication.isPlaying)
            {
                return null;
            }


            obj = UnityEditor.PrefabUtility.InstantiatePrefab(obj) as GameObject;
            obj.transform.position = pos;
            obj.transform.rotation = rot;

            WorldItem worldItem = obj.GetComponent<WorldItem>();
            if (worldItem == null)
            {
                worldItem = obj.AddComponent<WorldItem>();
            }
            worldItem.SetItem(item);
            return worldItem;
#endif

        }

        public bool IsValidItemID(int id)
        {
            return this.ItemTypeStrDict.ContainsKey(id);
        }

        public Sprite GetItemSprite(int id)
        {
            if (!this.ItemTypeStrDict.ContainsKey(id))
            {
                return null;
            }
            return this.ItemTypeStrDict[id].sprite;
        }

        public GameObject GetItemObject(int id)
        {
            if (!this.ItemTypeStrDict.ContainsKey(id))
            {
                return null;
            }
            return this.ItemTypeStrDict[id].worldObject;
        }

        public string GetItemName(int id)
        {
            if (!this.ItemTypeStrDict.ContainsKey(id))
            {
                return "";
            }
            return this.ItemTypeStrDict[id].name;
        }

        public static Type GetTypeByName(string fullName)
        {
            //获取应用程序用到的所有程序集
            var asy = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < asy.Length; i++)
            {
                //从对应的程序集中根据名字字符串找的对应的实体类型
                //需要填写包括命名空间的类型全名，例"System.Windows.Forms.Button"
                if (asy[i].GetType(fullName, false) != null)
                {
                    return asy[i].GetType(fullName, false);
                }
            }
            return null;
        }

#region to-do : 需读表导入所有所需的 Item 数据
        public const string typePath = "ML.Engine.InventorySystem.";
        public const string spritePath = "ui/Item/texture2d";
        public const string worldPath = "prefabs/Item/WorldItem";

        public const string ItemTableDataABPath = "Json/TabelData";
        public const string TableName = "ItemTableData";

        [System.Serializable]
        public struct ItemTabelJsonData
        {
            public int id;
            public string name;
            public string type;
            public bool bcanstack;
            public int maxamount;
            public string texture2d;
            public string worldobject;
        }
        [System.Serializable]
        private struct ItemTabelJsonDataArray
        {
            public ItemTabelJsonData[] table;
        }

        public IEnumerator LoadTableData(MonoBehaviour mono)
        {
            while (Manager.GameManager.Instance.ABResourceManager == null)
            {
                yield return null;
            }
            var abmgr = Manager.GameManager.Instance.ABResourceManager;
            AssetBundle ab;
            var crequest = abmgr.LoadLocalABAsync(ItemTableDataABPath, null, out ab);
            yield return crequest;
            ab = crequest.assetBundle;


            var request = ab.LoadAssetAsync<TextAsset>(TableName);
            yield return request;
            ItemTabelJsonDataArray datas = JsonUtility.FromJson<ItemTabelJsonDataArray>((request.asset as TextAsset).text);



            Coroutine[] coroutines = new Coroutine[datas.table.Length];

            for(int i = 0; i < coroutines.Length; ++i)
            {
                coroutines[i] = mono.StartCoroutine(LoadRowData(datas.table[i]));
            }

            // to-do : 需优化资源加载
            foreach(var coroutine in coroutines)
            {
                yield return coroutine;
            }

            abmgr.UnLoadLocalABAsync(ItemTableDataABPath, false, null);

            yield break;
        }

        private IEnumerator LoadRowData(ItemTabelJsonData row)
        {
            ItemTabelData data = new ItemTabelData();
            // ID -> 0
            int id = row.id;
            // Name -> 1
            data.name = row.name;
            // Type -> 2
            data.type = ItemSpawner.typePath + row.type;
            // bCanStack -> 3
            data.bCanStack = row.bcanstack;
            // MaxAmount -> 4
            data.maxAmount = row.maxamount;

            // to-do : 可考虑采用两套，unityeditor不使用AB包，方便调试
            var r1 = ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<Texture2D>(ItemSpawner.spritePath, row.texture2d, null);
            var r2 = ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<GameObject>(ItemSpawner.worldPath, row.worldobject, null);

            yield return r1;
            yield return r2;

            // SpritePath -> 5
            Texture2D tex = r1.asset as Texture2D;
#if UNITY_EDITOR
            if (tex == null)
            {
                Debug.LogError(spritePath + "/" + row.texture2d + " 文件缺失");
            }
#endif

            data.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

            // WorldObjectPath -> 6
#if UNITY_EDITOR
            if (r2.asset == null)
            {
                Debug.LogError(worldPath + "/" + row.worldobject + " 文件缺失");
            }
#endif
            data.worldObject = r2.asset as GameObject;

            this.ItemTypeStrDict.Add(id, data);

            yield break;
        }

#endregion


    }
}

