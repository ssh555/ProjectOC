using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.UI
{
    public class UIBasePanel : MonoBehaviour
    {
        /// <summary>
        /// Ψһ��ʶ��
        /// </summary>
        public string ID { get; protected set; }

        /// <summary>
        /// ����UIManager
        /// </summary>
        protected UIManager UIMgr;
        /// <summary>
        /// ����UIManager
        /// </summary>
        public UIManager SetUIMgr
        {
            set
            {
                UIMgr = value;
            }
        }

        /// <summary>
        /// ѹ��UIջʱ����
        /// </summary>
        public virtual void OnEnter()
        {
            return;
        }

        /// <summary>
        /// ��ͣʱ���ã���������ջ��ʱ
        /// </summary>
        public virtual void OnPause()
        {
            return;
        }

        /// <summary>
        /// �ٴγ�Ϊջ��ʱ����
        /// </summary>
        public virtual void OnRecovery()
        {
            return;
        }

        /// <summary>
        /// ��ջʱ����
        /// </summary>
        public virtual void OnExit()
        {
            return;
        }

    }

}
