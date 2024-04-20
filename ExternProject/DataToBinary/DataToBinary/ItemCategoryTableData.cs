using ExcelToJson;
using System;
using System.Collections.Generic;


namespace ML.Engine.InventorySystem
{
    [System.Serializable]
    public struct ItemCategoryTableData : IGenData
    {
        public string id;
        public ApplicationScenario applicationScenario;
        public CategoryManage categoryManage;

        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0]))
            {
                return false;
            }
            // 0 -> ID
            this.id = Program.ParseString(row[0]);
            // 1 -> Type
            this.applicationScenario = Program.ParseEnum<ApplicationScenario>(row[1]);
            // 2 3 4 -> CategoryManage
            this.categoryManage = new CategoryManage();
            this.categoryManage.CategoryName = Program.ParseTextContent(row[2]);
            this.categoryManage.CategoryIcon = Program.ParseString(row[3]);
            this.categoryManage.ItemTypes = Program.ParseEnumList<ItemType>(row[4]);
            return true;
        }
    }

    public enum ApplicationScenario
    {
        Bag = 0
    }

    [System.Serializable]
    public struct CategoryManage
    {
        public TextContent.TextContent CategoryName;
        public string CategoryIcon;
        public List<ItemType> ItemTypes;
    }
}
