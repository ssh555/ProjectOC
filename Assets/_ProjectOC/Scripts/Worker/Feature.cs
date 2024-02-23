using ProjectOC.ManagerNS;
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
        public string ID = "";
        public List<Effect> Effects = new List<Effect>();

        #region 读表属性
        /// <summary>
        /// 互斥键，会发生矛盾的特性
        /// </summary>
        public string IDExclude { get => LocalGameManager.Instance.FeatureManager.GetIDExclude(ID); }
        /// <summary>
        /// 序号，用于排序，从上到下的顺序为种族、增益、减益、整活特性
        /// </summary>
        public int Sort { get => LocalGameManager.Instance.FeatureManager.GetSort(ID); }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get => LocalGameManager.Instance.FeatureManager.GetName(ID); }
        /// <summary>
        /// 类型
        /// </summary>        
        public FeatureType FeatureType { get => LocalGameManager.Instance.FeatureManager.GetFeatureType(ID); }
        /// <summary>
        /// 特性本身的描述性文本
        /// </summary>
        public string Description { get => LocalGameManager.Instance.FeatureManager.GetItemDescription(ID); }
        /// <summary>
        /// 特性效果的描述性文本
        /// </summary>
        public string EffectsDescription { get => LocalGameManager.Instance.FeatureManager.GetEffectsDescription(ID); }
        #endregion

        public Feature(FeatureTableData config)
        {
            this.ID = config.ID;
            this.Effects = new List<Effect>();
            foreach (var tuple in config.Effects)
            {
                Effect effect = LocalGameManager.Instance.EffectManager.SpawnEffect(tuple.Item1, tuple.Item2);
                if (effect != null)
                {
                    this.Effects.Add(effect);
                }
                else
                {
                    //Debug.LogError($"Feature {this.ID} Effect {tuple.Item1} is Null");
                }
            }
        }

        public void ApplyFeature(Worker worker)
        {
            if (worker != null)
            {
                foreach (Effect effect in this.Effects)
                {
                    effect.ApplyEffect(worker);
                }
            }
            else
            {
                //Debug.LogError($"Feature {this.ID} Worker is Null");
            }
        }

        public class FeatureSort : IComparer<Feature>
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
                int xs = x.Sort;
                int ys = y.Sort;
                if (xs != ys)
                {
                    return xs.CompareTo(ys);
                }
                else
                {
                    return string.Compare(x.ID, y.ID);
                }
            }
        }
    }
}

