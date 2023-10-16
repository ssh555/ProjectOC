using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.NumericalProperty
{
    /// <summary>
    /// 战斗系统所用 float
    /// </summary>
    [System.Serializable]
    public struct NumericalFloat
    {
        #region Base
        [SerializeField, HideInInspector]
        private float baseValue;
        /// <summary>
        /// 基础值
        /// </summary>
        [ShowInInspector, LabelText("基础值")]
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
        /// 基础百分比修正
        /// </summary>
        [ShowInInspector, LabelText("基础修正比")]
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
        /// 额外值
        /// </summary>
        [ShowInInspector, LabelText("额外值")]
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
        /// 额外百分比修正
        /// </summary>
        [ShowInInspector, LabelText("额外修正比")]
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
        /// 基础最终值
        /// </summary>
        [ShowInInspector, ReadOnly, LabelText("最终基础值")]
        public float finalBaseValue
        {
            get => _finalBaseValue;
            private set => _finalBaseValue = value;
        }

        [SerializeField, HideInInspector]
        private float _finalExtraValue;
        /// <summary>
        /// 额外最终值
        /// </summary>
        [ShowInInspector, ReadOnly, LabelText("最终额外值")]
        public float finalExtraValue
        {
            get => _finalExtraValue;
            private set => _finalExtraValue = value;
        }
        
        [SerializeField, HideInInspector]
        private float finalModify;
        /// <summary>
        /// 最终值修正
        /// </summary>
        [ShowInInspector, LabelText("最终值修正比")]
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
        /// 最终上限值
        /// </summary>
        [ShowInInspector, ReadOnly, LabelText("最终上限值")]
        public float FinalValue
        {
            get => finalValue;
            private set => finalValue = value;
        }

        [SerializeField, HideInInspector]
        private float curValue;
        /// <summary>
        /// 当前值
        /// </summary>
        [ShowInInspector, LabelText("当前值")]
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

        [Button("重置当前值")]
        public void ResetCurValue()
        {
            this.curValue = this.FinalValue;
        }
    }
}

