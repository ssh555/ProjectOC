using System.Collections.Generic;
using ML.Engine.InventorySystem;
using ML.Engine.TextContent;
using Sirenix.OdinInspector;

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
        public WorkerNS.SkillType ExpType;
        public int MaxStack;
        public int StackThreshold;
        public int RawThreshold;
        public bool CanCharge;
    }

    [LabelText("生产节点管理器"), System.Serializable]
    public sealed class ProNodeManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        #region ILocalManager
        private Dictionary<string, ProNodeTableData> ProNodeTableDict = new Dictionary<string, ProNodeTableData>();
        public ML.Engine.ABResources.ABJsonAssetProcessor<ProNodeTableData[]> ABJAProcessor;
        public ProNodeConfig Config;
        public void OnRegister()
        {
            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<ProNodeTableData[]>("OCTableData", "ProNode", (datas) =>
            {
                foreach (var data in datas)
                {
                    ProNodeTableDict.Add(data.ID, data);
                }
            }, "生产节点表数据");
            ABJAProcessor.StartLoadJsonAssetData();
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<ProNodeConfigAsset>("Config_ProNode").Completed += (handle) =>
            {
                ProNodeConfigAsset data = handle.Result;
                Config = new ProNodeConfig(data.Config);
            };
        }
        #endregion

        #region Spawn
        private Dictionary<string, WorldProNode> WorldProNodeDict = new Dictionary<string, WorldProNode>();
        public ProNode SpawnProNode(string id)
        {
            if (IsValidID(id))
            {
                return new ProNode(ProNodeTableDict[id]);
            }
            return null;
        }

        public void WorldNodeSetData(WorldProNode worldNode, string nodeID)
        {
            if (worldNode != null && IsValidID(nodeID))
            {
                ProNode node = SpawnProNode(nodeID);
                WorldNodeSetData(worldNode, node);
            }
        }

        public void WorldNodeSetData(WorldProNode worldNode, ProNode node)
        {
            if (worldNode != null && node != null)
            {
                if (WorldProNodeDict.ContainsKey(worldNode.InstanceID))
                {
                    WorldProNodeDict[worldNode.InstanceID] = worldNode;
                }
                else
                {
                    WorldProNodeDict.Add(worldNode.InstanceID, worldNode);
                }
                if (worldNode.ProNode != node)
                {
                    if (worldNode.ProNode != null)
                    {
                        worldNode.ProNode.Destroy();
                        worldNode.ProNode.WorldProNode = null;
                    }
                    if (node.WorldProNode != null)
                    {
                        node.WorldProNode.ProNode = null;
                    }
                    worldNode.ProNode = node;
                    node.WorldProNode = worldNode;
                }
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

        public WorkerNS.SkillType GetExpType(string id)
        {
            if (IsValidID(id))
            {
                return ProNodeTableDict[id].ExpType;
            }
            return WorkerNS.SkillType.None;
        }

        public int GetMaxStack(string id)
        {
            if (IsValidID(id))
            {
                return ProNodeTableDict[id].MaxStack;
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
        public bool GetCanCharge(string id)
        {
            if (IsValidID(id))
            {
                return ProNodeTableDict[id].CanCharge;
            }
            return false;
        }
        #endregion
    }
}


