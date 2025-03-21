using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public struct FeatureTableData
    {
        public string ID;
        public ML.Engine.TextContent.TextContent Name;
        public string UpgradeID;
        public string ReduceID;
        public string ReverseID;
        public int Sort;
        public string Icon;
        public FeatureType Type;
        public string Condition;
        public List<string> TrueEffect;
        public List<string> FalseEffect;
        public string Event;
        public ML.Engine.TextContent.TextContent ItemDescription;
        public ML.Engine.TextContent.TextContent EffectsDescription;
    }

    [System.Serializable]
    public sealed class FeatureManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        #region ILocalManager
        private Dictionary<FeatureType, List<string>> FeatureTypeDict = new Dictionary<FeatureType, List<string>>();
        private Dictionary<string, FeatureTableData> FeatureTableDict = new Dictionary<string, FeatureTableData>();
        private Dictionary<WorkerCategory, string> RaceDict = new Dictionary<WorkerCategory, string>();
        public ML.Engine.ABResources.ABJsonAssetProcessor<FeatureTableData[]> ABJAProcessor;
        public FeatureConfig Config;
        public void OnRegister()
        {
            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<FeatureTableData[]>("OCTableData", "Feature", (datas) =>
            {
                foreach (var data in datas)
                {
                    FeatureTableDict.Add(data.ID, data);
                    if (!FeatureTypeDict.ContainsKey(data.Type))
                    {
                        FeatureTypeDict.Add(data.Type, new List<string>());
                    }
                    FeatureTypeDict[data.Type].Add(data.ID);
                }
            }, "����Feature������");
            ABJAProcessor.StartLoadJsonAssetData();
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<FeatureConfigAsset>("Config_Feature").Completed += (handle) =>
            {
                Config = new FeatureConfig(handle.Result.Config);
                foreach (var tup in Config.CategoryFeatureList)
                {
                    RaceDict.Add(tup.category, tup.feature);
                }
            };
        }
        #endregion

        #region Spawn
        public List<Feature> CreateFeature(WorkerCategory category)
        {
            return CreateFeature(Config.FeatureMax, Config.FeatureOdds, category);
        }
        public List<Feature> CreateFeature(List<int> featureMax, List<int> featureOdds, WorkerCategory category)
        {
            if (featureMax == null) { featureMax = Config.FeatureMax; }
            if (featureOdds == null) { featureOdds = Config.FeatureOdds; }
            List<Feature> result = new List<Feature>();
            if (featureMax.Count != 4 || featureOdds.Count != 2) 
            { 
                Debug.Log($"Error featureMax.Count: {featureMax.Count} featureOdds.Count:{featureOdds.Count}"); 
                return result;
            }
            int maxFeatureNum = ML.Engine.MathNS.Math.GetRandomIndex(featureMax);
            if (RaceDict.ContainsKey(category))
            {
                result.Add(SpawnFeature(RaceDict[category]));
            }
            HashSet<string> buffs = FeatureTypeDict[FeatureType.Buff].ToHashSet();
            HashSet<string> debuffs = FeatureTypeDict[FeatureType.DeBuff].ToHashSet();
            string featureID;
            for (int i = 0; i < maxFeatureNum; i++)
            {
                if (buffs.Count == 0 && debuffs.Count == 0) { break; }
                int index = ML.Engine.MathNS.Math.GetRandomIndex(featureOdds);
                HashSet<string> sets;
                if (buffs.Count > 0 && debuffs.Count > 0)
                {
                    sets = index == 0 ? buffs : debuffs;
                }
                else
                {
                    sets = buffs.Count > 0 ? buffs : debuffs;
                }
                featureID = sets.ToList()[UnityEngine.Random.Range(0, sets.Count)];
                sets.Remove(featureID);
                result.Add(SpawnFeature(featureID));
            }
            result.Sort(new Feature());
            return result;
        }
        public Feature SpawnFeature(string id)
        {
            return IsValidID(id) ? new Feature(FeatureTableDict[id]) : default(Feature);
        }
        #endregion

        #region Get
        public bool IsValidID(string id)
        {
            return !string.IsNullOrEmpty(id) ? FeatureTableDict.ContainsKey(id) : false;
        }
        public int GetSort(string id)
        {
            return IsValidID(id) ? (int)FeatureTableDict[id].Type : int.MaxValue;
        }
        public string GetName(string id)
        {
            return IsValidID(id) ? FeatureTableDict[id].Name : "";
        }
        public FeatureType GetFeatureType(string id)
        {
            return IsValidID(id) ? FeatureTableDict[id].Type : FeatureType.None;
        }
        public string GetItemDescription(string id)
        {
            return IsValidID(id) ? FeatureTableDict[id].ItemDescription : "";
        }
        public string GetEffectsDescription(string id)
        {
            return IsValidID(id) ? FeatureTableDict[id].EffectsDescription : "";
        }
        public string GetUpgradeID(string id)
        {
            return IsValidID(id) ? FeatureTableDict[id].UpgradeID : "";
        }
        public string GetReduceID(string id)
        {
            return IsValidID(id) ? FeatureTableDict[id].ReduceID : "";
        }
        public string GetReverseID(string id)
        {
            return IsValidID(id) ? FeatureTableDict[id].ReverseID : "";
        }
        public string GetIcon(string id)
        {
            return IsValidID(id) ? FeatureTableDict[id].Icon : "";
        }
        public string GetCondition(string id)
        {
            return IsValidID(id) ? FeatureTableDict[id].Condition : "";
        }
        public List<string> GetTrueEffect(string id)
        {
            return IsValidID(id) ? FeatureTableDict[id].TrueEffect : new List<string>();
        }
        public List<string> GetFalseEffect(string id)
        {
            return IsValidID(id) ? FeatureTableDict[id].FalseEffect : new List<string>();
        }
        public string GetEvent(string id)
        {
            return IsValidID(id) ? FeatureTableDict[id].Event : "";
        }
        public bool GetCanCorrect(string id, FeatureCorrectType type)
        {
            bool flag = false;
            if (IsValidID(id))
            {
                switch (type)
                {
                    case FeatureCorrectType.Upgrade: 
                        flag = !string.IsNullOrEmpty(GetUpgradeID(id));
                        break;
                    case FeatureCorrectType.Downgrade:
                        flag = !string.IsNullOrEmpty(GetReduceID(id));
                        break;
                    case FeatureCorrectType.Reverse:
                        flag = !string.IsNullOrEmpty(GetReverseID(id));
                        break;
                    case FeatureCorrectType.Delete:
                        flag = true;
                        break;
                }
            }
            return flag;
        }
        public int GetFeatureCorrectTime(FeatureCorrectType type)
        {
            switch (type)
            {
                case FeatureCorrectType.Upgrade: 
                    return Config.FeatUpTime;
                case FeatureCorrectType.Downgrade : 
                    return Config.FeatDownTime;
                case FeatureCorrectType.Reverse:
                    return Config.FeatReverseTime;
                default:
                    return Config.FeatDelTime;
            }
        }
        public string GetFeatureCorrectItemID(FeatureCorrectType type)
        {
            switch (type)
            {
                case FeatureCorrectType.Upgrade:
                    return Config.FeatUpCostItemID;
                case FeatureCorrectType.Downgrade:
                    return Config.FeatDownCostItemID;
                case FeatureCorrectType.Reverse:
                    return Config.FeatReverseCostItemID;
                default:
                    return Config.FeatDelCostItemID;
            }
        }
        public int GetFeatureCorrectItemNum(FeatureCorrectType type)
        {
            switch (type)
            {
                case FeatureCorrectType.Upgrade:
                    return Config.FeatUpCostItemNum;
                case FeatureCorrectType.Downgrade:
                    return Config.FeatDownCostItemNum;
                case FeatureCorrectType.Reverse:
                    return Config.FeatReverseCostItemNum;
                default:
                    return Config.FeatDelCostItemNum;
            }
        }
        public Color GetColorForUI(string id)
        {
            if (IsValidID(id))
            {
                FeatureType type = GetFeatureType(id);
                if (type == FeatureType.Buff) { return Color.green; }
                else if (type == FeatureType.DeBuff) { return Color.red; }
                else if (type == FeatureType.Reverse)
                {
                    FeatureType reverseType = GetFeatureType(GetReverseID(id));
                    if (reverseType == FeatureType.Buff) { return new Color(0.5f, 0f, 0.5f); }
                    if (reverseType == FeatureType.DeBuff) { return new Color(1.0f, 0.8431f, 0.0f); }
                }
            }
            return Color.green;
        }
        public string GetColorStrForUI(string id)
        {
            if (IsValidID(id))
            {
                FeatureType type = GetFeatureType(id);
                if (type == FeatureType.Buff) { return "green"; }
                else if (type == FeatureType.DeBuff) { return "red"; }
                else if (type == FeatureType.Reverse)
                {
                    FeatureType reverseType = GetFeatureType(GetReverseID(id));
                    if (reverseType == FeatureType.Buff) { return "#800080"; }
                    if (reverseType == FeatureType.DeBuff) { return "#FFD700"; }
                }
            }
            return "green";
        }
        #endregion
    }
}