using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    [LabelText("技能"), System.Serializable]
    public class Skill
    {
        [LabelText("ID"), ReadOnly]
        public string ID = "";
        [LabelText("效果"), ReadOnly]
        public List<Effect> Effects = new List<Effect>();
        [LabelText("当前等级"), ReadOnly]
        public int Level = 0;
        [LabelText("最大等级"), ReadOnly]
        public int LevelMax = 10;
        [LabelText("当前经验"), ShowInInspector, ReadOnly]
        private int Exp;

        #region 读表属性
        [LabelText("序号"), ShowInInspector, ReadOnly]
        public int Sort { get => LocalGameManager.Instance != null ? LocalGameManager.Instance.SkillManager.GetSort(ID) : 0; }
        [LabelText("技能类型"), ShowInInspector, ReadOnly]
        public WorkType SkillType { get => LocalGameManager.Instance != null ? LocalGameManager.Instance.SkillManager.GetSkillType(ID) : WorkType.None; }
        [LabelText("技能描述"), ShowInInspector, ReadOnly]
        public string Desciption { get => LocalGameManager.Instance != null ? LocalGameManager.Instance.SkillManager.GetItemDescription(ID) : ""; }
        [LabelText("技能效果描述"), ShowInInspector, ReadOnly]
        public string EffectsDescription { get => LocalGameManager.Instance != null ? LocalGameManager.Instance.SkillManager.GetEffectsDescription(ID) : ""; }
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
        }
    }
}

