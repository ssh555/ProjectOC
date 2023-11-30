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
        private UIManager _uiMgr;
        /// <summary>
        /// ����UIManager
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
        /// ѹ��UIջʱ����
        /// </summary>
        public virtual void OnEnter()
        {
            this.gameObject.SetActive(true);
        }

        /// <summary>
        /// ��ͣʱ���ã���������ջ��ʱ
        /// </summary>
        public virtual void OnPause()
        {
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// �ٴγ�Ϊջ��ʱ����
        /// </summary>
        public virtual void OnRecovery()
        {
            this.gameObject.SetActive(true);
        }

        /// <summary>
        /// ��ջʱ����
        /// </summary>
        public virtual void OnExit()
        {
            this.gameObject.SetActive(false);
        }

    }

}
