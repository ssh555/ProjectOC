using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem
{
    public class CombatSystem : Manager.GlobalManager.IGlobalManager
    {
        #region 内部类|枚举|结构体
        /// <summary>
        /// 受击参数
        /// </summary>
        public struct WoundedParams
        {
            /// <summary>
            /// 攻击者
            /// </summary>
            public CombatObject.ISpellAttackObject Instigator;
            /// <summary>
            /// 攻击来源方向
            /// </summary>
            public Vector3 Direction;
            /// <summary>
            /// 受击模式
            /// </summary>
            public DataConfig.StatusDataConfig.WoundedMode WoundedMode;
            /// <summary>
            /// 0 => 更改值
            /// 1 => 当前值
            /// 2 => 最大值
            /// </summary>
            public float[] hps;
        }
        #endregion

        private CombatSystem() { }

        private static CombatSystem instance;

        public static CombatSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CombatSystem();
                    Manager.GameManager.Instance.RegisterGlobalManager(instance);
                }
                return instance;
            }
        }

        /// <summary>
        /// 应用技能效果
        /// </summary>
        /// <param name="Victim"></param>
        /// <param name="skillActionData"></param>
        public void ApplySkillAction(CombatObject.ICombatObject Victim, DataConfig.SkillActionDataStruct skillActionData, object otherParams = null)
        {
            foreach(var status in skillActionData.SkillData.configs)
            {
                this.ApplyStatusAction(skillActionData.Instigator, Victim, status, otherParams);
            }
        }

        /// <summary>
        /// 数值计算 => to-do : 根据需求更改
        /// </summary>
        /// <param name="Victim"></param>
        /// <param name="skillActionData"></param>
        /// <returns></returns>
        protected float CalculateActionValue(CombatObject.ISpellAttackObject Instigator, CombatObject.ICombatObject Victim, DataConfig.StatusDataConfig statusData)
        {
            // 攻击
            if(statusData.mode == DataConfig.StatusDataConfig.StatusEffect.Attack)
            {
                return Mathf.Max(1, Instigator.combatProperty.AttackValue * (1 - Victim.combatProperty.DamageReduction) * (1 - Victim.combatProperty.GetArmorDmgRed() * statusData.AttackRatio));
            }
            // 治疗
            else if (statusData.mode == DataConfig.StatusDataConfig.StatusEffect.Cure)
            {
                return Mathf.Max(1, statusData.cureValue * (1 + Instigator.combatProperty.HealBonus));
            }
            Debug.Assert(false);
            return 0;
        }
    
        protected void ApplyStatusAction(CombatObject.ISpellAttackObject Instigator, CombatObject.ICombatObject Victim, DataConfig.StatusDataConfig statusData, object otherParams)
        {
            // 攻击
            if (statusData.mode == DataConfig.StatusDataConfig.StatusEffect.Attack)
            {
                var _params = (WoundedParams)otherParams;
                _params.WoundedMode = statusData.woundedMode;
                Victim.ApplyDamage(Instigator, this.CalculateActionValue(Instigator, Victim, statusData), _params, statusData.freezeFrameParams);
            }
            // 治疗
            else if (statusData.mode == DataConfig.StatusDataConfig.StatusEffect.Cure)
            {
                Victim.ApplyCure(Instigator, this.CalculateActionValue(Instigator, Victim, statusData));
            }
            // 应用buff
            else if(statusData.mode == DataConfig.StatusDataConfig.StatusEffect.Buff)
            {
                Victim.ApplyBuff(Instigator, statusData);
            }
        }
    }
}

