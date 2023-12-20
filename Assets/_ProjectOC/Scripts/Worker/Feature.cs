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
        public int Sort;
        /// <summary>
        /// ����
        /// </summary>
        public string Name = "";
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
        public string FeatureDescription = "";
        /// <summary>
        /// ����Ч�����������ı�
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

