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
    }
    [System.Serializable]
    public struct KeyTip
    {
        public string keyname;
        public KeyMap keymap;
        public TextContent description;
    }
}
