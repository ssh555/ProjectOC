using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ML.Engine.TextContent;
using Unity.VisualScripting;
using UnityEngine.U2D;

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

    /// <summary>
    /// û���� Manager Ϊ��׺�������ø������ط��ˣ�̫����
    /// </summary>
    public sealed class ItemManager : Manager.GlobalManager.IGlobalManager
    {
        #region Instance
        private ItemManager() { }

        private static ItemManager instance;

        public static ItemManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ItemManager();
                    Manager.GameManager.Instance.RegisterGlobalManager(instance);
                }
                return instance;
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
        public const string Texture2DPath = "ui/Item/texture2d";
        public const string WorldObjPath = "prefabs/Item/WorldItem";



        public static ML.Engine.ABResources.ABJsonAssetProcessor<ItemTableData[]> ABJAProcessor;

        public void LoadTableData()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<ItemTableData[]>("Json/TableData", "Item", (datas) =>
                {
                    foreach (var data in datas)
                    {
                        this.ItemTypeStrDict.Add(data.id, data);
                    }
                }, null, "����ϵͳ��ƷItem������");
                ABJAProcessor.StartLoadJsonAssetData();
            }
            
            if (itemAtlas == null)
            {
                AssetBundle ab = null;
                AssetBundleCreateRequest request = null;
                request = Manager.GameManager.Instance.ABResourceManager
                .LoadLocalABAsync(Texture2DPath, (asop) =>
                {
                    if (request != null)
                    {
                        ab = request.assetBundle;
                    }
                    AssetBundleRequest request2 = ab.LoadAssetAsync<SpriteAtlas>("SA_Item_UI");
                    if(request2 != null)
                    {
                        itemAtlas = request2.asset as SpriteAtlas;
                    }
                },out ab);
            }
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


        public WorldItem SpawnWorldItem(Item item, Vector3 pos, Quaternion rot)
        {
            
            if (item == null)
            {
                return null;
            }
            // to-do : �ɲ��ö������ʽ
            GameObject obj = GameObject.Instantiate(Manager.GameManager.Instance.ABResourceManager.LoadLocalAB(WorldObjPath).LoadAsset<GameObject>(this.ItemTypeStrDict[item.ID].worldobject), pos, rot);
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
            return Manager.GameManager.Instance.ABResourceManager.LoadLocalAB(Texture2DPath).LoadAsset<Texture2D>(this.ItemTypeStrDict[id].icon);
        }

        public Sprite GetItemSprite(string id)
        {
             if (!this.ItemTypeStrDict.ContainsKey(id))
             {
                 return null;
             }
            return itemAtlas.GetSprite(this.ItemTypeStrDict[id].icon);
        }

        public void AddItemIconObject(string itemID, Transform parent, Vector3 pos, Quaternion rot, Vector3 scale, bool isLocal = true)
        {
            if (parent.GetComponentInChildren<SpriteRenderer>() == null)
            {
                GameObject obj = GameObject.Instantiate(Manager.GameManager.Instance.ABResourceManager.LoadLocalAB(WorldObjPath).LoadAsset<GameObject>("ItemIcon"), parent);
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
            }
            SpriteRenderer spriteRenderer = parent.GetComponentInChildren<SpriteRenderer>();
            spriteRenderer.sprite = GetItemSprite(itemID);
        }

        public GameObject GetItemObject(string id)
        {
            if (!this.ItemTypeStrDict.ContainsKey(id))
            {
                return null;
            }

            return Manager.GameManager.Instance.ABResourceManager.LoadLocalAB(WorldObjPath).LoadAsset<GameObject>(this.ItemTypeStrDict[id].worldobject);
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

