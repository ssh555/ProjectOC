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
        public string ActionMapName;
        public string ActionName;
    }

    [System.Serializable]
    public struct KeyTip
    {
        public string keyname;
        public KeyMap keymap;
        public TextContent description;


        public string GetDescription()
        {
            return this.description.GetText();
        }
    }
}
