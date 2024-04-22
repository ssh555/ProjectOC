using System;
using System.Collections.Generic;

namespace ML.Engine.TextContent
{
    [System.Serializable]
    public struct KeyMap
    {
        public string ActionMapName;
        public string ActionName;

        public string KeyBoard;

        public string XBOX;

        public string GetKeyMapText()
        {
            if (Config.inputDevice == Config.InputDevice.XBOX)
            {
                return XBOX;
            }
            else if (Config.inputDevice == Config.InputDevice.Keyboard)
            {
                return KeyBoard;
            }
            return "";
        }

    }

    [System.Serializable]
    public struct KeyTip
    {
        public string keyname;
        public KeyMap keymap;
        public TextContent description;
        public int index;//规定该项为0时为未配置  => 选择binding时下标配置从1开始配置

        public string GetDescription()
        {
            return this.description.GetText();
        }

        public string GetKeyMapText()
        {
            return this.keymap.GetKeyMapText();
        }

    }
}
