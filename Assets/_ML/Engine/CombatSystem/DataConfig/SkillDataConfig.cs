using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.DataConfig
{
    /// <summary>
    /// ������ֵ�ļ���װ������ => to-do : ��δ���
    /// </summary>
    [System.Serializable]
    public struct SkillDataConfig
    {
        #region �ڲ���|�ṹ��|ö��
       
        /// <summary>
        /// ���ܹ���ģʽ
        /// </summary>
        public enum SkillAttackMode
        {
            /// <summary>
            /// ���幥��
            /// </summary>
            [LabelText("���幥��")]
            SingleAttack,
            /// <summary>
            /// Ⱥ�� ÿһ�����������һ��
            /// </summary>
            [LabelText("Ⱥ��һ�ι���")]
            DirectOnlyApplyOnce,
            [LabelText("Ⱥ���ι���")]
            /// <summary>
            /// Ⱥ�� ÿһ������ÿ��һ��ʱ������һ��
            /// </summary>
            IntervalApplyOnce,
        }

        #endregion

        #region ͨ������
        /// <summary>
        /// ����
        /// </summary>
        [SerializeField]
        public LayerMask combatDetectLayer;

        /// <summary>
        /// ������Ҫʩ�ӵ�Ч��
        /// </summary>
        public StatusDataConfig[] configs;

        /// <summary>
        /// ���ܳ���ʱ��
        /// �� <= 0 �����ʾ����
        /// </summary>
        public float duration;

        /// <summary>
        /// ����ģʽ
        /// </summary>
        public SkillAttackMode attackMode;

        /// <summary>
        /// ������Instigator
        /// </summary>
        public bool bApplyInstigator;
        #endregion

        #region IntervalApplyOnce
        /// <summary>
        /// IntervalApplyOnce ģʽ�µĹ���ʱ����
        /// </summary>
        public float ApplyIntervalTime;
        /// <summary>
        /// ���μ��Ӧ������trigger��ʱ��
        /// </summary>
        public float OnceIntervalDuration;
        #endregion
    }
}

