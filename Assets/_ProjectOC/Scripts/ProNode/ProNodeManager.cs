using System.Collections.Generic;
using UnityEngine;
using ML.Engine.InventorySystem;
using ProjectOC.WorkerNS;
using ML.Engine.TextContent;
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
        /// 实例化生成的生产节点，键为UID
        /// </summary>
        private Dictionary<string, WorldProNode> WorldProNodeDict = new Dictionary<string, WorldProNode>();

        /// <summary>
        /// 根据id创建新的生产节点
        /// </summary>
        public ProNode SpawnProNode(string id)
        {
            if (IsValidID(id))
            {
                return new ProNode(ProNodeTableDict[id]);
            }
            //Debug.LogError("没有对应ID为 " + id + " 的生产节点");
            return null;
        }

        public void WorldNodeSetData(WorldProNode worldNode, string nodeID)
        {
            if (worldNode != null && IsValidID(nodeID))
            {
                if (WorldProNodeDict.ContainsKey(worldNode.InstanceID))
                {
                    WorldProNodeDict[worldNode.InstanceID] = worldNode;
                }
                else
                {
                    WorldProNodeDict.Add(worldNode.InstanceID, worldNode);
                }
                ProNode node = SpawnProNode(nodeID);
                if (worldNode.ProNode != null)
                {
                    worldNode.ProNode.WorldProNode = null;
                }
                worldNode.ProNode = node;
                node.WorldProNode = worldNode;
            }
        }
        #endregion

        #region Getter
        public bool IsValidID(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                return ProNodeTableDict.ContainsKey(id);
            }
            return false;
        }
        public bool IsValidUID(string uid)
        {
            if (!string.IsNullOrEmpty(uid))
            {
                return WorldProNodeDict.ContainsKey(uid);
            }
            return false;
        }
        public WorldProNode GetWorldProNode(string uid)
        {
            if (IsValidUID(uid))
            {
                return this.WorldProNodeDict[uid];
            }
            return null;
        }

        public string GetName(string id)
        {
            if (IsValidID(id))
            {
                return ProNodeTableDict[id].Name;
            }
            return "";
        }

        public ProNodeType GetProNodeType(string id)
        {
            if (IsValidID(id))
            {
                return ProNodeTableDict[id].Type;
            }
            return ProNodeType.None;
        }

        public RecipeCategory GetCategory(string id)
        {
            if (IsValidID(id))
            {
                return ProNodeTableDict[id].Category;
            }
            return RecipeCategory.None;
        }

        public List<RecipeCategory> GetRecipeCategoryFilterd(string id)
        {
            if (IsValidID(id))
            {
                return ProNodeTableDict[id].RecipeCategoryFiltered;
            }
            return new List<RecipeCategory>();
        }

        public WorkType GetExpType(string id)
        {
            if (IsValidID(id))
            {
                return ProNodeTableDict[id].ExpType;
            }
            return WorkType.None;
        }

        public int GetStack(string id)
        {
            if (IsValidID(id))
            {
                return ProNodeTableDict[id].Stack;
            }
            return 0;
        }

        public int GetStackThreshold(string id)
        {
            if (IsValidID(id))
            {
                return ProNodeTableDict[id].StackThreshold;
            }
            return 0;
        }

        public int GetRawThreshold(string id)
        {
            if (IsValidID(id))
            {
                return ProNodeTableDict[id].RawThreshold;
            }
            return 0;
        }
        #endregion
    }
}


