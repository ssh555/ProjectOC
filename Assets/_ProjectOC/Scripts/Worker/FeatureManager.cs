using ML.Engine.Manager;
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
        public List<string> Effects;
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

        public const string Texture2DPath = "ui/Feature/texture2d";

        public static ML.Engine.ABResources.ABJsonAssetProcessor<FeatureTableData[]> ABJAProcessor;

        public void LoadTableData()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<FeatureTableData[]>("Json/TableData", "FeatureTableData", (datas) =>
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
            if (maxFeatureNum > 0 && maxFeatureNum < FeatureTableDict.Count)
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
            if (FeatureTableDict.TryGetValue(id, out FeatureTableData row))
            {
                Feature feature = new Feature(row);
                return feature;
            }
            Debug.LogError("没有对应ID为 " + id + " 的Feature");
            return null;
        }
        #endregion

        #region Getter
        public string[] GetAllFeatureID()
        {
            return FeatureTableDict.Keys.ToArray();
        }

        public bool IsValidID(string id)
        {
            return FeatureTableDict.ContainsKey(id);
        }

        public string GetIDExclude(string id)
        {
            if (!FeatureTableDict.ContainsKey(id))
            {
                return "";
            }
            return FeatureTableDict[id].IDExclude;
        }

        public int GetSort(string id)
        {
            if (!FeatureTableDict.ContainsKey(id))
            {
                return int.MaxValue;
            }
            return (int)FeatureTableDict[id].Type;
        }

        public string GetName(string id)
        {
            if (!FeatureTableDict.ContainsKey(id))
            {
                return "";
            }
            return FeatureTableDict[id].Name;
        }

        public Texture2D GetTexture2D(string id)
        {
            if (!FeatureTableDict.ContainsKey(id))
            {
                return null;
            }

            return GameManager.Instance.ABResourceManager.LoadLocalAB(Texture2DPath).LoadAsset<Texture2D>(FeatureTableDict[id].Icon);
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

        public FeatureType GetFeatureType(string id)
        {
            if (!FeatureTableDict.ContainsKey(id))
            {
                return FeatureType.None;
            }
            return FeatureTableDict[id].Type;
        }

        public string GetItemDescription(string id)
        {
            if (!FeatureTableDict.ContainsKey(id))
            {
                return "";
            }
            return FeatureTableDict[id].ItemDescription;
        }

        public string GetEffectsDescription(string id)
        {
            if (!FeatureTableDict.ContainsKey(id))
            {
                return "";
            }
            return FeatureTableDict[id].EffectsDescription;
        }
        #endregion        
    }
}
