using ExcelToJson;
using System;
using System.Collections.Generic;


namespace ML.Engine.InventorySystem
{
    [System.Serializable]
    public struct ItemCategoryTableData : IGenData
    {
        public string id;
        public int sort;
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
            // 1 -> Sort
            this.sort = Program.ParseInt(row[1]);
            // 2 -> Type
            this.applicationScenario = Program.ParseEnum<ApplicationScenario>(row[2]);
            // 3 4 5 -> CategoryManage
            this.categoryManage = new CategoryManage();
            this.categoryManage.CategoryName = Program.ParseTextContent(row[3]);
            this.categoryManage.CategoryIcon = Program.ParseString(row[4]);
            this.categoryManage.ItemTypes = Program.ParseEnumList<ItemType>(row[5]);
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
