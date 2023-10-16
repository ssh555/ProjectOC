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
        #region ͨ��
        [LabelText("ID"), FoldoutGroup("ͨ��")]
        public string ID;

        [LabelText("����"), FoldoutGroup("ͨ��")]
        public StatusEffect mode;
        #endregion

        #region Attack
        [LabelText("��������"), FoldoutGroup("Attack"), ShowIf("mode", StatusEffect.Attack)]
        public float AttackRatio;
        [LabelText("�ܻ�����"), FoldoutGroup("Attack"), ShowIf("mode", StatusEffect.Attack)]
        public WoundedMode woundedMode;
        [LabelText("����|��֡����"), FoldoutGroup("Attack"), ShowIf("mode", StatusEffect.Attack)]

        public CombatObject.FreezeFrameParams freezeFrameParams;
        #endregion

        #region Cure
        [LabelText("������"), FoldoutGroup("Cure"), ShowIf("mode", StatusEffect.Cure)]
        public float CureValue;
        #endregion

        #region BUFF
        [LabelText("��Զ����"), FoldoutGroup("BUFF"), ShowIf("mode", StatusEffect.Buff)]
        public bool IsInfinite;

        [LabelText("����ʱ��"), FoldoutGroup("BUFF"), HideIf("@this.mode != ML.Engine.CombatSystem.DataConfig.StatusDataConfig.StatusEffect.Buff || this.IsInfinite"), Range(0, float.MaxValue)]
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
