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
        /// ͼ��
        /// </summary>
        public Texture Icon;
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

        public Feature(string id)
        {
            // TODO: ����������
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

