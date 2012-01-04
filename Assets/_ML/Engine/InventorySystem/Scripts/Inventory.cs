using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InventorySystem
{
    /// <summary>
    /// ���ױ���
    /// </summary>
    public class Inventory : IInventory
    {
        #region Field|Property
        public readonly Transform Owner;

        /// <summary>
        /// ��������
        /// </summary>
        public readonly int MaxSize;

        /// <summary>
        /// ��ǰ�洢��
        /// </summary>
        public int Size
        {
            get; protected set;
        }

        /// <summary>
        /// ��������Item�б�
        /// </summary>
        [ShowInInspector]
        private Item[] itemList;

        /// <summary>
        /// �����б����ʱ����
        /// </summary>
        public event System.Action<Inventory> OnItemListChanged;
        #endregion

        public Inventory(int maxSize, Transform Owner)
        {
            this.Owner = Owner;
            this.MaxSize = maxSize;
            this.itemList = new Item[this.MaxSize];
        }

        public bool AddItem(Item item)
        {
            return this.AddItem(item, false) >= 0;
        }

        /// <summary>
        /// ��item���뱳��
        /// -1 => ����װ��
        /// 0 -> δ��ȫװ��
        /// 1 -> ��ȫװ��
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int AddItem(Item item, bool IsFillToNullItem = false)
        {
            if(item == null ||(IsFillToNullItem && this.Size == this.MaxSize))
            {
                return -1;
            }

            // Ѱ�Һ��ʸ���װ��
            for(int i = 0; i < MaxSize; ++i)
            {
                // Ϊ null
                if(this.itemList[i] == null)
                {
                    this.itemList[i] = item;
                    item.OwnInventory = this;
                    ++this.Size;
                    this.OnItemListChanged?.Invoke(this);
                    return 1;
                }
                // ͬ��δ�� && ����װ���null����
                else if (this.itemList[i].ID == item.ID && !IsFillToNullItem && !this.itemList[i].IsFillUp)
                {
                    // ����ȫװ��
                    if (this.itemList[i].Amount + item.Amount <= this.itemList[i].MaxAmount)
                    {
                        this.itemList[i].Amount += item.Amount;
                        this.OnItemListChanged?.Invoke(this);
                        return 1;
                    }
                    // ������ȫװ��
                    else
                    {
                        int delta = this.itemList[i].MaxAmount - this.itemList[i].Amount;
                        this.itemList[i].Amount = this.itemList[i].MaxAmount;
                        item.Amount -= delta;
                        continue;
                    }
                }
            }
            this.OnItemListChanged?.Invoke(this);
            // δ��ȫװ��
            return 0;
        }

        /// <summary>
        /// ��ȡ�����б�
        /// </summary>
        /// <returns></returns>
        public Item[] GetItemList()
        {
            return this.itemList;
        }

        /// <summary>
        /// ��item�Ƴ�����
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool RemoveItem(Item item)
        {
            if (item == null || item.OwnInventory != this)
                return false;
            for (int i = 0; i < MaxSize; ++i)
            {
                if(this.itemList[i] == item)
                {
                    item.OwnInventory = null;
                    this.itemList[i] = null;
                    --this.Size;
                    this.OnItemListChanged?.Invoke(this);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// ��item�Ƴ�����
        /// amount ���� ���ֵ����ȫ���Ƴ�
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Item RemoveItem(Item item, int amount)
        {
            if (item == null || item.OwnInventory != this || amount <= 0)
                return null;

            if (!item.bCanStack && amount == 1)
            {
                if (this.RemoveItem(item))
                {
                    return item;
                }
            }

            for (int i = 0; i < MaxSize; ++i)
            {
                if (this.itemList[i] == item)
                {
                    Item ans = ItemSpawner.Instance.SpawnItem(item.ID);
                    ans.Amount = Mathf.Min(amount, item.Amount);

                    item.Amount -= amount;
                    this.OnItemListChanged?.Invoke(this);
                    return ans;
                }
            }
            return null;
        }

        /// <summary>
        /// �Ƴ�ĳһ�� Item
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool RemoveItem(string itemID, int amount)
        {
            // ��������
            if (this.GetItemAllNum(itemID) < amount)
                return false;

            for (int i = 0; i < MaxSize; ++i)
            {
                if(this.itemList[i] == null)
                {
                    continue;
                }
                if (this.itemList[i].ID == itemID)
                {
                    // �����㹻
                    if(this.itemList[i].Amount >= amount)
                    {
                        this.itemList[i].Amount -= amount;
                        this.OnItemListChanged?.Invoke(this);
                        break;
                    }
                    // ��������
                    else
                    {
                        amount -= this.itemList[i].Amount;
                        // ����
                        this.itemList[i].Amount = 0;
                        continue;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// ���ֲ�� Item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool BinarySplitItem(Item item)
        {
            if(item == null || !item.bCanStack || item.Amount < 2 || this.Size >= this.MaxSize || item.OwnInventory != this)
            {
                return false;
            }

            for (int i = 0; i < MaxSize; ++i)
            {
                if (this.itemList[i] == item)
                {
                    int amount = item.Amount / 2;
                    item.Amount -= amount;

                    Item item1 = ItemSpawner.Instance.SpawnItem(item.ID);
                    item1.Amount = amount;
                    this.AddItem(item1, true);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// �� ID �������� Item
        /// </summary>
        public void SortItem()
        {
            System.Array.Sort(this.itemList, new Item.Sort());
            this.OnItemListChanged?.Invoke(this);
        }
    
        /// <summary>
        /// �ϲ�������ͬ���͵�Item
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public bool MergeItems(int AIndex, int BIndex)
        {
            if (((AIndex < 0 || AIndex > this.MaxSize) || BIndex < 0 || BIndex > this.MaxSize) || (this.itemList[AIndex] == null || this.itemList[BIndex] == null) || AIndex == BIndex || (this.itemList[AIndex].IsFillUp || this.itemList[BIndex].IsFillUp))
            {
                return false;
            }
            bool ans = Item.MergeItems(this.itemList[AIndex], this.itemList[BIndex]);
            this.OnItemListChanged?.Invoke(this);
            return ans;
        }
    
        /// <summary>
        /// ���������е�Item
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public bool SwapItem(int AIndex, int BIndex)
        {
            if((AIndex < 0 || AIndex > this.MaxSize) || BIndex < 0 || BIndex > this.MaxSize || AIndex == BIndex)
            {
                return false;
            }
            Item tmp = this.itemList[AIndex];
            this.itemList[AIndex] = this.itemList[BIndex];
            this.itemList[BIndex] = tmp;
            this.OnItemListChanged?.Invoke(this);
            return true;
        }

        /// <summary>
        /// ��ȡitem�ڱ����е�index
        /// -1 ��ʾ������
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int GetItemIndex(Item item)
        {
            if (item == null || item.OwnInventory != this)
                return -1;

            for (int i = 0; i < MaxSize; ++i)
            {
                if (this.itemList[i] == item)
                {
                    return i;
                }
            }
            return -1;
        }
       
        public void UseItem(int index, int amount)
        {
            if ((index < 0 || index > this.MaxSize) || this.itemList[index] == null)
            {
                return;
            }
            this.itemList[index].Execute(amount);
            OnItemListChanged?.Invoke(this);
        }

        /// <summary>
        /// ��ȡ������ָ�� id Item ������
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetItemAllNum(string id)
        {
            int ans = 0;

            foreach(var item in this.itemList)
            {
                if(item != null && item.ID == id)
                {
                    ans += item.Amount;
                }
            }

            return ans;
        }
    }
}
