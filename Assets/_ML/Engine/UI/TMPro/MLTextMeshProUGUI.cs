using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.UI.Text
{
    [System.Serializable]
    public sealed class MLTextMeshProUGUI : TMPro.TextMeshProUGUI
    {
        [LabelText("�Ƿ񱾵ػ�"), SerializeField, HideInInspector]
        private bool IsLocalize = false;

        [LabelText("���ػ�ID"), SerializeField, HideInInspector]
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
//                    Debug.LogWarning("���ػ���Text���ɴ��ⲿ����text��ʾ�ı�");
//                }
//#endif
//                base.text = value;
//            }
//        }

//        /// <summary>
//        /// �ɱ��ػ�ʱ����ͨ������������� -> �ɱ��ػ�ͳһ����
//        /// </summary>
//        public void SetLocalizeText()
//        {

//        }

    }

}
