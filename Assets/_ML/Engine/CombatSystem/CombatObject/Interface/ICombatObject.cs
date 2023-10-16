using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.CombatObject
{
    /// <summary>
    /// ��������ս���ı�������
    /// </summary>
    public interface ICombatObject
    {
        /// <summary>
        /// ��������
        /// </summary>
        public GameObject gameobject { get; set; }
        /// <summary>
        /// gameobject ԭ����
        /// </summary>
        public int _layer { get; set; }

        /// <summary>
        /// ս������ֵ
        /// </summary>
        public NumericalProperty.CombatPropertyStruct combatProperty { get; set; }

        protected Action.CombatTriggerAction triggerAction { get; set; }

        /// <summary>
        /// ����|��֡
        /// </summary>
        public IFreezeFrame freezeFrameAbility { get; set; }

        /// <summary>
        /// ��Trigger
        /// </summary>
        public void BindAction(System.Action<ISpellAttackObject, ICombatObject, object> action, Action.CombatTriggerAction.CombatTriggerMode mode)
        {
            this.triggerAction.BindAction(action, mode);
        }

        /// <summary>
        /// ���Trigger
        /// </summary>
        public void UnBindAction(System.Action<ISpellAttackObject, ICombatObject, object> action, Action.CombatTriggerAction.CombatTriggerMode mode)
        {
            this.triggerAction.UnBindAction(action, mode);
        }

        /// <summary>
        /// Ӧ���˺�
        /// </summary>
        /// <param name="Instigator"></param>
        /// <param name="deltaHP"></param>
        public void ApplyDamage(ISpellAttackObject Instigator, float deltaHP, CombatSystem.WoundedParams woundedParams, FreezeFrameParams freezeFrameParams)
        {
            // �˺�����Ϊ����
            Debug.Assert(deltaHP > float.Epsilon);

            woundedParams.hps = new float[3] { deltaHP, this.combatProperty.HP.CurValue, this.combatProperty.HP };
            // ����ǰ����
            this.triggerAction.TriggerAction(Action.CombatTriggerAction.CombatTriggerMode.PreDamage, Instigator, this, woundedParams);

            // Ӧ���˺�
            if (!this.combatProperty.bInvincible)
            {
                var tmp = this.combatProperty;
                tmp.HP.CurValue -= deltaHP;
                this.combatProperty = tmp;
            }

            // Ӧ�ÿ���|��֡
            if(Instigator.freezeFrameAbility != null && Instigator.combatProperty.immuneFreezeFrame < freezeFrameParams.freezeLevel)
            {
                Instigator.freezeFrameAbility.ApplyFreezeFrame(freezeFrameParams);
            }
            if (this.freezeFrameAbility != null && this.combatProperty.immuneFreezeFrame < freezeFrameParams.freezeLevel)
            {
                this.freezeFrameAbility.ApplyFreezeFrame(freezeFrameParams);
            }

            woundedParams.hps[1] = this.combatProperty.HP.CurValue;
            // ���˺����
            this.triggerAction.TriggerAction(Action.CombatTriggerAction.CombatTriggerMode.PostDamage, Instigator, this, woundedParams);

            if (this.combatProperty.IsDead)
            {
                this.triggerAction.TriggerAction(Action.CombatTriggerAction.CombatTriggerMode.Death, Instigator, this, null);
            }
        }

        /// <summary>
        /// Ӧ������
        /// </summary>
        /// <param name="Instigator"></param>
        /// <param name="deltaHP"></param>
        public void ApplyCure(ISpellAttackObject Instigator, float deltaHP)
        {
            // ���Ʊ���Ϊ����
            Debug.Assert(deltaHP > float.Epsilon);

            // ����ǰ����
            this.triggerAction.TriggerAction(Action.CombatTriggerAction.CombatTriggerMode.PreCure, Instigator, this, new float[3] { deltaHP, this.combatProperty.HP.CurValue, this.combatProperty.HP });

            // Ӧ������
            var tmp = this.combatProperty;
            tmp.HP.CurValue += deltaHP;
            this.combatProperty = tmp;

            // ���ƺ����
            this.triggerAction.TriggerAction(Action.CombatTriggerAction.CombatTriggerMode.PostCure, Instigator, this, new float[3] { deltaHP, this.combatProperty.HP.CurValue, this.combatProperty.HP });
        }
    
        /// <summary>
        /// Ӧ�� buff
        /// </summary>
        /// <param name="Instigator"></param>
        /// <param name=""></param>
        public void ApplyBuff(ISpellAttackObject Instigator, DataConfig.StatusDataConfig buff)
        {
            this.triggerAction.TriggerAction(Action.CombatTriggerAction.CombatTriggerMode.PreReceiveBuff, Instigator, this, this.combatProperty.buffList);

            Action.UpdateStatusAction updateStatus = this.gameobject.AddComponent<Action.UpdateStatusAction>();
            updateStatus.Init(Instigator, this, buff);
            this.combatProperty.buffList.Add(updateStatus);

            this.triggerAction.TriggerAction(Action.CombatTriggerAction.CombatTriggerMode.PostReceiveBuff, Instigator, this, this.combatProperty.buffList);
        }

        public void TriggerAction(Action.CombatTriggerAction.CombatTriggerMode mode, ISpellAttackObject Instigator, ICombatObject Victim, object Params)
        {
            this.triggerAction.TriggerAction(mode, Instigator, this, this.combatProperty.buffList);
        }

        /// <summary>
        /// to-do : �����Ƿ��ܱ�����
        /// </summary>
        /// <param name="isTrigger"></param>
        public void SetCanTriggerHit(bool isTrigger)
        {
            if (this.gameobject.layer != (LayerMask.NameToLayer("IgnoreCombat")))
            {
                this._layer = this.gameobject.layer;
            }
            if (isTrigger == true)
            {
                this.gameobject.layer = this._layer;
            }
            else
            {
                this.gameobject.layer = LayerMask.NameToLayer("IgnoreCombat");
            }
        }
    }
}

