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
        /// ��Ʒ��� : ��Χ [1, int.MaxValue) 
        /// Ĭ��Ϊ int.MaxValue,��ʾΪ null
        /// </summary>
        public readonly int ID = int.MaxValue;

        /// <summary>
        /// ���� Inventory
        /// </summary>
        public Inventory OwnInventory = null;

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
        public event Action<Inventory, Item> OnAmountToZero;
        #endregion

        public Item(int ID)
        {
            this.ID = ID;

            this.amount = 1;

            // Ĭ���������Ϊ0ʱ��Inventory�Ƴ�������
            this.OnAmountToZero += (Inventory inventory,Item item) =>
            {
                if(inventory != null)
                    inventory.RemoveItem(this);
            };
        }

        /// <summary>
        /// ��ʼ��
        /// </summary>
        /// <param name="config"></param>
        public abstract void Init(ItemSpawner.ItemTabelData config);

        public abstract void Init(Item item);

        /// <summary>
        /// ʹ����Ʒ���ú���
        /// Ĭ�Ͻ�������1
        /// </summary>
        public virtual void Execute(int amount)
        {
            this.Amount -= amount;
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

                if (x.ID == y.ID)
                {
                    return 0;
                }
                if (x.ID < y.ID)
                {
                    return -1;
                }
                return 1;
            }

            //int IComparer<Fruit>.Compare(Fruit x, Fruit y)
            //{
            //    return x.Name.CompareTo(y.Name);
            //}
        }

        public static bool operator >(Item A, Item B)
        {
            int idA = A == null ? int.MaxValue : A.ID;
            int idB = B == null ? int.MaxValue : B.ID;
            return idA > idB;
        }

        public static bool operator >=(Item A, Item B)
        {
            int idA = A == null ? int.MaxValue : A.ID;
            int idB = B == null ? int.MaxValue : B.ID;
            return idA >= idB;
        }

        public static bool operator<(Item A, Item B)
        {
            int idA = A == null ? int.MaxValue : A.ID;
            int idB = B == null ? int.MaxValue : B.ID;
            return idA < idB;
        }

        public static bool operator <=(Item A, Item B)
        {
            int idA = A == null ? int.MaxValue : A.ID;
            int idB = B == null ? int.MaxValue : B.ID;
            return idA <= idB;
        }
        #endregion
    }
}

