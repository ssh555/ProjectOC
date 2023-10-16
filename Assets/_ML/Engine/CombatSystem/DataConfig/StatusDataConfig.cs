using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.DataConfig
{
    /// <summary>
    /// Ч��Ӧ�û���ģ��
    /// </summary>
    public struct StatusDataConfig
    {
        public enum StatusEffect
        {
            /// <summary>
            /// ����
            /// </summary>
            [LabelText("����")]
            Attack,
            /// <summary>
            /// ����
            /// </summary>
            [LabelText("����")]
            Cure,
            /// <summary>
            /// BUFF
            /// </summary>
            [LabelText("BUFF")]
            Buff,
        }

        #region ͨ��
        /// <summary>
        /// ��ʶ��
        /// </summary>
        public string ID;

        /// <summary>
        /// Ч������
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
        /// ���ܳ���ʱ��
        /// �� <= 0 �����ʾ����
        /// </summary>
        public float duration;
        // to-do : �����
        #endregion
    }
}

