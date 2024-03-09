using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.CombatObject
{
    /// <summary>
    /// ʩ��ʱ���ɵ�Trigger����
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TriggerObject : MonoBehaviour
    {
        [SerializeField]
        protected LayerMask combatDetectLayer => skillData.combatDetectLayer;

        /// <summary>
        /// ʩ����
        /// </summary>
        [HideInInspector]
        protected ISpellAttackObject Instigator;

        /// <summary>
        /// ��������
        /// </summary>
        [HideInInspector]
        protected DataConfig.SkillDataConfig skillData;

        /// <summary>
        /// ʩ������
        /// </summary>
        [HideInInspector, SerializeField]
        protected DataConfig.SpellDataConfig spellData;

        private float _liveTime = 0;

        public virtual void Init(ISpellAttackObject Instigator, DataConfig.SkillDataConfig skillData, DataConfig.SpellDataConfig spellData)
        {
            this.Instigator = Instigator;
            this.skillData = skillData;
            this.spellData = spellData;

            skillActionData.Instigator = this.Instigator;
            skillActionData.SkillData = this.skillData;
            _intervalCDT = 0;

            this.triggers.Clear();
        }

        private void Awake()
        {
            this._ownCollider = this.GetComponent<Collider>();
        }

        private DataConfig.SkillActionDataStruct skillActionData = new DataConfig.SkillActionDataStruct();

        protected virtual void Update()
        {
            // �д���ʱ��
            if(this.skillData.duration > float.Epsilon)
            {
                this._liveTime += Time.deltaTime;
                if(this._liveTime >= this.skillData.duration)
                {
                    this.Destroy();
                    return;
                }
            }

            // ����ʩ��ģʽ����
            if(this.spellData.mode == DataConfig.SpellDataConfig.SpellMode.SpellToTarget)
            {
                UpdateToTarget();
            }
            else if(this.spellData.mode == DataConfig.SpellDataConfig.SpellMode.SpellToPoint)
            {
                UpdateToPoint();
            }
            else if (this.spellData.mode == DataConfig.SpellDataConfig.SpellMode.SpellToDirection)
            {
                UpdateToDirection();
            }

            _intervalCDT -= Time.deltaTime;
        }

        protected virtual void UpdateToTarget()
        {
            // ��һ֡���ƶ�����,ָ��target
            Vector3 direction = (this.spellData.data.toTarget.target.gameobject.transform.position - this.transform.position).normalized;
            // �� speed �ƶ�
            this.transform.Translate(direction * this.spellData.data.toTarget.speed * Time.deltaTime, Space.World);
        }

        protected virtual void UpdateToPoint()
        {

        }

        protected virtual void UpdateToDirection()
        {
            // ��һ֡���ƶ�����,ָ��target
            Vector3 direction = this.spellData.data.toDirection.direction.normalized;
            // �� speed �ƶ�
            this.transform.Translate(direction * this.spellData.data.toDirection.speed * Time.deltaTime, Space.World);
            if ((this.spellData.data.toDirection.distance -= this.spellData.data.toDirection.speed * Time.deltaTime) < float.Epsilon)
            {
                this.Destroy();
                return;
            }
        }

        private List<Collider> triggers = new List<Collider>();

        /// <summary>
        /// �����������ʱ
        /// </summary>
        private float _intervalCDT;

        public Collider _ownCollider;

        protected virtual void OnTriggerEnter(Collider other)
        {
            // �����ڼ���
            if ((this.combatDetectLayer.value & (1 << other.gameObject.layer)) == 0)
            {
                return;
            }
            // ���ò�Ӧ���� Instigator
            if (!this.skillData.bApplyInstigator && other.gameObject == this.Instigator.gameobject)
            {
                return;
            }
            // to-do : ͬһ�������ж��������ʱ�����¿���
            ICombatObject combatObject = other.GetComponent<ICombatObject>();
            //ICombatObject combatObject = other.GetComponentInParent<ICombatObject>();

            // ����ս������
            if (combatObject == null)
            {
                return;
            }

            // ���÷��� : Ŀ��collider.position - this.position
            Vector3 applyDirection = other.gameObject.transform.position - this.transform.position;
            // �������ض�Ŀ��
            if (this.spellData.mode == DataConfig.SpellDataConfig.SpellMode.SpellToTarget)
            {
                if(combatObject == this.spellData.data.toTarget.target)
                {
                    this.ApplyToTarget(combatObject, applyDirection);
                    this.Destroy();
                }
                return;
            }

            // ���幥��������һ�κ���ʧ
            if (this.skillData.attackMode == DataConfig.SkillDataConfig.SkillAttackMode.SingleAttack)
            {
                this.ApplyToTarget(combatObject, applyDirection);
                this.Destroy();
                return;
            }
            // Ⱥ�幥����ֱ��Ӧ�ã�ÿ�������Ӧ��һ��
            else if (this.skillData.attackMode == DataConfig.SkillDataConfig.SkillAttackMode.DirectOnlyApplyOnce)
            {
                if (!this.triggers.Contains(other))
                {
                    this.triggers.Add(other);
                    this.ApplyToTarget(combatObject, applyDirection);
                }
                return;
            }
            // Ⱥ�幥����ÿ��һ��ʱ��Ӧ��һ��
            else if (this.skillData.attackMode == DataConfig.SkillDataConfig.SkillAttackMode.IntervalApplyOnce)
            {
                // ���ù���
                if(this._intervalCDT <= float.Epsilon)
                {
                    this._intervalCDT = this.skillData.ApplyIntervalTime;
                    this.triggers.Clear();
                    this._ownCollider.enabled = true;
                    this.Invoke("ReverseTrigger", this.skillData.OnceIntervalDuration);
                }

                if (!this.triggers.Contains(other))
                {
                    this.triggers.Add(other);
                    this.ApplyToTarget(combatObject, applyDirection);
                }
                return;
            }
            else
            {
                // Ĭ��ֱ��Ӧ��
                this.ApplyToTarget(combatObject, applyDirection);
            }
        }

        private void ApplyToTarget(ICombatObject target, Vector3 applyDirection)
        {
            // to-do : ��Ҫ����һЩ�����������õ���Ϣ: ���ܻ���Ϣ��
            CombatSystem.WoundedParams woundedParams = new CombatSystem.WoundedParams();
            // Ӧ�÷���
            woundedParams.Direction = applyDirection;

            CombatSystem.Instance.ApplySkillAction(target, this.skillActionData, woundedParams);
        }
    
        private void ReverseTrigger()
        {
            this._ownCollider.enabled = !this._ownCollider.enabled;
        }
    
        private void Destroy()
        {
            if (this.spellData.mode == DataConfig.SpellDataConfig.SpellMode.SpellToPoint && this.spellData.data.toPoint.IsAttached)
            {
                this.gameObject.SetActive(false);
            }
            else
            {
                Manager.GameManager.DestroyObj(this.gameObject);
            }
        }
    }
}

