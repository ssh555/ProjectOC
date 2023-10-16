using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.NumericalProperty
{
    /// <summary>
    /// CombatObject 相关属性
    /// </summary>
    [System.Serializable]
    public struct CombatPropertyStruct
    {
        /// <summary>
        /// 生命值
        /// </summary>
        [LabelText("生命值")]
        public NumericalFloat HP;

        /// <summary>
        /// 护甲
        /// </summary>
        [LabelText("护甲")]
        public NumericalFloat Armor;

        /// <summary>
        /// 攻击力数值
        /// </summary>
        [LabelText("攻击力")]
        public NumericalFloat AttackValue;

        [SerializeField, Range(0, 1), LabelText("减伤")]
        private float _damageReduction;
        /// <summary>
        /// 减伤 [0, 1]
        /// </summary>
        public float DamageReduction
        {
            get => _damageReduction;
            set
            {
                _damageReduction = Mathf.Clamp01(value);
            }
        }

        [SerializeField, LabelText("治疗量加成")]
        private float _healBonus;
        /// <summary>
        /// 治疗量加成
        /// </summary>
        public float HealBonus
        {
            get => _healBonus;
            set
            {
                _healBonus = Mathf.Clamp01(value);
            }
        }

        public bool IsDead
        {
            get
            {
                return this.HP.ValuePercent <= float.Epsilon;
            }
        }

        ///// <summary>
        ///// 能否战斗交互 -> 不进行战斗交互，即不会 ApplyAction
        ///// </summary>
        //public bool bCanInteract;

        /// <summary>
        /// 无敌
        /// </summary>
        [SerializeField, LabelText("无敌")]
        public bool bInvincible;

        /// <summary>
        /// 霸体
        /// </summary>
        [SerializeField, LabelText("霸体")]
        public bool bSuperArmor;

        /// <summary>
        /// 免疫卡肉等级
        /// </summary>
        [SerializeField, LabelText("免疫卡肉等级")]
        public byte immuneFreezeFrame;

        /// <summary>
        /// 所存在的buff效果列表 => to-do : 联机接入 & Action.UpdateStatusAction优化
        /// </summary>
        [LabelText("拥有的buff列表")]
        public List<Action.UpdateStatusAction> buffList;

        /// <summary>
        /// 获取护甲的减伤率
        /// </summary>
        /// <returns></returns>
        public float GetArmorDmgRed()
        {
            return this.Armor >= 0 ? 100 / (100 + Armor) : -Armor * 0.01f;
        }
    }
}

