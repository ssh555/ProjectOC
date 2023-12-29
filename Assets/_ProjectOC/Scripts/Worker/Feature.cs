using ML.Engine.TextContent;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    /// <summary>
    /// 特性
    /// </summary>
    [System.Serializable]
    public class Feature
    {
        /// <summary>
        /// 键，Feature_类型_序号
        /// </summary>
        public string ID = "";
        /// <summary>
        /// 互斥键，会发生矛盾的特性
        /// </summary>
        public string IDExclude = "";
        /// <summary>
        /// 序号，用于排序，从上到下的顺序为增益、减益、整活特性
        /// </summary>
        public int SortNum;
        /// <summary>
        /// 名称
        /// </summary>
        public TextContent Name;
        /// <summary>
        /// 类型
        /// </summary>        
        public FeatureType Type;
        /// <summary>
        /// 效果
        /// </summary>
        public List<Effect> Effects = new List<Effect>();
        /// <summary>
        /// 特性本身的描述性文本
        /// </summary>
        public TextContent FeatureDescription;
        /// <summary>
        /// 特性效果的描述性文本
        /// </summary>
        public TextContent EffectDescription;

        public Feature(FeatureManager.FeatureTableJsonData config)
        {
            this.ID = config.id;
            this.IDExclude = config.idExclude;
            this.SortNum = config.sort;
            this.Name = config.name;
            this.Type = config.type;
            this.Effects = new List<Effect>();
            foreach (string effectID in config.effectIDs)
            {
                Effect effect = EffectManager.Instance.SpawnEffect(effectID);
                if (effect != null)
                {
                    this.Effects.Add(effect);
                }
                else
                {
                    Debug.LogError($"Feature {this.ID} Effect {effectID} is Null");
                }
            }
            this.FeatureDescription = config.featureDescription;
            this.EffectDescription = config.effectDescription;
        }
        public Feature(Feature feature)
        {
            this.ID = feature.ID;
            this.IDExclude = feature.IDExclude;
            this.SortNum = feature.SortNum;
            this.Name = feature.Name;
            this.Type = feature.Type;
            this.Effects = new List<Effect>();
            foreach (Effect effect in feature.Effects)
            {
                Effect newEffect = new Effect(effect);
                if (newEffect != null)
                {
                    this.Effects.Add(newEffect);
                }
                else
                {
                    Debug.LogError($"Feature {feature.ID} effect is Null");
                }
            }
            this.FeatureDescription = feature.FeatureDescription;
            this.EffectDescription = feature.EffectDescription;
        }

        public void ApplyEffectToWorker(Worker worker)
        {
            if (worker != null)
            {
                foreach (Effect effect in this.Effects)
                {
                    effect.ApplyEffectToWorker(worker);
                }
            }
            else
            {
                Debug.LogError($"Feature {this.ID} ApplyEffectToWorker Worker is Null");
            }
        }
        public void RemoveEffectToWorker(Worker worker)
        {
            if (worker != null)
            {
                foreach (Effect effect in this.Effects)
                {
                    effect.RemoveEffectToWorker(worker);
                }
            }
            else
            {
                Debug.LogError($"Feature {this.ID} RemoveEffectToWorker Worker is Null");
            }
        }

        /// <summary>
        /// 特性按照增益、减益、无功能的顺序从上至下排序
        /// </summary>
        public class Sort : IComparer<Feature>
        {
            public int Compare(Feature x, Feature y)
            {
                if (x == null)
                {
                    if (y == null)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
                if (y == null)
                {
                    return -1;
                }

                if (x.SortNum != y.SortNum)
                {
                    return x.SortNum.CompareTo(y.SortNum);
                }
                else
                {
                    return string.Compare(x.ID, y.ID);
                }
            }
        }
    }
}

