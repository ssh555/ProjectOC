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
        /// ��ʾ��������ţ�ԽС����Խǰ��
        /// </summary>
        public int SortNum;
        /// <summary>
        /// ��Ʒ�䷽��Ŀ
        /// </summary>
        public ItemCategory Category;
        /// <summary>
        /// ������Ʒ������
        /// </summary>
        public int SingleItemWeight;
        /// <summary>
        /// ������
        /// </summary>
        public int Weight {get { return SingleItemWeight * Amount; }}
        /// <summary>
        /// ��Ʒ��Ŀ
        /// </summary>
        public ItemType ItemType;
        /// <summary>
        /// ��Ʒ����
        /// </summary>
        public TextContent.TextContent ItemDescription;
        /// <summary>
        /// Ч������
        /// </summary>
        public TextContent.TextContent EffectsDescription;
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
                amount = Math.Clamp(value, 0, this.MaxAmount);
                if (amount == 0)
                    this.OnAmountToZero?.Invoke(this.OwnInventory, this);
            }
        }
        protected int maxAmount = 1;
        /// <summary>
        /// �������
        /// </summary>
        public int MaxAmount
        {
            get => maxAmount;
            set
            {
                if (!this.bCanStack)
                    maxAmount = 1;
                maxAmount = Math.Max(1, value);
            }
        }
        /// <summary>
        /// �����ﵽ����
        /// </summary>
        public bool IsFillUp
        {
            get
            {
                return this.Amount == this.MaxAmount;
            }
        }
        /// <summary>
        /// �ܷ���Ӵ洢
        /// </summary>
        public bool bCanStack;
        /// <summary>
        /// ������0ʱ����
        /// </summary>
        public event Action<IInventory, Item> OnAmountToZero;
        #endregion

        public Item(string ID, ItemSpawner.ItemTableJsonData config, int initAmount)
        {
            this.ID = ID;
            this.SortNum = config.sort;
            this.Category = config.category;
            this.SingleItemWeight = config.weight;
            this.ItemType = config.itemtype;
            this.ItemDescription = config.description;
            this.EffectsDescription = config.effectsDescription;
            this.amount = initAmount;
            this.maxAmount = config.maxamount;
            this.bCanStack = config.bcanstack;
            // Ĭ���������Ϊ0ʱ��Inventory�Ƴ�������
            this.OnAmountToZero += (IInventory inventory,Item item) =>
            {
                if(inventory != null)
                    inventory.RemoveItem(this);
            };
        }
        public Item(Item item, int initAmount)
        {
            this.ID = item.ID;
            this.SortNum = item.SortNum;
            this.Category = item.Category;
            this.SingleItemWeight = item.SingleItemWeight;
            this.ItemType = item.ItemType;
            this.ItemDescription = item.ItemDescription;
            this.EffectsDescription = item.EffectsDescription;
            this.amount = initAmount;
            this.maxAmount = item.MaxAmount;
            this.bCanStack = item.bCanStack;
            // Ĭ���������Ϊ0ʱ��Inventory�Ƴ�������
            this.OnAmountToZero += (IInventory inventory, Item item) =>
            {
                if (inventory != null)
                    inventory.RemoveItem(this);
            };
        }
        public Item(Item item) : this(item, item.Amount)
        {
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
            switch (this.ItemType)
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
            switch (this.ItemType)
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
            switch (this.ItemType)
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

                if (x.SortNum != y.SortNum)
                {
                    return x.SortNum.CompareTo(y.SortNum);
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

