using ML.Engine.InventorySystem;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ML.Example.InventorySystem
{
    /// <summary>
    /// ���� UI ���
    /// </summary>
    public sealed class UIInventory : MonoBehaviour
    {
        /// <summary>
        /// ��������
        /// </summary>
        private Inventory inventory;

        private void Awake()
        {
            this.itemSlotContainer = this.transform.Find("ItemSlotContainer");
            this.itemSlotTemplate = this.itemSlotContainer.GetChild(0).GetChild(0).Find("ItemSlotTemplate").GetComponent<UIItem>();

            Transform btnContainer = this.transform.Find("ButtonContainer");

            // һ������ ID ��������
            btnContainer.Find("SortBtn").GetComponent<Button>().onClick.AddListener(this.OneClickSort);
            // ʹ�ü����UIItem
            btnContainer.Find("UseBtn").GetComponent<Button>().onClick.AddListener(this.UseActivedItem);
            // ���ֲ�ּ����UIItem
            btnContainer.Find("SplitBtn").GetComponent<Button>().onClick.AddListener(this.BinarySplitActivedItem);
            // ���� 1�� �����UIItem
            btnContainer.Find("DropBtn").GetComponent<Button>().onClick.AddListener(this.DropOneActivedItem);


        }

        /// <summary>
        /// Item ��ʾ����
        /// �� UIInventory��������
        /// </summary>
        [ShowInInspector]
        public Transform itemSlotContainer { get; private set; }

        /// <summary>
        /// Item ��ʾģ��
        /// �� Item ����
        /// </summary>
        private UIItem itemSlotTemplate;

        /// <summary>
        /// ������������
        /// </summary>
        /// <param name="inventory"></param>
        public void SetInventory(Inventory inventory)
        {
            if(this.inventory != null)
            {
                this.inventory.OnItemListChanged -= Inventory_OnItemListChanged;
            }

            this.inventory = inventory;
            this.inventory.OnItemListChanged += Inventory_OnItemListChanged;
            InitInventoryItems();
        }
       
        /// <summary>
        /// �����������
        /// </summary>
        /// <returns></returns>
        public Inventory GetInventory()
        {
            return inventory;
        }
        
        /// <summary>
        /// �����б�ı�ʱ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Inventory_OnItemListChanged(Inventory inventory)
        {
            RefreshInventoryItems();
        }

        private void InitInventoryItems()
        {
            // ɾ���ɵ� UIItem
            foreach (Transform child in this.itemSlotTemplate.transform.parent)
            {
                // ��ɾ��ģ��
                if (child == itemSlotTemplate.transform) continue;
                // ɾ���������ɵ� ItemSlot
                Destroy(child.gameObject);
            }

            Item[] items = inventory.GetItemList();

            // �����µ�UIItem
            for (int i = 0; i < this.inventory.MaxSize; ++i)
            {
                UIItem itemSlot = Instantiate(itemSlotTemplate.transform, itemSlotTemplate.transform.parent).GetComponent<UIItem>();
                itemSlot.gameObject.SetActive(true);
                itemSlot.OwnUIInventoy = this;
                ItemManager.Instance.InitUIItem(itemSlot, i);
            }
        }

        /// <summary>
        /// ˢ�±���UI
        /// </summary>
        private void RefreshInventoryItems()
        {
            foreach (Transform child in this.itemSlotTemplate.transform.parent)
            {
                if (child == itemSlotTemplate.transform) continue;
                child.GetComponent<UIItem>().Refresh();
            }
        }

        /// <summary>
        /// һ������ ID ��������
        /// </summary>
        public void OneClickSort()
        {
            this.inventory.SortItem();
        }

        /// <summary>
        /// ʹ�ü����UIItem
        /// </summary>
        public void UseActivedItem()
        {
            if(UIItem.CurActiveItem == null)
            {
                return;
            }
            //int index = UIItem.CurActiveItem.ItemIndex;
            this.inventory.UseItem(UIItem.CurActiveItem.ItemIndex, 1);
            //if (this.inventory.GetItemList()[index] != null)
            //{
            //    UIItem.ActiveUIItem(this.itemSlotContainer.GetChild(this.inventory.MaxSize + index + 1).GetComponent<UIItem>());
            //}
        }

        /// <summary>
        /// ���ֲ�ּ����UIItem
        /// </summary>
        public void BinarySplitActivedItem()
        {
            if(UIItem.CurActiveItem == null)
            {
                return;
            }
            this.inventory.BinarySplitItem(UIItem.CurActiveItem.item);
            //int index = UIItem.CurActiveItem.ItemIndex;
            //if (this.inventory.BinarySplitItem(UIItem.CurActiveItem.item))
            //{
            //    if (this.inventory.GetItemList()[index] != null)
            //    {
            //        UIItem.ActiveUIItem(this.itemSlotContainer.GetChild(this.inventory.MaxSize + index + 1).GetComponent<UIItem>());
            //    }
            //}
        }

        /// <summary>
        /// ���� 1�� �����UIItem
        /// </summary>
        public void DropOneActivedItem()
        {
            if(UIItem.CurActiveItem == null)
            {
                return;
            }
            //int index = UIItem.CurActiveItem.ItemIndex;
            Transform owner = this.inventory.Owner;
            ItemManager.Instance.SpawnWorldItem(this.inventory.RemoveItem(UIItem.CurActiveItem.item, 1), owner.position, Quaternion.identity);
            //if (this.inventory.GetItemList()[index] != null)
            //{
            //    UIItem.ActiveUIItem(this.itemSlotContainer.GetChild(this.inventory.MaxSize + index + 1).GetComponent<UIItem>());
            //}
        }
    
        public WorldItem DropAllActivedItem()
        {
            Item item = UIItem.CurActiveItem.item;
            bool ans = this.inventory.RemoveItem(UIItem.CurActiveItem.item);
            if (ans)
            {
                Transform owner = this.inventory.Owner;
                return ItemManager.Instance.SpawnWorldItem(item, owner.position, Quaternion.identity);
            }

            return null;
        }
    
        public bool SwapItem(UIItem A, UIItem B)
        {
            if(A == null || B== null)
            {
                return false;
            }
            if(this.inventory.SwapItem(A.ItemIndex, B.ItemIndex))
            {
                UIItem.ActiveUIItem(null);
                return true;
            }
            return false;
        }
    
        public bool MergeItem(UIItem A, UIItem B)
        {
            if (A == null || B == null)
            {
                return false;
            }
            return this.inventory.MergeItems(A.ItemIndex, B.ItemIndex);
        }

        private void OnDestroy()
        {
            if(this.inventory != null)
                this.inventory.OnItemListChanged -= this.Inventory_OnItemListChanged;
        }
    }

}

