//using Sirenix.OdinInspector;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace ML.Engine.InventorySystem
//{
//    public class InfiniteInventory
//    {
//        #region Field|Property
//        private Transform m_owner;
//        /// <summary>
//        /// ������ӵ����
//        /// </summary>
//        public Transform Owner
//        {
//            get
//            {
//                return m_owner;
//            }
//        }

//        /// <summary>
//        /// ��ǰ�洢��
//        /// </summary>
//        public int Size => this.itemList.Count;

//        /// <summary>
//        /// ��������Item�б�
//        /// </summary>
//        [ShowInInspector]
//        private List<Item> itemList;

//        /// <summary>
//        /// �����б����ʱ����
//        /// </summary>
//        public event System.Action<InfiniteInventory> OnItemListChanged;
//        #endregion

//        public InfiniteInventory(Transform Owner)
//        {
//            this.m_owner = Owner;
//            this.itemList = new List<Item>();
//        }

//        /// <summary>
//        /// ��item���뱳��
//        /// </summary>
//        /// <param name="item"></param>
//        /// <returns></returns>
//        public bool AddItem(Item item)
//        {
//            if (item == null || !ItemSpawner.Instance.IsValidItemID(item.ID))
//            {
//                return false;
//            }

//            // Ѱ�Һ��ʸ���װ��
//            //if()
//            this.OnItemListChanged?.Invoke(this);
//            // δ��ȫװ��
//            return true;
//        }

//        /// <summary>
//        /// ��ȡ�����б�
//        /// </summary>
//        /// <returns></returns>
//        public Item[] GetItemList()
//        {
//            return this.itemList;
//        }

//        /// <summary>
//        /// ��item�Ƴ�����
//        /// </summary>
//        /// <param name="item"></param>
//        /// <returns></returns>
//        public bool RemoveItem(Item item)
//        {
//            if (item == null || item.OwnInventory != this)
//                return false;
//            for (int i = 0; i < MaxSize; ++i)
//            {
//                if (this.itemList[i] == item)
//                {
//                    item.OwnInventory = null;
//                    this.itemList[i] = null;
//                    --this.Size;
//                    this.OnItemListChanged?.Invoke(this);
//                    return true;
//                }
//            }
//            return false;
//        }

//        /// <summary>
//        /// ��item�Ƴ�����
//        /// amount ���� ���ֵ����ȫ���Ƴ�
//        /// </summary>
//        /// <param name="item"></param>
//        /// <returns></returns>
//        public Item RemoveItem(Item item, int amount)
//        {
//            if (item == null || item.OwnInventory != this || amount <= 0)
//                return null;

//            if (!item.bCanStack && amount == 1)
//            {
//                if (this.RemoveItem(item))
//                {
//                    return item;
//                }
//            }

//            for (int i = 0; i < MaxSize; ++i)
//            {
//                if (this.itemList[i] == item)
//                {
//                    Item ans = ItemSpawner.Instance.SpawnItem(item.ID);
//                    ans.Amount = Mathf.Min(amount, item.Amount);

//                    item.Amount -= amount;
//                    this.OnItemListChanged?.Invoke(this);
//                    return ans;
//                }
//            }
//            return null;
//        }

//        /// <summary>
//        /// �Ƴ�ĳһ�� Item
//        /// </summary>
//        /// <param name="itemID"></param>
//        /// <param name="amount"></param>
//        /// <returns></returns>
//        public bool RemoveItem(string itemID, int amount)
//        {
//            // ��������
//            if (this.GetItemAllNum(itemID) < amount)
//                return false;

//            for (int i = 0; i < MaxSize; ++i)
//            {
//                if (this.itemList[i] == null)
//                {
//                    continue;
//                }
//                if (this.itemList[i].ID == itemID)
//                {
//                    // �����㹻
//                    if (this.itemList[i].Amount >= amount)
//                    {
//                        this.itemList[i].Amount -= amount;
//                        this.OnItemListChanged?.Invoke(this);
//                        break;
//                    }
//                    // ��������
//                    else
//                    {
//                        amount -= this.itemList[i].Amount;
//                        // ����
//                        this.itemList[i].Amount = 0;
//                        continue;
//                    }
//                }
//            }
//            return true;
//        }

//        /// <summary>
//        /// ���ֲ�� Item
//        /// </summary>
//        /// <param name="item"></param>
//        /// <returns></returns>
//        public bool BinarySplitItem(Item item)
//        {
//            if (item == null || !item.bCanStack || item.Amount < 2 || this.Size >= this.MaxSize || item.OwnInventory != this)
//            {
//                return false;
//            }

//            for (int i = 0; i < MaxSize; ++i)
//            {
//                if (this.itemList[i] == item)
//                {
//                    int amount = item.Amount / 2;
//                    item.Amount -= amount;

//                    Item item1 = ItemSpawner.Instance.SpawnItem(item.ID);
//                    item1.Amount = amount;
//                    this.AddItem(item1, true);
//                    return true;
//                }
//            }

//            return false;
//        }

//        /// <summary>
//        /// �� ID �������� Item
//        /// </summary>
//        public void SortItem()
//        {
//            this.itemList.Sort((a, b) =>
//            {
//                return a.ID >= b.ID;
//            });
//            this.OnItemListChanged?.Invoke(this);
//        }

//        /// <summary>
//        /// �ϲ�������ͬ���͵�Item
//        /// </summary>
//        /// <param name="A"></param>
//        /// <param name="B"></param>
//        /// <returns></returns>
//        public bool MergeItems(int AIndex, int BIndex)
//        {
//            if (((AIndex < 0 || AIndex > this.MaxSize) || BIndex < 0 || BIndex > this.MaxSize) || (this.itemList[AIndex] == null || this.itemList[BIndex] == null) || AIndex == BIndex || (this.itemList[AIndex].IsFillUp || this.itemList[BIndex].IsFillUp))
//            {
//                return false;
//            }
//            bool ans = Item.MergeItems(this.itemList[AIndex], this.itemList[BIndex]);
//            this.OnItemListChanged?.Invoke(this);
//            return ans;
//        }

//        /// <summary>
//        /// ���������е�Item
//        /// </summary>
//        /// <param name="A"></param>
//        /// <param name="B"></param>
//        /// <returns></returns>
//        public bool SwapItem(int AIndex, int BIndex)
//        {
//            if ((AIndex < 0 || AIndex > this.MaxSize) || BIndex < 0 || BIndex > this.MaxSize || AIndex == BIndex)
//            {
//                return false;
//            }
//            Item tmp = this.itemList[AIndex];
//            this.itemList[AIndex] = this.itemList[BIndex];
//            this.itemList[BIndex] = tmp;
//            this.OnItemListChanged?.Invoke(this);
//            return true;
//        }

//        /// <summary>
//        /// ��ȡitem�ڱ����е�index
//        /// -1 ��ʾ������
//        /// </summary>
//        /// <param name="item"></param>
//        /// <returns></returns>
//        public int GetItemIndex(Item item)
//        {
//            if (item == null || item.OwnInventory != this)
//                return -1;

//            for (int i = 0; i < MaxSize; ++i)
//            {
//                if (this.itemList[i] == item)
//                {
//                    return i;
//                }
//            }
//            return -1;
//        }

//        public void UseItem(int index, int amount)
//        {
//            if ((index < 0 || index > this.MaxSize) || this.itemList[index] == null)
//            {
//                return;
//            }
//            this.itemList[index].Execute(amount);
//            OnItemListChanged?.Invoke(this);
//        }

//        /// <summary>
//        /// ��ȡ������ָ�� id Item ������
//        /// </summary>
//        /// <param name="id"></param>
//        /// <returns></returns>
//        public int GetItemAllNum(string id)
//        {
//            int ans = 0;

//            foreach (var item in this.itemList)
//            {
//                if (item != null && item.ID == id)
//                {
//                    ans += item.Amount;
//                }
//            }

//            return ans;
//        }
//    }
//}
