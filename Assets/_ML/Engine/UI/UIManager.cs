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

    public sealed class UIManager : Manager.GlobalManager.IGlobalManager
    {
        #region UI栈管理
        /// <summary>
        /// 当前显示的UI栈
        /// </summary>
        private Stack<UIBasePanel> panelStack = new Stack<UIBasePanel>();

        /// <summary>
        /// 关闭当前最上层UI，最底层UI无法关闭，但可以没有最底层UI，即为null
        /// </summary>
        public UIBasePanel PopPanel()
        {
            //最底层UI->start无法移除
            if (panelStack.Count < 2)
            {
                return null;
            }
            // 弹出栈顶
            UIBasePanel topPanel = panelStack.Pop();
            topPanel.OnExit();

            // 激活栈顶
            panelStack.Peek().OnRecovery();
            return topPanel;
        }

        /// <summary>
        /// 把UI显示在界面上
        /// </summary>
        /// <param name="panelType"></param>
        public void PushPanel(UIBasePanel panel)
        {
            // 暂停栈顶
            if (panelStack.Count > 0)
            {
                panelStack.Peek().OnPause();
            }
            // 压入栈
            panel.OnEnter();
            panelStack.Push(panel);
        }
        /// <summary>
        /// 当UI栈数量为0或1时，可以更改最底层UIPanel，只能通过此函数将最底层UIPanel出栈
        /// 但栈为空时，可以通过PushPanel压入最底层的UIPanel
        /// </summary>
        /// <returns></returns>
        public bool ChangeBotUIPanel(UIBasePanel panel)
        {
            if(this.panelStack.Count > 2)
            {
                return false;
            }
            if (this.panelStack.Count == 1)
            {
                this.panelStack.Pop().OnExit();
            }

            if(panel != null)
            {
                this.PushPanel(panel);
            }

            return true;
        }

        /// <summary>
        /// 获得栈顶的UIPanel
        /// </summary>
        /// <returns></returns>
        public UIBasePanel GetTopUIPanel()
        {
            return this.panelStack.Peek();
        }

        #endregion

        #region 交互列表项管理
        /// <summary>
        /// 交互项列表
        /// </summary>
        private List<IUIInteractionItem> interactionItemList = new List<IUIInteractionItem>();
        /// <summary>
        /// 当前选中的激活的交互项Index
        /// </summary>
        private int activeItemIndex = 0;
        /// <summary>
        /// 获取当前选中的UI交互项
        /// </summary>
        public IUIInteractionItem GetActivedInteractionItem
        {
            get
            {
                if (this.activeItemIndex < 0 || this.interactionItemList.Count <= this.activeItemIndex)
                {
                    return null;
                }
                return this.interactionItemList[this.activeItemIndex];
            }
        }
        private void TryActiveInteractionItem()
        {
            if (this.GetActivedInteractionItem != null)
            {
                this.GetActivedInteractionItem.OnActive();
            }
        }
        private void TryDisactiveInteractionItem()
        {
            if (this.GetActivedInteractionItem != null)
            {
                this.GetActivedInteractionItem.OnDisactive();
            }
        }
        private void ResetInteractionItemIndex()
        {
            this.TryDisactiveInteractionItem();
            this.activeItemIndex = 0;
            this.TryActiveInteractionItem();
        }

        /// <summary>
        /// 清空交互项列表
        /// </summary>
        public void ClearInteractionItemList()
        {
            this.ResetInteractionItemIndex();
            this.TryDisactiveInteractionItem();
            this.interactionItemList.Clear();
        }
        /// <summary>
        /// 根据调用时的InteractionItemList每一项的当前屏幕位置，升序排序
        /// 用于 左上 -> 右下 的顺序遍历 => ① y大的优先 ② x小的优先
        /// </summary>
        public void SortInteractionItemList()
        {
            this.TryDisactiveInteractionItem();
            this.interactionItemList.Sort((a, b) =>
            {
                // 先排上下，再排左右
                if (a.OnScreenPosition.y > b.OnScreenPosition.y || (a.OnScreenPosition.y == b.OnScreenPosition.y && a.OnScreenPosition.x <= b.OnScreenPosition.x))
                    return 1;
                return -1;
            });
            this.activeItemIndex = 0;
            this.TryActiveInteractionItem();
        }

        /// <summary>
        /// 加入一个UIInteractionItem => 不会重复加入
        /// </summary>
        /// <param name="item"></param>
        public bool AddInteractionItem(IUIInteractionItem item, bool isSort = false)
        {
            if (item == null || this.interactionItemList.Contains(item))
            {
                return false;
            }
            this.interactionItemList.Add(item);
            if (isSort)
            {
                this.SortInteractionItemList();
            }
            return true;
        }
        
        /// <summary>
        /// 移除特定的Item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool RemoveInteractionItem(IUIInteractionItem item)
        {
            if (item == null || this.interactionItemList.Contains(item))
            {
                return false;
            }
            if(this.GetActivedInteractionItem == item)
            {
                this.TryDisactiveInteractionItem();
                this.interactionItemList.Remove(item);
                this.activeItemIndex = 0;
                this.TryActiveInteractionItem();
            }
            else
            {
                this.interactionItemList.Remove(item);
            }
            return true;
        }

        /// <summary>
        /// 移除特定的Item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool RemoveAtInteractionItem(int index)
        {
            if (index < 0 || this.interactionItemList.Count <= index)
            {
                return false;
            }
            if(this.GetActivedInteractionItem == this.interactionItemList[index])
            {
                this.TryDisactiveInteractionItem();
                this.interactionItemList.RemoveAt(index);
                this.activeItemIndex = 0;
                this.TryActiveInteractionItem();
            }
            else
            {
                this.interactionItemList.RemoveAt(index);
            }
            return true;
        }

        /// <summary>
        /// 操作不当会重复加入
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="isSort"></param>
        public void AddRangeInteractionItem(IEnumerable<IUIInteractionItem> collection, bool isSort)
        {
            this.interactionItemList.AddRange(collection);
            if(isSort)
            {
                this.SortInteractionItemList();
            }
        }

        /// <summary>
        /// 移除满足条件的可交互项,移除之后必定重新排序
        /// </summary>
        /// <param name="match"></param>
        /// <param name="isSort"></param>
        public void RemoveAllInteractionItem(System.Predicate<IUIInteractionItem> match)
        {
            this.interactionItemList.RemoveAll(match);
            this.SortInteractionItemList();
        }

        /// <summary>
        /// 移除集合中的可交互项,移除后必定重新排序
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="isSort"></param>
        public void RemoveAllInteractionItem(IEnumerable<IUIInteractionItem> collection)
        {
            foreach(var item in collection)
            {
                this.interactionItemList.Remove(item);
            }
            this.SortInteractionItemList();
        }

        /// <summary>
        /// InteractionItemIndex 偏移
        /// </summary>
        /// <param name="last"></param>
        /// <returns></returns>
        public int OffsetInteractionItemActiveIndex(int offset)
        {
            return this.activeItemIndex = (this.interactionItemList.Count + this.activeItemIndex + offset) % this.interactionItemList.Count;
        }

        /// <summary>
        /// 后退一个
        /// </summary>
        /// <returns></returns>
        public int BackwardOneInteractionItemActiveIndex()
        {
            return this.OffsetInteractionItemActiveIndex(-1);
        }

        /// <summary>
        /// 前进一个
        /// </summary>
        /// <returns></returns>
        public int ForwardOneInteractionItemActiveIndex()
        {

            return this.OffsetInteractionItemActiveIndex(1);
        }

        #endregion
    }

}
