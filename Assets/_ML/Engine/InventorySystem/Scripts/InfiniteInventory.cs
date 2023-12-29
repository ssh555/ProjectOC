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
        /// �����������
        /// </summary>
        public readonly int MaxSize;

        private Transform m_owner;
        /// <summary>
        /// ������ӵ����
        /// </summary>
        public Transform Owner
        {
            get
            {
                return m_owner;
            }
        }

        /// <summary>
        /// ��ǰ�洢��
        /// </summary>
        public int Size => this.itemList.Count;

        /// <summary>
        /// ��������Item�б�
        /// </summary>
        [ShowInInspector]
        private List<Item> itemList;

        /// <summary>
        /// �����б����ʱ����
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
        /// ��item���뱳��
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool AddItem(Item item)
        {
            if (this.Size >= this.MaxSize || item == null || !ItemSpawner.Instance.IsValidItemID(item.ID))
            {
                return false;
            }
            // �����Զѵ�
            if(!item.bCanStack)
            {
                this.itemList.Add(item);
            }
            // ���Զѵ�
            else
            {
                Item it = this.itemList.Find(i => i.ID == item.ID);
                // Ѱ�Һ��ʸ���װ��
                if (it != null)
                {
                    // û�дﵽ��������
                    if(it.Amount + item.Amount <= it.MaxAmount)
                    {
                        it.Amount += item.Amount;
                    }
                    else
                    {
                        item.Amount = it.MaxAmount - it.Amount - item.Amount;
                        it.Amount = it.MaxAmount;
                        this.itemList.Add(item);
                    }
                }
                else
                {
                    this.itemList.Add(item);
                }
            }
            this.SortItem();
            return true;
        }

        /// <summary>
        /// ��ȡ�����б�
        /// </summary>
        /// <returns></returns>
        public Item[] GetItemList()
        {
            return this.itemList.ToArray();
        }

        /// <summary>
        /// ��item�����Ƴ�����
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool RemoveItem(Item item)
        {
            if (item == null)
            {
                return false;
            }
            var it = this.itemList.Find(i => i == item);
            if(it == null)
            {
                return false;
            }
            this.itemList.Remove(it);
            this.OnItemListChanged?.Invoke(this);
            return true;
        }

        /// <summary>
        /// ��item�����Ƴ�����
        /// amount ���� ���ֵ����ȫ���Ƴ�
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Item RemoveItem(Item item, int amount = 1)
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
                var res = ItemSpawner.Instance.SpawnItem(item.ID);
                res.Amount = amount;
                return res;
            }
        }

        /// <summary>
        /// �Ƴ�ĳһ�� Item
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool RemoveItem(string itemID, int amount = 1)
        {
            // ��������
            if (this.GetItemAllNum(itemID) < amount)
            {
                return false;
            }

            var items = this.itemList.FindAll(i => i.ID == itemID);
            foreach(var item in items)
            {
                if(item.Amount >= amount)
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

        /// <summary>
        /// �� SortNum ���� Item
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
        /// ��ȡ������ָ�� id Item ������
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetItemAllNum(string id)
        {
            if (!ItemSpawner.Instance.IsValidItemID(id))
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
