using ML.Engine.CombatSystem.CombatObject;
using UnityEngine;

namespace ML.Engine.CombatSystem.Action
{
    public class CombatTriggerAction
    {
        public enum CombatTriggerMode
        {
            /// <summary>
            /// ����ǰ
            /// </summary>
            PreDamage,
            /// <summary>
            /// ���˺�
            /// </summary>
            PostDamage,
            /// <summary>
            /// ����ǰ
            /// </summary>
            PreCure,
            /// <summary>
            /// ���ƺ�
            /// </summary>
            PostCure,
            /// <summary>
            /// ��ȡbuffǰ
            /// </summary>
            PreReceiveBuff,
            /// <summary>
            /// ��ȡbuff��
            /// </summary>
            PostReceiveBuff,
            /// <summary>
            /// buff ����ǰ
            /// </summary>
            PreRemoveBuff,
            /// <summary>
            /// buff ������
            /// </summary>
            PostRemoveBuff,
            /// <summary>
            /// ����ʱ
            /// </summary>
            Death,
        }

        #region TriggerAction
        /// <summary>
        /// ����ǰ Trigger
        /// </summary>
        public System.Action<ISpellAttackObject, ICombatObject, object> PreDamageAction;

        /// <summary>
        /// ���˺� Trigger
        /// </summary>
        public System.Action<ISpellAttackObject, ICombatObject, object> PostDamageAction;

        /// <summary>
        /// ����ǰ Trigger
        /// </summary>
        public System.Action<ISpellAttackObject, ICombatObject, object> PreCureAction;

        /// <summary>
        /// ���ƺ� Trigger
        /// </summary>
        public System.Action<ISpellAttackObject, ICombatObject, object> PostCureAction;

        /// <summary>
        /// ����buffǰ Trigger
        /// </summary>
        public System.Action<ISpellAttackObject, ICombatObject, object> PreReceicveBuffAction;

        /// <summary>
        /// ����buff�� Trigger
        /// </summary>
        public System.Action<ISpellAttackObject, ICombatObject, object> PostReceicveBuffAction;

        /// <summary>
        /// buff ����ǰ Trigger
        /// </summary>
        public System.Action<ISpellAttackObject, ICombatObject, object> PreRemoveBuffAction;

        /// <summary>
        /// buff ������ Trigger
        /// </summary>
        public System.Action<ISpellAttackObject, ICombatObject, object> PostRemoveBuffAction;

        /// <summary>
        /// ����ʱ
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

