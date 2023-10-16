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
        #region ����
        [LabelText("����HP")]
        public float BaseHP;

        [LabelText("��������")]
        public float BaseArmor;

        [LabelText("����������")]
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
