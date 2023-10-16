using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.NumericalProperty
{
    /// <summary>
    /// ս��ϵͳ���� float
    /// </summary>
    [System.Serializable]
    public struct NumericalFloat
    {
        #region Base
        [SerializeField, HideInInspector]
        private float baseValue;
        /// <summary>
        /// ����ֵ
        /// </summary>
        [ShowInInspector, LabelText("����ֵ")]
        public float BaseValue
        {
            get
            {
                return baseValue;
            }
            set
            {
                this.baseValue = value;
                this.finalBaseValue = this.baseValue * (1 + this.baseModify);
                this.FinalValue = (this.finalBaseValue + this.finalExtraValue) * (1 + this.finalModify);
            }
        }
        [SerializeField, HideInInspector]
        private float baseModify;
        /// <summary>
        /// �����ٷֱ�����
        /// </summary>
        [ShowInInspector, LabelText("����������")]
        public float BaseModify
        {
            get
            {
                return baseModify;
            }
            set
            {
                this.baseModify = value;
                this.finalBaseValue = this.baseValue * (1 + this.baseModify);
                this.FinalValue = (this.finalBaseValue + this.finalExtraValue) * (1 + this.finalModify);
            }
        }
        #endregion

        #region Extra
        [SerializeField, HideInInspector]
        private float extraValue;
        /// <summary>
        /// ����ֵ
        /// </summary>
        [ShowInInspector, LabelText("����ֵ")]
        public float ExtraValue
        {
            get
            {
                return extraValue;
            }
            set
            {
                this.extraValue = value;
                this.finalExtraValue = this.extraValue * (1 + this.extraModify);
                this.FinalValue = (this.finalBaseValue + this.finalExtraValue) * (1 + this.finalModify);
            }
        }
        [SerializeField, HideInInspector]
        private float extraModify;
        /// <summary>
        /// ����ٷֱ�����
        /// </summary>
        [ShowInInspector, LabelText("����������")]
        public float ExtraModify
        {
            get
            {
                return extraModify;
            }
            set
            {
                this.extraModify = value;
                this.finalExtraValue = this.extraValue * (1 + this.extraModify);
                this.FinalValue = (this.finalBaseValue + this.finalExtraValue) * (1 + this.finalModify);
            }
        }
        #endregion

        #region Final
        [SerializeField, HideInInspector]
        private float _finalBaseValue;
        /// <summary>
        /// ��������ֵ
        /// </summary>
        [ShowInInspector, ReadOnly, LabelText("���ջ���ֵ")]
        public float finalBaseValue
        {
            get => _finalBaseValue;
            private set => _finalBaseValue = value;
        }

        [SerializeField, HideInInspector]
        private float _finalExtraValue;
        /// <summary>
        /// ��������ֵ
        /// </summary>
        [ShowInInspector, ReadOnly, LabelText("���ն���ֵ")]
        public float finalExtraValue
        {
            get => _finalExtraValue;
            private set => _finalExtraValue = value;
        }
        
        [SerializeField, HideInInspector]
        private float finalModify;
        /// <summary>
        /// ����ֵ����
        /// </summary>
        [ShowInInspector, LabelText("����ֵ������")]
        public float FinalModify
        {
            get
            {
                return this.finalModify;
            }
            set
            {
                this.finalModify = value;
                this.FinalValue = (this.finalBaseValue + this.finalExtraValue) * (1 + this.finalModify);
            }
        }

        [SerializeField, HideInInspector]
        private float finalValue;
        /// <summary>
        /// ��������ֵ
        /// </summary>
        [ShowInInspector, ReadOnly, LabelText("��������ֵ")]
        public float FinalValue
        {
            get => finalValue;
            private set => finalValue = value;
        }

        [SerializeField, HideInInspector]
        private float curValue;
        /// <summary>
        /// ��ǰֵ
        /// </summary>
        [ShowInInspector, LabelText("��ǰֵ")]
        public float CurValue
        {
            get
            {
                return this.curValue;
            }
            set
            {
                this.curValue = Mathf.Clamp(value, 0, this.FinalValue);
            }
        }

        [SerializeField, HideInInspector]
        public float ValuePercent
        {
            get
            {
                return this.CurValue / this.FinalValue;
            }
        }
        #endregion
    
        public static implicit operator float(NumericalFloat A)
        {
            return A.FinalValue;
        }

        [Button("���õ�ǰֵ")]
        public void ResetCurValue()
        {
            this.curValue = this.FinalValue;
        }
    }
}

