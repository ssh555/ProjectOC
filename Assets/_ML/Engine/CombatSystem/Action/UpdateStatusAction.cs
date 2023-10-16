using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.Action
{
    // to-do : ��������ƽ�һ��ʵ�� & �����һ�� Manager ���������� Status ���������� Mono
    /// <summary>
    /// BuffStatus ��������
    /// </summary>
    public class UpdateStatusAction : MonoBehaviour
    {
        /// <summary>
        /// ������
        /// </summary>
        protected CombatObject.ISpellAttackObject Instigator;

        /// <summary>
        /// ����ս������
        /// </summary>
        protected CombatObject.ICombatObject combatObject;

        /// <summary>
        /// �˴��ж���buffЧ��
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
        /// ���ʱĬ��Ӧ��һ��
        /// </summary>
        private void Start()
        {
            this.ApplyBuffEffect();
        }

        private void Update()
        {
            // �д���ʱ��
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

