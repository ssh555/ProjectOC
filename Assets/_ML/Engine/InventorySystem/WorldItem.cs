using ML.Engine.InteractSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ML.Engine.InventorySystem
{
    [RequireComponent(typeof(Collider))]
    public class WorldItem : MonoBehaviour, InteractSystem.IInteraction
    {
        public interface IWorldItemData
        {
            public int Amount { get; }
            public void SetItemData(Item item);
        }

        [System.Serializable]
        public struct WorldItemData : IWorldItemData
        {
            [LabelText("Item ����")]
            public int amount;
            public int Amount { get => amount; }

            public WorldItemData(int num)
            {
                amount = num;
            }
            public void SetItemData(Item item)
            {
                item.Amount = Amount;
            }
        }
        [System.Serializable]
        public struct WorldCreatureItemData : IWorldItemData
        {
            [LabelText("Item ����")]
            public int amount;
            public int Amount { get => amount; }
            public Gender Gender;
            public int Activity;
            public int Output;
            public WorldCreatureItemData(int amount, Gender gender, int activity, int output)
            {
                this.amount = amount;
                Gender = gender;
                Activity = activity;
                Output = output;
            }
            public void SetItemData(Item item)
            {
                item.Amount = Amount;
                if (item is CreatureItem creature)
                {
                    creature.Gender = Gender;
                    creature.Activity = Activity;
                    creature.Output = Output;
                }
                else
                {
                    Debug.Log($"Error WorldCreatureItemData SetItemData {item.ID}");
                }
            }
        }

        [LabelText("ItemID"), SerializeField]
        protected string itemID = null;

        [LabelText("Item ����ʱ������"), SerializeField]
        protected IWorldItemData itemData = new WorldItemData();
        public void SetItem(string id, IWorldItemData Data)
        {
            if(ItemManager.Instance.IsValidItemID(id))
            {
                this.itemID = id;
                itemData = Data;
            }
        }

        public void SetItem(Item item)
        {
            this.itemID = item.ID;
            this.itemData = item.GetItemWorldData();
        }

        /// <summary>
        /// �ɸ��ĳ�ʼ��
        /// </summary>
        protected virtual void Start()
        {
            // ����Ҫ Update
            this.enabled = false;
        }

        /// <summary>
        /// ʰȡ
        /// </summary>
        /// <param name="inventory"></param>
        public virtual void PickUp(IInventory inventory)
        {
            if (ItemManager.Instance.IsValidItemID(itemID))
            {
                Item item = ItemManager.Instance.SpawnItem(itemID);
                itemData.SetItemData(item);
                inventory.AddItem(item);
                Manager.GameManager.DestroyObj(gameObject);
            }
            else
            {
                throw new System.Exception(gameObject.name + " WorldItem.Item == null !!!");
            }

        }

        #region IInteraction
        public string InteractType { get; set; } = "Item";
        public Vector3 PosOffset { get; set; } = Vector3.zero;

        public virtual void Interact(InteractComponent component)
        {
            //component.SetInteractionNull();
            // to-do : Inventory ����ʱ����Ҳ��Ҫ����
            PickUp(component.gameObject.GetComponentInParent<ProjectOC.Player.PlayerCharacter>().Inventory);
        }
        #endregion
    }

}

