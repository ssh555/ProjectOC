using ML.Engine.TextContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectOC.InventorySystem.UI
{
    public class UIInfiniteInventory
    {
        [System.Serializable]
        public struct InventoryPanel
        {
            public TextContent toptitle;
            public TextTip[] itemtype;
            public KeyTip lastterm;
            public KeyTip nextterm;
            public TextContent weightprefix;
            public TextContent descriptionprefix;
            public TextContent effectdescriptionprefix;
            public KeyTip use;
            public KeyTip back;
            public KeyTip drop;
            public KeyTip destroy;
        }

    }
}
