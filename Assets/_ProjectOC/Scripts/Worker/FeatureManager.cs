using ML.Engine.Manager.LocalManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public sealed class FeatureManager : ILocalManager
    {
        private System.Random Random = new System.Random();
        private Dictionary<string, FeatureType> FeatureDict = new Dictionary<string, FeatureType>();
        private Dictionary<FeatureType, List<string>> FeatureTypeDict = new Dictionary<FeatureType, List<string>>();

        public FeatureManager() 
        {
            FeatureTypeDict.Add(FeatureType.Buff, new List<string>());
            FeatureTypeDict.Add(FeatureType.Debuff, new List<string>());
            FeatureTypeDict.Add(FeatureType.None, new List<string>());
            // TODO: 读表拿数据
            // 初始化FeatureDict和FeatureTypeDict
        }

        public List<Feature> CreateFeatureForWorker()
        {
            // 特性上限在2-4个之间等概率随机
            int maxFeatureNum = this.Random.Next(2, 5);
            return CreateFeatureForWorker(maxFeatureNum);
        }

        public List<Feature> CreateFeatureForWorker(int maxFeatureNum)
        {
            List<Feature> result = new List<Feature>();
            if (maxFeatureNum > 0 && maxFeatureNum < FeatureDict.Count)
            {
                List<string> positiveFeature = FeatureTypeDict[FeatureType.Buff];
                if (positiveFeature.Count > 0)
                {
                    HashSet<string> featureIDSets = FeatureDict.Keys.ToHashSet();
                    //至少一个增益特性，某些特性会相互矛盾，他们不能出现在同一刁民身上
                    int randomIndex = Random.Next(0, positiveFeature.Count);
                    result.Add(new Feature(positiveFeature[randomIndex]));
                    featureIDSets.Remove(result[0].ID);
                    featureIDSets.Remove(result[0].IDExclude);

                    //抽取规则为“拿出不放回”
                    int curSampleNum = 1;
                    while (curSampleNum <= maxFeatureNum && featureIDSets.Count > 0)
                    {
                        randomIndex = Random.Next(0, featureIDSets.Count);
                        result.Add(new Feature(featureIDSets.ElementAt(randomIndex)));
                        featureIDSets.Remove(result[curSampleNum].ID);
                        featureIDSets.Remove(result[curSampleNum].IDExclude);
                        curSampleNum += 1;
                    }
                }
                // 特性按照增益、减益、无功能的顺序从上至下排序
                // TODO: 测试的时候检查顺序
                result.Sort((f1, f2) => f1.Sort.CompareTo(f2.Sort));
            }
            return result;
        }
    }
}
