using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.DataConfig
{
    /// <summary>
    /// 基于数值的技能装饰配置 => to-do : 暂未设计
    /// </summary>
    [System.Serializable]
    public struct SkillDataConfig
    {
        #region 内部类|结构体|枚举
       
        /// <summary>
        /// 技能攻击模式
        /// </summary>
        public enum SkillAttackMode
        {
            /// <summary>
            /// 单体攻击
            /// </summary>
            [LabelText("单体攻击")]
            SingleAttack,
            /// <summary>
            /// 群体 每一个对象仅作用一次
            /// </summary>
            [LabelText("群体一次攻击")]
            DirectOnlyApplyOnce,
            [LabelText("群体多次攻击")]
            /// <summary>
            /// 群体 每一个对象每隔一段时间作用一次
            /// </summary>
            IntervalApplyOnce,
        }

        #endregion

        #region 通用配置
        /// <summary>
        /// 检测层
        /// </summary>
        [SerializeField]
        public LayerMask combatDetectLayer;

        /// <summary>
        /// 技能所要施加的效果
        /// </summary>
        public StatusDataConfig[] configs;

        /// <summary>
        /// 技能持续时间
        /// 若 <= 0 ，则表示无限
        /// </summary>
        public float duration;

        /// <summary>
        /// 攻击模式
        /// </summary>
        public SkillAttackMode attackMode;

        /// <summary>
        /// 作用于Instigator
        /// </summary>
        public bool bApplyInstigator;
        #endregion

        #region IntervalApplyOnce
        /// <summary>
        /// IntervalApplyOnce 模式下的攻击时间间隔
        /// </summary>
        public float ApplyIntervalTime;
        /// <summary>
        /// 单次间隔应用启用trigger的时间
        /// </summary>
        public float OnceIntervalDuration;
        #endregion
    }
}

