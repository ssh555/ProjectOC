using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.Engine.TextContent
{
    [System.Serializable]
    public struct MultiTextContent
    {
        public TextContent[] description;
        public int index;
    }

}
