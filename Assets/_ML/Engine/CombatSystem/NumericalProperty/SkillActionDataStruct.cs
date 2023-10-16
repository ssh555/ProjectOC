using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.DataConfig
{
    /// <summary>
    /// 施法技能数据
    /// </summary>
    public struct SkillActionDataStruct
    {
        /// <summary>
        /// 施法者
        /// </summary>
        public CombatObject.ISpellAttackObject Instigator;

        /// <summary>
        /// 装饰数据
        /// </summary>
        public SkillDataConfig SkillData;
    }
}

