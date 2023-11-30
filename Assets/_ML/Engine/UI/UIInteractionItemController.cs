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


    public class UIInteractionItemController
    {
        #region �����б������
        /// <summary>
        /// �������б�
        /// </summary>
        private List<IUIInteractionItem> itemList = new List<IUIInteractionItem>();
        /// <summary>
        /// ��ǰѡ�еļ���Ľ�����Index
        /// </summary>
        private int activeIndex = 0;
        /// <summary>
        /// ��ȡ��ǰѡ�е�UI������
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
        /// ��ս������б�
        /// </summary>
        public void ClearItemList()
        {
            this.ResetItemIndex();
            this.TryDisactiveItem();
            this.itemList.Clear();
        }
        /// <summary>
        /// ���ݵ���ʱ��InteractionItemListÿһ��ĵ�ǰ��Ļλ�ã���������
        /// ���� ���� -> ���� ��˳����� => �� y������� �� xС������
        /// </summary>
        public void SortItemList()
        {
            this.TryDisactiveItem();
            this.itemList.Sort((a, b) =>
            {
                // �������£���������
                if (a.OnScreenPosition.y > b.OnScreenPosition.y || (a.OnScreenPosition.y == b.OnScreenPosition.y && a.OnScreenPosition.x <= b.OnScreenPosition.x))
                    return 1;
                return -1;
            });
            this.activeIndex = 0;
            this.TryActiveItem();
        }

        /// <summary>
        /// ����һ��UIInteractionItem => �����ظ�����
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
        /// �Ƴ��ض���Item
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
        /// �Ƴ��ض���Item
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
        /// �����������ظ�����
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
        /// �Ƴ����������Ŀɽ�����,�Ƴ�֮��ض���������
        /// </summary>
        /// <param name="match"></param>
        /// <param name="isSort"></param>
        public void RemoveAllItem(System.Predicate<IUIInteractionItem> match)
        {
            this.itemList.RemoveAll(match);
            this.SortItemList();
        }

        /// <summary>
        /// �Ƴ������еĿɽ�����,�Ƴ���ض���������
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
        /// InteractionItemIndex ƫ��
        /// </summary>
        /// <param name="last"></param>
        /// <returns></returns>
        public int OffsetActiveIndex(int offset)
        {
            return this.activeIndex = (this.itemList.Count + this.activeIndex + offset) % this.itemList.Count;
        }

        /// <summary>
        /// ����һ��
        /// </summary>
        /// <returns></returns>
        public int BackwardOneActiveIndex()
        {
            return this.OffsetActiveIndex(-1);
        }

        /// <summary>
        /// ǰ��һ��
        /// </summary>
        /// <returns></returns>
        public int ForwardOneActiveIndex()
        {

            return this.OffsetActiveIndex(1);
        }

        #endregion
    }

}
