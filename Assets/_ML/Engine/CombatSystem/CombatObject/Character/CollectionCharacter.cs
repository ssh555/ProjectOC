using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.CombatObject.Character
{
    /// <summary>
    /// ����������
    /// </summary>
    [System.Serializable]
    public struct DroppedItem
    {
        /// <summary>
        /// ������ID
        /// </summary>
        [LabelText("ItemID")]
        public string id;
        /// <summary>
        /// ��������
        /// </summary>
        [LabelText("��������"), Range(1, int.MaxValue)]
        public int amount;
        /// <summary>
        /// ���伸��
        /// </summary>
        [LabelText("���伸��"), Range(0, 1)]
        public float probabilty;

        /// <summary>
        /// ���� roll ���ȡ������
        /// </summary>
        /// <returns>����ֵΪnull ��ʾû�е���������</returns>
        public ML.Engine.InventorySystem.Item TryGetDroppedItem()
        {
            return Random.value <= probabilty ? ML.Engine.InventorySystem.ItemManager.Instance.SpawnItem(this.id) : null;
        }
    }

    /// <summary>
    /// to-do : Ϊ�������������޸�
    /// </summary>
    [RequireComponent(typeof(Collider), typeof(Renderer))]
    public class CollectionCharacter : MonoCombatObject
    {
        #region Property
        /// <summary>
        /// ����ܻ�����
        /// </summary>
        [LabelText("����ܻ�����"), PropertyRange(1, int.MaxValue), SerializeField, FoldoutGroup("�ɼ�����")]
        protected int MaxHP = 3;

        /// <summary>
        /// ʣ����ܻ�����
        /// </summary>
        [SerializeField, ShowInInspector, ReadOnly, LabelText("ʣ��HP"), FoldoutGroup("�ɼ�����")]
        protected int hp;

        /// <summary>
        /// �������б�
        /// </summary>
        [SerializeField, LabelText("�������б�"), FoldoutGroup("�ɼ�����")]
        protected DroppedItem[] droppedItems;

        /// <summary>
        /// ������ˢ�µ���ʱ
        /// </summary>
        [SerializeField, LabelText("ˢ��ʱ��"), FoldoutGroup("�ɼ�����")]
        protected float refreshCDTime;

        /// <summary>
        /// ˢ��ʱ�������������غϣ����ӳ�ˢ��
        /// </summary>
        [SerializeField, LabelText("�ӳ�ʱ��"), FoldoutGroup("�ɼ�����")]
        protected float delayTime;

        /// <summary>
        /// ����뾶
        /// </summary>
        [SerializeField, LabelText("����뾶"), HideInInspector, FoldoutGroup("�ɼ�����")]
        protected float droppedRadius;

        /// <summary>
        /// to-do : ����Ϊ��Ӧ������
        /// �ض����������ܶԴ˲ɼ������Ч��
        /// </summary>
        [SerializeField, LabelText("Ҫ�����������"), HideInInspector, FoldoutGroup("�ɼ�����")]
        protected object requiredWeaponType;

        /// <summary>
        /// �Ƿ�������ˢ��״̬
        /// </summary>
        [SerializeField, ReadOnly, LabelText("�Ƿ�������״̬"), FoldoutGroup("�ɼ�����")]
        protected bool IsDeath = false;

        /// <summary>
        /// �غϼ���
        /// </summary>
        [SerializeField, LabelText("�غϼ���"), FoldoutGroup("�ɼ�����")]
        protected LayerMask overrideLayer;
        /// <summary>
        /// �������Ƿ������������غ� => �غϼ���
        /// </summary>
        [SerializeField, ReadOnly, LabelText("�غϼ���"), FoldoutGroup("�ɼ�����")]
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
                // to-do : �ж���������
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
        /// to-do : ����������Ҫ������������Ƿ�һ��
        /// </summary>
        /// <param name="wType"></param>
        /// <returns></returns>
        public bool IsSuitableWeapon(object wType)
        {
            return wType == this.requiredWeaponType;
        }

        /// <summary>
        /// ������ˢ��
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
        /// to-do : �ݶ�ԭ������
        /// </summary>
        protected virtual void GenerateDroppedItem()
        {
            //Vector3 point = Random.insideUnitCircle * this.droppedRadius;
            //Vector3 tPos = this.transform.position;
            //point.Set(tPos.x + point.x, tPos.y, tPos.z + point.y);
            //// �������߼����������

            // to-do : �ݶ�ԭ������
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
        /// ˢ��Э��
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
        /// ����Ƿ������������غ�
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckOverride()
        {
            return this.overrideCount > 0;
        }
        #endregion
       
        #region UnityEditor

#if UNITY_EDITOR
        [Button("����һ��HP"), ShowIf("@UnityEditor.EditorApplication.isPlaying == true")]
        protected void SubOneHP()
        {
            
            --this.hp;
            if(this.hp <= 0)
            {
                this.DieAndRefresh();
            }
        }

        [Button("ֱ������"), ShowIf("@UnityEditor.EditorApplication.isPlaying == true")]
        protected void SubAllHP()
        {
            this.DieAndRefresh();
        }
#endif
        #endregion

    }
}

