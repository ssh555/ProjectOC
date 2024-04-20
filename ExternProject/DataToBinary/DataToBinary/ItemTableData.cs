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
        public bool canstack;
        public bool candestroy;
        public TextContent.TextContent itemdescription;
        public TextContent.TextContent effectsdescription;
        public string type;
        public int maxamount;
        public string worldobject;
        
        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0]))
            {
                return false;
            }
            // 0 -> ID
            this.id = Program.ParseString(row[0]);
            // 1 -> Sort
            this.sort = Program.ParseInt(row[1]);
            // 2 -> ItemType
            this.itemtype = Program.ParseEnum<ItemType>(row[2]);
            // 3 -> Name
            this.name = Program.ParseTextContent(row[3]);
            // 4 -> Weight
            this.weight = Program.ParseInt(row[4]);
            // 5 -> Icon
            this.icon = Program.ParseString(row[5]);
            // 6 -> CanStack
            this.canstack = Program.ParseBool(row[6]); ;
            // 7 -> CanDestroy
            this.candestroy = Program.ParseBool(row[7]); ;
            // 8 -> ItemsDescription
            this.itemdescription = Program.ParseTextContent(row[8]);
            // 9 -> EffectsDescription
            this.effectsdescription = Program.ParseTextContent(row[9]);
            // TODO: Change
            this.type = "ResourceItem";
            this.maxamount = int.MaxValue;
            this.worldobject = "Item";
            return true;
        }
    }

    [System.Serializable]
    public enum ItemType
    {
        None,
        Vegetable,
        Meat,
        Feed,
        BoxLunch,
        Juice,
        Snack,
        MedicalPro,
        Medicine,
        Wood,
        Fabric,
        Stone,
        Metal,
        Magic,
        Calculus,
        Creature,
        Cloth,
        Mission,
    }
}
