using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.UI
{
    public class UIBasePanel : MonoBehaviour
    {
        /// <summary>
        /// 唯一标识符
        /// </summary>
        public string ID { get; protected set; }

        /// <summary>
        /// 所属UIManager
        /// </summary>
        private UIManager _uiMgr;
        /// <summary>
        /// 所属UIManager
        /// </summary>
        public UIManager UIMgr
        {
            get
            {
                if(_uiMgr == null)
                {
                    _uiMgr = ML.Engine.Manager.GameManager.Instance.UIManager;
                }
                return _uiMgr;
            }
            set
            {
                UIMgr = value;
            }
        }

        /// <summary>
        /// 压入UI栈时调用
        /// </summary>
        public virtual void OnEnter()
        {
            this.gameObject.SetActive(true);
        }

        /// <summary>
        /// 暂停时调用，即不处于栈顶时
        /// </summary>
        public virtual void OnPause()
        {
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// 再次成为栈顶时调用
        /// </summary>
        public virtual void OnRecovery()
        {
            this.gameObject.SetActive(true);
        }

        /// <summary>
        /// 出栈时调用
        /// </summary>
        public virtual void OnExit()
        {
            this.gameObject.SetActive(false);
        }

    }

}
