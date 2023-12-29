using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ProjectOC.WorkerNS;
using static ML.Engine.InventorySystem.CompositeSystem.CompositeSystem;

namespace ProjectOC.ProductionNodeNS
{
    /// <summary>
    /// 生产节点管理器
    /// </summary>
    public sealed class ProductionNodeManager : ML.Engine.Manager.GlobalManager.IGlobalManager
    {
        #region Instance
        private ProductionNodeManager() { }

        private static ProductionNodeManager instance;

        public static ProductionNodeManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ProductionNodeManager();
                    GameManager.Instance.RegisterGlobalManager(instance);
                    instance.LoadTableData();
                }
                return instance;
            }
        }
        #endregion


        /// <summary>
        /// 生成的生产节点，键为ID
        /// </summary>
        private Dictionary<string, List<ProductionNode>> ProductionNodeDict = new Dictionary<string, List<ProductionNode>>();
        /// <summary>
        /// 实例化生成的生产节点，键为UID
        /// </summary>
        private Dictionary<string, WorldProductionNode> WorldProductionNodeDict = new Dictionary<string, WorldProductionNode>();
        /// <summary>
        /// 基础ProductionNode数据表
        /// </summary>
        private Dictionary<string, ProductionNodeTableJsonData> ProductionNodeTableDict = new Dictionary<string, ProductionNodeTableJsonData>();

        /// <summary>
        /// 是否是有效的生产节点ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsValidID(string id)
        {
            return this.ProductionNodeTableDict.ContainsKey(id);
        }
        /// <summary>
        /// 是否是有效的生产节点UID
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public bool IsValidUID(string uid)
        {
            return WorldProductionNodeDict.ContainsKey(uid);
        }
        public List<ProductionNode> GetProductionNode(string id)
        {
            if (this.ProductionNodeDict.ContainsKey(id))
            {
                return this.ProductionNodeDict[id];
            }
            return new List<ProductionNode>();
        }
        public WorldProductionNode GetWorldProductionNode(string uid)
        {
            if (this.WorldProductionNodeDict.ContainsKey(uid))
            {
                return this.WorldProductionNodeDict[uid];
            }
            return null;
        }

        /// <summary>
        /// 根据id创建新的生产节点
        /// </summary>
        /// <param name="id">生产节点id</param>
        /// <returns></returns>
        public ProductionNode SpawnProductionNode(string id)
        {
            if (ProductionNodeTableDict.ContainsKey(id))
            {
                ProductionNodeTableJsonData row = this.ProductionNodeTableDict[id];
                ProductionNode node = new ProductionNode();
                node.Init(row);
                if (!ProductionNodeDict.ContainsKey(node.ID))
                {
                    ProductionNodeDict.Add(node.ID, new List<ProductionNode>());
                }
                ProductionNodeDict[id].Add(node);
                return node;
            }
            Debug.LogError("没有对应ID为 " + id + " 的生产节点");
            return null;
        }
        public WorldProductionNode SpawnWorldProductionNode(ProductionNode node, Vector3 pos, Quaternion rot)
        {
            if (node == null)
            {
                return null;
            }
            // to-do : 可采用对象池形式
            GameObject obj = GameObject.Instantiate(GameManager.Instance.ABResourceManager.LoadLocalAB(WorldObjPath).LoadAsset<GameObject>(this.ProductionNodeTableDict[node.ID].worldobject), pos, rot);
            WorldProductionNode worldNode = obj.GetComponent<WorldProductionNode>();
            if (worldNode == null)
            {
                worldNode = obj.AddComponent<WorldProductionNode>();
            }
            worldNode.SetProductionNode(node);
            WorldProductionNodeDict.Add(node.UID, worldNode);
            return worldNode;
        }
        public static WorldProductionNode SpawnWorldProductionNodeInEditor(GameObject obj, ProductionNode node, Vector3 pos, Quaternion rot)
        {
#if !UNITY_EDITOR
            return null;
#else
            if (node == null || UnityEditor.EditorApplication.isPlaying)
            {
                return null;
            }

            obj = UnityEditor.PrefabUtility.InstantiatePrefab(obj) as GameObject;
            obj.transform.position = pos;
            obj.transform.rotation = rot;

            WorldProductionNode worldNode = obj.GetComponent<WorldProductionNode>();
            if (worldNode == null)
            {
                worldNode = obj.AddComponent<WorldProductionNode>();
            }
            worldNode.SetProductionNode(node);
            Instance.WorldProductionNodeDict.Add(node.UID, worldNode);
            return worldNode;
#endif
        }

        public Texture2D GetTexture2D(string id)
        {
            if (!this.ProductionNodeTableDict.ContainsKey(id))
            {
                return null;
            }

            return GameManager.Instance.ABResourceManager.LoadLocalAB(Texture2DPath).LoadAsset<Texture2D>(this.ProductionNodeTableDict[id].texture2d);
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
            if (!this.ProductionNodeTableDict.ContainsKey(id))
            {
                return null;
            }

            return GameManager.Instance.ABResourceManager.LoadLocalAB(WorldObjPath).LoadAsset<GameObject>(this.ProductionNodeTableDict[id].worldobject);
        }

        #region to-do : 需读表导入所有所需的 ProductionNode 数据
        public const string Texture2DPath = "ui/ProductionNode/texture2d";
        public const string WorldObjPath = "prefabs/ProductionNode/WorldProductionNode";

        public const string TableDataABPath = "Json/TableData";
        public const string TableName = "ProductionNodeTableData";

        [System.Serializable]
        public struct ProductionNodeTableJsonData
        {
            public string id;
            public string name;
            public ProductionNodeType type;
            public ProductionNodeCategory category;
            public List<ItemCategory> recipeCategoryFiltered;
            public WorkType expType;
            public List<Dictionary<string, int>> levelUpgradeRequire;
            public int stackNumMax;
            public int stackCarryThreshold;
            public int needQuantityThreshold;
            public string texture2d;
            public string worldobject;
        }

        public static ML.Engine.ABResources.ABJsonAssetProcessor<ProductionNodeTableJsonData[]> ABJAProcessor;

        public void LoadTableData()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<ProductionNodeTableJsonData[]>("Json/TableData", "CompositionTableData", (datas) =>
                {
                    foreach (var data in datas)
                    {
                        this.ProductionNodeTableDict.Add(data.id, data);
                    }
                }, null, "生产节点表数据");
            }
            ABJAProcessor.StartLoadJsonAssetData();
        }


        #endregion
    }
}


