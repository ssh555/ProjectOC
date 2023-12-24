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
        public int Sort;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name = "";
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
        public string FeatureDescription = "";
        /// <summary>
        /// 特性效果的描述性文本
        /// </summary>
        public string EffectDescription = "";

        public void Init(FeatureManager.FeatureTableJsonData config)
        {
            this.ID = config.id;
            this.IDExclude = config.idExclude; 
            this.Sort = config.sort;
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
            }
            this.FeatureDescription = config.featureDescription;
            this.EffectDescription = config.effectDescription;
        }
        public void Init(Feature feature)
        {
            this.ID = feature.ID;
            this.IDExclude = feature.IDExclude;
            this.Sort = feature.Sort;
            this.Name = feature.Name;
            this.Type = feature.Type;
            this.Effects = new List<Effect>();
            foreach (Effect effect in feature.Effects)
            {
                Effect newEffect = new Effect();
                newEffect.Init(effect);
                this.Effects.Add(newEffect);
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
        }
    }
}

