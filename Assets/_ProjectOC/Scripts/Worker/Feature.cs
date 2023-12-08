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
        /// 图标
        /// </summary>
        public Texture Icon;
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

        public Feature(string id)
        {
            // TODO: 读表拿数据
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
    }
}

