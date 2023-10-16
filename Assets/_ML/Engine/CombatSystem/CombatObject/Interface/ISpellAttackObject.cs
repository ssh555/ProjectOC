using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.CombatObject
{
    public interface ISpellAttackObject
    {
        /// <summary>
        /// ��������
        /// </summary>
        public GameObject gameobject { get; set; }

        /// <summary>
        /// ս������ֵ
        /// </summary>
        public NumericalProperty.CombatPropertyStruct combatProperty { get; set; }

        /// <summary>
        /// ����|��֡
        /// </summary>
        public IFreezeFrame freezeFrameAbility { get; set; }

        /// <summary>
        /// ʩ��
        /// </summary>
        /// <param name="skillData"></param>
        /// <param name=""></param>
        public void SpellAction(DataConfig.SkillDataConfig skillData, DataConfig.SpellDataConfig spellData)
        {
            if(spellData.mode == DataConfig.SpellDataConfig.SpellMode.SpellToTarget)
            {
                // trigger Ϊnull : ֱ��Ӧ��
                if(spellData.data.toTarget.trigger == null)
                {
                    Debug.Assert(spellData.data.toTarget.target != null);

                    DataConfig.SkillActionDataStruct skillActionData = new DataConfig.SkillActionDataStruct();
                    skillActionData.Instigator = this;
                    skillActionData.SkillData = skillData;

                    CombatSystem.Instance.ApplySkillAction(spellData.data.toTarget.target, skillActionData);
                }
                // trigger ��Ϊnull : ���� TriggerObject
                else
                {
                    Debug.Assert(spellData.data.toTarget.trigger.gameObject != null);

                    TriggerObject triggerObject = GameObject.Instantiate(spellData.data.toTarget.trigger.gameObject, spellData.data.toTarget.spawnPoint, spellData.data.toTarget.spawnRot).GetComponent<TriggerObject>();
                    triggerObject.Init(this, skillData, spellData);
                }
            }
            else if (spellData.mode == DataConfig.SpellDataConfig.SpellMode.SpellToPoint)
            {
                Debug.Assert(spellData.data.toPoint.trigger != null);
                TriggerObject triggerObject = null;
                if (spellData.data.toPoint.IsAttached)
                {
                    triggerObject = spellData.data.toPoint.trigger;
                    triggerObject.gameObject.SetActive(true);
                }
                else
                {
                    triggerObject = GameObject.Instantiate(spellData.data.toPoint.trigger.gameObject, spellData.data.toPoint.targetPoint, spellData.data.toPoint.spawnRot).GetComponent<TriggerObject>();
                }
                triggerObject.Init(this, skillData, spellData);
                triggerObject._ownCollider.enabled = spellData.data.toPoint.bTriggerOnAwake;
            }
            else if (spellData.mode == DataConfig.SpellDataConfig.SpellMode.SpellToDirection)
            {
                Debug.Assert(spellData.data.toDirection.trigger != null);

                TriggerObject triggerObject = GameObject.Instantiate(spellData.data.toPoint.trigger.gameObject, spellData.data.toDirection.spawnPoint, spellData.data.toDirection.spawnRot).GetComponent<TriggerObject>();
                triggerObject.Init(this, skillData, spellData);
                triggerObject._ownCollider.enabled = spellData.data.toDirection.bTriggerOnAwake;
            }
        }
    }
}

