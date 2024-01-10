using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    /// <summary>
    /// ����
    /// </summary>
    [System.Serializable]
    public class Feature
    {
        /// <summary>
        /// ID
        /// </summary>
        public string ID = "";
        /// <summary>
        /// Ч��
        /// </summary>
        public List<Effect> Effects = new List<Effect>();

        #region ��������
        /// <summary>
        /// ��������ᷢ��ì�ܵ�����
        /// </summary>
        public string IDExclude { get => FeatureManager.Instance.GetIDExclude(ID); }
        /// <summary>
        /// ��ţ��������򣬴��ϵ��µ�˳��Ϊ���塢���桢���桢��������
        /// </summary>
        public int Sort { get => FeatureManager.Instance.GetSort(ID); }
        /// <summary>
        /// ����
        /// </summary>
        public string Name { get => FeatureManager.Instance.GetName(ID); }
        /// <summary>
        /// ����
        /// </summary>        
        public FeatureType FeatureType { get => FeatureManager.Instance.GetFeatureType(ID); }
        /// <summary>
        /// ���Ա�����������ı�
        /// </summary>
        public string Description { get => FeatureManager.Instance.GetItemDescription(ID); }
        /// <summary>
        /// ����Ч�����������ı�
        /// </summary>
        public string EffectsDescription { get => FeatureManager.Instance.GetEffectsDescription(ID); }
        #endregion

        public Feature(FeatureManager.FeatureTableJsonData config)
        {
            this.ID = config.ID;
            this.Effects = new List<Effect>();
            foreach (string effectID in config.Effects)
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
        }
        public Feature(Feature feature)
        {
            this.ID = feature.ID;
            this.Effects = new List<Effect>();
            foreach (Effect effect in feature.Effects)
            {
                if (effect != null)
                {
                    this.Effects.Add(new Effect(effect));
                }
                else
                {
                    Debug.LogError($"Feature {feature.ID} effect is Null");
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
                Debug.LogError($"Feature {this.ID} Worker is Null");
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

