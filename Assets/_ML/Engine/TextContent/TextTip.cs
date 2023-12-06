using ML.Engine.TextContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.Engine.TextContent
{
    [System.Serializable]
    public class TextTip
    {
        public string name;
        public TextContent description;

        public string GetDescription()
        {
            return this.description.GetText();
        }
    }

    [System.Serializable]
    public class TextTipArray
    {
        public TextTip[] array;
    }
}
