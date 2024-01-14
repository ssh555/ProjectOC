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
        /// 物品编号 : Item_类型_序号
        /// 默认为空字符串，表示null。
        /// </summary>
        public readonly string ID = "";
        /// <summary>
        /// 总重量
        /// </summary>
        public int Weight {get { return ItemManager.Instance.GetWeight(ID) * Amount; }}
        /// <summary>
        /// 所属 Inventory
        /// </summary>
        public IInventory OwnInventory = null;
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
                amount = Math.Clamp(value, 0, ItemManager.Instance.GetMaxAmount(ID));
                if (amount == 0)
                    this.OnAmountToZero?.Invoke(this.OwnInventory, this);
            }
        }
        /// <summary>
        /// 数量达到上限
        /// </summary>
        public bool IsFillUp
        {
            get
            {
                return this.Amount == ItemManager.Instance.GetMaxAmount(ID);
            }
        }

        /// <summary>
        /// 数量归0时调用
        /// </summary>
        public event Action<IInventory, Item> OnAmountToZero;
        #endregion

        public Item(string ID, ItemTableJsonData config, int initAmount)
        {
            this.ID = ID;
            this.amount = initAmount;

            // 默认添加数量为0时从Inventory移除并销毁
            this.OnAmountToZero += (IInventory inventory,Item item) =>
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
        /// 是否能使用 -> 仅供UI使用
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
        /// 是否能丢弃 -> 仅供UI使用
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
        /// 是否能销毁 -> 仅供UI使用
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

