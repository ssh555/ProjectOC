using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ML.Example.UI
{
    public class SingletonActivedButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        #region static => to-do : 丑陋但能用
        private static Dictionary<System.Type, SingletonActivedButton> _curActivedDict = new Dictionary<System.Type, SingletonActivedButton>();
        /// <summary>
        /// 当前激活
        /// </summary>
        public static SingletonActivedButton GetCurActived(System.Type type)
        {
            if (_curActivedDict.ContainsKey(type))
            {
                return _curActivedDict[type];
            }
            return null;
        }
        public static bool SetCurActived(System.Type type, SingletonActivedButton actived)
        {
            if (_curActivedDict.ContainsKey(type))
            {
                _curActivedDict[type] = actived;
                return true;
            }
            return false;
        }

        public virtual void SetActived(SingletonActivedButton actived, System.Type activeNullType)
        {
            SingletonActivedButton ac = GetCurActived(actived == null ? activeNullType : actived.GetType());
            if (ac == actived)
            {
                return;
            }

            if(ac != null)
            {
                ac.OnDisactiveListener?.Invoke(ac, actived);
            }
            SetCurActived(actived == null ? activeNullType : actived.GetType(), actived);
            if(actived != null)
                actived.OnActiveListener?.Invoke(ac, actived);
        }

        #endregion

        #region Component
        public RectTransform rectTransform { get; protected set; }
        #endregion

        #region PointerDown
        /// <summary>
        /// 任意点击
        /// </summary>
        public System.Action<PointerEventData> PointerDownListener;
        /// <summary>
        /// 激活后不响应
        /// </summary>
        public System.Action<PointerEventData> PointerDownForActivedListener;
        #endregion

        #region PointerEnter
        /// <summary>
        /// 任意进入
        /// </summary>
        public System.Action<PointerEventData> PointerEnterListener;
        /// <summary>
        /// 激活后不响应
        /// </summary>
        public System.Action<PointerEventData> PointerEnterForActivedListener;
        #endregion

        #region PointerExit
        /// <summary>
        /// 任意离开
        /// </summary>
        public System.Action<PointerEventData> PointerExitListener;
        /// <summary>
        /// 激活后不响应
        /// </summary>
        public System.Action<PointerEventData> PointerExitForActivedListener;
        #endregion

        #region Active
        /// <summary>
        /// 激活时调用
        /// PreObject
        /// PostObject => this
        /// </summary>
        public System.Action<SingletonActivedButton, SingletonActivedButton> OnActiveListener;
        /// <summary>
        /// 取消激活时调用
        /// PreObject => this
        /// PostObject
        /// </summary>
        public System.Action<SingletonActivedButton, SingletonActivedButton> OnDisactiveListener;

        #endregion

        protected virtual void Awake()
        {
            this.rectTransform = this.transform as RectTransform;
            if (!_curActivedDict.ContainsKey(this.GetType()))
            {
                _curActivedDict.Add(this.GetType(), null);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (this != GetCurActived(this.GetType()))
            {
                this.PointerDownForActivedListener?.Invoke(eventData);
            }
            this.PointerDownListener?.Invoke(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if(this != GetCurActived(this.GetType()))
            {
                this.PointerEnterForActivedListener?.Invoke(eventData);
            }
            this.PointerEnterListener?.Invoke(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (this != GetCurActived(this.GetType()))
            {
                this.PointerExitForActivedListener?.Invoke(eventData);
            }
            this.PointerExitListener?.Invoke(eventData);
        }

        private void OnDestroy()
        {
            if (GetCurActived(this.GetType()) == this)
            {
                SetActived(this, this.GetType());
            }
        }
    }

}
