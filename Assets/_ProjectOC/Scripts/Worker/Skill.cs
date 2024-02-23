using ProjectOC.ManagerNS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    /// <summary>
    /// 技能
    /// </summary>
    [System.Serializable]
    public class Skill
    {
        public string ID = "";
        /// <summary>
        /// 效果
        /// </summary>
        public List<Effect> Effects = new List<Effect>();
        /// <summary>
        /// 当前等级
        /// </summary>
        public int Level = 0;
        /// <summary>
        /// 最大等级
        /// </summary>
        public int LevelMax = 10;
        /// <summary>
        /// 当前经验
        /// </summary>
        private int Exp;

        #region 读表属性
        /// <summary>
        /// 序号，用于排序
        /// </summary>
        public int Sort { get => LocalGameManager.Instance.SkillManager.GetSort(ID); }
        /// <summary>
        /// 技能类型
        /// </summary>
        public WorkType SkillType { get => LocalGameManager.Instance.SkillManager.GetSkillType(ID); }
        /// <summary>
        /// 技能描述
        /// </summary>
        public string Desciption { get => LocalGameManager.Instance.SkillManager.GetItemDescription(ID); }
        /// <summary>
        /// 技能效果描述
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
        /// 等级增加
        /// </summary>
        public void AddLevel()
        {
            if (this.Level < this.LevelMax)
            {
                this.Level++;
                this.Exp = 0;
                // TODO:改变效果
            }
        }
        /// <summary>
        /// 等级减少
        /// </summary>
        /// <param name="clearExp">是否清空经验值</param>
        public void MinusLevel(bool clearExp = false)
        {
            if (this.Level >= 0)
            {
                this.Level--;
                this.Exp = clearExp ? 0 : 100 * (this.Level + 1);
                // TODO:改变效果
            }
        }
        /// <summary>
        /// 检查等级和经验是否合理
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
        /// 修改经验值。
        /// 每次升级会带来新的经验值需求和新的技能效果。
        /// 升级时若经验值溢出将保留至下一级。
        /// 达到最高级时不增加经验。
        /// </summary>
        /// <param name="value">经验值</param>
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

