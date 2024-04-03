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
        private T datas;
        private Dictionary<string, UIKeyTipComponent> KeyTipDic = new Dictionary<string, UIKeyTipComponent>();

        public UIKeyTipList(Transform parent, T datas)
        {
            if (parent != null)
            {
                UIKeyTipComponent[] KeyTips = parent.GetComponentsInChildren<UIKeyTipComponent>(true);
                foreach (var keytip in KeyTips)
                {
                    keytip.Init();
                    KeyTipDic.Add(keytip.gameObject.name, keytip);
                }
            }
            this.datas = datas;
            RefreshKetTip();
        }
        /// <summary>
        /// 设置按键提示文本与设置按键描述文本
        /// </summary>
        private void RefreshKeyTiptext(string keyName, string keyTipName,string keytipText)
        {
            if (this.KeyTipDic.ContainsKey(keyName))
            {
                KeyTipDic[keyName].Refresh(keyTipName, keytipText);
            }
        }
        /// <summary>
        /// 外部调用刷新KeyTip接口
        /// </summary>
        public void RefreshKetTip()
        {
            KeyTip[] keyTips = GameManager.Instance.InputManager.ExportKeyTipValues(this.datas);
            foreach (var keyTip in keyTips)
            {
                InputAction inputAction = GameManager.Instance.InputManager.GetInputAction((keyTip.keymap.ActionMapName, keyTip.keymap.ActionName));
                RefreshKeyTiptext(keyTip.keyname, GameManager.Instance.InputManager.GetInputActionBindText(inputAction), keyTip.description.GetText());
            }
        }
    }
}


