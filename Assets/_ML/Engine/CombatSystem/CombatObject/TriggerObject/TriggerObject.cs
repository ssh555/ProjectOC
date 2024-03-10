using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.CombatObject
{
    /// <summary>
    /// 施法时生成的Trigger所用
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TriggerObject : MonoBehaviour
    {
        [SerializeField]
        protected LayerMask combatDetectLayer => skillData.combatDetectLayer;

        /// <summary>
        /// 施法者
        /// </summary>
        [HideInInspector]
        protected ISpellAttackObject Instigator;

        /// <summary>
        /// 技能数据
        /// </summary>
        [HideInInspector]
        protected DataConfig.SkillDataConfig skillData;

        /// <summary>
        /// 施法数据
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
            // 有存在时间
            if(this.skillData.duration > float.Epsilon)
            {
                this._liveTime += Time.deltaTime;
                if(this._liveTime >= this.skillData.duration)
                {
                    this.Destroy();
                    return;
                }
            }

            // 根据施法模式更新
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
            // 这一帧的移动方向,指向target
            Vector3 direction = (this.spellData.data.toTarget.target.gameobject.transform.position - this.transform.position).normalized;
            // 以 speed 移动
            this.transform.Translate(direction * this.spellData.data.toTarget.speed * Time.deltaTime, Space.World);
        }

        protected virtual void UpdateToPoint()
        {

        }

        protected virtual void UpdateToDirection()
        {
            // 这一帧的移动方向,指向target
            Vector3 direction = this.spellData.data.toDirection.direction.normalized;
            // 以 speed 移动
            this.transform.Translate(direction * this.spellData.data.toDirection.speed * Time.deltaTime, Space.World);
            if ((this.spellData.data.toDirection.distance -= this.spellData.data.toDirection.speed * Time.deltaTime) < float.Epsilon)
            {
                this.Destroy();
                return;
            }
        }

        private List<Collider> triggers = new List<Collider>();

        /// <summary>
        /// 攻击间隔倒计时
        /// </summary>
        private float _intervalCDT;

        public Collider _ownCollider;

        protected virtual void OnTriggerEnter(Collider other)
        {
            // 不属于检测层
            if ((this.combatDetectLayer.value & (1 << other.gameObject.layer)) == 0)
            {
                return;
            }
            // 启用不应用于 Instigator
            if (!this.skillData.bApplyInstigator && other.gameObject == this.Instigator.gameobject)
            {
                return;
            }
            // to-do : 同一个对象有多个命中体时需重新考虑
            ICombatObject combatObject = other.GetComponent<ICombatObject>();
            //ICombatObject combatObject = other.GetComponentInParent<ICombatObject>();

            // 不是战斗对象
            if (combatObject == null)
            {
                return;
            }

            // 作用方向 : 目标collider.position - this.position
            Vector3 applyDirection = other.gameObject.transform.position - this.transform.position;
            // 作用于特定目标
            if (this.spellData.mode == DataConfig.SpellDataConfig.SpellMode.SpellToTarget)
            {
                if(combatObject == this.spellData.data.toTarget.target)
                {
                    this.ApplyToTarget(combatObject, applyDirection);
                    this.Destroy();
                }
                return;
            }

            // 单体攻击，作用一次后消失
            if (this.skillData.attackMode == DataConfig.SkillDataConfig.SkillAttackMode.SingleAttack)
            {
                this.ApplyToTarget(combatObject, applyDirection);
                this.Destroy();
                return;
            }
            // 群体攻击，直接应用，每个对象仅应用一次
            else if (this.skillData.attackMode == DataConfig.SkillDataConfig.SkillAttackMode.DirectOnlyApplyOnce)
            {
                if (!this.triggers.Contains(other))
                {
                    this.triggers.Add(other);
                    this.ApplyToTarget(combatObject, applyDirection);
                }
                return;
            }
            // 群体攻击，每隔一段时间应用一次
            else if (this.skillData.attackMode == DataConfig.SkillDataConfig.SkillAttackMode.IntervalApplyOnce)
            {
                // 启用攻击
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
                // 默认直接应用
                this.ApplyToTarget(combatObject, applyDirection);
            }
        }

        private void ApplyToTarget(ICombatObject target, Vector3 applyDirection)
        {
            // to-do : 还要传递一些其他可能有用的信息: 如受击信息等
            CombatSystem.WoundedParams woundedParams = new CombatSystem.WoundedParams();
            // 应用方向
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

