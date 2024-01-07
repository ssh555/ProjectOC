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
    /// 背包 UI 面板
    /// </summary>
    public sealed class UIInventory : MonoBehaviour
    {
        /// <summary>
        /// 所属背包
        /// </summary>
        private Inventory inventory;

        private void Awake()
        {
            this.itemSlotContainer = this.transform.Find("ItemSlotContainer");
            this.itemSlotTemplate = this.itemSlotContainer.GetChild(0).GetChild(0).Find("ItemSlotTemplate").GetComponent<UIItem>();

            Transform btnContainer = this.transform.Find("ButtonContainer");

            // 一键根据 ID 升序整理
            btnContainer.Find("SortBtn").GetComponent<Button>().onClick.AddListener(this.OneClickSort);
            // 使用激活的UIItem
            btnContainer.Find("UseBtn").GetComponent<Button>().onClick.AddListener(this.UseActivedItem);
            // 二分拆分激活的UIItem
            btnContainer.Find("SplitBtn").GetComponent<Button>().onClick.AddListener(this.BinarySplitActivedItem);
            // 丢弃 1个 激活的UIItem
            btnContainer.Find("DropBtn").GetComponent<Button>().onClick.AddListener(this.DropOneActivedItem);


        }

        /// <summary>
        /// Item 显示容器
        /// 即 UIInventory背包区域
        /// </summary>
        [ShowInInspector]
        public Transform itemSlotContainer { get; private set; }

        /// <summary>
        /// Item 显示模板
        /// 即 Item 格子
        /// </summary>
        private UIItem itemSlotTemplate;

        /// <summary>
        /// 设置所属背包
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
        /// 获得所属背包
        /// </summary>
        /// <returns></returns>
        public Inventory GetInventory()
        {
            return inventory;
        }
        
        /// <summary>
        /// 背包列表改变时调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Inventory_OnItemListChanged(Inventory inventory)
        {
            RefreshInventoryItems();
        }

        private void InitInventoryItems()
        {
            // 删除旧的 UIItem
            foreach (Transform child in this.itemSlotTemplate.transform.parent)
            {
                // 不删除模板
                if (child == itemSlotTemplate.transform) continue;
                // 删除其余生成的 ItemSlot
                Destroy(child.gameObject);
            }

            Item[] items = inventory.GetItemList();

            // 生成新的UIItem
            for (int i = 0; i < this.inventory.MaxSize; ++i)
            {
                UIItem itemSlot = Instantiate(itemSlotTemplate.transform, itemSlotTemplate.transform.parent).GetComponent<UIItem>();
                itemSlot.gameObject.SetActive(true);
                itemSlot.OwnUIInventoy = this;
                ItemManager.Instance.InitUIItem(itemSlot, i);
            }
        }

        /// <summary>
        /// 刷新背包UI
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
        /// 一键根据 ID 升序整理
        /// </summary>
        public void OneClickSort()
        {
            this.inventory.SortItem();
        }

        /// <summary>
        /// 使用激活的UIItem
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
        /// 二分拆分激活的UIItem
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
        /// 丢弃 1个 激活的UIItem
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

