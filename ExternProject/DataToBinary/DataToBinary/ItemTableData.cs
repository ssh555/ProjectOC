using ExcelToJson;
using System;

// 命名空间
namespace ML.Engine.InventorySystem
{
    // 如果是类内部的类型，需要用类包裹
    // 可序列化
    [System.Serializable]
    // 结构体|类 名称
    public struct ItemTableData : IGenData
    {
        // 属性名称和类型, 如果有其他类型引用，其他类型也必须一模一样
        public string id;
        public int sort;
        public ItemType itemtype;
        public TextContent.TextContent name;
        public int weight;
        public string icon;
        public TextContent.TextContent itemdescription;
        public TextContent.TextContent effectsdescription;
        public string type;
        public bool bcanstack;
        public int maxamount;
        public string worldobject;

        public bool GenData(string[] row)
        {
            if (row[0] == null || row[0] == "")
            {
                return false;
            }
            // 0 -> ID
            this.id = row[0];
            // 1 -> Sort
            this.sort = int.Parse(row[1]);
            // 2 -> ItemType
            this.itemtype = (ItemType)Enum.Parse(typeof(ItemType), row[2]);
            // 3 -> Name
            this.name = new TextContent.TextContent();
            this.name.Chinese = row[3];
            this.name.English = row[3];
            // 4 -> Weight
            this.weight = int.Parse(row[4]);
            // 5 -> Icon
            this.icon = row[5];
            // 6 -> ItemsDescription
            this.itemdescription = new TextContent.TextContent();
            this.itemdescription.Chinese = row[6];
            this.itemdescription.English = row[6];
            // 7 -> EffectsDescription
            this.effectsdescription = new TextContent.TextContent();
            this.effectsdescription.Chinese = row[7];
            this.effectsdescription.English = row[7];
            // TODO: Change
            this.type = "ResourceItem";
            this.bcanstack = true;
            this.maxamount = 999;
            this.worldobject = "Item";
            return true;
        }
    }

    [System.Serializable]
    public enum ItemType
    {
        None,
        Equip,
        Food,
        Material,
        Mission,
    }
}
