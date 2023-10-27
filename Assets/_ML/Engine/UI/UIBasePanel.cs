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
        protected UIManager UIMgr;
        /// <summary>
        /// 所属UIManager
        /// </summary>
        public UIManager SetUIMgr
        {
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
            return;
        }

        /// <summary>
        /// 暂停时调用，即不处于栈顶时
        /// </summary>
        public virtual void OnPause()
        {
            return;
        }

        /// <summary>
        /// 再次成为栈顶时调用
        /// </summary>
        public virtual void OnRecovery()
        {
            return;
        }

        /// <summary>
        /// 出栈时调用
        /// </summary>
        public virtual void OnExit()
        {
            return;
        }

    }

}
