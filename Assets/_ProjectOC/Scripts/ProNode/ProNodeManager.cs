using System.Collections.Generic;
using UnityEngine;
using ML.Engine.InventorySystem;
using ProjectOC.WorkerNS;
using ML.Engine.TextContent;
using System.Linq;
using System;

namespace ProjectOC.ProNodeNS
{
    [System.Serializable]
    public struct ProNodeTableData
    {
        public string ID;
        public TextContent Name;
        public ProNodeType Type;
        public RecipeCategory Category;
        public List<RecipeCategory> RecipeCategoryFiltered;
        public WorkType ExpType;
        public int Stack;
        public int StackThreshold;
        public int RawThreshold;
    }

    /// <summary>
    /// 生产节点管理器
    /// </summary>
    public sealed class ProNodeManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        #region Load And Data
        /// <summary>
        /// 是否已加载完数据
        /// </summary>
        public bool IsLoadOvered => ABJAProcessor != null && ABJAProcessor.IsLoaded;

        /// <summary>
        /// 基础ProNode数据表
        /// </summary>
        private Dictionary<string, ProNodeTableData> ProNodeTableDict = new Dictionary<string, ProNodeTableData>();

        public static ML.Engine.ABResources.ABJsonAssetProcessor<ProNodeTableData[]> ABJAProcessor;

        public void LoadTableData()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<ProNodeTableData[]>("Binary/TableData", "ProNode", (datas) =>
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
            if (ProNodeTableDict.TryGetValue(id, out ProNodeTableData row))
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

        public void WorldNodeSetData(WorldProNode worldNode, string nodeID)
        {
            ProNode node = SpawnProNode(nodeID);
            if (node != null)
            {
                if (worldNode.ProNode != null)
                {
                    worldNode.ProNode.WorldProNode = null;
                }
                worldNode.ProNode = node;
                node.WorldProNode = worldNode;
                if (WorldProNodeDict.ContainsKey(worldNode.InstanceID))
                {
                    WorldProNodeDict[worldNode.InstanceID] =  worldNode;
                }
                else
                {
                    WorldProNodeDict.Add(worldNode.InstanceID, worldNode);
                }
            }
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
            return ProNodeTableDict[id].RecipeCategoryFiltered;
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
        #endregion
    }
}


