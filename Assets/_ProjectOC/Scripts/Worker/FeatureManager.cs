using ML.Engine.TextContent;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public struct FeatureTableData
    {
        public string ID;
        public string IDExclude;
        public int Sort;
        public TextContent Name;
        public string Icon;
        public FeatureType Type;
        public List<Tuple<string, string>> Effects;
        public TextContent ItemDescription;
        public TextContent EffectsDescription;
    }

    [System.Serializable]
    public sealed class FeatureManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        #region Load And Data
        /// <summary>
        /// 是否已加载完数据
        /// </summary>
        public bool IsLoadOvered => ABJAProcessor != null && ABJAProcessor.IsLoaded;

        private System.Random Random = new System.Random();

        private Dictionary<FeatureType, List<string>> FeatureTypeDict = new Dictionary<FeatureType, List<string>>();
        
        /// <summary>
        /// Feature数据表
        /// </summary>
        private Dictionary<string, FeatureTableData> FeatureTableDict = new Dictionary<string, FeatureTableData>();

        public static ML.Engine.ABResources.ABJsonAssetProcessor<FeatureTableData[]> ABJAProcessor;

        public void LoadTableData()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<FeatureTableData[]>("Binary/TableData", "Feature", (datas) =>
                {
                    foreach (var data in datas)
                    {
                        this.FeatureTableDict.Add(data.ID, data);

                        if (!FeatureTypeDict.ContainsKey(data.Type))
                        {
                            this.FeatureTypeDict.Add(data.Type, new List<string>());
                        }
                        this.FeatureTypeDict[data.Type].Add(data.ID);
                    }
                }, null, "隐兽Feature表数据");
                ABJAProcessor.StartLoadJsonAssetData();
            }
        }
        #endregion

        #region Spawn
        public List<Feature> CreateFeature()
        {
            // 特性上限在2-4个之间等概率随机
            int maxFeatureNum = this.Random.Next(2, 5);
            return CreateFeature(maxFeatureNum);
        }
        public List<Feature> CreateFeature(int maxFeatureNum)
        {
            List<Feature> result = new List<Feature>();
            if (0 < maxFeatureNum && maxFeatureNum < FeatureTableDict.Count)
            {
                List<string> positiveFeature = FeatureTypeDict[FeatureType.Buff];
                if (positiveFeature.Count > 0)
                {
                    HashSet<string> featureIDSets = FeatureTableDict.Keys.ToHashSet();
                    //至少一个增益特性，某些特性会相互矛盾，他们不能出现在同一刁民身上
                    int randomIndex = Random.Next(0, positiveFeature.Count);
                    result.Add(this.SpawnFeature(positiveFeature[randomIndex]));
                    featureIDSets.Remove(result[0].ID);
                    featureIDSets.Remove(result[0].IDExclude);

                    //抽取规则为“拿出不放回”
                    int curSampleNum = 1;
                    while (curSampleNum <= maxFeatureNum && featureIDSets.Count > 0)
                    {
                        randomIndex = Random.Next(0, featureIDSets.Count);
                        result.Add(this.SpawnFeature(featureIDSets.ElementAt(randomIndex)));
                        featureIDSets.Remove(result[curSampleNum].ID);
                        featureIDSets.Remove(result[curSampleNum].IDExclude);
                        curSampleNum += 1;
                    }
                }
                result.Sort(new Feature.FeatureSort());
            }
            return result;
        }
        public Feature SpawnFeature(string id)
        {
            if (IsValidID(id))
            {
                return new Feature(FeatureTableDict[id]);
            }
            //Debug.LogError("没有对应ID为 " + id + " 的Feature");
            return null;
        }
        #endregion

        #region Getter
        public string[] GetAllID()
        {
            return FeatureTableDict.Keys.ToArray();
        }

        public bool IsValidID(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                return FeatureTableDict.ContainsKey(id);
            }
            return false;
        }

        public string GetIDExclude(string id)
        {
            if (IsValidID(id))
            {
                return FeatureTableDict[id].IDExclude;
            }
            return "";
        }

        public int GetSort(string id)
        {
            if (IsValidID(id))
            {
                return (int)FeatureTableDict[id].Type;
            }
            return int.MaxValue;
        }

        public string GetName(string id)
        {
            if (IsValidID(id))
            {
                return FeatureTableDict[id].Name;
            }
            return "";
        }
        public FeatureType GetFeatureType(string id)
        {
            if (IsValidID(id))
            {
                return FeatureTableDict[id].Type;
            }
            return FeatureType.None;
        }

        public string GetItemDescription(string id)
        {
            if (IsValidID(id))
            {
                return FeatureTableDict[id].ItemDescription;
            }
            return "";
        }

        public string GetEffectsDescription(string id)
        {
            if (IsValidID(id))
            {
                return FeatureTableDict[id].EffectsDescription;
            }
            return "";
        }
        #endregion        
    }
}
