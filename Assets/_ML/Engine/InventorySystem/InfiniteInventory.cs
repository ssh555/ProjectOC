using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InventorySystem
{
    public class InfiniteInventory : IInventory
    {
        #region Field|Property
        /// <summary>
        /// 背包最大容量
        /// </summary>
        public readonly int MaxSize;

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

        public InfiniteInventory(Transform Owner, int maxSize)
        {
            this.m_owner = Owner;
            this.MaxSize = maxSize;
            this.itemList = new List<Item>();
        }

        /// <summary>
        /// 将item加入背包
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool AddItem(Item item)
        {
            lock (this)
            {
                if (this.Size >= this.MaxSize || item == null || !ItemManager.Instance.IsValidItemID(item.ID))
                {
                    return false;
                }
                // 不可以堆叠
                if (!ItemManager.Instance.GetCanStack(item.ID))
                {
                    item.OwnInventory = this;
                    this.itemList.Add(item);
                }
                // 可以堆叠
                else
                {
                    Item it = this.itemList.Find(i => i.ID == item.ID);
                    // 寻找合适格子装入
                    if (it != null)
                    {
                        var ma = ItemManager.Instance.GetMaxAmount(it.ID);
                        // 没有达到数量上限
                        if (it.Amount + item.Amount <= ma)
                        {
                            it.Amount += item.Amount;
                        }
                        else
                        {
                            item.Amount = it.Amount + item.Amount - ma;
                            it.Amount = ma;
                            item.OwnInventory = this;
                            this.itemList.Add(item);
                        }
                    }
                    else
                    {
                        item.OwnInventory = this;
                        this.itemList.Add(item);
                    }
                }
                this.SortItem();
                return true;
            }
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
        /// 将item对象移除背包
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool RemoveItem(Item item)
        {
            lock (this)
            {
                if (item == null)
                {
                    return false;
                }
                var it = this.itemList.Find(i => i == item);
                if (it == null)
                {
                    return false;
                }
                this.itemList.Remove(it);
                this.OnItemListChanged?.Invoke(this);
                return true;
            }
        }

        /// <summary>
        /// 将item对象移除背包
        /// amount 超过 最大值，则全部移除
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Item RemoveItem(Item item, int amount = 1)
        {
            lock (this)
            {
                if (item == null || amount <= 0)
                {
                    return null;
                }
                var it = this.itemList.Find(i => i == item);
                if (it == null)
                {
                    return null;
                }

                if (it.Amount <= amount)
                {
                    this.itemList.Remove(it);
                    this.OnItemListChanged?.Invoke(this);
                    return it;
                }
                else
                {
                    it.Amount -= amount;
                    this.OnItemListChanged?.Invoke(this);
                    var res = ItemManager.Instance.SpawnItem(item.ID);
                    res.Amount = amount;
                    return res;
                }
            }
        }

        /// <summary>
        /// 移除某一种 Item
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool RemoveItem(string itemID, int amount = 1)
        {
            lock (this)
            {
                // 数量不够
                if (this.GetItemAllNum(itemID) < amount)
                {
                    return false;
                }

                var items = this.itemList.FindAll(i => i.ID == itemID);
                foreach (var item in items)
                {
                    if (item.Amount >= amount)
                    {
                        item.Amount -= amount;
                        break;
                    }
                    else
                    {
                        amount -= item.Amount;
                        item.Amount = 0;
                    }
                }

                this.OnItemListChanged?.Invoke(this);
                return true;
            }
        }

        /// <summary>
        /// 按 SortNum 排序 Item
        /// </summary>
        public void SortItem(bool IsDesc = false)
        {
            this.itemList.Sort((a, b) =>
            {
                var sa = ItemManager.Instance.GetSortNum(a.ID);
                var sb = ItemManager.Instance.GetSortNum(b.ID);
                return IsDesc ? sb.CompareTo(sa) : sa.CompareTo(sb);
            });
            this.OnItemListChanged?.Invoke(this);
        }

        public void UseItem(string itemID, int amount = 1)
        {
            if (!ItemManager.Instance.IsValidItemID(itemID))
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
            if (!ItemManager.Instance.IsValidItemID(id))
            {
                return 0;
            }
            var items = this.itemList.FindAll(i => i.ID == id);
            int res = 0;
            foreach(var item in items)
            {
                res += item.Amount;
            }
            return res;
        }
    }
}
