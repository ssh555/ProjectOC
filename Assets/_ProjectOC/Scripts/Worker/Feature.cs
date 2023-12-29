using ML.Engine.TextContent;
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
        /// ����Feature_����_���
        /// </summary>
        public string ID = "";
        /// <summary>
        /// ��������ᷢ��ì�ܵ�����
        /// </summary>
        public string IDExclude = "";
        /// <summary>
        /// ��ţ��������򣬴��ϵ��µ�˳��Ϊ���桢���桢��������
        /// </summary>
        public int SortNum;
        /// <summary>
        /// ����
        /// </summary>
        public TextContent Name;
        /// <summary>
        /// ����
        /// </summary>        
        public FeatureType Type;
        /// <summary>
        /// Ч��
        /// </summary>
        public List<Effect> Effects = new List<Effect>();
        /// <summary>
        /// ���Ա�����������ı�
        /// </summary>
        public TextContent FeatureDescription;
        /// <summary>
        /// ����Ч�����������ı�
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
        /// ���԰������桢���桢�޹��ܵ�˳�������������
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

