using ML.Engine.CombatSystem.NumericalProperty;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.DataConfig
{
    [CreateAssetMenu(fileName = "CharacterConfig", menuName = "ML/CombatSystem/Config/CharacterConfig", order = 1)]
    public class CombatDataConfigAsset : SerializedScriptableObject
    {
        #region 配置
        [LabelText("基础HP")]
        public float BaseHP;

        [LabelText("基础护甲")]
        public float BaseArmor;

        [LabelText("基础攻击力")]
        public float BaseAttackValue;
        #endregion

        public CombatPropertyStruct ToData()
        {
            CombatPropertyStruct data = new CombatPropertyStruct();

            data.HP.BaseValue = this.BaseHP;
            data.HP.ResetCurValue();

            data.Armor.BaseValue = this.BaseArmor;
            data.Armor.ResetCurValue();

            data.AttackValue.BaseValue = this.BaseAttackValue;
            data.AttackValue.ResetCurValue();

            data.bInvincible = false;

            data.buffList = new List<Action.UpdateStatusAction>();
            return data;
        }

    }

}
