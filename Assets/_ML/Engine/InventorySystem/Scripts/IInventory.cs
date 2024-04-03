using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InventorySystem
{
    public interface IInventory
    {
        public Item[] GetItemList();

        public bool AddItem(Item item);

        /// <summary>
        ///isAtom 是否为原子性操作 返回值为此次加入操作剩余的List<Item>
        /// </summary>
        public List<Item> AddItem(List<Item> items, bool isAtom = false)
        {
            if (isAtom)
            {
                List<Item> tmplist = new List<Item>();
                foreach (var item in items)
                {

                    if (this.AddItem(item))
                    {
                        tmplist.Add(item);
                    }
                    else
                    {
                        foreach (var titem in tmplist)
                        {
                            this.RemoveItem(titem);
                        }
                        return items;
                    }
                }
            }
            else
            {
                List<Item> tmplist = new List<Item>();
                foreach (var item in items)
                {
                    if (this.AddItem(item) == false)
                    {
                        tmplist.Add(item);
                    }
                }
                return tmplist;
            }

            return null;
        }
        public bool RemoveItem(Item item);

        public Item RemoveItem(Item item, int amount);

        public bool RemoveItem(string itemID, int amount);

        public int GetItemAllNum(string id);

    }

}
