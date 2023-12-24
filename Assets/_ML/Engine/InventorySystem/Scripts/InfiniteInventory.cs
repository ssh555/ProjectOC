using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InventorySystem
{
    public class InfiniteInventory : IInventory
    {
        #region Field|Property
        private Transform m_owner;
        /// <summary>
        /// 背包的拥有者
        /// </summary>
        public Transform Owner
        {
            get
            {
                return m_owner;
            }
        }

        /// <summary>
        /// 当前存储量
        /// </summary>
        public int Size => this.itemList.Count;

        /// <summary>
        /// 背包物体Item列表
        /// </summary>
        [ShowInInspector]
        private List<Item> itemList;

        /// <summary>
        /// 背包列表更改时调用
        /// </summary>
        public event System.Action<InfiniteInventory> OnItemListChanged;
        #endregion

        public InfiniteInventory(Transform Owner)
        {
            this.m_owner = Owner;
            this.itemList = new List<Item>();
        }

        /// <summary>
        /// 将item加入背包
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool AddItem(Item item)
        {
            if (item == null || !ItemSpawner.Instance.IsValidItemID(item.ID))
            {
                return false;
            }
            Item it = this.itemList.Find(i => i.ID == item.ID);
            // 寻找合适格子装入
            if (it != null)
            {
                it.Amount += item.Amount;
            }
            else
            {
                this.itemList.Add(item);
            }
            this.SortItem();
            return true;
        }

        /// <summary>
        /// 获取背包列表
        /// </summary>
        /// <returns></returns>
        public Item[] GetItemList()
        {
            return this.itemList.ToArray();
        }

        /// <summary>
        /// 将item移除背包
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool RemoveItem(Item item)
        {
            if (item == null)
            {
                return false;
            }
            var it = this.itemList.Find(i => i.ID == item.ID);
            if(it == null)
            {
                return false;
            }
            this.itemList.Remove(it);
            this.OnItemListChanged?.Invoke(this);
            return true;
        }

        /// <summary>
        /// 将item移除背包
        /// amount 超过 最大值，则全部移除
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Item RemoveItem(Item item, int amount)
        {
            if (item == null || amount <= 0)
            {
                return null;
            }
            var it = this.itemList.Find(i => i.ID == item.ID);
            if (it == null)
            {
                return null;
            }

            if(it.Amount <= amount)
            {
                this.itemList.Remove(it);
                this.OnItemListChanged?.Invoke(this);
                return it;
            }
            else
            {
                it.Amount -= amount;
                this.OnItemListChanged?.Invoke(this);
                return ItemSpawner.Instance.SpawnItem(item.ID, amount);
            }
        }

        /// <summary>
        /// 移除某一种 Item
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool RemoveItem(string itemID, int amount)
        {
            // 数量不够
            if (this.GetItemAllNum(itemID) < amount)
            {
                return false;
            }

            var item = this.itemList.Find(i => i.ID == itemID);
            if(item == null || item.Amount < amount)
            {
                return false;
            }
            item.Amount -= amount;
            return true;
        }

        /// <summary>
        /// 按 SortNum 排序 Item
        /// </summary>
        public void SortItem(bool IsDesc = false)
        {
            this.itemList.Sort((a, b) =>
            {
                return IsDesc ? b.SortNum.CompareTo(a.SortNum) : a.SortNum.CompareTo(b.SortNum);
            });
            this.OnItemListChanged?.Invoke(this);
        }

        public void UseItem(string itemID, int amount = 1)
        {
            if (!ItemSpawner.Instance.IsValidItemID(itemID))
            {
                return;
            }
            var item = this.itemList.Find(i => i.ID == itemID);
            if(item == null)
            {
                return;
            }
            item.Execute(amount);
            OnItemListChanged?.Invoke(this);
        }

        /// <summary>
        /// 获取背包中指定 id Item 的数量
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetItemAllNum(string id)
        {
            if (!ItemSpawner.Instance.IsValidItemID(id))
            {
                return 0;
            }
            var item = this.itemList.Find(i => i.ID == id);
            if (item == null)
            {
                return 0;
            }
            return item.Amount;
        }
    }
}
