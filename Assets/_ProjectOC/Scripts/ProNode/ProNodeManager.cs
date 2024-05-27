using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace ProjectOC.ProNodeNS
{
    [System.Serializable]
    public struct ProNodeTableData
    {
        public string ID;
        public ML.Engine.TextContent.TextContent Name;
        public ProNodeType Type;
        public ML.Engine.InventorySystem.RecipeCategory Category;
        public List<ML.Engine.InventorySystem.RecipeCategory> RecipeCategoryFiltered;
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
        [ShowInInspector]
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
                Config = new ProNodeConfig(handle.Result.Config);
            };
        }
        #endregion

        #region Spawn
        private Dictionary<string, IWorldProNode> WorldProNodeDict = new Dictionary<string, IWorldProNode>();
        public IProNode SpawnProNode(string id, IWorldProNode worldProNode)
        {
            if (IsValidID(id) && worldProNode != null)
            {
                if (worldProNode is AutoWorldProNode)
                {
                    return new AutoProNode(ProNodeTableDict[id]);
                }
                else if (worldProNode is ManualWorldProNode)
                {
                    return new ManualProNode(ProNodeTableDict[id]);
                }
                else if (worldProNode is BreedWoldProNode)
                {
                    return new BreedProNode(ProNodeTableDict[id]);
                }
                else if (worldProNode is CreatureWorldProNode)
                {
                    return new CreatureProNode(ProNodeTableDict[id]);
                }
                else if (worldProNode is MineWorldProNode)
                {
                    return new MineProNode(ProNodeTableDict[id]);
                }
            }
            return null;
        }

        public void WorldNodeSetData(IWorldProNode worldNode, string nodeID)
        {
            if (worldNode != null && IsValidID(nodeID))
            {
                WorldNodeSetData(worldNode, SpawnProNode(nodeID, worldNode));
            }
        }

        public void WorldNodeSetData(IWorldProNode worldNode, IProNode node)
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

        #region Get
        public bool IsValidID(string id)
        {
            return !string.IsNullOrEmpty(id) && ProNodeTableDict.ContainsKey(id);
        }
        public bool IsValidUID(string uid)
        {
            return !string.IsNullOrEmpty(uid) && WorldProNodeDict.ContainsKey(uid);
        }
        public IWorldProNode GetWorldProNode(string uid)
        {
            return IsValidUID(uid) ? WorldProNodeDict[uid] : null;
        }
        public string GetName(string id)
        {
            return IsValidID(id) ? ProNodeTableDict[id].Name : "";
        }
        public ProNodeType GetProNodeType(string id)
        {
            return IsValidID(id) ? ProNodeTableDict[id].Type : ProNodeType.None;
        }
        public ML.Engine.InventorySystem.RecipeCategory GetCategory(string id)
        {
            return IsValidID(id) ? ProNodeTableDict[id].Category : ML.Engine.InventorySystem.RecipeCategory.None;
        }
        public List<ML.Engine.InventorySystem.RecipeCategory> GetRecipeCategoryFilterd(string id)
        {
            return IsValidID(id) ? ProNodeTableDict[id].RecipeCategoryFiltered : new List<ML.Engine.InventorySystem.RecipeCategory>();
        }
        public WorkerNS.SkillType GetExpType(string id)
        {
            return IsValidID(id) ? ProNodeTableDict[id].ExpType : WorkerNS.SkillType.None;
        }
        public int GetMaxStack(string id)
        {
            return IsValidID(id) ? ProNodeTableDict[id].MaxStack : 0;
        }
        public int GetStackThreshold(string id)
        {
            return IsValidID(id) ? ProNodeTableDict[id].StackThreshold : 0;
        }
        public int GetRawThreshold(string id)
        {
            return IsValidID(id) ? ProNodeTableDict[id].RawThreshold : 0;
        }
        public bool GetCanCharge(string id)
        {
            return IsValidID(id) && ProNodeTableDict[id].CanCharge;
        }
        public bool GetIsAuto(string id)
        {
            return IsValidID(id) && ProNodeTableDict[id].Type == ProNodeType.Auto;
        }
        public UnityEngine.Color GetAPBarColor(int ap)
        {
            foreach (var config in Config.APBarColorConfigs)
            {
                if (config.Start <= ap && (config.End == 0 || ap <= config.End))
                {
                    return config.Color;
                }
            }
            return default(UnityEngine.Color);
        }
        public int GetExpRateIconNum(int eff)
        {
            foreach (var config in Config.ExpRateIconNumConfigs)
            {
                if (config.Start <= eff && (config.End == 0 || eff <= config.End))
                {
                    return config.Num;
                }
            }
            return 0;
        }
        #endregion
    }
}