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
            // TODO: ����������
            // ��ʼ��FeatureDict��FeatureTypeDict
        }

        public List<Feature> CreateFeatureForWorker()
        {
            // ����������2-4��֮��ȸ������
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
                    //����һ���������ԣ�ĳЩ���Ի��໥ì�ܣ����ǲ��ܳ�����ͬһ��������
                    int randomIndex = Random.Next(0, positiveFeature.Count);
                    result.Add(new Feature(positiveFeature[randomIndex]));
                    featureIDSets.Remove(result[0].ID);
                    featureIDSets.Remove(result[0].IDExclude);

                    //��ȡ����Ϊ���ó����Żء�
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
                // ���԰������桢���桢�޹��ܵ�˳�������������
                // TODO: ���Ե�ʱ����˳��
                result.Sort((f1, f2) => f1.Sort.CompareTo(f2.Sort));
            }
            return result;
        }
    }
}
