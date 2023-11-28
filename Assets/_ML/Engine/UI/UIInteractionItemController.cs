using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.UI
{
    /// <summary>
    /// UI交互项接口
    /// </summary>
    public interface IUIInteractionItem
    {
        /// <summary>
        /// 所处UI的RectTransform
        /// </summary>
        public RectTransform rectTransform { get; set; }

        /// <summary>
        /// 在屏幕上的坐标 => 引用计算
        /// </summary>
        public Vector2 OnScreenPosition { get; }

        /// <summary>
        /// 成为激活项时调用
        /// </summary>
        public virtual void OnActive()
        {
            // to-do : 默认实现，加一个边框，表示正在选中
        }

        /// <summary>
        /// 由激活项成为非激活项时调用
        /// </summary>
        public virtual void OnDisactive()
        {
            // to-do : 默认实现，移除表示正在选中的边框
        }

        public virtual void Interact(object Instigator)
        {

        }
    }


    public class UIInteractionItemController
    {
        #region 交互列表项管理
        /// <summary>
        /// 交互项列表
        /// </summary>
        private List<IUIInteractionItem> itemList = new List<IUIInteractionItem>();
        /// <summary>
        /// 当前选中的激活的交互项Index
        /// </summary>
        private int activeIndex = 0;
        /// <summary>
        /// 获取当前选中的UI交互项
        /// </summary>
        public IUIInteractionItem GetActivedItem
        {
            get
            {
                if (this.activeIndex < 0 || this.itemList.Count <= this.activeIndex)
                {
                    return null;
                }
                return this.itemList[this.activeIndex];
            }
        }
        private void TryActiveItem()
        {
            if (this.GetActivedItem != null)
            {
                this.GetActivedItem.OnActive();
            }
        }
        private void TryDisactiveItem()
        {
            if (this.GetActivedItem != null)
            {
                this.GetActivedItem.OnDisactive();
            }
        }
        private void ResetItemIndex()
        {
            this.TryDisactiveItem();
            this.activeIndex = 0;
            this.TryActiveItem();
        }

        /// <summary>
        /// 清空交互项列表
        /// </summary>
        public void ClearItemList()
        {
            this.ResetItemIndex();
            this.TryDisactiveItem();
            this.itemList.Clear();
        }
        /// <summary>
        /// 根据调用时的InteractionItemList每一项的当前屏幕位置，升序排序
        /// 用于 左上 -> 右下 的顺序遍历 => ① y大的优先 ② x小的优先
        /// </summary>
        public void SortItemList()
        {
            this.TryDisactiveItem();
            this.itemList.Sort((a, b) =>
            {
                // 先排上下，再排左右
                if (a.OnScreenPosition.y > b.OnScreenPosition.y || (a.OnScreenPosition.y == b.OnScreenPosition.y && a.OnScreenPosition.x <= b.OnScreenPosition.x))
                    return 1;
                return -1;
            });
            this.activeIndex = 0;
            this.TryActiveItem();
        }

        /// <summary>
        /// 加入一个UIInteractionItem => 不会重复加入
        /// </summary>
        /// <param name="item"></param>
        public bool AddItem(IUIInteractionItem item, bool isSort = false)
        {
            if (item == null || this.itemList.Contains(item))
            {
                return false;
            }
            this.itemList.Add(item);
            if (isSort)
            {
                this.SortItemList();
            }
            return true;
        }

        /// <summary>
        /// 移除特定的Item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool RemoveItem(IUIInteractionItem item)
        {
            if (item == null || this.itemList.Contains(item))
            {
                return false;
            }
            if (this.GetActivedItem == item)
            {
                this.TryDisactiveItem();
                this.itemList.Remove(item);
                this.activeIndex = 0;
                this.TryActiveItem();
            }
            else
            {
                this.itemList.Remove(item);
            }
            return true;
        }

        /// <summary>
        /// 移除特定的Item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool RemoveAtItem(int index)
        {
            if (index < 0 || this.itemList.Count <= index)
            {
                return false;
            }
            if (this.GetActivedItem == this.itemList[index])
            {
                this.TryDisactiveItem();
                this.itemList.RemoveAt(index);
                this.activeIndex = 0;
                this.TryActiveItem();
            }
            else
            {
                this.itemList.RemoveAt(index);
            }
            return true;
        }

        /// <summary>
        /// 操作不当会重复加入
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="isSort"></param>
        public void AddRangeItem(IEnumerable<IUIInteractionItem> collection, bool isSort)
        {
            this.itemList.AddRange(collection);
            if (isSort)
            {
                this.SortItemList();
            }
        }

        /// <summary>
        /// 移除满足条件的可交互项,移除之后必定重新排序
        /// </summary>
        /// <param name="match"></param>
        /// <param name="isSort"></param>
        public void RemoveAllItem(System.Predicate<IUIInteractionItem> match)
        {
            this.itemList.RemoveAll(match);
            this.SortItemList();
        }

        /// <summary>
        /// 移除集合中的可交互项,移除后必定重新排序
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="isSort"></param>
        public void RemoveAllItem(IEnumerable<IUIInteractionItem> collection)
        {
            foreach (var item in collection)
            {
                this.itemList.Remove(item);
            }
            this.SortItemList();
        }

        /// <summary>
        /// InteractionItemIndex 偏移
        /// </summary>
        /// <param name="last"></param>
        /// <returns></returns>
        public int OffsetActiveIndex(int offset)
        {
            return this.activeIndex = (this.itemList.Count + this.activeIndex + offset) % this.itemList.Count;
        }

        /// <summary>
        /// 后退一个
        /// </summary>
        /// <returns></returns>
        public int BackwardOneActiveIndex()
        {
            return this.OffsetActiveIndex(-1);
        }

        /// <summary>
        /// 前进一个
        /// </summary>
        /// <returns></returns>
        public int ForwardOneActiveIndex()
        {

            return this.OffsetActiveIndex(1);
        }

        #endregion
    }

}
