using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InventorySystem
{
    [RequireComponent(typeof(Collider))]
    public class WorldItem : MonoBehaviour
    {
        [SerializeField, ReadOnly]
        protected int _id = -1;

        /// <summary>
        /// 拥有的 Item
        /// </summary>
        [ShowInInspector, ReadOnly, SerializeField]
        protected Item item;

        public void SetItem(Item item)
        {
            this.item = item;
            _id = this.item != null ? this.item.ID : -1;
        }

        private void Start()
        {
            InitItem();

            // 不需要 Update
            this.enabled = false;
        }

        protected void InitItem()
        {
            if (this.item == null && ItemSpawner.Instance.IsValidItemID(this._id))
            {
                this.item = ItemSpawner.Instance.SpawnItem(this._id);
            }
        }

        /// <summary>
        /// 拾取
        /// </summary>
        /// <param name="inventory"></param>
        public void PickUp(Inventory inventory)
        {
            if (this.item == null && ItemSpawner.Instance.IsValidItemID(this._id))
            {
                this.item = ItemSpawner.Instance.SpawnItem(this._id);
            }
            else
            {
                throw new System.Exception(this.gameObject.name + " WorldItem.Item == null !!!");
            }

            inventory.AddItem(item);

            Destroy(this.gameObject);
        }

    }
   
}

