using ML.Engine.CombatSystem.Action;
using ML.Engine.CombatSystem.NumericalProperty;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.CombatObject
{
    public class MonoCombatObject : MonoBehaviour, ISpellAttackObject, ICombatObject
    {
        #region ISpellAttackObject
        public ISpellAttackObject spellAttackObject { get { return this as ISpellAttackObject; } }

        GameObject ISpellAttackObject.gameobject { get => this.gameObject; set => throw new System.NotImplementedException(); }
        CombatPropertyStruct ISpellAttackObject.combatProperty { get => this._combatProperty; set => this._combatProperty = value; }

        #endregion

        #region ICombatObject
        public ICombatObject combatObject { get { return this as ICombatObject; } }

        GameObject ICombatObject.gameobject { get => this.gameObject; set => throw new System.NotImplementedException(); }
        CombatPropertyStruct ICombatObject.combatProperty { get => this._combatProperty; set => this._combatProperty = value; }
        CombatTriggerAction ICombatObject.triggerAction { get => this._triggerAction; set => throw new System.NotImplementedException(); }
        public int _layer { get; set; }

        #endregion

        public IFreezeFrame freezeFrameAbility { get; set; }

        [ShowInInspector]
        public CombatPropertyStruct _combatProperty;
        protected CombatTriggerAction _triggerAction = new CombatTriggerAction();
    }

}
