using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ML.Engine.CombatSystem.DataConfig.StatusDataConfig;

namespace ML.Engine.CombatSystem.DataConfig
{
    [CreateAssetMenu(fileName = "StatusConfig", menuName = "ML/CombatSystem/Config/StatusConfig", order = 1)]
    public class StatusDataConfigAsset : SerializedScriptableObject
    {
        #region 通用
        [LabelText("ID"), FoldoutGroup("通用")]
        public string ID;

        [LabelText("类型"), FoldoutGroup("通用")]
        public StatusEffect mode;
        #endregion

        #region Attack
        [LabelText("攻击倍率"), FoldoutGroup("Attack"), ShowIf("mode", StatusEffect.Attack)]
        public float AttackRatio;
        [LabelText("受击类型"), FoldoutGroup("Attack"), ShowIf("mode", StatusEffect.Attack)]
        public WoundedMode woundedMode;
        [LabelText("卡肉|顿帧参数"), FoldoutGroup("Attack"), ShowIf("mode", StatusEffect.Attack)]

        public CombatObject.FreezeFrameParams freezeFrameParams;
        #endregion

        #region Cure
        [LabelText("治疗量"), FoldoutGroup("Cure"), ShowIf("mode", StatusEffect.Cure)]
        public float CureValue;
        #endregion

        #region BUFF
        [LabelText("永远存在"), FoldoutGroup("BUFF"), ShowIf("mode", StatusEffect.Buff)]
        public bool IsInfinite;

        [LabelText("持续时间"), FoldoutGroup("BUFF"), HideIf("@this.mode != ML.Engine.CombatSystem.DataConfig.StatusDataConfig.StatusEffect.Buff || this.IsInfinite"), Range(0, float.MaxValue)]
        public float duration;
        #endregion

        public StatusDataConfig ToData()
        {
            StatusDataConfig data = new StatusDataConfig();

            data.ID = this.ID;

            data.mode = this.mode;
            if(this.mode == StatusEffect.Attack)
            {
                data.AttackRatio = this.AttackRatio;
                data.woundedMode = this.woundedMode;
                data.freezeFrameParams = this.freezeFrameParams;
            }
            else if(this.mode == StatusEffect.Cure)
            {
                data.cureValue = this.CureValue;
            }
            else if(this.mode == StatusEffect.Buff)
            {
                if (this.IsInfinite)
                {
                    data.duration = -1;
                }
                else
                {
                    data.duration = this.duration;
                }
            }

            return data;
        }
    }

}
