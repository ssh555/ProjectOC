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
            RefreshKetTip(datas);
        }
        /// <summary>
        /// ���ð�����ʾ�ı������ð��������ı�
        /// </summary>
        private void RefreshKeyTiptext(string keyName, string keyTipName,string keytipText)
        {
            if (this.KeyTipDic.ContainsKey(keyName))
            {
                KeyTipDic[keyName].Refresh(keyTipName, keytipText);
            }
        }
        /// <summary>
        /// �ⲿ����ˢ��KeyTip�ӿ�
        /// </summary>
        public void RefreshKetTip(T datas)
        {
            KeyTip[] keyTips = GameManager.Instance.InputManager.ExportKeyTipValues(datas);
            foreach (var keyTip in keyTips)
            {
                //��̬������ʾ
                if(keyTip.keymap.ActionMapName!=null&& keyTip.keymap.ActionName!=null)
                {
                    Debug.Log("��̬������ʾ "+keyTip.keyname + " | " + keyTip.keymap.ActionMapName + " | " + keyTip.keymap.ActionName + " | " + keyTip.keymap.XBOX + " | " + keyTip.keymap.KeyBoard);
                    InputAction inputAction = GameManager.Instance.InputManager.GetInputAction((keyTip.keymap.ActionMapName, keyTip.keymap.ActionName));
                    RefreshKeyTiptext(keyTip.keyname, GameManager.Instance.InputManager.GetInputActionBindText(inputAction), keyTip.description.GetText());
                }
                else if(keyTip.keymap.XBOX != null && keyTip.keymap.KeyBoard != null)//��̬������ʾ
                {
                    Debug.Log("��̬������ʾ "+keyTip.keyname + " | " + keyTip.keymap.ActionMapName + " | " + keyTip.keymap.ActionName + " | " + keyTip.keymap.XBOX + " | " + keyTip.keymap.KeyBoard);
                    Debug.Log(keyTip.GetKeyMapText() + " " + keyTip.GetDescription());
                    RefreshKeyTiptext(keyTip.keyname, keyTip.GetKeyMapText(), keyTip.GetDescription());
                }

                
            }
        }
    }
}


