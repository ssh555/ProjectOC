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
                // ��ǰ�����TP�ĸ��ߵ���һ����λ�ڴ����� -> ���´��ڻ���
                RectTransform uiRectTransform = curSelect.GetComponent<RectTransform>();
                RectTransform scrollRectTransform = this.scrollRect.GetComponent<RectTransform>();
                // ��ȡ Content �� RectTransform ���
                RectTransform contentRect = this.scrollRect.content;

                // ��ȡ UI Ԫ�ص��ĸ��ǵ�
                Vector3[] corners = new Vector3[4];
                uiRectTransform.GetWorldCorners(corners);
                bool allCornersVisible = true;
                for (int i = 0; i < 4; ++i)
                {
                    // ������ռ�ĵ�ת��Ϊ��Ļ�ռ�ĵ�
                    Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                    // �ж� ScrollRect �Ƿ���������
                    if (!RectTransformUtility.RectangleContainsScreenPoint(scrollRectTransform, screenPoint, null))
                    {
                        allCornersVisible = false;
                        break;
                    }
                }
                // ��ǰ�����TP�ĸ��ߵ���һ����λ�ڴ����� -> ���´��ڻ���
                if (!allCornersVisible)
                {
                    // ����ǰѡ�е������������һ������TP��λ��

                    // ���û���λ��

                    // ��ȡ�� A �͵� B �� Content �е�λ��
                    Vector2 positionA = (lastSelect.transform as RectTransform).anchoredPosition;
                    Vector2 positionB = (curSelect.transform as RectTransform).anchoredPosition;

                    // ����� B ����ڵ� A ��ƫ����
                    Vector2 offset = positionB - positionA;

                    // ����ƫ�������� ScrollRect �Ļ���λ��
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
                // ��ǰ�����TP�ĸ��ߵ���һ����λ�ڴ����� -> ���´��ڻ���
                RectTransform uiRectTransform = curSelect.GetComponent<RectTransform>();
                RectTransform scrollRectTransform = this.scrollRect.GetComponent<RectTransform>();

                RectTransform contentRect = this.scrollRect.content;

                // ��ȡ UI Ԫ�ص��ĸ��ǵ�
                Vector3[] corners = new Vector3[4];
                uiRectTransform.GetWorldCorners(corners);
                bool allCornersVisible = true;
                for (int i = 0; i < 4; ++i)
                {
                    // ������ռ�ĵ�ת��Ϊ��Ļ�ռ�ĵ�
                    Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                    // �ж� ScrollRect �Ƿ���������
                    if (!RectTransformUtility.RectangleContainsScreenPoint(scrollRectTransform, screenPoint, null))
                    {
                        allCornersVisible = false;
                        break;
                    }
                }
                // ��ǰ�����TP�ĸ��ߵ���һ����λ�ڴ����� -> ���´��ڻ���
                if (!allCornersVisible)
                {
                    // ����ǰѡ�е������������һ������TP��λ��

                    // ���û���λ��

                    // ��ȡ�� A �͵� B �� Content �е�λ��
                    Vector2 positionA = (lastSelect.transform as RectTransform).anchoredPosition;
                    Vector2 positionB = (curSelect.transform as RectTransform).anchoredPosition;

                    // ����� B ����ڵ� A ��ƫ����
                    Vector2 offset = positionB - positionA;

                    // ����ƫ�������� ScrollRect �Ļ���λ��
                    Vector2 normalizedPosition = scrollRect.normalizedPosition;
                    normalizedPosition += new Vector2(offset.x / (contentRect.rect.width - (contentRect.parent as RectTransform).rect.width), offset.y / (contentRect.rect.height - (contentRect.parent as RectTransform).rect.height));
                    scrollRect.normalizedPosition = normalizedPosition;
                }
            }
        }
        
    }
}


