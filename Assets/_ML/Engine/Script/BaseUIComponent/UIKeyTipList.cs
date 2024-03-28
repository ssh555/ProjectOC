using ML.Engine.Manager;
using ML.Engine.TextContent;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ML.Engine.UI
{
    public class UIKeyTipList<T>
    {
        private UIKeyTipComponent[] KeyTips;
        private T datas;
        private Dictionary<string, UIKeyTipComponent> KeyTipDic = new Dictionary<string, UIKeyTipComponent>();

        public UIKeyTipList(Transform parent, T datas)
        {
            if (parent != null)
            {
                this.KeyTips = parent.GetComponentsInChildren<UIKeyTipComponent>(true);
                foreach (var keytip in KeyTips)
                {
                    keytip.Init();
                    KeyTipDic.Add(keytip.gameObject.name, keytip);
                }
            }
            this.datas = datas;
            RefreshKetTip(datas);
        }
        /// <summary>
        /// 设置按键提示文本
        /// </summary>
        private void SetKeyTiptext(string keyTipName,string keytipText)
        {
            if (this.KeyTipDic.ContainsKey(keyTipName))
            {
                if (KeyTipDic[keyTipName].keytip != null)
                {
                    KeyTipDic[keyTipName].keytip.text = keytipText;
                }
                
            }
        }
        /// <summary>
        /// 设置按键描述文本
        /// </summary>
        private void SetDescriptiontext(string keyTipName, string descriptionText)
        {
            if (this.KeyTipDic.ContainsKey(keyTipName))
            {
                if (KeyTipDic[keyTipName].description != null)
                {
                    KeyTipDic[keyTipName].description.text = descriptionText;
                }
            }
        }
        /// <summary>
        /// 外部调用刷新KeyTip接口
        /// </summary>
        public void RefreshKetTip(T datas)
        {
            KeyTip[] keyTips = GameManager.Instance.InputManager.ExportKeyTipValues(datas);
            foreach (var keyTip in keyTips)
            {
                InputAction inputAction = GameManager.Instance.InputManager.GetInputAction((keyTip.keymap.ActionMapName, keyTip.keymap.ActionName));

                this.SetKeyTiptext(keyTip.keyname, GameManager.Instance.InputManager.GetInputActionBindText(inputAction));
                this.SetDescriptiontext(keyTip.keyname, keyTip.description.GetText());
            }
        }
    }
}


