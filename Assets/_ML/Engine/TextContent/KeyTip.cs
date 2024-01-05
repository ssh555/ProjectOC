using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.Engine.TextContent
{
    [System.Serializable]
    public struct KeyMap
    {
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

        public string GetKeyMapText()
        {
            return this.keymap.GetKeyMapText();
        }

        public string GetDescription()
        {
            return this.description.GetText();
        }
    }
}
