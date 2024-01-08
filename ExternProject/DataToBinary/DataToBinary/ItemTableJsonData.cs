using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.Engine.InventorySystem
{
    [System.Serializable]
    public enum ItemType
    {
        None,
        Equip,
        Food,
        Material,
        Mission,
    }

    [System.Serializable]
    public struct ItemTableJsonData
    {
        public string id;
        public int sort;
        public ItemType itemtype;
        public ML.Engine.TextContent.TextContent name;
        public int weight;
        public string icon;
        // TODO: 修改为TextContent
        public string itemdescription;
        // TODO: 修改为TextContent
        public string effectsdescription;
        public string type;
        public bool bcanstack;
        public int maxamount;
        public string worldobject;
    }
}
