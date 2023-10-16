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
        #region 通用
        [LabelText("攻击模式"), FoldoutGroup("通用")]
        public SkillAttackMode attackMode;

        [LabelText("对施法者有效"), FoldoutGroup("通用")]
        public bool bApplyInstigator;

        [LabelText("永远存在"), FoldoutGroup("通用/持续时间")]
        public bool IsInfinite;

        [LabelText("持续时间"), FoldoutGroup("通用/持续时间"), HideIf("IsInfinite"), Range(0, float.MaxValue)]
        public float duration;

        [LabelText("技能效果"), FoldoutGroup("通用")]
        public StatusDataConfigAsset[] configs;

        [LabelText("技能作用层"), FoldoutGroup("通用")]
        public LayerMask combatDetectLayer;
        #endregion

        #region 群体多次攻击
        [LabelText("技能再次响应时间间隔"), FoldoutGroup("群体多次攻击"), ShowIf("attackMode", SkillAttackMode.IntervalApplyOnce)]
        public float ApplyIntervalTime;
        [LabelText("单次间隔应用启用trigger的时间"), FoldoutGroup("群体多次攻击"), ShowIf("attackMode", SkillAttackMode.IntervalApplyOnce)]
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

