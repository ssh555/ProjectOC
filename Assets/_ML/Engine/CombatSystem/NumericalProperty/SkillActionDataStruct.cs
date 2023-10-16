using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.DataConfig
{
    /// <summary>
    /// ʩ����������
    /// </summary>
    public struct SkillActionDataStruct
    {
        /// <summary>
        /// ʩ����
        /// </summary>
        public CombatObject.ISpellAttackObject Instigator;

        /// <summary>
        /// װ������
        /// </summary>
        public SkillDataConfig SkillData;
    }
}

