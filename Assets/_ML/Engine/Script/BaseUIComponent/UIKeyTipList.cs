using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.UI
{
    public class UIKeyTipList
    {
        private UIKeyTipComponent[] KeyTips;
        private Dictionary<string, UIKeyTipComponent> KeyTipDic = new Dictionary<string, UIKeyTipComponent>();

        public UIKeyTipList(Transform parent)
        {
            if(parent != null)
            {
                this.KeyTips = parent.GetComponentsInChildren<UIKeyTipComponent>(true);
                foreach (var keytip in KeyTips)
                {
                    keytip.Init();
                    KeyTipDic.Add(keytip.gameObject.name, keytip);
                }
            }
        }
        /// <summary>
        /// 设置按键提示文本
        /// </summary>
        public void SetKeyTiptext(string keyTipName,string keytipText)
        {
            if (this.KeyTipDic.ContainsKey(keyTipName))
            {
                KeyTipDic[keyTipName].keytip.text = keytipText;
            }
        }
        /// <summary>
        /// 设置按键描述文本
        /// </summary>
        public void SetDescriptiontext(string keyTipName, string descriptionText)
        {
            if (this.KeyTipDic.ContainsKey(keyTipName))
            {
                KeyTipDic[keyTipName].description.text = descriptionText;
            }
        }
    }
}


