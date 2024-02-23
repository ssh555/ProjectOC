using ProjectOC.ManagerNS;
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
        public string ID = "";
        public List<Effect> Effects = new List<Effect>();

        #region ��������
        /// <summary>
        /// ��������ᷢ��ì�ܵ�����
        /// </summary>
        public string IDExclude { get => LocalGameManager.Instance.FeatureManager.GetIDExclude(ID); }
        /// <summary>
        /// ��ţ��������򣬴��ϵ��µ�˳��Ϊ���塢���桢���桢��������
        /// </summary>
        public int Sort { get => LocalGameManager.Instance.FeatureManager.GetSort(ID); }
        /// <summary>
        /// ����
        /// </summary>
        public string Name { get => LocalGameManager.Instance.FeatureManager.GetName(ID); }
        /// <summary>
        /// ����
        /// </summary>        
        public FeatureType FeatureType { get => LocalGameManager.Instance.FeatureManager.GetFeatureType(ID); }
        /// <summary>
        /// ���Ա�����������ı�
        /// </summary>
        public string Description { get => LocalGameManager.Instance.FeatureManager.GetItemDescription(ID); }
        /// <summary>
        /// ����Ч�����������ı�
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

