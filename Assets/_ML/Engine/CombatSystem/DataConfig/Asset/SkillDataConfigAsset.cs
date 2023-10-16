using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ML.Engine.CombatSystem.DataConfig.SkillDataConfig;

namespace ML.Engine.CombatSystem.DataConfig
{
    [CreateAssetMenu(fileName = "SkillConfig", menuName = "ML/CombatSystem/Config/SkillConfig", order = 1)]
    public class SkillDataConfigAsset : SerializedScriptableObject
    {
        #region ͨ��
        [LabelText("����ģʽ"), FoldoutGroup("ͨ��")]
        public SkillAttackMode attackMode;

        [LabelText("��ʩ������Ч"), FoldoutGroup("ͨ��")]
        public bool bApplyInstigator;

        [LabelText("��Զ����"), FoldoutGroup("ͨ��/����ʱ��")]
        public bool IsInfinite;

        [LabelText("����ʱ��"), FoldoutGroup("ͨ��/����ʱ��"), HideIf("IsInfinite"), Range(0, float.MaxValue)]
        public float duration;

        [LabelText("����Ч��"), FoldoutGroup("ͨ��")]
        public StatusDataConfigAsset[] configs;

        [LabelText("�������ò�"), FoldoutGroup("ͨ��")]
        public LayerMask combatDetectLayer;
        #endregion

        #region Ⱥ���ι���
        [LabelText("�����ٴ���Ӧʱ����"), FoldoutGroup("Ⱥ���ι���"), ShowIf("attackMode", SkillAttackMode.IntervalApplyOnce)]
        public float ApplyIntervalTime;
        [LabelText("���μ��Ӧ������trigger��ʱ��"), FoldoutGroup("Ⱥ���ι���"), ShowIf("attackMode", SkillAttackMode.IntervalApplyOnce)]
        public float OnceIntervalDuration;
        #endregion

        public SkillDataConfig ToData()
        {
            SkillDataConfig data = new SkillDataConfig();
            if(this.configs.Length > 0)
            {
                data.configs = new StatusDataConfig[this.configs.Length];
                for(int i = 0; i < this.configs.Length; ++i)
                {
                    data.configs[i] = this.configs[i].ToData();
                }
            }

            if (this.IsInfinite)
            {
                data.duration = -1;
            }
            else
            {
                data.duration = this.duration;
            }

            data.attackMode = this.attackMode;
            if(this.attackMode == SkillAttackMode.IntervalApplyOnce)
            {
                data.ApplyIntervalTime = this.ApplyIntervalTime;
                data.OnceIntervalDuration = this.OnceIntervalDuration;
            }

            data.bApplyInstigator = this.bApplyInstigator;

            data.combatDetectLayer = this.combatDetectLayer;

            return data;
        }
    }
}

