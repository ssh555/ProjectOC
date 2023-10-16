using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.Action
{
    // to-do : 待根据设计进一步实现 & 可设计一个 Manager ，管理所有 Status ，避免过多的 Mono
    /// <summary>
    /// BuffStatus 更新所用
    /// </summary>
    public class UpdateStatusAction : MonoBehaviour
    {
        /// <summary>
        /// 作用者
        /// </summary>
        protected CombatObject.ISpellAttackObject Instigator;

        /// <summary>
        /// 所属战斗对象
        /// </summary>
        protected CombatObject.ICombatObject combatObject;

        /// <summary>
        /// 此次行动的buff效果
        /// </summary>
        protected DataConfig.StatusDataConfig buff;
        public DataConfig.StatusDataConfig Buff
        {
            get
            {
                return buff;
            }
        }

        private float _liveTime = 0;

        public void Init(CombatObject.ISpellAttackObject Instigator, CombatObject.ICombatObject combatObject, DataConfig.StatusDataConfig buff)
        {
            this.Instigator = Instigator;
            this.combatObject = combatObject;
            this.buff = buff;
        }

        /// <summary>
        /// 添加时默认应用一次
        /// </summary>
        private void Start()
        {
            this.ApplyBuffEffect();
        }

        private void Update()
        {
            // 有存在时间
            if (this.buff.duration > float.Epsilon)
            {
                this._liveTime += Time.deltaTime;
                if (this._liveTime >= this.buff.duration)
                {
                    Destroy(this);
                    this.enabled = false;
                    return;
                }
            }
        }

        private void OnDestroy()
        {
            this.combatObject.TriggerAction(Action.CombatTriggerAction.CombatTriggerMode.PreRemoveBuff, null, this.combatObject, this.combatObject.combatProperty.buffList);

            this.combatObject.combatProperty.buffList.Remove(this);

            this.combatObject.TriggerAction(Action.CombatTriggerAction.CombatTriggerMode.PostRemoveBuff, null, this.combatObject, this.combatObject.combatProperty.buffList);
        }
    
        protected virtual void ApplyBuffEffect()
        {

        }
    }
}

