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
    /// 物品
    /// </summary>
    [System.Serializable]
    public abstract class Item
    {
        #region Field|Property
        /// <summary>
        /// 物品编号 : 范围 [1, int.MaxValue) 
        /// 默认为 int.MaxValue,表示为 null
        /// </summary>
        public readonly string ID;

        /// <summary>
        /// 所属 Inventory
        /// </summary>
        public Inventory OwnInventory = null;

        /// <summary>
        /// 数量
        /// </summary>
        protected int amount;
        /// <summary>
        /// 数量
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
        /// 最大数量
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
        /// 数量达到上限
        /// </summary>
        public bool IsFillUp
        {
            get
            {
                return this.Amount == this.MaxAmount;
            }
        }

        /// <summary>
        /// 能否叠加存储
        /// </summary>
        public bool bCanStack;

        /// <summary>
        /// 数量归0时调用
        /// </summary>
        public event Action<Inventory, Item> OnAmountToZero;

        /// <summary>
        /// 用于排序的数值
        /// </summary>
        public int sort;
        #endregion

        public Item(string ID, ItemSpawner.ItemTabelJsonData config, int initAmount)
        {
            this.ID = ID;

            this.bCanStack = config.bcanstack;

            this.amount = initAmount;

            this.maxAmount = config.maxamount;

            this.sort = config.sort;

            // 默认添加数量为0时从Inventory移除并销毁
            this.OnAmountToZero += (Inventory inventory,Item item) =>
            {
                if(inventory != null)
                    inventory.RemoveItem(this);
            };
        }

        /// <summary>
        /// 使用物品调用函数
        /// 默认仅数量减1
        /// </summary>
        public virtual void Execute(int amount)
        {
            this.Amount -= amount;
        }

        /// <summary>
        /// 合并 Item
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns>合并是否成功</returns>
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

        #region 按ID比较
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

        #endregion
    }
}

