using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.UI
{
    /// <summary>
    /// UI������ӿ�
    /// </summary>
    public interface IUIInteractionItem
    {
        /// <summary>
        /// ����UI��RectTransform
        /// </summary>
        public RectTransform rectTransform { get; set; }

        /// <summary>
        /// ����Ļ�ϵ����� => ���ü���
        /// </summary>
        public Vector2 OnScreenPosition { get; }

        /// <summary>
        /// ��Ϊ������ʱ����
        /// </summary>
        public virtual void OnActive()
        {
            // to-do : Ĭ��ʵ�֣���һ���߿򣬱�ʾ����ѡ��
        }

        /// <summary>
        /// �ɼ������Ϊ�Ǽ�����ʱ����
        /// </summary>
        public virtual void OnDisactive()
        {
            // to-do : Ĭ��ʵ�֣��Ƴ���ʾ����ѡ�еı߿�
        }
    
        public virtual void Interact(object Instigator)
        {

        }
    }

    public sealed class UIManager : Manager.GlobalManager.IGlobalManager
    {
        #region UIջ����
        /// <summary>
        /// ��ǰ��ʾ��UIջ
        /// </summary>
        private Stack<UIBasePanel> panelStack = new Stack<UIBasePanel>();

        /// <summary>
        /// �رյ�ǰ���ϲ�UI����ײ�UI�޷��رգ�������û����ײ�UI����Ϊnull
        /// </summary>
        public UIBasePanel PopPanel()
        {
            //��ײ�UI->start�޷��Ƴ�
            if (panelStack.Count < 2)
            {
                return null;
            }
            // ����ջ��
            UIBasePanel topPanel = panelStack.Pop();
            topPanel.OnExit();

            // ����ջ��
            panelStack.Peek().OnRecovery();
            return topPanel;
        }

        /// <summary>
        /// ��UI��ʾ�ڽ�����
        /// </summary>
        /// <param name="panelType"></param>
        public void PushPanel(UIBasePanel panel)
        {
            // ��ͣջ��
            if (panelStack.Count > 0)
            {
                panelStack.Peek().OnPause();
            }
            // ѹ��ջ
            panel.OnEnter();
            panelStack.Push(panel);
        }
        /// <summary>
        /// ��UIջ����Ϊ0��1ʱ�����Ը�����ײ�UIPanel��ֻ��ͨ���˺�������ײ�UIPanel��ջ
        /// ��ջΪ��ʱ������ͨ��PushPanelѹ����ײ��UIPanel
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
        /// ���ջ����UIPanel
        /// </summary>
        /// <returns></returns>
        public UIBasePanel GetTopUIPanel()
        {
            return this.panelStack.Peek();
        }

        #endregion

        #region �����б������
        /// <summary>
        /// �������б�
        /// </summary>
        private List<IUIInteractionItem> interactionItemList = new List<IUIInteractionItem>();
        /// <summary>
        /// ��ǰѡ�еļ���Ľ�����Index
        /// </summary>
        private int activeItemIndex = 0;
        /// <summary>
        /// ��ȡ��ǰѡ�е�UI������
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
        /// ��ս������б�
        /// </summary>
        public void ClearInteractionItemList()
        {
            this.ResetInteractionItemIndex();
            this.TryDisactiveInteractionItem();
            this.interactionItemList.Clear();
        }
        /// <summary>
        /// ���ݵ���ʱ��InteractionItemListÿһ��ĵ�ǰ��Ļλ�ã���������
        /// ���� ���� -> ���� ��˳����� => �� y������� �� xС������
        /// </summary>
        public void SortInteractionItemList()
        {
            this.TryDisactiveInteractionItem();
            this.interactionItemList.Sort((a, b) =>
            {
                // �������£���������
                if (a.OnScreenPosition.y > b.OnScreenPosition.y || (a.OnScreenPosition.y == b.OnScreenPosition.y && a.OnScreenPosition.x <= b.OnScreenPosition.x))
                    return 1;
                return -1;
            });
            this.activeItemIndex = 0;
            this.TryActiveInteractionItem();
        }

        /// <summary>
        /// ����һ��UIInteractionItem => �����ظ�����
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
        /// �Ƴ��ض���Item
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
        /// �Ƴ��ض���Item
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
        /// �����������ظ�����
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
        /// �Ƴ����������Ŀɽ�����,�Ƴ�֮��ض���������
        /// </summary>
        /// <param name="match"></param>
        /// <param name="isSort"></param>
        public void RemoveAllInteractionItem(System.Predicate<IUIInteractionItem> match)
        {
            this.interactionItemList.RemoveAll(match);
            this.SortInteractionItemList();
        }

        /// <summary>
        /// �Ƴ������еĿɽ�����,�Ƴ���ض���������
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
        /// InteractionItemIndex ƫ��
        /// </summary>
        /// <param name="last"></param>
        /// <returns></returns>
        public int OffsetInteractionItemActiveIndex(int offset)
        {
            return this.activeItemIndex = (this.interactionItemList.Count + this.activeItemIndex + offset) % this.interactionItemList.Count;
        }

        /// <summary>
        /// ����һ��
        /// </summary>
        /// <returns></returns>
        public int BackwardOneInteractionItemActiveIndex()
        {
            return this.OffsetInteractionItemActiveIndex(-1);
        }

        /// <summary>
        /// ǰ��һ��
        /// </summary>
        /// <returns></returns>
        public int ForwardOneInteractionItemActiveIndex()
        {

            return this.OffsetInteractionItemActiveIndex(1);
        }

        #endregion
    }

}
