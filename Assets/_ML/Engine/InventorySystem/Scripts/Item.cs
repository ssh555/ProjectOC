using System;
using System.Collections;
using System.Collections.Generic;
#if ENABLE_NETWORK && ML_NGO_SUPPROT
using Unity.Netcode;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace ML.Engine.InventorySystem
{
    /// <summary>
    /// ��Ʒ
    /// </summary>
    [System.Serializable]
    public abstract class Item
    {
        #region Field|Property
        /// <summary>
        /// ��Ʒ��� : Item_����_���
        /// Ĭ��Ϊ���ַ�������ʾnull��
        /// </summary>
        public readonly string ID = "";
        /// <summary>
        /// ������
        /// </summary>
        public int Weight {get { return ItemManager.Instance.GetWeight(ID) * Amount; }}
        /// <summary>
        /// ���� Inventory
        /// </summary>
        public IInventory OwnInventory = null;
        /// <summary>
        /// ����
        /// </summary>
        protected int amount;
        /// <summary>
        /// ����
        /// </summary>
        public int Amount
        {
            get => amount;
            set
            {
                amount = Math.Clamp(value, 0, ItemManager.Instance.GetMaxAmount(ID));
                if (amount == 0)
                    this.OnAmountToZero?.Invoke(this.OwnInventory, this);
            }
        }
        /// <summary>
        /// �����ﵽ����
        /// </summary>
        public bool IsFillUp
        {
            get
            {
                return this.Amount == ItemManager.Instance.GetMaxAmount(ID);
            }
        }

        /// <summary>
        /// ������0ʱ����
        /// </summary>
        public event Action<IInventory, Item> OnAmountToZero;
        #endregion

        public Item(string ID, ItemTableJsonData config, int initAmount)
        {
            this.ID = ID;
            this.amount = initAmount;

            // Ĭ���������Ϊ0ʱ��Inventory�Ƴ�������
            this.OnAmountToZero += (IInventory inventory,Item item) =>
            {
                if(inventory != null)
                    inventory.RemoveItem(this);
            };
        }

        /// <summary>
        /// ʹ����Ʒ���ú���
        /// Ĭ�Ͻ�������1
        /// </summary>
        public virtual void Execute(int amount)
        {
            this.Amount -= amount;
        }

        /// <summary>
        /// �Ƿ���ʹ�� -> ����UIʹ��
        /// </summary>
        /// <returns></returns>
        public bool CanUse()
        {
            switch (ItemManager.Instance.GetItemType(ID))
            {
                case ItemType.Equip:
                case ItemType.Food:
                    return true;
                case ItemType.Material:
                case ItemType.Mission:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// �Ƿ��ܶ��� -> ����UIʹ��
        /// </summary>
        /// <returns></returns>
        public bool CanDrop()
        {
            switch (ItemManager.Instance.GetItemType(ID))
            {
                case ItemType.Equip:
                case ItemType.Food:
                case ItemType.Material:
                    return true;
                case ItemType.Mission:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// �Ƿ������� -> ����UIʹ��
        /// </summary>
        /// <returns></returns>
        public bool CanDestroy()
        {
            switch (ItemManager.Instance.GetItemType(ID))
            {
                case ItemType.Equip:
                case ItemType.Food:
                case ItemType.Material:
                    return true;
                case ItemType.Mission:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// �ϲ� Item
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns>�ϲ��Ƿ�ɹ�</returns>
        public static bool MergeItems(Item A, Item B)
        {
            if(A.ID != B.ID)
            {
                return false;
            }

            int mergeAmount = A.Amount + B.Amount;
            A.Amount += B.Amount;
            B.Amount = mergeAmount - A.Amount;
            return true;
        }

        #region ��ID�Ƚ�
        public class SortByID : IComparer<Item>
        {
            public int Compare(Item x, Item y)
            {
                if (x == null)
                {
                    if (y == null)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
                if(y == null)
                {
                    return -1;
                }

                return string.Compare(x.ID, y.ID);
            }

            //int IComparer<Fruit>.Compare(Fruit x, Fruit y)
            //{
            //    return x.Name.CompareTo(y.Name);
            //}
        }
        public class Sort : IComparer<Item>
        {
            public int Compare(Item x, Item y)
            {
                if (x == null)
                {
                    if (y == null)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
                if (y == null)
                {
                    return -1;
                }
                var xs = ItemManager.Instance.GetSortNum(x.ID);
                var ys = ItemManager.Instance.GetSortNum(y.ID);
                if (xs != ys)
                {
                    return xs.CompareTo(ys);
                }
                else
                {
                    return string.Compare(x.ID, y.ID);
                }
            }
        }
        #endregion
    }
}

