using ML.Engine.TextContent;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    /// <summary>
    /// ����
    /// </summary>
    [System.Serializable]
    public class Skill
    {
        /// <summary>
        /// ����Skill_��������
        /// </summary>
        public string ID = "";
        /// <summary>
        /// ����
        /// </summary>
        public TextContent Name;
        /// <summary>
        /// ��ţ���������
        /// </summary>
        public int SortNum;
        /// <summary>
        /// ��������
        /// </summary>
        public WorkType Type;
        /// <summary>
        /// ��������
        /// </summary>
        public TextContent Desciption;
        /// <summary>
        /// ����Ч������
        /// </summary>
        public TextContent EffectsDescription;
        /// <summary>
        /// Ч��
        /// </summary>
        public List<Effect> Effects = new List<Effect>();
        /// <summary>
        /// ��ǰ�ȼ�
        /// </summary>
        public int Level = 0;
        /// <summary>
        /// ���ȼ�
        /// </summary>
        public int LevelMax = 10;
        /// <summary>
        /// ��ǰ����
        /// </summary>
        private int Exp;
        public Skill(SkillManager.SkillTableJsonData config)
        {
            this.ID = config.id;
            this.Name = config.name;
            this.SortNum = config.sort;
            this.Type = config.type;
            this.Desciption = config.desciption;
            this.EffectsDescription = config.effectsDescription;
            foreach (string effectID in config.effectIDs)
            {
                Effect effect = EffectManager.Instance.SpawnEffect(effectID);
                if (effect != null)
                {
                    this.Effects.Add(effect);
                }
                else
                {
                    Debug.LogError($"Skill {this.ID} Effect {effectID} is Null");
                }
            }
            this.Level = 0;
            this.LevelMax = 10;
            this.Exp = 0;
        }
        public Skill(Skill skill)
        {
            this.ID = skill.ID;
            this.Name = skill.Name;
            this.SortNum = skill.SortNum;
            this.Type = skill.Type;
            this.Desciption = skill.Desciption;
            this.EffectsDescription = skill.EffectsDescription;
            this.Effects = new List<Effect>();
            foreach (Effect effect in skill.Effects)
            {
                Effect newEffect = new Effect(effect);
                if (newEffect != null)
                {
                    this.Effects.Add(newEffect);
                }
                else
                {
                    Debug.LogError($"Skill {skill.ID} effect is Null");
                }
            }
            this.Level = skill.Level;
            this.LevelMax = skill.LevelMax;
            this.Exp = skill.Exp;
        }
        /// <summary>
        /// �ȼ�����
        /// </summary>
        public void AddLevel()
        {
            if (this.Level < this.LevelMax)
            {
                this.Level++;
                this.Exp = 0;
                // TODO:�ı�Ч��
            }
        }
        /// <summary>
        /// �ȼ�����
        /// </summary>
        /// <param name="clearExp">�Ƿ���վ���ֵ</param>
        public void MinusLevel(bool clearExp = false)
        {
            if (this.Level >= 0)
            {
                this.Level--;
                this.Exp = clearExp ? 0 : 100 * (this.Level + 1);
                // TODO:�ı�Ч��
            }
        }
        /// <summary>
        /// ���ȼ��;����Ƿ����
        /// </summary>
        public void CheckLevel()
        {
            if (this.Level >= this.LevelMax)
            {
                this.Level = this.LevelMax;
                this.Exp = 0;
            }
            if (this.Level < 0)
            {
                this.Level = 0;
                this.Exp = 0;
            }
        }

        /// <summary>
        /// �޸ľ���ֵ��
        /// ÿ������������µľ���ֵ������µļ���Ч����
        /// ����ʱ������ֵ�������������һ����
        /// �ﵽ��߼�ʱ�����Ӿ��顣
        /// </summary>
        /// <param name="value">����ֵ</param>
        public void AlterExp(int value)
        {
            while (value > 0 && this.Level < this.LevelMax)
            {
                int expNeeded = 100 * (this.Level + 1) - this.Exp;
                if (value >= expNeeded)
                {
                    value -= expNeeded;
                    this.AddLevel();
                }
                else
                {
                    this.Exp = this.Level >= this.LevelMax ? 0 : this.Exp + value;
                    value = 0;
                }
            }

            while (value < 0 && (this.Level >= 0 && this.Exp > 0))
            {
                if (value + this.Exp >= 0)
                {
                    this.Exp -= value;
                    value = 0;
                }
                else
                {
                    value += this.Exp;
                    this.MinusLevel();
                }
            }
            CheckLevel();
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
                Debug.LogError($"Skill {this.ID} ApplyEffectToWorker Worker is Null");
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
                Debug.LogError($"Skill {this.ID} RemoveEffectToWorker Worker is Null");
            }
        }
    }
}

