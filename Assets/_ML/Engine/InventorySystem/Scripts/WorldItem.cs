using ML.Engine.InteractSystem;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InventorySystem
{
    [RequireComponent(typeof(Collider))]
    public class WorldItem : MonoBehaviour, InteractSystem.IInteraction
    {
        [SerializeField, ReadOnly]
        protected string _id = "";

        /// <summary>
        /// ӵ�е� Item
        /// </summary>
        [ShowInInspector, ReadOnly, SerializeField]
        protected Item item;


        public void SetItem(Item item)
        {
            this.item = item;
            _id = this.item != null ? this.item.ID : "";
        }

        private void Start()
        {
            InitItem();

            // ����Ҫ Update
            this.enabled = false;
        }

        protected void InitItem()
        {
            if (this.item == null && ItemManager.Instance.IsValidItemID(this._id))
            {
                this.item = ItemManager.Instance.SpawnItem(this._id);
            }
        }

        /// <summary>
        /// ʰȡ
        /// </summary>
        /// <param name="inventory"></param>
        public void PickUp(IInventory inventory)
        {
            if (this.item == null)
            {
                if(ItemManager.Instance.IsValidItemID(this._id))
                {
                    this.item = ItemManager.Instance.SpawnItem(this._id);

                }
                else
                {
                    throw new System.Exception(this.gameObject.name + " WorldItem.Item == null !!!");
                }
            }


            inventory.AddItem(item);

            Manager.GameManager.DestroyObj(this.gameObject);
        }

        #region IInteraction
        public string InteractType { get; set; } = "";
        public Vector3 PosOffset { get; set; } = Vector3.zero;

        public void Interact(InteractComponent component)
        {
            component.SetInteractionNull();
            // to-do : Inventory ����ʱ����Ҳ��Ҫ����
            PickUp(component.gameObject.GetComponentInParent<ProjectOC.Player.PlayerCharacter>().Inventory);
        }
        #endregion

    }

}

