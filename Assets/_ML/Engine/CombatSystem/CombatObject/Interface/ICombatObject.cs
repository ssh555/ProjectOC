using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.CombatObject
{
    /// <summary>
    /// 被动参与战斗的被攻击者
    /// </summary>
    public interface ICombatObject
    {
        /// <summary>
        /// 所属物体
        /// </summary>
        public GameObject gameobject { get; set; }
        /// <summary>
        /// gameobject 原生层
        /// </summary>
        public int _layer { get; set; }

        /// <summary>
        /// 战斗属性值
        /// </summary>
        public NumericalProperty.CombatPropertyStruct combatProperty { get; set; }

        protected Action.CombatTriggerAction triggerAction { get; set; }

        /// <summary>
        /// 卡肉|顿帧
        /// </summary>
        public IFreezeFrame freezeFrameAbility { get; set; }

        /// <summary>
        /// 绑定Trigger
        /// </summary>
        public void BindAction(System.Action<ISpellAttackObject, ICombatObject, object> action, Action.CombatTriggerAction.CombatTriggerMode mode)
        {
            this.triggerAction.BindAction(action, mode);
        }

        /// <summary>
        /// 解绑Trigger
        /// </summary>
        public void UnBindAction(System.Action<ISpellAttackObject, ICombatObject, object> action, Action.CombatTriggerAction.CombatTriggerMode mode)
        {
            this.triggerAction.UnBindAction(action, mode);
        }

        /// <summary>
        /// 应用伤害
        /// </summary>
        /// <param name="Instigator"></param>
        /// <param name="deltaHP"></param>
        public void ApplyDamage(ISpellAttackObject Instigator, float deltaHP, CombatSystem.WoundedParams woundedParams, FreezeFrameParams freezeFrameParams)
        {
            // 伤害必须为正数
            Debug.Assert(deltaHP > float.Epsilon);

            woundedParams.hps = new float[3] { deltaHP, this.combatProperty.HP.CurValue, this.combatProperty.HP };
            // 受伤前调用
            this.triggerAction.TriggerAction(Action.CombatTriggerAction.CombatTriggerMode.PreDamage, Instigator, this, woundedParams);

            // 应用伤害
            if (!this.combatProperty.bInvincible)
            {
                var tmp = this.combatProperty;
                tmp.HP.CurValue -= deltaHP;
                this.combatProperty = tmp;
            }

            // 应用卡肉|顿帧
            if(Instigator.freezeFrameAbility != null && Instigator.combatProperty.immuneFreezeFrame < freezeFrameParams.freezeLevel)
            {
                Instigator.freezeFrameAbility.ApplyFreezeFrame(freezeFrameParams);
            }
            if (this.freezeFrameAbility != null && this.combatProperty.immuneFreezeFrame < freezeFrameParams.freezeLevel)
            {
                this.freezeFrameAbility.ApplyFreezeFrame(freezeFrameParams);
            }

            woundedParams.hps[1] = this.combatProperty.HP.CurValue;
            // 受伤后调用
            this.triggerAction.TriggerAction(Action.CombatTriggerAction.CombatTriggerMode.PostDamage, Instigator, this, woundedParams);

            if (this.combatProperty.IsDead)
            {
                this.triggerAction.TriggerAction(Action.CombatTriggerAction.CombatTriggerMode.Death, Instigator, this, null);
            }
        }

        /// <summary>
        /// 应用治疗
        /// </summary>
        /// <param name="Instigator"></param>
        /// <param name="deltaHP"></param>
        public void ApplyCure(ISpellAttackObject Instigator, float deltaHP)
        {
            // 治疗必须为正数
            Debug.Assert(deltaHP > float.Epsilon);

            // 治疗前调用
            this.triggerAction.TriggerAction(Action.CombatTriggerAction.CombatTriggerMode.PreCure, Instigator, this, new float[3] { deltaHP, this.combatProperty.HP.CurValue, this.combatProperty.HP });

            // 应用治疗
            var tmp = this.combatProperty;
            tmp.HP.CurValue += deltaHP;
            this.combatProperty = tmp;

            // 治疗后调用
            this.triggerAction.TriggerAction(Action.CombatTriggerAction.CombatTriggerMode.PostCure, Instigator, this, new float[3] { deltaHP, this.combatProperty.HP.CurValue, this.combatProperty.HP });
        }
    
        /// <summary>
        /// 应用 buff
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
        /// to-do : 设置是否能被命中
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

