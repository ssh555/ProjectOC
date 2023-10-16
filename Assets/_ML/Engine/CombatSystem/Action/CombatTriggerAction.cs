using ML.Engine.CombatSystem.CombatObject;
using UnityEngine;

namespace ML.Engine.CombatSystem.Action
{
    public class CombatTriggerAction
    {
        public enum CombatTriggerMode
        {
            /// <summary>
            /// 受伤前
            /// </summary>
            PreDamage,
            /// <summary>
            /// 受伤后
            /// </summary>
            PostDamage,
            /// <summary>
            /// 治疗前
            /// </summary>
            PreCure,
            /// <summary>
            /// 治疗后
            /// </summary>
            PostCure,
            /// <summary>
            /// 获取buff前
            /// </summary>
            PreReceiveBuff,
            /// <summary>
            /// 获取buff后
            /// </summary>
            PostReceiveBuff,
            /// <summary>
            /// buff 结束前
            /// </summary>
            PreRemoveBuff,
            /// <summary>
            /// buff 结束后
            /// </summary>
            PostRemoveBuff,
            /// <summary>
            /// 死亡时
            /// </summary>
            Death,
        }

        #region TriggerAction
        /// <summary>
        /// 受伤前 Trigger
        /// </summary>
        public System.Action<ISpellAttackObject, ICombatObject, object> PreDamageAction;

        /// <summary>
        /// 受伤后 Trigger
        /// </summary>
        public System.Action<ISpellAttackObject, ICombatObject, object> PostDamageAction;

        /// <summary>
        /// 治疗前 Trigger
        /// </summary>
        public System.Action<ISpellAttackObject, ICombatObject, object> PreCureAction;

        /// <summary>
        /// 治疗后 Trigger
        /// </summary>
        public System.Action<ISpellAttackObject, ICombatObject, object> PostCureAction;

        /// <summary>
        /// 接受buff前 Trigger
        /// </summary>
        public System.Action<ISpellAttackObject, ICombatObject, object> PreReceicveBuffAction;

        /// <summary>
        /// 接受buff后 Trigger
        /// </summary>
        public System.Action<ISpellAttackObject, ICombatObject, object> PostReceicveBuffAction;

        /// <summary>
        /// buff 结束前 Trigger
        /// </summary>
        public System.Action<ISpellAttackObject, ICombatObject, object> PreRemoveBuffAction;

        /// <summary>
        /// buff 结束后 Trigger
        /// </summary>
        public System.Action<ISpellAttackObject, ICombatObject, object> PostRemoveBuffAction;

        /// <summary>
        /// 死亡时
        /// </summary>
        public System.Action<ISpellAttackObject, ICombatObject, object> DeathAction;
        #endregion

        public void BindAction(System.Action<ISpellAttackObject, ICombatObject, object> action, CombatTriggerMode mode)
        {
            switch (mode)
            {
                case CombatTriggerMode.PreDamage:
                    this.PreDamageAction += action;
                    break;
                case CombatTriggerMode.PostDamage:
                    this.PostDamageAction += action;
                    break;
                case CombatTriggerMode.PreCure:
                    this.PreCureAction += action;
                    break;
                case CombatTriggerMode.PostCure:
                    this.PostCureAction += action;
                    break;
                case CombatTriggerMode.PreReceiveBuff:
                    this.PreReceicveBuffAction += action;
                    break;
                case CombatTriggerMode.PostReceiveBuff:
                    this.PostReceicveBuffAction += action;
                    break;
                case CombatTriggerMode.PreRemoveBuff:
                    this.PreRemoveBuffAction += action;
                    break;
                case CombatTriggerMode.PostRemoveBuff:
                    this.PostRemoveBuffAction += action;
                    break;
                case CombatTriggerMode.Death:
                    this.DeathAction += action;
                    break;
            }
        }

        public void UnBindAction(System.Action<ISpellAttackObject, ICombatObject, object> action, CombatTriggerMode mode)
        {
            switch (mode)
            {
                case CombatTriggerMode.PreDamage:
                    this.PreDamageAction -= action;
                    break;
                case CombatTriggerMode.PostDamage:
                    this.PostDamageAction -= action;
                    break;
                case CombatTriggerMode.PreCure:
                    this.PreCureAction -= action;
                    break;
                case CombatTriggerMode.PostCure:
                    this.PostCureAction -= action;
                    break;
                case CombatTriggerMode.PreReceiveBuff:
                    this.PreReceicveBuffAction -= action;
                    break;
                case CombatTriggerMode.PostReceiveBuff:
                    this.PostReceicveBuffAction -= action;
                    break;
                case CombatTriggerMode.PreRemoveBuff:
                    this.PreRemoveBuffAction -= action;
                    break;
                case CombatTriggerMode.PostRemoveBuff:
                    this.PostRemoveBuffAction -= action;
                    break;
                case CombatTriggerMode.Death:
                    this.DeathAction -= action;
                    break;
            }
        }

        public void TriggerAction(CombatTriggerMode mode, ISpellAttackObject Instigator, ICombatObject Victim, object Params)
        {
            switch (mode)
            {
                case CombatTriggerMode.PreDamage:
                    this.PreDamageAction?.Invoke(Instigator, Victim, Params);
                    break;
                case CombatTriggerMode.PostDamage:
                    this.PostDamageAction?.Invoke(Instigator, Victim, Params);
                    break;
                case CombatTriggerMode.PreCure:
                    this.PreCureAction?.Invoke(Instigator, Victim, Params);
                    break;
                case CombatTriggerMode.PostCure:
                    this.PostCureAction?.Invoke(Instigator, Victim, Params);
                    break;
                case CombatTriggerMode.PreReceiveBuff:
                    this.PreReceicveBuffAction?.Invoke(Instigator, Victim, Params);
                    break;
                case CombatTriggerMode.PostReceiveBuff:
                    this.PostReceicveBuffAction?.Invoke(Instigator, Victim, Params);
                    break;
                case CombatTriggerMode.PreRemoveBuff:
                    this.PreRemoveBuffAction?.Invoke(Instigator, Victim, Params);
                    break;
                case CombatTriggerMode.PostRemoveBuff:
                    this.PostRemoveBuffAction?.Invoke(Instigator, Victim, Params);
                    break;
                case CombatTriggerMode.Death:
                    this.DeathAction?.Invoke(Instigator, Victim, Params);
                    break;
            }
        }
    }
}

