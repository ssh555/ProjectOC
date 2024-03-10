using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.CombatObject.Character
{
    /// <summary>
    /// 掉落物配置
    /// </summary>
    [System.Serializable]
    public struct DroppedItem
    {
        /// <summary>
        /// 掉落物ID
        /// </summary>
        [LabelText("ItemID")]
        public string id;
        /// <summary>
        /// 掉落数量
        /// </summary>
        [LabelText("掉落数量"), Range(1, int.MaxValue)]
        public int amount;
        /// <summary>
        /// 掉落几率
        /// </summary>
        [LabelText("掉落几率"), Range(0, 1)]
        public float probabilty;

        /// <summary>
        /// 尝试 roll 点获取掉落物
        /// </summary>
        /// <returns>返回值为null 表示没有掉落物生成</returns>
        public ML.Engine.InventorySystem.Item TryGetDroppedItem()
        {
            return Random.value <= probabilty ? ML.Engine.InventorySystem.ItemManager.Instance.SpawnItem(this.id) : null;
        }
    }

    /// <summary>
    /// to-do : 为用于联机，待修改
    /// </summary>
    [RequireComponent(typeof(Collider), typeof(Renderer))]
    public class CollectionCharacter : MonoCombatObject
    {
        #region Property
        /// <summary>
        /// 最大受击次数
        /// </summary>
        [LabelText("最大受击次数"), PropertyRange(1, int.MaxValue), SerializeField, FoldoutGroup("采集参数")]
        protected int MaxHP = 3;

        /// <summary>
        /// 剩余可受击次数
        /// </summary>
        [SerializeField, ShowInInspector, ReadOnly, LabelText("剩余HP"), FoldoutGroup("采集参数")]
        protected int hp;

        /// <summary>
        /// 掉落物列表
        /// </summary>
        [SerializeField, LabelText("掉落物列表"), FoldoutGroup("采集参数")]
        protected DroppedItem[] droppedItems;

        /// <summary>
        /// 死亡后刷新倒计时
        /// </summary>
        [SerializeField, LabelText("刷新时间"), FoldoutGroup("采集参数")]
        protected float refreshCDTime;

        /// <summary>
        /// 刷新时若与其他物体重合，则延迟刷新
        /// </summary>
        [SerializeField, LabelText("延迟时间"), FoldoutGroup("采集参数")]
        protected float delayTime;

        /// <summary>
        /// 掉落半径
        /// </summary>
        [SerializeField, LabelText("掉落半径"), HideInInspector, FoldoutGroup("采集参数")]
        protected float droppedRadius;

        /// <summary>
        /// to-do : 更改为对应的类型
        /// 特定的武器才能对此采集物造成效果
        /// </summary>
        [SerializeField, LabelText("要求的武器类型"), HideInInspector, FoldoutGroup("采集参数")]
        protected object requiredWeaponType;

        /// <summary>
        /// 是否处于死亡刷新状态
        /// </summary>
        [SerializeField, ReadOnly, LabelText("是否处于死亡状态"), FoldoutGroup("采集参数")]
        protected bool IsDeath = false;

        /// <summary>
        /// 重合检测层
        /// </summary>
        [SerializeField, LabelText("重合检测层"), FoldoutGroup("采集参数")]
        protected LayerMask overrideLayer;
        /// <summary>
        /// 死亡后是否与其他物体重合 => 重合计数
        /// </summary>
        [SerializeField, ReadOnly, LabelText("重合计数"), FoldoutGroup("采集参数")]
        protected int overrideCount = 0;

        protected Collider _collider;
        protected Renderer _renderer;
        #endregion

        #region UnityMethods
        private void Awake()
        {
            this.hp = this.MaxHP;
            this._collider = this.GetComponent<Collider>();
            this._renderer = this.GetComponent<Renderer>();
#if UNITY_EDITOR
            if (this._collider == null)
            {
                Debug.LogError("Collider Is Null !");
            }
            if(this._renderer == null)
            {
                Debug.LogError("Renderer Is Null !");
            }
#endif
        }

        private void Start()
        {
            this._combatProperty.bInvincible = true;
            this._combatProperty.HP.BaseValue = 1;
            this._combatProperty.HP.ResetCurValue();

            this.combatObject.BindAction((ISpellAttackObject a,ICombatObject b,object c) =>
            {
                // to-do : 判断武器类型
                --this.hp;
                if(this.hp <= 0)
                {
                    this.DieAndRefresh();
                }
            }, Action.CombatTriggerAction.CombatTriggerMode.PostDamage);
        }

        private void OnTriggerEnter(Collider other)
        {
            if ((this.overrideLayer.value | other.gameObject.layer) != 0)
            {
                ++this.overrideCount;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if ((this.overrideLayer.value | other.gameObject.layer) != 0)
            {
                --this.overrideCount;
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// to-do : 武器类型与要求的武器类型是否一致
        /// </summary>
        /// <param name="wType"></param>
        /// <returns></returns>
        public bool IsSuitableWeapon(object wType)
        {
            return wType == this.requiredWeaponType;
        }

        /// <summary>
        /// 死亡并刷新
        /// </summary>
        protected virtual void DieAndRefresh()
        {
            this._collider.isTrigger = true;
            this._renderer.enabled = false;
            this.hp = this.MaxHP;
            this.GenerateDroppedItem();
            StartCoroutine(RefreshCD());
        }

        /// <summary>
        /// to-do : 暂定原地生成
        /// </summary>
        protected virtual void GenerateDroppedItem()
        {
            //Vector3 point = Random.insideUnitCircle * this.droppedRadius;
            //Vector3 tPos = this.transform.position;
            //point.Set(tPos.x + point.x, tPos.y, tPos.z + point.y);
            //// 向下射线检测贴地生成

            // to-do : 暂定原地生成
            foreach(var drop in this.droppedItems)
            {
                ML.Engine.InventorySystem.Item item = drop.TryGetDroppedItem();
                if (item != null)
                {

#pragma warning disable CS4014
                    ML.Engine.InventorySystem.ItemManager.Instance.SpawnWorldItem(item, this.transform.position, this.transform.rotation);
#pragma warning restore CS4014
                }
            }
        }

        /// <summary>
        /// 刷新协程
        /// </summary>
        /// <returns></returns>
        protected IEnumerator RefreshCD()
        {
            yield return new WaitForSeconds(this.refreshCDTime);
            while (CheckOverride())
            {
                yield return new WaitForSeconds(this.delayTime);
            }

            this._collider.isTrigger = false;
            this._renderer.enabled = true;
            yield break;
        }

        /// <summary>
        /// 检测是否与其他物体重合
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckOverride()
        {
            return this.overrideCount > 0;
        }
        #endregion
       
        #region UnityEditor

#if UNITY_EDITOR
        [Button("减少一点HP"), ShowIf("@UnityEditor.EditorApplication.isPlaying == true")]
        protected void SubOneHP()
        {
            
            --this.hp;
            if(this.hp <= 0)
            {
                this.DieAndRefresh();
            }
        }

        [Button("直接死亡"), ShowIf("@UnityEditor.EditorApplication.isPlaying == true")]
        protected void SubAllHP()
        {
            this.DieAndRefresh();
        }
#endif
        #endregion

    }
}

