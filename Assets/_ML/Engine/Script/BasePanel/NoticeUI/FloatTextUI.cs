using ML.Engine.Manager;
using ML.Engine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace ML.Engine.UI
{
    public class FloatTextUI : UIBasePanel,INoticeUI
    {
        public TextMeshProUGUI Text;
        private Animator animator;
        protected override void Awake()
        {
            base.Awake();
            Text = transform.Find("Image").Find("Text").GetComponent<TextMeshProUGUI>();
            animator = GetComponent<Animator>();
        }

        #region INoticeUI
        public void DestroySelf()
        {
            GameManager.DestroyObj(gameObject);
        }
        public void SaveAsInstance()
        {
            this.gameObject.SetActive(false);
            this.animator.enabled = false;
        }
        #endregion


        public void CopyInstance<D>(D data)
        {
            this.gameObject.SetActive(true);
            this.animator.enabled = true;
            if(data is UIManager.FloatTextUIData floatTextUIData)
            {
                Text.text = floatTextUIData.msg;
            }
        }
    }
}

