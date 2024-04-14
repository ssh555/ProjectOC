using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;
using System.Threading.Tasks;
using ML.Engine.Manager.LocalManager;
using ProjectOC.Player;


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
        public TextContent.TextContent itemdescription;
        public TextContent.TextContent effectsdescription;
        public string type;
        public bool bcanstack;
        public int maxamount;
        public string worldobject;
    }

    [System.Serializable]
    public sealed class ItemManager : ILocalManager
    {
        #region Instance
        public ItemManager() 
        {
        }

        ~ItemManager()
        {
            foreach(var sa in this.itemAtlasList)
            {
                Manager.GameManager.Instance.ABResourceManager.Release(sa);
            }
        }

        public static ItemManager Instance { get { return instance; } }

        private static ItemManager instance;

        /// <summary>
        /// ��������
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
        }

        #endregion

        #region Load And Data
        /// <summary>
        /// �Ƿ��Ѽ���������
        /// </summary>
        public bool IsLoadOvered => ABJAProcessor != null && ABJAProcessor.IsLoaded;

        /// <summary>
        /// ���������Զ����
        /// </summary>
        private Dictionary<string, Type> ItemDict = new Dictionary<string, Type>();

        /// <summary>
        /// to-do : �����ʼ��
        /// ����Item���ݱ� => �ɼ��������������������Ӧ�е�����
        /// </summary>
        private Dictionary<string, ItemTableData> ItemTypeStrDict = new Dictionary<string, ItemTableData>();

        #region to-do : ���������������� Item ����
        public const string TypePath = "ML.Engine.InventorySystem.";
        public const string ItemIconLabel = "ItemTexture2D";
        public const string WorldObjLabel = "ML/InventorySystem/WorldItemPrefabs";



        public ML.Engine.ABResources.ABJsonAssetProcessor<ItemTableData[]> ABJAProcessor;

        public void LoadTableData()
        {
            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<ItemTableData[]>("OC/Json/TableData", "Item", (datas) =>
            {
                foreach (var data in datas)
                {
                    this.ItemTypeStrDict.Add(data.id, data);
                }
            }, "����ϵͳ��ƷItem������");
            ABJAProcessor.StartLoadJsonAssetData();
        }
        #endregion

        #endregion

        #region Spawn
        /// <summary>
        /// ���� id ����һ��ո�µ�Item������Ϊ1
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
                        Debug.LogError($"û�ж�ӦIDΪ {id} TypeΪ {itemRow.type} ��Item");
                        return null;
                    }
                    this.ItemDict.Add(id, type);
                }
                // to-do : ���÷��䣬���ܻ�����������
                Item item = System.Activator.CreateInstance(type, id, itemRow, 1) as Item;
                return item;
            }
            Debug.LogError("û�ж�ӦIDΪ " + id + " ��Item");
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
            // to-do : �ɲ��ö������ʽ
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

        private List<SpriteAtlas> itemAtlasList;
        public void LoadItemAtlas()
        {
            itemAtlasList = new List<SpriteAtlas>();
            Manager.GameManager.Instance.ABResourceManager.LoadAssetsAsync<SpriteAtlas>(ItemIconLabel, (asList) =>
            {
                lock(itemAtlasList)
                {
                    itemAtlasList.Add(asList);
                }
            });
        }
        

        #endregion
        
        #region Getter
        public string[] GetAllItemID()
        {
            return ItemTypeStrDict.Keys.ToArray();
        }

        public bool IsValidItemID(string id)
        {
            return this.ItemTypeStrDict.ContainsKey(id);
        }

        public Texture2D GetItemTexture2D(string id)
        {
            if (!this.ItemTypeStrDict.ContainsKey(id))
            {
                return null;
            }
            foreach(var sa in this.itemAtlasList)
            {
                var s = sa.GetSprite(this.ItemTypeStrDict[id].icon);
                if(s != null)
                {
                    return s.texture;
                }
            }
            return null;
        }

        public Sprite GetItemSprite(string id)
        {
            if (!this.ItemTypeStrDict.ContainsKey(id))
            {

                foreach (var sa in this.itemAtlasList)
                {
                    var s = sa.GetSprite(id);
                    if (s != null)
                    {
                        return s;
                    }
                }
                return null;
            }

        
            foreach (var sa in this.itemAtlasList)
            {
                var s = sa.GetSprite(this.ItemTypeStrDict[id].icon);
                if (s != null)
                {
                    return s;
                }
            }
            return null;
        }

        public async void AddItemIconObject(string itemID, Transform parent, Vector3 pos, Quaternion rot, Vector3 scale, bool isLocal = true)
        {
            ItemIcon itemIcon = parent.GetComponentInChildren<ItemIcon>();
            if (itemIcon == null)
            {
                // �첽������Դ
                var handle = Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(WorldObjLabel + "/ItemIcon.prefab", parent);

                // �ȴ��������
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
            itemIcon.Target = (Manager.GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController).currentCharacter.transform;
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
            //��ȡӦ�ó����õ������г���
            var asy = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < asy.Length; i++)
            {
                //�Ӷ�Ӧ�ĳ����и��������ַ����ҵĶ�Ӧ��ʵ������
                //��Ҫ��д���������ռ������ȫ������"System.Windows.Forms.Button"
                if (asy[i].GetType(fullName, false) != null)
                {
                    return asy[i].GetType(fullName, false);
                }
            }
            return null;
        }
        #endregion
    }
}

