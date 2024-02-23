using ProjectOC.ManagerNS;
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
        public string ID = "";
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

        #region ��������
        /// <summary>
        /// ��ţ���������
        /// </summary>
        public int Sort { get => LocalGameManager.Instance.SkillManager.GetSort(ID); }
        /// <summary>
        /// ��������
        /// </summary>
        public WorkType SkillType { get => LocalGameManager.Instance.SkillManager.GetSkillType(ID); }
        /// <summary>
        /// ��������
        /// </summary>
        public string Desciption { get => LocalGameManager.Instance.SkillManager.GetItemDescription(ID); }
        /// <summary>
        /// ����Ч������
        /// </summary>
        public string EffectsDescription { get => LocalGameManager.Instance.SkillManager.GetEffectsDescription(ID); }
        #endregion
        public Skill()
        {
        }
        public Skill(SkillTableData config)
        {
            this.ID = config.ID;
            foreach (var tuple in config.Effects)
            {
                Effect effect = LocalGameManager.Instance.EffectManager.SpawnEffect(tuple.Item1, tuple.Item2);
                if (effect != null)
                {
                    this.Effects.Add(effect);
                }
                else
                {
                    //Debug.LogError($"Skill {this.ID} Effect {tuple.Item1} is Null");
                }
            }
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

        public void ApplySkill(Worker worker)
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
                //Debug.LogError($"Skill {this.ID} Worker is Null");
            }
        }
    }
}

