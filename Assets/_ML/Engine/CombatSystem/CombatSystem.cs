using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem
{
    public class CombatSystem : Manager.GlobalManager.IGlobalManager
    {
        #region �ڲ���|ö��|�ṹ��
        /// <summary>
        /// �ܻ�����
        /// </summary>
        public struct WoundedParams
        {
            /// <summary>
            /// ������
            /// </summary>
            public CombatObject.ISpellAttackObject Instigator;
            /// <summary>
            /// ������Դ����
            /// </summary>
            public Vector3 Direction;
            /// <summary>
            /// �ܻ�ģʽ
            /// </summary>
            public DataConfig.StatusDataConfig.WoundedMode WoundedMode;
            /// <summary>
            /// 0 => ����ֵ
            /// 1 => ��ǰֵ
            /// 2 => ���ֵ
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
        /// Ӧ�ü���Ч��
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
        /// ��ֵ���� => to-do : �����������
        /// </summary>
        /// <param name="Victim"></param>
        /// <param name="skillActionData"></param>
        /// <returns></returns>
        protected float CalculateActionValue(CombatObject.ISpellAttackObject Instigator, CombatObject.ICombatObject Victim, DataConfig.StatusDataConfig statusData)
        {
            // ����
            if(statusData.mode == DataConfig.StatusDataConfig.StatusEffect.Attack)
            {
                return Mathf.Max(1, Instigator.combatProperty.AttackValue * (1 - Victim.combatProperty.DamageReduction) * (1 - Victim.combatProperty.GetArmorDmgRed() * statusData.AttackRatio));
            }
            // ����
            else if (statusData.mode == DataConfig.StatusDataConfig.StatusEffect.Cure)
            {
                return Mathf.Max(1, statusData.cureValue * (1 + Instigator.combatProperty.HealBonus));
            }
            Debug.Assert(false);
            return 0;
        }
    
        protected void ApplyStatusAction(CombatObject.ISpellAttackObject Instigator, CombatObject.ICombatObject Victim, DataConfig.StatusDataConfig statusData, object otherParams)
        {
            // ����
            if (statusData.mode == DataConfig.StatusDataConfig.StatusEffect.Attack)
            {
                var _params = (WoundedParams)otherParams;
                _params.WoundedMode = statusData.woundedMode;
                Victim.ApplyDamage(Instigator, this.CalculateActionValue(Instigator, Victim, statusData), _params, statusData.freezeFrameParams);
            }
            // ����
            else if (statusData.mode == DataConfig.StatusDataConfig.StatusEffect.Cure)
            {
                Victim.ApplyCure(Instigator, this.CalculateActionValue(Instigator, Victim, statusData));
            }
            // Ӧ��buff
            else if(statusData.mode == DataConfig.StatusDataConfig.StatusEffect.Buff)
            {
                Victim.ApplyBuff(Instigator, statusData);
            }
        }
    }
}

