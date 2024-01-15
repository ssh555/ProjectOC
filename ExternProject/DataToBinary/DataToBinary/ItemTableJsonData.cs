using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 命名空间
namespace ML.Engine.InventorySystem
{
    // 如果是类内部的类型，需要用类包裹
    [System.Serializable]
    public enum ItemType
    {
        None,
        Equip,
        Food,
        Material,
        Mission,
    }
    
    // 可序列化
    [System.Serializable]
    // 结构体|类 名称
    public struct ItemTableJsonData
    {
        // 属性名称和类型
        public string id;
        public int sort;
        public ItemType itemtype;
        // 如果有其他类型引用，其他类型也必须一模一样
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
