using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
#if ENABLE_NETWORK && ML_NGO_SUPPROT
using Unity.Netcode;
#endif


namespace ML.Engine.InventorySystem
{
    [LabelText("物品"), System.Serializable]
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

        public Item(string ID, ItemTableData config, int initAmount)
        {
            this.ID = ID;
            this.amount = initAmount;

            // 默认添加数量为0时从Inventory移除并销毁
            this.OnAmountToZero += (IInventory inventory, Item item) =>
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

        public virtual WorldItem.IWorldItemData GetItemWorldData()
        {
            return new WorldItem.WorldItemData(amount);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        #region 按ID比较
        public class SortByID : IComparer<Item>
        {
            public int Compare(Item x, Item y)
            {
                if (x == null || y == null)
                {
                    return (x == null).CompareTo((y == null));
                }
                return string.Compare(x.ID, y.ID);
            }
        }
        public class Sort : IComparer<Item>
        {
            public int Compare(Item x, Item y)
            {
                if (x == null || y == null)
                {
                    return (x == null).CompareTo((y == null));
                }
                var xs = ItemManager.Instance.GetSortNum(x.ID);
                var ys = ItemManager.Instance.GetSortNum(y.ID);
                if (xs != ys)
                {
                    return xs.CompareTo(ys);
                }
                return string.Compare(x.ID, y.ID);
            }
        }
        #endregion
    }
}

