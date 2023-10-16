using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.NumericalProperty
{
    /// <summary>
    /// CombatObject �������
    /// </summary>
    [System.Serializable]
    public struct CombatPropertyStruct
    {
        /// <summary>
        /// ����ֵ
        /// </summary>
        [LabelText("����ֵ")]
        public NumericalFloat HP;

        /// <summary>
        /// ����
        /// </summary>
        [LabelText("����")]
        public NumericalFloat Armor;

        /// <summary>
        /// ��������ֵ
        /// </summary>
        [LabelText("������")]
        public NumericalFloat AttackValue;

        [SerializeField, Range(0, 1), LabelText("����")]
        private float _damageReduction;
        /// <summary>
        /// ���� [0, 1]
        /// </summary>
        public float DamageReduction
        {
            get => _damageReduction;
            set
            {
                _damageReduction = Mathf.Clamp01(value);
            }
        }

        [SerializeField, LabelText("�������ӳ�")]
        private float _healBonus;
        /// <summary>
        /// �������ӳ�
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
        ///// �ܷ�ս������ -> ������ս�������������� ApplyAction
        ///// </summary>
        //public bool bCanInteract;

        /// <summary>
        /// �޵�
        /// </summary>
        [SerializeField, LabelText("�޵�")]
        public bool bInvincible;

        /// <summary>
        /// ����
        /// </summary>
        [SerializeField, LabelText("����")]
        public bool bSuperArmor;

        /// <summary>
        /// ���߿���ȼ�
        /// </summary>
        [SerializeField, LabelText("���߿���ȼ�")]
        public byte immuneFreezeFrame;

        /// <summary>
        /// �����ڵ�buffЧ���б� => to-do : �������� & Action.UpdateStatusAction�Ż�
        /// </summary>
        [LabelText("ӵ�е�buff�б�")]
        public List<Action.UpdateStatusAction> buffList;

        /// <summary>
        /// ��ȡ���׵ļ�����
        /// </summary>
        /// <returns></returns>
        public float GetArmorDmgRed()
        {
            return this.Armor >= 0 ? 100 / (100 + Armor) : -Armor * 0.01f;
        }
    }
}

