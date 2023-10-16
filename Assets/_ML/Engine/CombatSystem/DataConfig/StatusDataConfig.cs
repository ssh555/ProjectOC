using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.DataConfig
{
    /// <summary>
    /// 效果应用基本模块
    /// </summary>
    public struct StatusDataConfig
    {
        public enum StatusEffect
        {
            /// <summary>
            /// 攻击
            /// </summary>
            [LabelText("攻击")]
            Attack,
            /// <summary>
            /// 治疗
            /// </summary>
            [LabelText("治疗")]
            Cure,
            /// <summary>
            /// BUFF
            /// </summary>
            [LabelText("BUFF")]
            Buff,
        }

        #region 通用
        /// <summary>
        /// 标识符
        /// </summary>
        public string ID;

        /// <summary>
        /// 效果类型
        /// </summary>
        public StatusEffect mode;
        #endregion

        #region Attack
        public enum WoundedMode : byte
        {
            None = 0,
            LeanBack = 1,
            HitBack = 2,
            HitFly = 3,
        }

        public float AttackRatio;

        public WoundedMode woundedMode;

        public CombatObject.FreezeFrameParams freezeFrameParams;
        #endregion

        #region Cure
        public float cureValue;

        #endregion

        #region Buff
        /// <summary>
        /// 技能持续时间
        /// 若 <= 0 ，则表示无限
        /// </summary>
        public float duration;
        // to-do : 待设计
        #endregion
    }
}

