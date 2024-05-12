using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.UI;

namespace ML.Engine.UI
{
    public class UIBtnListSlideWindow
    {
        private ScrollRect scrollRect;
        private UIBtnList uIBtnList;
        private UIBtnListContainer uIBtnListContainer;
        private Transform lastSelect;
        private Transform curSelect;
        public UIBtnListSlideWindow(ScrollRect scrollRect,UIBtnList uIBtnList)
        {
            this.scrollRect = scrollRect;
            this.uIBtnList = uIBtnList;
            this.lastSelect = null;
            this.curSelect = this.uIBtnList.GetCurSelected()?.transform;
            this.uIBtnList.OnSelectButtonChanged += () =>
            {
                this.lastSelect = this.curSelect;
                this.curSelect = this.uIBtnList.GetCurSelected()?.transform;
                SlideWindowBtn();
            };
        }

        public UIBtnListSlideWindow(ScrollRect scrollRect, UIBtnListContainer uIBtnListContainer)
        {
            this.scrollRect = scrollRect;
            this.uIBtnListContainer = uIBtnListContainer;
            this.lastSelect = null;
            this.curSelect = this.uIBtnListContainer.CurSelectUIBtnList?.Parent;
            /*this.uIBtnListContainer.AddOnSelectButtonListChangedAction(() =>
            {
                this.lastSelect = this.curSelect;
                this.curSelect = this.uIBtnListContainer.CurSelectUIBtnList?.Parent;
                SlideWindowBtnList();
            });*/
            
            this.uIBtnListContainer.AddOnSelectButtonChangedAction(() =>
            {
                this.lastSelect = this.curSelect;
                this.curSelect = this.uIBtnListContainer.CurSelectUIBtnList?.GetCurSelected()?.transform;
                SlideWindowBtn();
            });
        }

        private void SlideWindowBtn()
        {
            if (this.scrollRect == null) return;

            if (curSelect != null && lastSelect != null)
            {
                // 当前激活的TP四个边点有一个不位于窗口内 -> 更新窗口滑动
                RectTransform uiRectTransform = curSelect.GetComponent<RectTransform>();
                RectTransform scrollRectTransform = this.scrollRect.GetComponent<RectTransform>();
                // 获取 Content 的 RectTransform 组件
                RectTransform contentRect = this.scrollRect.content;

                // 获取 UI 元素的四个角点
                Vector3[] corners = new Vector3[4];
                uiRectTransform.GetWorldCorners(corners);
                bool allCornersVisible = true;
                for (int i = 0; i < 4; ++i)
                {
                    // 将世界空间的点转换为屏幕空间的点
                    Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                    // 判断 ScrollRect 是否包含这个点
                    if (!RectTransformUtility.RectangleContainsScreenPoint(scrollRectTransform, screenPoint, null))
                    {
                        allCornersVisible = false;
                        break;
                    }
                }
                // 当前激活的TP四个边点有一个不位于窗口内 -> 更新窗口滑动
                if (!allCornersVisible)
                {
                    // 将当前选中的这个放置于上一个激活TP的位置

                    // 设置滑动位置

                    // 获取点 A 和点 B 在 Content 中的位置
                    Vector2 positionA = (lastSelect.transform as RectTransform).anchoredPosition;
                    Vector2 positionB = (curSelect.transform as RectTransform).anchoredPosition;

                    // 计算点 B 相对于点 A 的偏移量
                    Vector2 offset = positionB - positionA;

                    // 根据偏移量更新 ScrollRect 的滑动位置
                    Vector2 normalizedPosition = scrollRect.normalizedPosition;
                    normalizedPosition += new Vector2(offset.x / (contentRect.rect.width - (contentRect.parent as RectTransform).rect.width), offset.y / (contentRect.rect.height - (contentRect.parent as RectTransform).rect.height));
                    scrollRect.normalizedPosition = normalizedPosition;
                }
            }
        }
        private void SlideWindowBtnList()
        {
            if (this.scrollRect == null || this.uIBtnListContainer == null) return;

            if (curSelect != null && lastSelect != null)
            {
                // 当前激活的TP四个边点有一个不位于窗口内 -> 更新窗口滑动
                RectTransform uiRectTransform = curSelect.GetComponent<RectTransform>();
                RectTransform scrollRectTransform = this.scrollRect.GetComponent<RectTransform>();

                RectTransform contentRect = this.scrollRect.content;

                // 获取 UI 元素的四个角点
                Vector3[] corners = new Vector3[4];
                uiRectTransform.GetWorldCorners(corners);
                bool allCornersVisible = true;
                for (int i = 0; i < 4; ++i)
                {
                    // 将世界空间的点转换为屏幕空间的点
                    Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                    // 判断 ScrollRect 是否包含这个点
                    if (!RectTransformUtility.RectangleContainsScreenPoint(scrollRectTransform, screenPoint, null))
                    {
                        allCornersVisible = false;
                        break;
                    }
                }
                // 当前激活的TP四个边点有一个不位于窗口内 -> 更新窗口滑动
                if (!allCornersVisible)
                {
                    // 将当前选中的这个放置于上一个激活TP的位置

                    // 设置滑动位置

                    // 获取点 A 和点 B 在 Content 中的位置
                    Vector2 positionA = (lastSelect.transform as RectTransform).anchoredPosition;
                    Vector2 positionB = (curSelect.transform as RectTransform).anchoredPosition;

                    // 计算点 B 相对于点 A 的偏移量
                    Vector2 offset = positionB - positionA;

                    // 根据偏移量更新 ScrollRect 的滑动位置
                    Vector2 normalizedPosition = scrollRect.normalizedPosition;
                    normalizedPosition += new Vector2(offset.x / (contentRect.rect.width - (contentRect.parent as RectTransform).rect.width), offset.y / (contentRect.rect.height - (contentRect.parent as RectTransform).rect.height));
                    scrollRect.normalizedPosition = normalizedPosition;
                }
            }
        }
        
    }
}


