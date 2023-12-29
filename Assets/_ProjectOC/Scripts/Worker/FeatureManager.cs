using ML.Engine.Manager;
using ML.Engine.TextContent;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ProjectOC.WorkerNS.SkillManager;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public sealed class FeatureManager : ML.Engine.Manager.GlobalManager.IGlobalManager
    {
        #region Instance
        private FeatureManager()
        {
            FeatureTypeDict.Add(FeatureType.Buff, new List<string>());
            FeatureTypeDict.Add(FeatureType.Debuff, new List<string>());
            FeatureTypeDict.Add(FeatureType.None, new List<string>());
        }

        private static FeatureManager instance;

        public static FeatureManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FeatureManager();
                    GameManager.Instance.RegisterGlobalManager(instance);
                    instance.LoadTableData();
                }
                return instance;
            }
        }
        #endregion

        private System.Random Random = new System.Random();
        private Dictionary<FeatureType, List<string>> FeatureTypeDict = new Dictionary<FeatureType, List<string>>();
        /// <summary>
        /// ����Feature���ݱ�
        /// </summary>
        private Dictionary<string, FeatureTableJsonData> FeatureTableDict = new Dictionary<string, FeatureTableJsonData>();

        /// <summary>
        /// �Ƿ�����Ч��ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsValidID(string id)
        {
            return this.FeatureTableDict.ContainsKey(id);
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
            if (maxFeatureNum > 0 && maxFeatureNum < FeatureTableDict.Count)
            {
                List<string> positiveFeature = FeatureTypeDict[FeatureType.Buff];
                if (positiveFeature.Count > 0)
                {
                    HashSet<string> featureIDSets = FeatureTableDict.Keys.ToHashSet();
                    //����һ���������ԣ�ĳЩ���Ի��໥ì�ܣ����ǲ��ܳ�����ͬһ��������
                    int randomIndex = Random.Next(0, positiveFeature.Count);
                    result.Add(this.SpawnFeature(positiveFeature[randomIndex]));
                    featureIDSets.Remove(result[0].ID);
                    featureIDSets.Remove(result[0].IDExclude);

                    //��ȡ����Ϊ���ó����Żء�
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
                result.Sort(new Feature.Sort());
            }
            return result;
        }
        public Feature SpawnFeature(string id)
        {
            if (FeatureTableDict.ContainsKey(id))
            {
                FeatureTableJsonData row = this.FeatureTableDict[id];
                Feature feature = new Feature(row);
                return feature;
            }
            Debug.LogError("û�ж�ӦIDΪ " + id + " ������");
            return null;
        }
        public Texture2D GetTexture2D(string id)
        {
            if (!this.FeatureTableDict.ContainsKey(id))
            {
                return null;
            }

            return GameManager.Instance.ABResourceManager.LoadLocalAB(Texture2DPath).LoadAsset<Texture2D>(this.FeatureTableDict[id].texture2d);
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

        #region to-do : ���������������� Feature ����
        public const string Texture2DPath = "ui/Feature/texture2d";

        [System.Serializable]
        public struct FeatureTableJsonData
        {
            public string id;
            public string idExclude;
            public int sort;
            public TextContent name;
            public string texture2d;
            public FeatureType type;
            public List<string> effectIDs;
            public TextContent featureDescription;
            public TextContent effectDescription;
        }
        public static ML.Engine.ABResources.ABJsonAssetProcessor<FeatureTableJsonData[]> ABJAProcessor;

        public void LoadTableData()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<FeatureTableJsonData[]>("Json/TableData", "FeatureTableData", (datas) =>
                {
                    foreach (var data in datas)
                    {
                        this.FeatureTableDict.Add(data.id, data);
                        if (!FeatureTypeDict.ContainsKey(data.type))
                        {
                            this.FeatureTypeDict.Add(data.type, new List<string>());
                        }
                        this.FeatureTypeDict[data.type].Add(data.id);
                    }
                }, null, "����Feature������");
                ABJAProcessor.StartLoadJsonAssetData();
            }
        }
        #endregion
    }
}
