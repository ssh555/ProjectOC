using System.Collections.Generic;
using UnityEngine;
using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ProjectOC.WorkerNS;
using ML.Engine.TextContent;
using System.Linq;

namespace ProjectOC.ProNodeNS
{
    /// <summary>
    /// 生产节点管理器
    /// </summary>
    public sealed class ProNodeManager : ML.Engine.Manager.GlobalManager.IGlobalManager
    {
        #region Instance
        private ProNodeManager() { }

        private static ProNodeManager instance;

        public static ProNodeManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ProNodeManager();
                    GameManager.Instance.RegisterGlobalManager(instance);
                    instance.LoadTableData();
                }
                return instance;
            }
        }
        #endregion

        #region Load And Data
        /// <summary>
        /// 是否已加载完数据
        /// </summary>
        public bool IsLoadOvered => ABJAProcessor != null && ABJAProcessor.IsLoaded;

        /// <summary>
        /// 基础ProNode数据表
        /// </summary>
        private Dictionary<string, ProNodeTableJsonData> ProNodeTableDict = new Dictionary<string, ProNodeTableJsonData>();

        public const string Texture2DPath = "ui/ProNode/texture2d";
        public const string WorldObjPath = "prefabs/ProNode/WorldProNode";

        [System.Serializable]
        public struct ProNodeTableJsonData
        {
            public string ID;
            public TextContent Name;
            public ProNodeType Type;
            public RecipeCategory Category;
            public List<RecipeCategory> RecipeCategoryFilterd;
            public WorkType ExpType;
            public int Stack;
            public int StackThreshold;
            public int RawThreshold;
            public Dictionary<string, int> Lv1Required;
            public Dictionary<string, int> Lv2Required;

            public string texture2d;
            public string worldobject;
        }

        public static ML.Engine.ABResources.ABJsonAssetProcessor<ProNodeTableJsonData[]> ABJAProcessor;

        public void LoadTableData()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<ProNodeTableJsonData[]>("Json/TableData", "ProNodesTableData", (datas) =>
                {
                    foreach (var data in datas)
                    {
                        this.ProNodeTableDict.Add(data.ID, data);
                    }
                }, null, "生产节点表数据");
                ABJAProcessor.StartLoadJsonAssetData();
            }
        }
        #endregion

        #region Spawn
        /// <summary>
        /// 生成的生产节点，键为ID
        /// </summary>
        private Dictionary<string, List<ProNode>> ProNodeDict = new Dictionary<string, List<ProNode>>();
        /// <summary>
        /// 实例化生成的生产节点，键为UID
        /// </summary>
        private Dictionary<string, WorldProNode> WorldProNodeDict = new Dictionary<string, WorldProNode>();

        /// <summary>
        /// 根据id创建新的生产节点
        /// </summary>
        public ProNode SpawnProNode(string id)
        {
            if (ProNodeTableDict.TryGetValue(id, out ProNodeTableJsonData row))
            {
                ProNode node = new ProNode(row);
                if (!ProNodeDict.ContainsKey(node.ID))
                {
                    ProNodeDict.Add(node.ID, new List<ProNode>());
                }
                ProNodeDict[id].Add(node);
                return node;
            }
            Debug.LogError("没有对应ID为 " + id + " 的生产节点");
            return null;
        }
        public WorldProNode SpawnWorldProNode(ProNode node, Vector3 pos, Quaternion rot)
        {
            if (node == null)
            {
                return null;
            }
            // to-do : 可采用对象池形式
            GameObject obj = GameObject.Instantiate(GameManager.Instance.ABResourceManager.LoadLocalAB(WorldObjPath).LoadAsset<GameObject>(this.ProNodeTableDict[node.ID].worldobject), pos, rot);
            WorldProNode worldNode = obj.GetComponent<WorldProNode>();
            if (worldNode == null)
            {
                worldNode = obj.AddComponent<WorldProNode>();
            }
            worldNode.SetProNode(node);
            WorldProNodeDict.Add(node.UID, worldNode);
            return worldNode;
        }
        #endregion

        #region Getter
        public string[] GetAllProNodeID()
        {
            return ProNodeTableDict.Keys.ToArray();
        }

        public bool IsValidID(string id)
        {
            return ProNodeTableDict.ContainsKey(id);
        }

        public bool IsValidUID(string uid)
        {
            return WorldProNodeDict.ContainsKey(uid);
        }

        public List<ProNode> GetProNode(string id)
        {
            if (this.ProNodeDict.ContainsKey(id))
            {
                return this.ProNodeDict[id];
            }
            return new List<ProNode>();
        }

        public WorldProNode GetWorldProNode(string uid)
        {
            if (this.WorldProNodeDict.ContainsKey(uid))
            {
                return this.WorldProNodeDict[uid];
            }
            return null;
        }

        public Texture2D GetTexture2D(string id)
        {
            if (!this.ProNodeTableDict.ContainsKey(id))
            {
                return null;
            }

            return GameManager.Instance.ABResourceManager.LoadLocalAB(Texture2DPath).LoadAsset<Texture2D>(this.ProNodeTableDict[id].texture2d);
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
            if (!this.ProNodeTableDict.ContainsKey(id))
            {
                return null;
            }

            return GameManager.Instance.ABResourceManager.LoadLocalAB(WorldObjPath).LoadAsset<GameObject>(this.ProNodeTableDict[id].worldobject);
        }

        public string GetName(string id)
        {
            if (!ProNodeTableDict.ContainsKey(id))
            {
                return "";
            }
            return ProNodeTableDict[id].Name;
        }

        public ProNodeType GetProNodeType(string id)
        {
            if (!ProNodeTableDict.ContainsKey(id))
            {
                return ProNodeType.None;
            }
            return ProNodeTableDict[id].Type;
        }

        public RecipeCategory GetCategory(string id)
        {
            if (!ProNodeTableDict.ContainsKey(id))
            {
                return RecipeCategory.None;
            }
            return ProNodeTableDict[id].Category;
        }

        public List<RecipeCategory> GetRecipeCategoryFilterd(string id)
        {
            if (!ProNodeTableDict.ContainsKey(id))
            {
                return new List<RecipeCategory>();
            }
            return ProNodeTableDict[id].RecipeCategoryFilterd;
        }

        public WorkType GetExpType(string id)
        {
            if (!ProNodeTableDict.ContainsKey(id))
            {
                return WorkType.None;
            }
            return ProNodeTableDict[id].ExpType;
        }

        public int GetStack(string id)
        {
            if (!ProNodeTableDict.ContainsKey(id))
            {
                return 0;
            }
            return ProNodeTableDict[id].Stack;
        }

        public int GetStackThreshold(string id)
        {
            if (!ProNodeTableDict.ContainsKey(id))
            {
                return 0;
            }
            return ProNodeTableDict[id].StackThreshold;
        }

        public int GetRawThreshold(string id)
        {
            if (!ProNodeTableDict.ContainsKey(id))
            {
                return 0;
            }
            return ProNodeTableDict[id].RawThreshold;
        }

        public Dictionary<string, int> GetLv1Required(string id)
        {
            if (!ProNodeTableDict.ContainsKey(id))
            {
                return new Dictionary<string, int>();
            }
            return ProNodeTableDict[id].Lv1Required;
        }

        public Dictionary<string, int> GetLv2Required(string id)
        {
            if (!ProNodeTableDict.ContainsKey(id))
            {
                return new Dictionary<string, int>();
            }
            return ProNodeTableDict[id].Lv2Required;
        }
        #endregion
    }
}


