using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using ML.Engine.InventorySystem;

namespace ML.Example.InventorySystem
{
    public class UIItem : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerDownHandler
    {
        #region Field|Property
        [ShowInInspector, ReadOnly]
        /// <summary>
        /// 当前选择激活的Item
        /// </summary>
        public static UIItem CurActiveItem
        {
            get;
            protected set;
        }

        /// <summary>
        /// 所属 UIInventory
        /// </summary>
        public UIInventory OwnUIInventoy;

        /// <summary>
        /// 在仓库中的索引
        /// </summary>
        public int ItemIndex;

        /// <summary>
        /// 对应的Item
        /// </summary>
        public Item item
        {
            get
            {
                return this.OwnUIInventoy.GetInventory().GetItemList()[this.ItemIndex];
            }
        }

        protected RectTransform ItemImage;

        protected Text AmountText;

        protected Image Border;
        #endregion

        public void Init()
        {
            if (this.item == null || !ItemSpawner.Instance.GetCanStack(this.item.ID))
            {
                this.AmountText.text = "";
            }
            else
            {
                this.AmountText.text = this.item.Amount.ToString();
            }
        }

        private void Awake()
        {
            this.ItemImage = this.transform.Find("ItemImage") as RectTransform;
            this.AmountText = this.transform.Find("AmountText").GetComponent<Text>();
            this.Border = this.transform.Find("GridSlot").Find("Border").GetComponent<Image>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            //Debug.Log("Begin Drag Item " + this.item.ID);
        }

        public void OnDrag(PointerEventData eventData)
        {
            // 无法操作空格子
            if (CurActiveItem == null)
            {
                return;
            }
            CurActiveItem.ItemImage.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // 无法操作空格子
            if (CurActiveItem == null)
            {
                return;
            }
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);
            // 有命中
            if(raycastResults.Count != 0)
            {
                RaycastResult hit = raycastResults[0];
                UIItem hitItem = hit.gameObject.GetComponent<UIItem>();
                // 命中自己
                if (hitItem == CurActiveItem)
                {
                    this.ItemImage.localPosition = Vector2.zero;
                }
                // 命中格子
                if (this.OwnUIInventoy.MergeItem(hitItem, CurActiveItem) || this.OwnUIInventoy.SwapItem(hitItem, CurActiveItem))
                {
                    this.ItemImage.localPosition = Vector2.zero;
                }
                // 命中背包
                else if (hit.gameObject.transform == this.OwnUIInventoy.itemSlotContainer)
                {
                    this.ItemImage.localPosition = Vector2.zero;
                }
                // 未命中
                else
                {
                    this.OwnUIInventoy.DropAllActivedItem();
                    this.ItemImage.localPosition = Vector2.zero;
                }
            }
            // 未命中
            else
            {
                Item item = CurActiveItem.item;
                this.OwnUIInventoy.DropAllActivedItem();
                this.ItemImage.localPosition = Vector2.zero;
            }
        }


        /// <summary>
        /// 保证 Up 能生效
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerDown(PointerEventData eventData)
        {
            // 无法操作空格子
            if (this.item == null)
            {
                if (CurActiveItem != null)
                {
                    CurActiveItem.Border.color = new Color(103f / 256, 99f / 256, 99f / 256);
                }
                CurActiveItem = null;
                return;
            }
            ActiveUIItem(this);
        }

        public void SetImage(Sprite sprite)
        {
            this.ItemImage.GetComponent<Image>().sprite = sprite;
        }

        public void Refresh()
        {
            if (CurActiveItem == this && this.item == null)
            {
                ActiveUIItem(null);
            }

            this.Init();

            if (this.item != null)
            {
                this.SetImage(ItemSpawner.Instance.GetItemSprite(this.item.ID));
            }
            else
            {
                this.SetImage(null);
            }
        }

        public static void ActiveUIItem(UIItem item)
        {
            if(item == CurActiveItem)
            {
                return;
            }
            if (CurActiveItem != null)
            {
                CurActiveItem.Border.color = new Color(103f / 256, 99f / 256, 99f / 256);
            }
            CurActiveItem = item;
            if(CurActiveItem)
                CurActiveItem.Border.color = new Color(222f / 256, 175f / 256, 175f / 256);
        }

    }
}

