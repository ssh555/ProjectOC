using ML.Engine.BuildingSystem.BuildingPart;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static ML.Engine.BuildingSystem.Test_BuildingManager;

namespace ML.Engine.InventorySystem
{
    /// <summary>
    /// û���� Manager Ϊ��׺�������ø������ط��ˣ�̫����
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
        /// ���������Զ����
        /// </summary>
        private Dictionary<int, Type> ItemDict = new Dictionary<int, Type>();

        /// <summary>
        /// to-do : �����ʼ��
        /// ����Item���ݱ� => �ɼ��������������������Ӧ�е�����
        /// </summary>
        private Dictionary<int, ItemTabelJsonData> ItemTypeStrDict = new Dictionary<int, ItemTabelJsonData>();

        /// <summary>
        /// ���� id ����һ��ո�µ�Item������Ϊ1
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Item SpawnItem(int id)
        {
            if (this.ItemTypeStrDict.TryGetValue(id, out ItemTabelJsonData itemRow))
            {
                Type type;
                if(!this.ItemDict.TryGetValue(id, out type))
                {
                    type = GetTypeByName(itemRow.type);
                    if (type == null)
                    {
                        Debug.LogError("û�ж�ӦIDΪ " + id + " ��Item");
                        return null;
                    }
                    this.ItemDict.Add(id, type);
                }
                // to-do : ���÷��䣬���ܻ�����������
                Item item = System.Activator.CreateInstance(type, id) as Item;
                item.Init(itemRow);
                return item;
            }
            Debug.LogError("û�ж�ӦIDΪ " + id + " ��Item");
            return null;
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

        public bool IsValidItemID(int id)
        {
            return this.ItemTypeStrDict.ContainsKey(id);
        }

        public Texture2D GetItemTexture2D(int id)
        {
            if (!this.ItemTypeStrDict.ContainsKey(id))
            {
                return null;
            }
            
            return Manager.GameManager.Instance.ABResourceManager.LoadLocalAB(Texture2DPath).LoadAsset<Texture2D>(this.ItemTypeStrDict[id].texture2d);
        }

        public Sprite GetItemSprite(int id)
        {
            var tex = this.GetItemTexture2D(id);
            if(tex == null)
            {
                return null;
            }
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        public GameObject GetItemObject(int id)
        {
            if (!this.ItemTypeStrDict.ContainsKey(id))
            {
                return null;
            }

            return Manager.GameManager.Instance.ABResourceManager.LoadLocalAB(WorldObjPath).LoadAsset<GameObject>(this.ItemTypeStrDict[id].worldobject);
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

#region to-do : ���������������� Item ����
        public const string TypePath = "ML.Engine.InventorySystem.";
        public const string Texture2DPath = "ui/Item/texture2d";
        public const string WorldObjPath = "prefabs/Item/WorldItem";

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

            foreach (var data in datas.table)
            {
                this.ItemTypeStrDict.Add(data.id, data);
            }

            abmgr.UnLoadLocalABAsync(ItemTableDataABPath, false, null);

            yield break;
        }

#endregion


    }
}

