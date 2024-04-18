using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.UI.Text
{
    [System.Serializable]
    public sealed class MLTextMeshProUGUI : TMPro.TextMeshProUGUI
    {
        [LabelText("是否本地化"), SerializeField, HideInInspector]
        private bool IsLocalize = false;

        [LabelText("本地化ID"), SerializeField, HideInInspector]
        private string localizeID;

//        public override string text
//        {
//            get
//            {
//                return base.text;
//            }
//            set
//            {
//#if UNITY_EDITOR
//                if (IsLocalize)
//                {
//                    Debug.LogWarning("本地化的Text不可从外部设置text显示文本");
//                }
//#endif
//                base.text = value;
//            }
//        }

//        /// <summary>
//        /// 可本地化时才能通过这个函数设置 -> 由本地化统一设置
//        /// </summary>
//        public void SetLocalizeText()
//        {

//        }

    }

}
