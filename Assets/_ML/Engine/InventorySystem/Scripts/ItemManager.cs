using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;
using System.Threading.Tasks;
using ML.Engine.Manager.LocalManager;


namespace ML.Engine.InventorySystem
{
    [System.Serializable]
    public struct ItemTableData
    {
        public string id;
        public int sort;
        public ItemType itemtype;
        public TextContent.TextContent name;
        public int weight;
        public string icon;
        public bool bcanstack;
        public bool bcandestroy;
        public TextContent.TextContent itemdescription;
        public TextContent.TextContent effectsdescription;
        public bool bcanuse;
        public string type;
        public int maxamount;
        public string worldobject;
    }

    public enum ApplicationScenario
    {
        Bag=0,
    }
    [System.Serializable]
    public struct CategoryManage
    {
        public TextContent.TextContent CategoryName;
        public string CategoryIcon;
        public List<ItemType> ItemTypes;
    }


    [System.Serializable]
    public struct ItemCategoryTableData
    {
        public string id;
        public int sort;
        public ApplicationScenario applicationScenario;
        public CategoryManage categoryManage;
    }

    [System.Serializable]
    public sealed class ItemManager : ILocalManager
    {
        #region Instance



        public static ItemManager Instance { get { return instance; } }

        private static ItemManager instance;

        /// <summary>
        /// 单例管理
        /// </summary>
        public void Init()
        {
            LoadTableData();
            LoadItemAtlas();
        }

        public void OnRegister()
        {
            if (instance == null)
            {
                instance = this;
                Init();
            }
        }

        public void OnUnregister()
        {
            if (instance == this)
            {
                instance = null;
            }
            Manager.GameManager.Instance.ABResourceManager.Release(itemAtlas);
        }

        #endregion

        #region Load And Data
        /// <summary>
        /// 是否已加载完数据
        /// </summary>
        public bool IsLoadOvered => ABJAProcessorItemTableData != null && ABJAProcessorItemTableData.IsLoaded;

        /// <summary>
        /// 根据需求自动添加
        /// </summary>
        private Dictionary<string, Type> ItemDict = new Dictionary<string, Type>();

        /// <summary>
        /// to-do : 读表初始化
        /// 基础Item数据表 => 可加入联合体包含具体类型应有的数据
        /// </summary>
        private Dictionary<string, ItemTableData> ItemTypeStrDict = new Dictionary<string, ItemTableData>();

        private Dictionary<string,ItemCategoryTableData> ItemCategoryTableDataDicOnID = new Dictionary<string, ItemCategoryTableData>();
        private Dictionary<ApplicationScenario, List<(int,CategoryManage)>> ItemCategoryTableDataDicOnApplicationScenario = new Dictionary<ApplicationScenario, List<(int, CategoryManage)>>();

        #region to-do : 需读表导入所有所需的 Item 数据
        public const string TypePath = "ML.Engine.InventorySystem.";
        public const string ItemIconLabel = "Item/SA_Item_UI_ItemIcon";
        public const string WorldObjLabel = "Item/Prefab_WorldItem";



        public ML.Engine.ABResources.ABJsonAssetProcessor<ItemTableData[]> ABJAProcessorItemTableData;
        public ML.Engine.ABResources.ABJsonAssetProcessor<ItemCategoryTableData[]> ABJAProcessorItemCategoryTableData;

        public void LoadTableData()
        {
            ABJAProcessorItemTableData = new ML.Engine.ABResources.ABJsonAssetProcessor<ItemTableData[]>("OC/Json/TableData", "Item", (datas) =>
            {
                foreach (var data in datas)
                {
                    this.ItemTypeStrDict.Add(data.id, data);
                }
            }, "背包系统物品Item表数据");
            ABJAProcessorItemTableData.StartLoadJsonAssetData();

            ABJAProcessorItemCategoryTableData = new ML.Engine.ABResources.ABJsonAssetProcessor<ItemCategoryTableData[]>("OC/Json/TableData", "ItemCategory", (datas) =>
            {
                foreach (var data in datas)
                {
                    this.ItemCategoryTableDataDicOnID.Add(data.id, data);

                    if(!this.ItemCategoryTableDataDicOnApplicationScenario.ContainsKey(data.applicationScenario))
                    {
                        this.ItemCategoryTableDataDicOnApplicationScenario.Add(data.applicationScenario, new List<(int, CategoryManage)>());
                    }
                    this.ItemCategoryTableDataDicOnApplicationScenario[data.applicationScenario].Add((data.sort, data.categoryManage));
                }
            }, "ItemCategoryTable数据");
            ABJAProcessorItemCategoryTableData.StartLoadJsonAssetData();
        }
        #endregion

        #endregion

        #region Spawn
        /// <summary>
        /// 根据 id 生成一个崭新的Item，数量为1
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Item SpawnItem(string id)
        {
            if (this.ItemTypeStrDict.TryGetValue(id, out ItemTableData itemRow))
            {
                Type type;
                if(!this.ItemDict.TryGetValue(id, out type))
                {
                    type = GetTypeByName(TypePath + itemRow.type);
                    if (type == null)
                    {
                        Debug.LogError($"没有对应ID为 {id} Type为 {itemRow.type} 的Item");
                        return null;
                    }
                    this.ItemDict.Add(id, type);
                }
                // to-do : 采用反射，可能会有性能问题
                Item item = System.Activator.CreateInstance(type, id, itemRow, 1) as Item;
                return item;
            }
            Debug.LogError("没有对应ID为 " + id + " 的Item");
            return null;
        }

        public List<Item> SpawnItems(string id, int amount)
        {
            List<Item> res = new List<Item>();
            var item = SpawnItem(id);
            var ma = GetMaxAmount(item.ID);
            if (item != null)
            {
                while(amount > 0)
                {
                    if (ma >= amount)
                    {
                        item.Amount = amount;
                        res.Add(item);
                        amount = 0;
                    }
                    else
                    {
                        amount -= ma;
                        item.Amount = ma;
                        res.Add(item);
                        item = SpawnItem(id);
                    }
                }
            }
            return res;
        }

        public async Task<WorldItem> SpawnWorldItem(Item item, Vector3 pos, Quaternion rot)
        {
            if (item == null)
            {
                return null;
            }
            // to-do : 可采用对象池形式
            var handle = Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(WorldObjLabel + "/" + this.ItemTypeStrDict[item.ID].worldobject + ".prefab", pos, rot);

            await handle.Task;

            GameObject obj = handle.Result;

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
        #endregion

        #region SpriteAtlas

        private SpriteAtlas itemAtlas;
        public void LoadItemAtlas()
        {
            Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<SpriteAtlas>(ItemIconLabel).Completed += (handle) =>
            {
                itemAtlas = handle.Result;
            };
        }
        

        #endregion
        
        #region Getter
        public string[] GetAllItemID()
        {
            return ItemTypeStrDict.Keys.ToArray();
        }

        public List<string> SortItemIDs(List<string> itemIDs)
        {
            List<string> resultes = new List<string>();
            if (itemIDs != null)
            {
                List<Tuple<string, int>> temps = new List<Tuple<string, int>>();
                foreach (string id in itemIDs)
                {
                    temps.Add(new Tuple<string, int>(id, GetSortNum(id)));
                }
                temps.Sort((t1, t2) => { return t1.Item2 != t2.Item2 ? t1.Item2.CompareTo(t2.Item2) : t1.Item1.CompareTo(t2.Item1); });
                foreach (var tuple in temps)
                {
                    resultes.Add(tuple.Item1);
                }
            }
            return resultes;
        }

        public bool IsValidItemID(string id)
        {
            return this.ItemTypeStrDict.ContainsKey(id);
        }

        public Texture2D GetItemTexture2D(string id)
        {
            var s = GetItemSprite(id);
            if(s)
            {
                return s.texture;
            }
            return null;
        }

        public Sprite GetItemSprite(string id)
        {
            if (!this.ItemTypeStrDict.ContainsKey(id))
            {
                return this.itemAtlas.GetSprite(id);
            }

            return this.itemAtlas.GetSprite(this.ItemTypeStrDict[id].icon);
        }

        public async void AddItemIconObject(string itemID, Transform parent, Vector3 pos, Quaternion rot, Vector3 scale, Transform target=null, bool isLocal = true)
        {
            ItemIcon itemIcon = parent.GetComponentInChildren<ItemIcon>();
            if (itemIcon == null)
            {
                // 异步加载资源
                var handle = Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(WorldObjLabel + "/ItemIcon.prefab", parent);

                // 等待加载完成
                await handle.Task;

                var obj = handle.Result;

                if (isLocal)
                {
                    obj.transform.localPosition = pos;
                    obj.transform.localRotation = rot;
                }
                else
                {
                    obj.transform.position = pos;
                    obj.transform.rotation = rot;
                }
                obj.transform.localScale = scale;
                itemIcon = obj.GetComponentInChildren<ItemIcon>();
            }
            itemIcon.SetSprite(GetItemSprite(itemID ?? ""));
            itemIcon.Target = target;
        }

        public string GetItemName(string id)
        {
            if (!this.ItemTypeStrDict.ContainsKey(id))
            {
                return "";
            }
            return this.ItemTypeStrDict[id].name;
        }

        public bool GetCanStack(string id)
        {
            if (!this.ItemTypeStrDict.ContainsKey(id))
            {
                return false;
            }
            return this.ItemTypeStrDict[id].bcanstack;
        }

        //TODO GetCanUse GetCanDestroy
        public bool GetCanUse(string id)
        {
            if (!this.ItemTypeStrDict.ContainsKey(id))
            {
                return false;
            }
            return this.ItemTypeStrDict[id].bcanuse;
        }

        public bool GetCanDestroy(string id)
        {
            if (!this.ItemTypeStrDict.ContainsKey(id))
            {
                return false;
            }
            return this.ItemTypeStrDict[id].bcandestroy;
        }

        public int GetWeight(string id)
        {
            if (!this.ItemTypeStrDict.ContainsKey(id))
            {
                return -1;
            }
            return this.ItemTypeStrDict[id].weight;
        }

        public int GetSortNum(string id)
        {
            if (!this.ItemTypeStrDict.ContainsKey(id))
            {
                return int.MaxValue;
            }
            return this.ItemTypeStrDict[id].sort;
        }

        public ItemType GetItemType(string id)
        {
            if (!this.ItemTypeStrDict.ContainsKey(id))
            {
                return ItemType.None;
            }
            return this.ItemTypeStrDict[id].itemtype;
        }

        public int GetMaxAmount(string id)
        {
            if (!this.ItemTypeStrDict.ContainsKey(id))
            {
                return 0;
            }
            return this.ItemTypeStrDict[id].maxamount;
        }

        public string GetItemDescription(string id)
        {
            if (!this.ItemTypeStrDict.ContainsKey(id))
            {
                return null;
            }
            return this.ItemTypeStrDict[id].itemdescription;
        }

        public string GetEffectDescription(string id)
        {
            if (!this.ItemTypeStrDict.ContainsKey(id))
            {
                return null;
            }
            return this.ItemTypeStrDict[id].effectsdescription;
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

        public List<CategoryManage> GetCategoryManageByApplicationScenario(ApplicationScenario applicationScenario)
        {
            if(this.ItemCategoryTableDataDicOnApplicationScenario.ContainsKey(applicationScenario))
            {
                List<(int, CategoryManage)> tmp = this.ItemCategoryTableDataDicOnApplicationScenario[applicationScenario];
                tmp.Sort((x, y) => x.Item1.CompareTo(y.Item1));
                List<CategoryManage> categoryManages = new List<CategoryManage>();
                foreach (var(a, b) in tmp)
                {
                    categoryManages.Add(b);
                }
                return categoryManages;
            }
            return new List<CategoryManage>();
        }
        #endregion
    }
}

